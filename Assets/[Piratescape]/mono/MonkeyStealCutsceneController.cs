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

    [Header("Animación de la tienda")]
    [SerializeField] private GameObject tiendaNormal;

    [Tooltip(
        "Partes visuales de la tienda normal. " +
        "Puedes dejarlo vacío y se buscarán automáticamente."
    )]
    [SerializeField] private Renderer[] renderersTiendaNormal;

    [SerializeField] private GameObject tiendaAnimada;
    [SerializeField] private Animator animatorTiendaAnimada;
    [SerializeField] private string estadoAnimacionTienda = "Take 001";
    [SerializeField] private float duracionAnimacionTienda = 3.35f;
    [SerializeField] private float esperaTrasAnimacionTienda = 0.2f;

    [Header("Cámaras")]
    [SerializeField] private GameObject camaraJugador;
    [SerializeField] private GameObject camaraCinematica;

    [Header("Tiempos")]
    [SerializeField] private float esperaDentroTienda = 1.2f;
    [SerializeField] private float esperaFinal = 0.5f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadIrTienda = 4f;
    [SerializeField] private float velocidadSalirCorriendo = 10f;
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

    private Coroutine rutinaMovimiento;
    private bool cinematicaActiva;

    private void Awake()
    {
        PrepararEstadoInicial();
    }

    private void PrepararEstadoInicial()
    {
        cinematicaActiva = false;
        rutinaMovimiento = null;

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

        // La tienda normal permanece activa.
        // Solo se muestran u ocultan sus partes visuales.
        MostrarTiendaNormal(true);

        if (tiendaAnimada != null)
        {
            tiendaAnimada.SetActive(false);
        }

        BuscarAnimatorTienda();
        OcultarMonos();
    }

    public void PrepararCinematica()
    {
        DetenerRutinaMovimiento();

        cinematicaActiva = true;

        PrepararMonos();

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(false);
        }

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "MonkeyStealCutsceneController: falta asignar Cámara Cinemática.",
                this
            );
        }
    }

    public void IniciarMovimientoMonos()
    {
        DetenerRutinaMovimiento();

        if (!cinematicaActiva)
        {
            PrepararCinematica();
        }

        rutinaMovimiento = StartCoroutine(
            ReproducirMovimientoMonos()
        );
    }

    public IEnumerator ReproducirMovimientoMonos()
    {
        ReproducirAnimacionTodos(estadoWalk);

        yield return MoverMonosA(
            puntoEntradaTienda,
            velocidadIrTienda
        );

        if (!cinematicaActiva)
        {
            yield break;
        }

        OcultarMonos();

        if (esperaDentroTienda > 0f)
        {
            yield return new WaitForSecondsRealtime(
                esperaDentroTienda
            );
        }

        if (!cinematicaActiva)
        {
            yield break;
        }

        yield return ReproducirAnimacionTienda();

        if (esperaTrasAnimacionTienda > 0f)
        {
            yield return new WaitForSecondsRealtime(
                esperaTrasAnimacionTienda
            );
        }

        if (!cinematicaActiva)
        {
            yield break;
        }

        ColocarMonosEnEntradaTienda();
        MostrarMonos();

        ReproducirAnimacionTodos(estadoRun);

        yield return MoverMonosAPuntosIniciales(
            velocidadSalirCorriendo
        );

        if (esperaFinal > 0f)
        {
            yield return new WaitForSecondsRealtime(
                esperaFinal
            );
        }

        rutinaMovimiento = null;
    }

    public void FinalizarCinematica()
    {
        cinematicaActiva = false;

        DetenerRutinaMovimiento();
        OcultarMonos();

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "MonkeyStealCutsceneController: falta asignar Cámara Jugador.",
                this
            );
        }
    }

    private void DetenerRutinaMovimiento()
    {
        if (rutinaMovimiento == null)
        {
            return;
        }

        StopCoroutine(rutinaMovimiento);
        rutinaMovimiento = null;
    }

    private IEnumerator ReproducirAnimacionTienda()
    {
        /*
         * No usamos tiendaNormal.SetActive(false).
         *
         * Si ShelterSleep está en la tienda normal o en uno de sus
         * padres/hijos, desactivar la tienda detendría su coroutine.
         * Solo ocultamos los Renderer.
         */
        MostrarTiendaNormal(false);

        if (tiendaAnimada == null)
        {
            Debug.LogWarning(
                "MonkeyStealCutsceneController: falta asignar Tienda Animada.",
                this
            );

            yield break;
        }

        tiendaAnimada.SetActive(true);
        BuscarAnimatorTienda();

        if (animatorTiendaAnimada == null)
        {
            Debug.LogWarning(
                "MonkeyStealCutsceneController: no se encontró el Animator de la tienda.",
                this
            );

            yield break;
        }

        if (string.IsNullOrWhiteSpace(estadoAnimacionTienda))
        {
            Debug.LogWarning(
                "MonkeyStealCutsceneController: falta el nombre del estado de la tienda.",
                this
            );

            yield break;
        }

        animatorTiendaAnimada.enabled = true;
        animatorTiendaAnimada.applyRootMotion = false;
        animatorTiendaAnimada.cullingMode =
            AnimatorCullingMode.AlwaysAnimate;

        animatorTiendaAnimada.Rebind();
        animatorTiendaAnimada.Update(0f);

        animatorTiendaAnimada.Play(
            estadoAnimacionTienda,
            0,
            0f
        );

        animatorTiendaAnimada.Update(0f);

        if (duracionAnimacionTienda > 0f)
        {
            yield return new WaitForSecondsRealtime(
                duracionAnimacionTienda
            );
        }
    }

    private void MostrarTiendaNormal(bool mostrar)
    {
        /*
         * Si no has rellenado la lista manualmente,
         * el script busca automáticamente todos los Renderer
         * que haya dentro de la tienda normal.
         */
        if ((renderersTiendaNormal == null ||
             renderersTiendaNormal.Length == 0) &&
            tiendaNormal != null)
        {
            renderersTiendaNormal =
                tiendaNormal.GetComponentsInChildren<Renderer>(
                    true
                );
        }

        if (renderersTiendaNormal == null)
        {
            return;
        }

        for (int i = 0;
             i < renderersTiendaNormal.Length;
             i++)
        {
            Renderer rendererTienda =
                renderersTiendaNormal[i];

            if (rendererTienda != null)
            {
                rendererTienda.enabled = mostrar;
            }
        }
    }

    private void BuscarAnimatorTienda()
    {
        if (animatorTiendaAnimada != null ||
            tiendaAnimada == null)
        {
            return;
        }

        animatorTiendaAnimada =
            tiendaAnimada.GetComponent<Animator>();

        if (animatorTiendaAnimada == null)
        {
            animatorTiendaAnimada =
                tiendaAnimada.GetComponentInChildren<Animator>(
                    true
                );
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

            if (monoRobo == null ||
                monoRobo.mono == null ||
                monoRobo.puntoInicial == null)
            {
                continue;
            }

            monoRobo.mono.position =
                AjustarPosicionAlSuelo(
                    monoRobo.puntoInicial.position,
                    monoRobo.mono
                );

            monoRobo.mono.rotation =
                monoRobo.puntoInicial.rotation;

            monoRobo.mono.gameObject.SetActive(true);

            BuscarAnimatorMono(monoRobo);

            if (monoRobo.animator != null)
            {
                monoRobo.animator.enabled = true;
                monoRobo.animator.applyRootMotion = false;
                monoRobo.animator.cullingMode =
                    AnimatorCullingMode.AlwaysAnimate;

                monoRobo.animator.Rebind();
                monoRobo.animator.Update(0f);
            }
        }
    }

    private void BuscarAnimatorMono(MonoRobo monoRobo)
    {
        if (monoRobo == null ||
            monoRobo.animator != null ||
            monoRobo.mono == null)
        {
            return;
        }

        monoRobo.animator =
            monoRobo.mono.GetComponent<Animator>();

        if (monoRobo.animator == null)
        {
            monoRobo.animator =
                monoRobo.mono.GetComponentInChildren<Animator>(
                    true
                );
        }
    }

    private void ReproducirAnimacionTodos(
        string nombreEstado
    )
    {
        if (monos == null ||
            string.IsNullOrWhiteSpace(nombreEstado))
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            MonoRobo monoRobo = monos[i];

            if (monoRobo == null)
            {
                continue;
            }

            BuscarAnimatorMono(monoRobo);

            if (monoRobo.animator == null)
            {
                continue;
            }

            monoRobo.animator.enabled = true;
            monoRobo.animator.applyRootMotion = false;
            monoRobo.animator.cullingMode =
                AnimatorCullingMode.AlwaysAnimate;

            monoRobo.animator.Play(
                nombreEstado,
                0,
                0f
            );

            monoRobo.animator.Update(0f);
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
            if (monos[i] != null &&
                monos[i].mono != null)
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
            MonoRobo monoRobo = monos[i];

            if (monoRobo == null ||
                monoRobo.mono == null)
            {
                continue;
            }

            monoRobo.mono.gameObject.SetActive(true);

            BuscarAnimatorMono(monoRobo);

            if (monoRobo.animator != null)
            {
                monoRobo.animator.enabled = true;
                monoRobo.animator.applyRootMotion = false;
                monoRobo.animator.cullingMode =
                    AnimatorCullingMode.AlwaysAnimate;

                monoRobo.animator.Update(0f);
            }
        }
    }

    private void ColocarMonosEnEntradaTienda()
    {
        if (monos == null ||
            puntoEntradaTienda == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            MonoRobo monoRobo = monos[i];

            if (monoRobo == null ||
                monoRobo.mono == null)
            {
                continue;
            }

            Vector3 posicion =
                puntoEntradaTienda.position +
                ObtenerOffsetMono(i);

            monoRobo.mono.position =
                AjustarPosicionAlSuelo(
                    posicion,
                    monoRobo.mono
                );

            monoRobo.mono.rotation =
                puntoEntradaTienda.rotation;
        }
    }

    private IEnumerator MoverMonosA(
        Transform destino,
        float velocidad
    )
    {
        if (destino == null || monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron && cinematicaActiva)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null ||
                    monoRobo.mono == null)
                {
                    continue;
                }

                bool llego = MoverMonoHacia(
                    monoRobo.mono,
                    destino.position +
                    ObtenerOffsetMono(i),
                    velocidad
                );

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private IEnumerator MoverMonosAPuntosIniciales(
        float velocidad
    )
    {
        if (monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron && cinematicaActiva)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null ||
                    monoRobo.mono == null ||
                    monoRobo.puntoInicial == null)
                {
                    continue;
                }

                bool llego = MoverMonoHacia(
                    monoRobo.mono,
                    monoRobo.puntoInicial.position,
                    velocidad
                );

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private bool MoverMonoHacia(
        Transform mono,
        Vector3 destino,
        float velocidad
    )
    {
        if (mono == null)
        {
            return true;
        }

        Vector3 posicionActual = mono.position;

        Vector3 destinoPlano = new Vector3(
            destino.x,
            posicionActual.y,
            destino.z
        );

        Vector3 direccion =
            destinoPlano - posicionActual;

        direccion.y = 0f;

        if (direccion.magnitude <= distanciaLlegada)
        {
            Vector3 posicionFinal = new Vector3(
                destino.x,
                posicionActual.y,
                destino.z
            );

            mono.position =
                AjustarPosicionAlSuelo(
                    posicionFinal,
                    mono
                );

            return true;
        }

        Vector3 direccionNormalizada =
            direccion.normalized;

        Vector3 nuevaPosicion =
            posicionActual +
            direccionNormalizada *
            velocidad *
            Time.deltaTime;

        mono.position =
            AjustarPosicionAlSuelo(
                nuevaPosicion,
                mono
            );

        if (direccionNormalizada.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(
                    direccionNormalizada
                );

            mono.rotation = Quaternion.Slerp(
                mono.rotation,
                rotacionObjetivo,
                Time.deltaTime * velocidadRotacion
            );
        }

        return false;
    }

    private Vector3 AjustarPosicionAlSuelo(
        Vector3 posicion,
        Transform mono
    )
    {
        if (!ajustarAlturaAlSuelo)
        {
            return posicion;
        }

        Vector3 origen =
            posicion + Vector3.up * alturaRaycast;

        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            Vector3.down,
            distanciaRaycast,
            capaSuelo,
            QueryTriggerInteraction.Ignore
        );

        if (impactos == null ||
            impactos.Length == 0)
        {
            return posicion;
        }

        bool encontrado = false;
        float menorDistancia = float.MaxValue;
        RaycastHit mejorImpacto = new RaycastHit();

        for (int i = 0; i < impactos.Length; i++)
        {
            RaycastHit impacto = impactos[i];

            if (mono != null &&
                impacto.transform.IsChildOf(mono))
            {
                continue;
            }

            if (impacto.distance >= menorDistancia)
            {
                continue;
            }

            menorDistancia = impacto.distance;
            mejorImpacto = impacto;
            encontrado = true;
        }

        if (encontrado)
        {
            posicion.y =
                mejorImpacto.point.y +
                alturaSobreSuelo;
        }

        return posicion;
    }

    private Vector3 ObtenerOffsetMono(int indice)
    {
        const float separacion = 0.45f;

        if (indice == 0)
        {
            return Vector3.zero;
        }

        if (indice % 2 == 0)
        {
            return Vector3.right *
                   separacion *
                   indice;
        }

        return Vector3.left *
               separacion *
               indice;
    }

    private void OnDisable()
    {
        FinalizarCinematica();
    }
}