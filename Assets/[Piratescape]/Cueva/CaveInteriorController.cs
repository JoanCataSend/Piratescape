using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class CaveInteriorController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private RainController rainController;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Profundidad de la cueva")]
    [Tooltip("Punto de la entrada. Profundidad 0 = aqui todavia entra luz.")]
    [SerializeField] private Transform puntoEntrada;
    [Tooltip("Punto del fondo. Profundidad 1 = aqui casi no llega luz exterior.")]
    [SerializeField] private Transform puntoFondo;
    [Tooltip("Si no asignas entrada/fondo, se usa esta direccion local del trigger.")]
    [SerializeField] private Vector3 direccionLocalProfundidad = Vector3.forward;
    [SerializeField] private float distanciaProfundidadFallback = 12f;
    [SerializeField] private bool invertirProfundidad = false;
    [SerializeField] private float suavizadoProfundidad = 8f;

    [Header("Lluvia dentro de la cueva")]
    [SerializeField] private bool bloquearLluviaDentro = true;
    [Tooltip("A partir de que profundidad empieza a desaparecer la lluvia.")]
    [Range(0f, 1f)]
    [SerializeField] private float profundidadInicioBloqueoLluvia = 0.02f;
    [Tooltip("A partir de que profundidad la lluvia ya desaparece por completo.")]
    [Range(0f, 1f)]
    [SerializeField] private float profundidadBloqueoTotalLluvia = 0.18f;
    [SerializeField] private bool bloquearLluviaAlInstanteAlEntrar = true;

    [Header("Bloqueo de luz exterior")]
    [Tooltip("No usa postprocesado ni exposure: solo reduce luz solar/lunar/ambiente. Las antorchas y faroles siguen iluminando.")]
    [SerializeField] private bool reducirLuzExterior = true;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorSolEnEntrada = 0.85f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorSolEnFondo = 0.04f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorLunaEnEntrada = 0.80f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorLunaEnFondo = 0.06f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorReflejosEnEntrada = 0.75f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorReflejosEnFondo = 0.12f;
    [Range(0f, 1f)] [SerializeField] private float mezclaColorAmbienteEntrada = 0.15f;
    [Range(0f, 1f)] [SerializeField] private float mezclaColorAmbienteFondo = 0.85f;
    [SerializeField] private Color colorAmbienteCueva = new Color(0.035f, 0.042f, 0.055f);

    [Header("Oscuridad real interior")]
    [Tooltip("Hace que la cueva se comporte como interior: no baja la exposicion de camara, solo quita luz exterior/ambiente. Antorchas y faroles siguen iluminando.")]
    [SerializeField] private bool usarOscuridadInteriorReal = true;
    [Tooltip("Desde esta profundidad empieza a apagarse fuerte la luz exterior.")]
    [Range(0f, 1f)]
    [SerializeField] private float profundidadInicioOscuridadTotal = 0.12f;
    [Tooltip("A partir de esta profundidad la cueva queda practicamente negra si no hay antorcha/farol.")]
    [Range(0f, 1f)]
    [SerializeField] private float profundidadOscuridadTotal = 0.72f;
    [Tooltip("Curva extra de oscuridad. 0 = entrada, 1 = fondo.")]
    [SerializeField] private AnimationCurve curvaOscuridadInterior = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Range(0f, 1f)] [SerializeField] private float multiplicadorSolOscuridadTotal = 0f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorLunaOscuridadTotal = 0f;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorReflejosOscuridadTotal = 0f;
    [Tooltip("Si esta activo, cambia el modo de ambiente a Flat dentro de la cueva para que no entre luz de skybox.")]
    [SerializeField] private bool forzarAmbientePlanoEnCueva = true;
    [Tooltip("Color de ambiente cuando estas al fondo. Negro = sin luz ambiental.")]
    [SerializeField] private Color colorAmbienteOscuridadTotal = Color.black;
    [Tooltip("Opcional: apaga otras luces exteriores que puedan colarse dentro de la cueva.")]
    [SerializeField] private Light[] lucesExterioresExtra;

    [Header("Opcional - luz de entrada")]
    [Tooltip("Si tienes una luz suave en la boca de la cueva, puede apagarse hacia el fondo.")]
    [SerializeField] private Light luzEntradaCueva;
    [Range(0f, 1f)] [SerializeField] private float multiplicadorLuzEntradaEnFondo = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs = false;
    [SerializeField] private bool jugadorDentroDebug;
    [Range(0f, 1f)] [SerializeField] private float profundidadActualDebug;
    [Range(0f, 1f)] [SerializeField] private float oclusionLluviaDebug;

    private bool jugadorDentro;
    private float profundidadObjetivo;
    private float profundidadActual;

    private bool ajustesAplicados;
    private float sunIntensityAntes;
    private float moonIntensityAntes;
    private float reflectionIntensityAntes;
    private Color ambientColorAntes;
    private float luzEntradaIntensityAntes;
    private AmbientMode ambientModeAntes;
    private float[] lucesExtraIntensityAntes;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (rainController == null)
        {
            rainController = FindFirstObjectByType<RainController>();
        }

        if (jugador == null)
        {
            BuscarJugador();
        }
    }

    private void OnDisable()
    {
        RestaurarAjustesAplicados();

        if (rainController != null)
        {
            rainController.ClearInteriorRainOcclusion();
        }
    }

    private void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
        }

        if (jugadorDentro && jugador != null)
        {
            profundidadObjetivo = CalcularProfundidad(jugador.position);
        }
        else
        {
            profundidadObjetivo = 0f;
        }

        float velocidad = Mathf.Max(0.1f, suavizadoProfundidad);
        profundidadActual = Mathf.MoveTowards(profundidadActual, profundidadObjetivo, Time.deltaTime * velocidad);

        ActualizarBloqueoLluvia();
        ActualizarDebug();
    }

    private void LateUpdate()
    {
        RestaurarAjustesAplicados();

        if (!reducirLuzExterior || profundidadActual <= 0.001f)
        {
            return;
        }

        AplicarBloqueoLuzExterior(profundidadActual);
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform player = ObtenerJugadorDesdeCollider(other);
        if (player == null)
        {
            return;
        }

        jugador = player;
        jugadorDentro = true;

        if (mostrarLogs)
        {
            Debug.Log("[CaveInteriorController] Jugador ha entrado en la cueva.", this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Transform player = ObtenerJugadorDesdeCollider(other);
        if (player == null)
        {
            return;
        }

        jugador = player;
        jugadorDentro = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Transform player = ObtenerJugadorDesdeCollider(other);
        if (player == null || jugador == null || player != jugador)
        {
            return;
        }

        jugadorDentro = false;

        if (mostrarLogs)
        {
            Debug.Log("[CaveInteriorController] Jugador ha salido de la cueva.", this);
        }
    }

    private void BuscarJugador()
    {
        GameObject go = GameObject.FindGameObjectWithTag(tagJugador);
        if (go != null)
        {
            jugador = go.transform;
        }
    }

    private Transform ObtenerJugadorDesdeCollider(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        if (other.CompareTag(tagJugador))
        {
            return other.transform;
        }

        Transform root = other.transform.root;
        if (root != null && root.CompareTag(tagJugador))
        {
            return root;
        }

        Transform actual = other.transform;
        while (actual != null)
        {
            if (actual.CompareTag(tagJugador))
            {
                return actual;
            }

            actual = actual.parent;
        }

        return null;
    }

    private float CalcularProfundidad(Vector3 posicionJugador)
    {
        float t;

        if (puntoEntrada != null && puntoFondo != null && puntoEntrada != puntoFondo)
        {
            Vector3 inicio = puntoEntrada.position;
            Vector3 fin = puntoFondo.position;
            Vector3 direccion = fin - inicio;
            float longitudCuadrada = direccion.sqrMagnitude;

            if (longitudCuadrada <= 0.0001f)
            {
                t = 0f;
            }
            else
            {
                t = Vector3.Dot(posicionJugador - inicio, direccion) / longitudCuadrada;
            }
        }
        else
        {
            Vector3 local = transform.InverseTransformPoint(posicionJugador);
            Vector3 direccionLocal = direccionLocalProfundidad.sqrMagnitude > 0.0001f
                ? direccionLocalProfundidad.normalized
                : Vector3.forward;

            float distancia = Mathf.Max(0.1f, distanciaProfundidadFallback);
            float valor = Vector3.Dot(local, direccionLocal);
            t = Mathf.InverseLerp(-distancia * 0.5f, distancia * 0.5f, valor);
        }

        t = Mathf.Clamp01(t);

        if (invertirProfundidad)
        {
            t = 1f - t;
        }

        return t;
    }

    private void ActualizarBloqueoLluvia()
    {
        if (!bloquearLluviaDentro || rainController == null)
        {
            return;
        }

        float oclusion;

        if (bloquearLluviaAlInstanteAlEntrar && jugadorDentro)
        {
            oclusion = 1f;
        }
        else
        {
            float inicio = Mathf.Clamp01(profundidadInicioBloqueoLluvia);
            float fin = Mathf.Clamp01(Mathf.Max(inicio + 0.001f, profundidadBloqueoTotalLluvia));
            oclusion = Mathf.InverseLerp(inicio, fin, profundidadActual);
        }

        rainController.SetInteriorRainOcclusion(oclusion);
        oclusionLluviaDebug = oclusion;
    }

    private void AplicarBloqueoLuzExterior(float profundidad)
    {
        GuardarValoresAntesDeAplicar();

        float profundidadOscura = CalcularFactorOscuridadReal(profundidad);

        float solMultiplier = Mathf.Lerp(multiplicadorSolEnEntrada, multiplicadorSolEnFondo, profundidad);
        float lunaMultiplier = Mathf.Lerp(multiplicadorLunaEnEntrada, multiplicadorLunaEnFondo, profundidad);
        float reflejosMultiplier = Mathf.Lerp(multiplicadorReflejosEnEntrada, multiplicadorReflejosEnFondo, profundidad);
        float mezclaAmbiente = Mathf.Lerp(mezclaColorAmbienteEntrada, mezclaColorAmbienteFondo, profundidad);
        Color colorAmbienteObjetivo = colorAmbienteCueva;

        if (usarOscuridadInteriorReal)
        {
            solMultiplier = Mathf.Lerp(solMultiplier, multiplicadorSolOscuridadTotal, profundidadOscura);
            lunaMultiplier = Mathf.Lerp(lunaMultiplier, multiplicadorLunaOscuridadTotal, profundidadOscura);
            reflejosMultiplier = Mathf.Lerp(reflejosMultiplier, multiplicadorReflejosOscuridadTotal, profundidadOscura);
            mezclaAmbiente = Mathf.Lerp(mezclaAmbiente, 1f, profundidadOscura);
            colorAmbienteObjetivo = Color.Lerp(colorAmbienteCueva, colorAmbienteOscuridadTotal, profundidadOscura);

            if (forzarAmbientePlanoEnCueva && profundidadOscura > 0.01f)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
            }
        }

        if (sunLight != null)
        {
            sunLight.intensity *= solMultiplier;
        }

        if (moonLight != null)
        {
            moonLight.intensity *= lunaMultiplier;
        }

        AplicarLucesExterioresExtra(profundidadOscura);

        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, colorAmbienteObjetivo, mezclaAmbiente);
        RenderSettings.reflectionIntensity *= reflejosMultiplier;

        if (luzEntradaCueva != null)
        {
            float entradaMultiplier = Mathf.Lerp(1f, multiplicadorLuzEntradaEnFondo, profundidad);
            luzEntradaCueva.intensity *= entradaMultiplier;
        }

        ajustesAplicados = true;
    }

    private float CalcularFactorOscuridadReal(float profundidad)
    {
        if (!usarOscuridadInteriorReal)
        {
            return 0f;
        }

        float inicio = Mathf.Clamp01(profundidadInicioOscuridadTotal);
        float fin = Mathf.Clamp01(Mathf.Max(inicio + 0.001f, profundidadOscuridadTotal));
        float t = Mathf.InverseLerp(inicio, fin, Mathf.Clamp01(profundidad));

        if (curvaOscuridadInterior != null)
        {
            t = Mathf.Clamp01(curvaOscuridadInterior.Evaluate(t));
        }

        return t;
    }

    private void AplicarLucesExterioresExtra(float profundidadOscura)
    {
        if (lucesExterioresExtra == null || lucesExterioresExtra.Length == 0 || profundidadOscura <= 0.001f)
        {
            return;
        }

        float multiplier = Mathf.Lerp(1f, 0f, profundidadOscura);

        foreach (Light luz in lucesExterioresExtra)
        {
            if (luz == null || luz == sunLight || luz == moonLight || luz == luzEntradaCueva)
            {
                continue;
            }

            luz.intensity *= multiplier;
        }
    }

    private void GuardarValoresAntesDeAplicar()
    {
        sunIntensityAntes = sunLight != null ? sunLight.intensity : 0f;
        moonIntensityAntes = moonLight != null ? moonLight.intensity : 0f;
        reflectionIntensityAntes = RenderSettings.reflectionIntensity;
        ambientColorAntes = RenderSettings.ambientLight;
        ambientModeAntes = RenderSettings.ambientMode;
        luzEntradaIntensityAntes = luzEntradaCueva != null ? luzEntradaCueva.intensity : 0f;

        if (lucesExterioresExtra != null && lucesExterioresExtra.Length > 0)
        {
            if (lucesExtraIntensityAntes == null || lucesExtraIntensityAntes.Length != lucesExterioresExtra.Length)
            {
                lucesExtraIntensityAntes = new float[lucesExterioresExtra.Length];
            }

            for (int i = 0; i < lucesExterioresExtra.Length; i++)
            {
                lucesExtraIntensityAntes[i] = lucesExterioresExtra[i] != null ? lucesExterioresExtra[i].intensity : 0f;
            }
        }
    }

    private void RestaurarAjustesAplicados()
    {
        if (!ajustesAplicados)
        {
            return;
        }

        if (sunLight != null)
        {
            sunLight.intensity = sunIntensityAntes;
        }

        if (moonLight != null)
        {
            moonLight.intensity = moonIntensityAntes;
        }

        RenderSettings.reflectionIntensity = reflectionIntensityAntes;
        RenderSettings.ambientLight = ambientColorAntes;
        RenderSettings.ambientMode = ambientModeAntes;

        if (luzEntradaCueva != null)
        {
            luzEntradaCueva.intensity = luzEntradaIntensityAntes;
        }

        if (lucesExterioresExtra != null && lucesExtraIntensityAntes != null)
        {
            int count = Mathf.Min(lucesExterioresExtra.Length, lucesExtraIntensityAntes.Length);

            for (int i = 0; i < count; i++)
            {
                if (lucesExterioresExtra[i] != null)
                {
                    lucesExterioresExtra[i].intensity = lucesExtraIntensityAntes[i];
                }
            }
        }

        ajustesAplicados = false;
    }

    private void ActualizarDebug()
    {
        jugadorDentroDebug = jugadorDentro;
        profundidadActualDebug = profundidadActual;
    }

    [ContextMenu("Debug/Simular entrada cueva")]
    private void DebugSimularEntradaCueva()
    {
        jugadorDentro = true;
        profundidadObjetivo = 1f;
    }

    [ContextMenu("Debug/Simular salida cueva")]
    private void DebugSimularSalidaCueva()
    {
        jugadorDentro = false;
        profundidadObjetivo = 0f;

        if (rainController != null)
        {
            rainController.ClearInteriorRainOcclusion();
        }
    }
}
