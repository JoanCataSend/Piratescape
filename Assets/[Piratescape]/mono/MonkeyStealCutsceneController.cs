using System.Collections;
using UnityEngine;

public class MonkeyStealCutsceneController : MonoBehaviour
{
    [System.Serializable]
    private class MonoRobo
    {
        public Transform mono;
        public Transform puntoInicial;
        public Animator animator;
    }

    [Header("Monos")]
    [SerializeField] private MonoRobo[] monos;

    [Header("Puntos de la tienda")]
    [SerializeField] private Transform puntoEntradaTienda;
    [SerializeField] private Transform puntoInteriorTienda;

    [Header("Cámaras")]
    [SerializeField] private GameObject camaraJugador;
    [SerializeField] private GameObject camaraCinematica;

    [Header("Tiempos")]
    [SerializeField] private float esperaDentroTienda = 1f;
    [SerializeField] private float esperaFinal = 0.5f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadIrTienda = 2f;
    [SerializeField] private float velocidadSalirCorriendo = 5f;
    [SerializeField] private float velocidadRotacion = 10f;
    [SerializeField] private float distanciaLlegada = 0.08f;

    [Header("Animaciones")]
    [SerializeField] private string estadoWalk = "Walk";
    [SerializeField] private string estadoRun = "Run";

    [Header("Suelo")]
    [SerializeField] private bool ajustarAlturaAlSuelo = true;
    [SerializeField] private LayerMask capaSuelo = ~0;
    [SerializeField] private float alturaRaycast = 5f;
    [SerializeField] private float distanciaRaycast = 20f;
    [SerializeField] private float alturaSobreSuelo = 0.02f;

    private void Awake()
    {
        PrepararEstadoInicial();
    }

    private void PrepararEstadoInicial()
    {
        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] != null && monos[i].mono != null)
            {
                monos[i].mono.gameObject.SetActive(false);
            }
        }
    }

    public void PrepararCinematica()
    {
        PrepararMonos();

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(false);
        }

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(true);
        }
    }

    public IEnumerator ReproducirMovimientoMonos()
    {
        ReproducirAnimacionTodos(estadoWalk);

        yield return MoverMonosA(puntoEntradaTienda, velocidadIrTienda);

        OcultarMonos();

        yield return new WaitForSeconds(esperaDentroTienda);

        ColocarMonosEnEntradaTienda();
        MostrarMonos();

        ReproducirAnimacionTodos(estadoRun);

        yield return MoverMonosAPuntosIniciales(velocidadSalirCorriendo);

        yield return new WaitForSeconds(esperaFinal);
    }

    public void FinalizarCinematica()
    {
        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

        if (monos != null)
        {
            for (int i = 0; i < monos.Length; i++)
            {
                if (monos[i] != null && monos[i].mono != null)
                {
                    monos[i].mono.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PrepararMonos()
    {
        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            MonoRobo monoRobo = monos[i];

            if (monoRobo == null || monoRobo.mono == null || monoRobo.puntoInicial == null)
            {
                continue;
            }

            Vector3 posicionInicial = AjustarPosicionAlSuelo(
                monoRobo.puntoInicial.position,
                monoRobo.mono
            );

            monoRobo.mono.position = posicionInicial;
            monoRobo.mono.rotation = monoRobo.puntoInicial.rotation;
            monoRobo.mono.gameObject.SetActive(true);

            if (monoRobo.animator == null)
            {
                monoRobo.animator = monoRobo.mono.GetComponent<Animator>();

                if (monoRobo.animator == null)
                {
                    monoRobo.animator = monoRobo.mono.GetComponentInChildren<Animator>();
                }
            }

            if (monoRobo.animator != null)
            {
                monoRobo.animator.enabled = true;
                monoRobo.animator.applyRootMotion = false;
                monoRobo.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                monoRobo.animator.Rebind();
                monoRobo.animator.Update(0f);
            }
        }
    }

    private void ReproducirAnimacionTodos(string nombreEstado)
    {
        if (monos == null || string.IsNullOrWhiteSpace(nombreEstado))
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] == null)
            {
                continue;
            }

            if (monos[i].animator == null && monos[i].mono != null)
            {
                monos[i].animator = monos[i].mono.GetComponent<Animator>();

                if (monos[i].animator == null)
                {
                    monos[i].animator = monos[i].mono.GetComponentInChildren<Animator>();
                }
            }

            if (monos[i].animator == null)
            {
                Debug.LogWarning("Mono sin Animator asignado en MonkeyStealCutsceneController", this);
                continue;
            }

            monos[i].animator.enabled = true;
            monos[i].animator.applyRootMotion = false;
            monos[i].animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            monos[i].animator.Play(nombreEstado, 0, 0f);
            monos[i].animator.Update(0f);
        }
    }

    private void OcultarMonos()
    {
        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] != null && monos[i].mono != null)
            {
                monos[i].mono.gameObject.SetActive(false);
            }
        }
    }

    private void MostrarMonos()
    {
        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] != null && monos[i].mono != null)
            {
                monos[i].mono.gameObject.SetActive(true);

                if (monos[i].animator == null)
                {
                    monos[i].animator = monos[i].mono.GetComponent<Animator>();

                    if (monos[i].animator == null)
                    {
                        monos[i].animator = monos[i].mono.GetComponentInChildren<Animator>();
                    }
                }

                if (monos[i].animator != null)
                {
                    monos[i].animator.enabled = true;
                    monos[i].animator.applyRootMotion = false;
                    monos[i].animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    monos[i].animator.Update(0f);
                }
            }
        }
    }

    private void ColocarMonosEnEntradaTienda()
    {
        if (monos == null || puntoEntradaTienda == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] == null || monos[i].mono == null)
            {
                continue;
            }

            Vector3 posicion = puntoEntradaTienda.position + ObtenerOffsetMono(i);
            posicion = AjustarPosicionAlSuelo(posicion, monos[i].mono);

            monos[i].mono.position = posicion;
            monos[i].mono.rotation = puntoEntradaTienda.rotation;
        }
    }

    private IEnumerator MoverMonosA(Transform destino, float velocidad)
    {
        if (destino == null || monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null || monoRobo.mono == null)
                {
                    continue;
                }

                Vector3 destinoConOffset = destino.position + ObtenerOffsetMono(i);
                bool llego = MoverMonoHacia(monoRobo.mono, destinoConOffset, velocidad);

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private IEnumerator MoverMonosAPuntosIniciales(float velocidad)
    {
        if (monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null || monoRobo.mono == null || monoRobo.puntoInicial == null)
                {
                    continue;
                }

                bool llego = MoverMonoHacia(monoRobo.mono, monoRobo.puntoInicial.position, velocidad);

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private bool MoverMonoHacia(Transform mono, Vector3 destino, float velocidad)
    {
        Vector3 posicionActual = mono.position;

        Vector3 destinoPlano = new Vector3(
            destino.x,
            posicionActual.y,
            destino.z
        );

        Vector3 direccion = destinoPlano - posicionActual;
        direccion.y = 0f;

        if (direccion.magnitude <= distanciaLlegada)
        {
            Vector3 posicionFinal = new Vector3(destino.x, posicionActual.y, destino.z);
            posicionFinal = AjustarPosicionAlSuelo(posicionFinal, mono);

            mono.position = posicionFinal;
            return true;
        }

        Vector3 direccionNormalizada = direccion.normalized;

        Vector3 nuevaPosicion = posicionActual + direccionNormalizada * velocidad * Time.deltaTime;
        nuevaPosicion = AjustarPosicionAlSuelo(nuevaPosicion, mono);

        mono.position = nuevaPosicion;

        if (direccionNormalizada.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionNormalizada);

            mono.rotation = Quaternion.Slerp(
                mono.rotation,
                rotacionObjetivo,
                Time.deltaTime * velocidadRotacion
            );
        }

        return false;
    }

    private Vector3 AjustarPosicionAlSuelo(Vector3 posicion, Transform mono)
    {
        if (!ajustarAlturaAlSuelo)
        {
            return posicion;
        }

        Vector3 origen = posicion + Vector3.up * alturaRaycast;

        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            Vector3.down,
            distanciaRaycast,
            capaSuelo,
            QueryTriggerInteraction.Ignore
        );

        if (impactos == null || impactos.Length == 0)
        {
            return posicion;
        }

        RaycastHit mejorImpacto = new RaycastHit();
        bool hayImpactoValido = false;
        float menorDistancia = float.MaxValue;

        for (int i = 0; i < impactos.Length; i++)
        {
            RaycastHit impacto = impactos[i];

            if (mono != null && impacto.transform.IsChildOf(mono))
            {
                continue;
            }

            if (impacto.distance < menorDistancia)
            {
                menorDistancia = impacto.distance;
                mejorImpacto = impacto;
                hayImpactoValido = true;
            }
        }

        if (!hayImpactoValido)
        {
            return posicion;
        }

        posicion.y = mejorImpacto.point.y + alturaSobreSuelo;
        return posicion;
    }

    private Vector3 ObtenerOffsetMono(int indice)
    {
        float separacion = 0.45f;

        if (indice == 0)
        {
            return Vector3.zero;
        }

        if (indice % 2 == 0)
        {
            return Vector3.right * separacion * indice;
        }

        return Vector3.left * separacion * indice;
    }
}