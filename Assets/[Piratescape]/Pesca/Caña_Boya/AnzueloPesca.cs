using System.Collections;
using UnityEngine;

public sealed class AnzueloPesca : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform visualAnzuelo;
    [SerializeField] private float amplitudFlotacion = 0.08f;
    [SerializeField] private float velocidadFlotacion = 2.2f;
    [SerializeField] private float suavizadoPosicion = 12f;

    [Header("Hundimiento al picar")]
    [SerializeField] private float profundidadHundimiento = 0.45f;
    [SerializeField] private float duracionHundimiento = 0.18f;
    [SerializeField] private AnimationCurve curvaHundimiento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Particulas")]
    [SerializeField] private ParticleSystem particulasCaidaAgua;
    [SerializeField] private ParticleSystem particulasFlotando;
    [SerializeField] private ParticleSystem particulasPicada;

    [Header("Particulas por codigo")]
    [SerializeField] private bool crearParticulasAutomaticas = true;
    [SerializeField] private bool configurarParticulasPorCodigo = true;
    [SerializeField] private Material materialParticulasAgua;
    [SerializeField] private Vector3 offsetParticulasAgua = Vector3.zero;
    [SerializeField] private bool usarSimulationSpaceWorld = true;

    [Header("Ajuste splash caida")]
    [SerializeField] private int particulasCaidaMin = 18;
    [SerializeField] private int particulasCaidaMax = 25;
    [SerializeField] private float tamanoCaidaMin = 0.05f;
    [SerializeField] private float tamanoCaidaMax = 0.12f;
    [SerializeField] private float velocidadVerticalCaidaMin = 0.6f;
    [SerializeField] private float velocidadVerticalCaidaMax = 1.3f;
    [SerializeField] private float expansionHorizontalCaida = 0.25f;

    [Header("Ajuste burbujas flotando")]
    [SerializeField] private float burbujasPorSegundo = 5f;
    [SerializeField] private float radioBurbujas = 0.18f;
    [SerializeField] private float tamanoBurbujasMin = 0.035f;
    [SerializeField] private float tamanoBurbujasMax = 0.08f;

    [Header("Ajuste splash picada")]
    [SerializeField] private int particulasPicadaMin = 38;
    [SerializeField] private int particulasPicadaMax = 52;
    [SerializeField] private float tamanoPicadaMin = 0.07f;
    [SerializeField] private float tamanoPicadaMax = 0.18f;
    [SerializeField] private float velocidadVerticalPicadaMin = 0.9f;
    [SerializeField] private float velocidadVerticalPicadaMax = 1.8f;
    [SerializeField] private float expansionHorizontalPicada = 0.55f;

    [Header("Linea de cana opcional")]
    [SerializeField] private LineRenderer lineaCana;
    [SerializeField] private Transform puntoSalidaCana;
    [SerializeField] private bool actualizarLinea = true;

    private Vector3 posicionBaseAgua;
    private bool estaFlotando;
    private bool estaHundido;
    private Coroutine rutinaHundimiento;
    private Material materialRuntimeAgua;

    public bool EstaHundido => estaHundido;

    private void Awake()
    {
        if (visualAnzuelo == null)
        {
            visualAnzuelo = transform;
        }

        if (lineaCana == null)
        {
            lineaCana = GetComponent<LineRenderer>();
        }

        PrepararParticulas();
    }

    private void Update()
    {
        ActualizarFlotacion();
        ActualizarLineaCana();
    }

    public void PrepararParaLanzamiento(Vector3 posicionInicial, Transform nuevoPuntoSalidaCana)
    {
        puntoSalidaCana = nuevoPuntoSalidaCana;
        transform.position = posicionInicial;
        posicionBaseAgua = posicionInicial;
        estaFlotando = false;
        estaHundido = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        DetenerParticulas(true);
        PrepararLineaCana();
    }

    public void Inicializar(Vector3 posicionAgua, Transform nuevoPuntoSalidaCana)
    {
        posicionBaseAgua = posicionAgua;
        puntoSalidaCana = nuevoPuntoSalidaCana;
        transform.position = posicionBaseAgua;
        estaFlotando = true;
        estaHundido = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        ReproducirParticulaUnaVez(particulasCaidaAgua);

        if (particulasFlotando != null)
        {
            particulasFlotando.Clear(true);
            particulasFlotando.Play(true);
        }

        if (particulasPicada != null)
        {
            particulasPicada.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        PrepararLineaCana();
    }

    public void ActivarPicada()
    {
        if (estaHundido)
        {
            return;
        }

        ReproducirParticulaUnaVez(particulasPicada);

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
        }

        rutinaHundimiento = StartCoroutine(HundirRoutine());
    }

    public void DetenerVisuales()
    {
        DetenerVisuales(false);
    }

    public void DetenerVisuales(bool mantenerLineaCana)
    {
        estaFlotando = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        DetenerParticulas(false);

        if (!mantenerLineaCana && lineaCana != null)
        {
            lineaCana.enabled = false;
        }
    }

    public void ActivarLineaCana(bool activa)
    {
        if (lineaCana == null)
        {
            return;
        }

        lineaCana.enabled = activa && puntoSalidaCana != null;
        ActualizarLineaCana();
    }

    private void ActualizarFlotacion()
    {
        if (!estaFlotando || estaHundido)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        Vector3 posicionObjetivo = posicionBaseAgua + Vector3.up * offsetY;

        transform.position = Vector3.Lerp(
            transform.position,
            posicionObjetivo,
            Time.deltaTime * suavizadoPosicion
        );
    }

    private IEnumerator HundirRoutine()
    {
        estaHundido = true;
        estaFlotando = false;

        if (particulasFlotando != null)
        {
            particulasFlotando.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Vector3 inicio = transform.position;
        Vector3 destino = posicionBaseAgua + Vector3.down * profundidadHundimiento;
        float tiempo = 0f;

        while (tiempo < duracionHundimiento)
        {
            tiempo += Time.deltaTime;
            float t = duracionHundimiento > 0f ? tiempo / duracionHundimiento : 1f;
            float evaluado = curvaHundimiento != null ? curvaHundimiento.Evaluate(t) : t;

            transform.position = Vector3.Lerp(inicio, destino, evaluado);
            yield return null;
        }

        transform.position = destino;
        rutinaHundimiento = null;
    }

    private void PrepararLineaCana()
    {
        if (lineaCana == null)
        {
            return;
        }

        lineaCana.positionCount = 2;
        lineaCana.enabled = puntoSalidaCana != null;
        ActualizarLineaCana();
    }

    private void ActualizarLineaCana()
    {
        if (!actualizarLinea || lineaCana == null || !lineaCana.enabled || puntoSalidaCana == null)
        {
            return;
        }

        lineaCana.SetPosition(0, puntoSalidaCana.position);
        lineaCana.SetPosition(1, transform.position);
    }

    private void PrepararParticulas()
    {
        if (!crearParticulasAutomaticas && !configurarParticulasPorCodigo)
        {
            return;
        }

        if (crearParticulasAutomaticas)
        {
            if (particulasCaidaAgua == null)
            {
                particulasCaidaAgua = CrearParticleSystemHijo("FX_Splash_Caida_Codigo");
            }

            if (particulasFlotando == null)
            {
                particulasFlotando = CrearParticleSystemHijo("FX_Boya_Flotando_Codigo");
            }

            if (particulasPicada == null)
            {
                particulasPicada = CrearParticleSystemHijo("FX_Splash_Picada_Codigo");
            }
        }

        if (configurarParticulasPorCodigo)
        {
            ConfigurarSplashCaida(particulasCaidaAgua);
            ConfigurarBurbujasFlotando(particulasFlotando);
            ConfigurarSplashPicada(particulasPicada);
        }

        DetenerParticulas(true);
    }

    private ParticleSystem CrearParticleSystemHijo(string nombre)
    {
        GameObject hijo = new GameObject(nombre);
        hijo.transform.SetParent(transform, false);
        hijo.transform.localPosition = offsetParticulasAgua;
        hijo.transform.localRotation = Quaternion.identity;
        hijo.transform.localScale = Vector3.one;

        ParticleSystem ps = hijo.AddComponent<ParticleSystem>();
        return ps;
    }

    private void ConfigurarSplashCaida(ParticleSystem ps)
    {
        if (ps == null)
        {
            return;
        }

        var main = ps.main;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoCaidaMin, tamanoCaidaMax);
        main.gravityModifier = 0f;
        main.simulationSpace = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 35;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(particulasCaidaMin, particulasCaidaMax))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.12f;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-expansionHorizontalCaida, expansionHorizontalCaida);
        velocity.y = new ParticleSystem.MinMaxCurve(velocidadVerticalCaidaMin, velocidadVerticalCaidaMax);
        velocity.z = new ParticleSystem.MinMaxCurve(-expansionHorizontalCaida, expansionHorizontalCaida);

        ConfigurarTamanoDesaparece(ps, true);
        ConfigurarColorAgua(ps, 0.82f, 0.02f);
        ConfigurarRenderer(ps);
    }

    private void ConfigurarBurbujasFlotando(ParticleSystem ps)
    {
        if (ps == null)
        {
            return;
        }

        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.15f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoBurbujasMin, tamanoBurbujasMax);
        main.gravityModifier = 0f;
        main.simulationSpace = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 24;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = burbujasPorSegundo;
        emission.SetBursts(new ParticleSystem.Burst[0]);

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radioBurbujas;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);

        ConfigurarTamanoBurbujas(ps);
        ConfigurarColorAgua(ps, 0.55f, 0f);
        ConfigurarRenderer(ps);
    }

    private void ConfigurarSplashPicada(ParticleSystem ps)
    {
        if (ps == null)
        {
            return;
        }

        var main = ps.main;
        main.duration = 0.45f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.62f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoPicadaMin, tamanoPicadaMax);
        main.gravityModifier = 0.02f;
        main.simulationSpace = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 70;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(particulasPicadaMin, particulasPicadaMax))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.18f;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-expansionHorizontalPicada, expansionHorizontalPicada);
        velocity.y = new ParticleSystem.MinMaxCurve(velocidadVerticalPicadaMin, velocidadVerticalPicadaMax);
        velocity.z = new ParticleSystem.MinMaxCurve(-expansionHorizontalPicada, expansionHorizontalPicada);

        ConfigurarTamanoDesaparece(ps, false);
        ConfigurarColorAgua(ps, 0.95f, 0.02f);
        ConfigurarRenderer(ps);
    }

    private void ConfigurarTamanoDesaparece(ParticleSystem ps, bool caidaPequena)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;

        AnimationCurve curva = new AnimationCurve();
        curva.AddKey(0f, caidaPequena ? 0.85f : 1f);
        curva.AddKey(0.35f, 1f);
        curva.AddKey(1f, 0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, curva);
    }

    private void ConfigurarTamanoBurbujas(ParticleSystem ps)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;

        AnimationCurve curva = new AnimationCurve();
        curva.AddKey(0f, 0f);
        curva.AddKey(0.35f, 1f);
        curva.AddKey(1f, 0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, curva);
    }

    private void ConfigurarColorAgua(ParticleSystem ps, float alphaMedio, float alphaFinal)
    {
        var color = ps.colorOverLifetime;
        color.enabled = true;

        Gradient gradiente = new Gradient();
        Color colorAgua = new Color(0.82f, 0.95f, 1f);

        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorAgua, 0f),
                new GradientColorKey(Color.white, 0.45f),
                new GradientColorKey(colorAgua, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(alphaMedio, 0.2f),
                new GradientAlphaKey(alphaFinal, 1f)
            }
        );

        color.color = gradiente;
    }

    private void ConfigurarRenderer(ParticleSystem ps)
    {
        ParticleSystemRenderer rendererParticulas = ps.GetComponent<ParticleSystemRenderer>();

        if (rendererParticulas == null)
        {
            rendererParticulas = ps.gameObject.AddComponent<ParticleSystemRenderer>();
        }

        rendererParticulas.renderMode = ParticleSystemRenderMode.Billboard;
        rendererParticulas.sortingFudge = 1f;
        rendererParticulas.material = ObtenerMaterialParticulasAgua();
    }

    private Material ObtenerMaterialParticulasAgua()
    {
        if (materialParticulasAgua != null)
        {
            return materialParticulasAgua;
        }

        if (materialRuntimeAgua != null)
        {
            return materialRuntimeAgua;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        materialRuntimeAgua = new Material(shader);
        materialRuntimeAgua.name = "M_Pesca_Agua_Particulas_Runtime";
        materialRuntimeAgua.color = new Color(0.82f, 0.95f, 1f, 0.85f);

        return materialRuntimeAgua;
    }

    private void ReproducirParticulaUnaVez(ParticleSystem ps)
    {
        if (ps == null)
        {
            return;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play(true);
    }

    private void DetenerParticulas(bool limpiar)
    {
        ParticleSystemStopBehavior comportamiento = limpiar
            ? ParticleSystemStopBehavior.StopEmittingAndClear
            : ParticleSystemStopBehavior.StopEmitting;

        if (particulasCaidaAgua != null)
        {
            particulasCaidaAgua.Stop(true, comportamiento);
        }

        if (particulasFlotando != null)
        {
            particulasFlotando.Stop(true, comportamiento);
        }

        if (particulasPicada != null)
        {
            particulasPicada.Stop(true, comportamiento);
        }
    }
}
