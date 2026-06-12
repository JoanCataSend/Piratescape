using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private GameObject tiendaAnimada;
    [SerializeField] private Animator animatorTiendaAnimada;
    [SerializeField] private string estadoAnimacionTienda = "Take 001";
    [SerializeField] private float duracionAnimacionTienda = 3.35f;
    [SerializeField] private float esperaTrasAnimacionTienda = 0.2f;

    [Header("Ocultar tienda normal sin desactivarla")]
    [SerializeField] private Renderer[] renderersTiendaNormal;

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

    [Header("Audio - Ronquido pirata")]
    [SerializeField] private AudioSource audioSourceRonquido;

    [Tooltip("Grupo del Audio Mixer para ronquidos y mi-mi-mi del pirata.")]
    [SerializeField] private AudioMixerGroup outputRonquidoPirata;

    [Tooltip("Opcional. Se usa como respaldo si el array de ronquidos está vacío.")]
    [SerializeField] private AudioClip sonidoRonquidoPirata;

    [Tooltip("Ronquidos posibles. El script elige uno random cada ciclo.")]
    [SerializeField] private AudioClip[] sonidosRonquidoPirata;

    [Tooltip("Sonidos tipo mi-mi-mi que suenan después de cada ronquido.")]
    [SerializeField] private AudioClip[] sonidosMimimiPirata;

    [Range(0f, 1f)]
    [SerializeField] private float volumenRonquido = 0.55f;

    [Tooltip("Pausa entre el ronquido y el mi-mi-mi.")]
    [Min(0f)]
    [SerializeField] private float pausaEntreRonquidoYMimimi = 0.05f;

    [Tooltip("Pausa mínima antes de volver a empezar otro ciclo ronquido + mi-mi-mi.")]
    [Min(0f)]
    [SerializeField] private float pausaEntreCiclosRonquidoMin = 0.15f;

    [Tooltip("Pausa máxima antes de volver a empezar otro ciclo ronquido + mi-mi-mi.")]
    [Min(0f)]
    [SerializeField] private float pausaEntreCiclosRonquidoMax = 0.45f;

    [Header("Audio - Pasos monos entrando")]
    [SerializeField] private AudioSource audioSourcePasosMonos;

    [Tooltip("Grupo del Audio Mixer para pasos de monos en la cinemática.")]
    [SerializeField] private AudioMixerGroup outputPasosMonos;

    [SerializeField] private AudioClip[] sonidosPasosEntrada;

    [Range(0f, 1f)]
    [SerializeField] private float volumenPasosEntrada = 0.55f;

    [SerializeField] private float intervaloPasosEntrada = 0.42f;

    [Header("Audio - Robo dentro de la tienda")]
    [SerializeField] private AudioSource audioSourceRoboTienda;

    [Tooltip("Grupo del Audio Mixer para jaleo/robo dentro de la tienda.")]
    [SerializeField] private AudioMixerGroup outputRoboTienda;

    [SerializeField] private AudioClip[] sonidosRoboTienda;

    [Range(0f, 1f)]
    [SerializeField] private float volumenRoboTienda = 0.8f;

    [SerializeField] private float intervaloRoboMin = 0.18f;
    [SerializeField] private float intervaloRoboMax = 0.45f;

    [Header("Audio - Monos saliendo corriendo")]
    [SerializeField] private AudioClip[] sonidosPasosSalidaRapidos;

    [Range(0f, 1f)]
    [SerializeField] private float volumenPasosSalida = 0.65f;

    [SerializeField] private float intervaloPasosSalida = 0.18f;

    [Header("Audio - Risas monos")]
    [SerializeField] private AudioSource audioSourceRisasMonos;

    [Tooltip("Grupo del Audio Mixer para risas/voces de los monos.")]
    [SerializeField] private AudioMixerGroup outputRisasMonos;

    [SerializeField] private AudioClip[] sonidosRisasSalida;

    [Range(0f, 1f)]
    [SerializeField] private float volumenRisas = 0.8f;

    [SerializeField] private float intervaloRisasMin = 0.45f;
    [SerializeField] private float intervaloRisasMax = 0.9f;

    [Header("Audio - Variación")]
    [SerializeField] private float pitchMinimo = 0.96f;
    [SerializeField] private float pitchMaximo = 1.06f;

    [Header("Música del juego a detener")]
    [Tooltip("Actívalo para parar la música normal del juego cuando empiece la cinemática de los monos.")]
    [SerializeField] private bool detenerMusicaJuegoAlEmpezar = true;

    [Tooltip("Arrastra aquí el AudioSource de la música normal del juego.")]
    [SerializeField] private AudioSource musicaJuegoSource;

    [Tooltip("Si está activo, la música normal baja suavemente antes de pararse.")]
    [SerializeField] private bool hacerFadeMusicaJuego = true;

    [Min(0f)]
    [SerializeField] private float duracionFadeMusicaJuego = 1f;

    private Coroutine rutinaMovimiento;
    private Coroutine rutinaRonquido;
    private Coroutine rutinaPasosEntrada;
    private Coroutine rutinaRoboTienda;
    private Coroutine rutinaPasosSalida;
    private Coroutine rutinaRisas;
    private Coroutine rutinaDetenerMusicaJuego;

    private bool cinematicaActiva;

    private int ultimoRonquido = -1;
    private int ultimoMimimi = -1;
    private int ultimoPasoEntrada = -1;
    private int ultimoRoboTienda = -1;
    private int ultimoPasoSalida = -1;
    private int ultimaRisa = -1;

    private void Awake()
    {
        PrepararAudioSources();
        PrepararEstadoInicial();
    }

    private void PrepararEstadoInicial()
    {
        cinematicaActiva = false;
        rutinaMovimiento = null;

        DetenerTodosLosSonidos();

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

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
        DetenerMusicaJuegoAlEmpezarCinematica();

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

        IniciarRonquidoPirata();

        rutinaMovimiento = StartCoroutine(ReproducirMovimientoMonos());
    }

    public IEnumerator ReproducirMovimientoMonos()
    {
        ReproducirAnimacionTodos(estadoWalk);

        IniciarPasosEntrada();

        yield return MoverMonosA(
            puntoEntradaTienda,
            velocidadIrTienda
        );

        DetenerPasosEntrada();

        if (!cinematicaActiva)
        {
            yield break;
        }

        OcultarMonos();

        IniciarRoboTienda();

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

        DetenerRoboTienda();

        if (!cinematicaActiva)
        {
            yield break;
        }

        ColocarMonosEnEntradaTienda();
        MostrarMonos();

        ReproducirAnimacionTodos(estadoRun);

        IniciarPasosSalida();
        IniciarRisasSalida();

        yield return MoverMonosAPuntosIniciales(
            velocidadSalirCorriendo
        );

        DetenerPasosSalida();
        DetenerRisasSalida();

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
        DetenerTodosLosSonidos();
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

    private void DetenerMusicaJuegoAlEmpezarCinematica()
    {
        if (!detenerMusicaJuegoAlEmpezar)
        {
            return;
        }

        if (musicaJuegoSource == null)
        {
            return;
        }

        if (!musicaJuegoSource.isPlaying)
        {
            return;
        }

        if (rutinaDetenerMusicaJuego != null)
        {
            StopCoroutine(rutinaDetenerMusicaJuego);
            rutinaDetenerMusicaJuego = null;
        }

        rutinaDetenerMusicaJuego =
            StartCoroutine(DetenerMusicaJuegoRoutine());
    }

    private IEnumerator DetenerMusicaJuegoRoutine()
    {
        if (musicaJuegoSource == null)
        {
            yield break;
        }

        if (!hacerFadeMusicaJuego ||
            duracionFadeMusicaJuego <= 0f)
        {
            musicaJuegoSource.Stop();
            rutinaDetenerMusicaJuego = null;
            yield break;
        }

        float volumenInicial = musicaJuegoSource.volume;
        float tiempo = 0f;

        while (tiempo < duracionFadeMusicaJuego &&
               musicaJuegoSource != null)
        {
            tiempo += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    tiempo / duracionFadeMusicaJuego
                );

            musicaJuegoSource.volume =
                Mathf.Lerp(
                    volumenInicial,
                    0f,
                    t
                );

            yield return null;
        }

        if (musicaJuegoSource != null)
        {
            musicaJuegoSource.Stop();
            musicaJuegoSource.volume = volumenInicial;
        }

        rutinaDetenerMusicaJuego = null;
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
        if ((renderersTiendaNormal == null ||
             renderersTiendaNormal.Length == 0) &&
            tiendaNormal != null)
        {
            renderersTiendaNormal =
                tiendaNormal.GetComponentsInChildren<Renderer>(true);
        }

        if (renderersTiendaNormal == null)
        {
            return;
        }

        for (int i = 0; i < renderersTiendaNormal.Length; i++)
        {
            if (renderersTiendaNormal[i] != null)
            {
                renderersTiendaNormal[i].enabled = mostrar;
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
                tiendaAnimada.GetComponentInChildren<Animator>(true);
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
                monoRobo.mono.GetComponentInChildren<Animator>(true);
        }
    }

    private void ReproducirAnimacionTodos(string nombreEstado)
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
        float velocidad)
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
                    destino.position + ObtenerOffsetMono(i),
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
        float velocidad)
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
        float velocidad)
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
        Transform mono)
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

        if (impactos == null || impactos.Length == 0)
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

    private void PrepararAudioSources()
    {
        audioSourceRonquido =
            PrepararAudioSource(
                audioSourceRonquido,
                false,
                outputRonquidoPirata
            );

        audioSourcePasosMonos =
            PrepararAudioSource(
                audioSourcePasosMonos,
                false,
                outputPasosMonos
            );

        audioSourceRoboTienda =
            PrepararAudioSource(
                audioSourceRoboTienda,
                false,
                outputRoboTienda
            );

        audioSourceRisasMonos =
            PrepararAudioSource(
                audioSourceRisasMonos,
                false,
                outputRisasMonos
            );
    }

    private AudioSource PrepararAudioSource(
        AudioSource source,
        bool loop,
        AudioMixerGroup outputMixerGroup)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = loop;

        // Sonido de cinemática, 2D para que siempre se oiga bien.
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;
        source.outputAudioMixerGroup = outputMixerGroup;

        return source;
    }

    private void IniciarRonquidoPirata()
    {
        DetenerRonquidoPirata();

        if (audioSourceRonquido == null)
        {
            return;
        }

        rutinaRonquido =
            StartCoroutine(RutinaRonquidoPirata());
    }

    private void DetenerRonquidoPirata()
    {
        if (rutinaRonquido != null)
        {
            StopCoroutine(rutinaRonquido);
            rutinaRonquido = null;
        }

        if (audioSourceRonquido != null)
        {
            audioSourceRonquido.Stop();
        }
    }

    private IEnumerator RutinaRonquidoPirata()
    {
        while (cinematicaActiva)
        {
            AudioClip clipRonquido =
                ObtenerClipRonquidoPirata();

            AudioClip clipMimimi =
                ObtenerClipAleatorioSinRepetir(
                    sonidosMimimiPirata,
                    ref ultimoMimimi
                );

            if (clipRonquido == null &&
                clipMimimi == null)
            {
                yield return new WaitForSecondsRealtime(0.25f);
                continue;
            }

            if (clipRonquido != null)
            {
                ReproducirClip(
                    audioSourceRonquido,
                    clipRonquido,
                    volumenRonquido
                );

                yield return EsperarMientrasCinematicaActiva(
                    clipRonquido.length
                );
            }

            if (!cinematicaActiva)
            {
                yield break;
            }

            if (pausaEntreRonquidoYMimimi > 0f &&
                clipMimimi != null)
            {
                yield return EsperarMientrasCinematicaActiva(
                    pausaEntreRonquidoYMimimi
                );
            }

            if (!cinematicaActiva)
            {
                yield break;
            }

            if (clipMimimi != null)
            {
                ReproducirClip(
                    audioSourceRonquido,
                    clipMimimi,
                    volumenRonquido
                );

                yield return EsperarMientrasCinematicaActiva(
                    clipMimimi.length
                );
            }

            if (!cinematicaActiva)
            {
                yield break;
            }

            float pausaMin = Mathf.Min(
                pausaEntreCiclosRonquidoMin,
                pausaEntreCiclosRonquidoMax
            );

            float pausaMax = Mathf.Max(
                pausaEntreCiclosRonquidoMin,
                pausaEntreCiclosRonquidoMax
            );

            float pausa = Random.Range(
                pausaMin,
                pausaMax
            );

            yield return EsperarMientrasCinematicaActiva(pausa);
        }
    }

    private AudioClip ObtenerClipRonquidoPirata()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosRonquidoPirata,
                ref ultimoRonquido
            );

        if (clip != null)
        {
            return clip;
        }

        return sonidoRonquidoPirata;
    }

    private IEnumerator EsperarMientrasCinematicaActiva(
        float segundos)
    {
        if (segundos <= 0f)
        {
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < segundos && cinematicaActiva)
        {
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void IniciarPasosEntrada()
    {
        DetenerPasosEntrada();

        rutinaPasosEntrada =
            StartCoroutine(RutinaPasosEntrada());
    }

    private void DetenerPasosEntrada()
    {
        if (rutinaPasosEntrada != null)
        {
            StopCoroutine(rutinaPasosEntrada);
            rutinaPasosEntrada = null;
        }
    }

    private IEnumerator RutinaPasosEntrada()
    {
        while (cinematicaActiva)
        {
            AudioClip clip =
                ObtenerClipAleatorioSinRepetir(
                    sonidosPasosEntrada,
                    ref ultimoPasoEntrada
                );

            ReproducirClip(
                audioSourcePasosMonos,
                clip,
                volumenPasosEntrada
            );

            yield return new WaitForSecondsRealtime(
                intervaloPasosEntrada
            );
        }
    }

    private void IniciarRoboTienda()
    {
        DetenerRoboTienda();

        rutinaRoboTienda =
            StartCoroutine(RutinaRoboTienda());
    }

    private void DetenerRoboTienda()
    {
        if (rutinaRoboTienda != null)
        {
            StopCoroutine(rutinaRoboTienda);
            rutinaRoboTienda = null;
        }
    }

    private IEnumerator RutinaRoboTienda()
    {
        while (cinematicaActiva)
        {
            AudioClip clip =
                ObtenerClipAleatorioSinRepetir(
                    sonidosRoboTienda,
                    ref ultimoRoboTienda
                );

            ReproducirClip(
                audioSourceRoboTienda,
                clip,
                volumenRoboTienda
            );

            float espera =
                Random.Range(
                    Mathf.Min(intervaloRoboMin, intervaloRoboMax),
                    Mathf.Max(intervaloRoboMin, intervaloRoboMax)
                );

            yield return new WaitForSecondsRealtime(
                espera
            );
        }
    }

    private void IniciarPasosSalida()
    {
        DetenerPasosSalida();

        rutinaPasosSalida =
            StartCoroutine(RutinaPasosSalida());
    }

    private void DetenerPasosSalida()
    {
        if (rutinaPasosSalida != null)
        {
            StopCoroutine(rutinaPasosSalida);
            rutinaPasosSalida = null;
        }
    }

    private IEnumerator RutinaPasosSalida()
    {
        while (cinematicaActiva)
        {
            AudioClip clip =
                ObtenerClipAleatorioSinRepetir(
                    sonidosPasosSalidaRapidos,
                    ref ultimoPasoSalida
                );

            ReproducirClip(
                audioSourcePasosMonos,
                clip,
                volumenPasosSalida
            );

            yield return new WaitForSecondsRealtime(
                intervaloPasosSalida
            );
        }
    }

    private void IniciarRisasSalida()
    {
        DetenerRisasSalida();

        rutinaRisas =
            StartCoroutine(RutinaRisasSalida());
    }

    private void DetenerRisasSalida()
    {
        if (rutinaRisas != null)
        {
            StopCoroutine(rutinaRisas);
            rutinaRisas = null;
        }
    }

    private IEnumerator RutinaRisasSalida()
    {
        while (cinematicaActiva)
        {
            AudioClip clip =
                ObtenerClipAleatorioSinRepetir(
                    sonidosRisasSalida,
                    ref ultimaRisa
                );

            ReproducirClip(
                audioSourceRisasMonos,
                clip,
                volumenRisas
            );

            float espera =
                Random.Range(
                    Mathf.Min(intervaloRisasMin, intervaloRisasMax),
                    Mathf.Max(intervaloRisasMin, intervaloRisasMax)
                );

            yield return new WaitForSecondsRealtime(
                espera
            );
        }
    }

    private void DetenerTodosLosSonidos()
    {
        DetenerRonquidoPirata();
        DetenerPasosEntrada();
        DetenerRoboTienda();
        DetenerPasosSalida();
        DetenerRisasSalida();

        if (audioSourcePasosMonos != null)
        {
            audioSourcePasosMonos.Stop();
        }

        if (audioSourceRoboTienda != null)
        {
            audioSourceRoboTienda.Stop();
        }

        if (audioSourceRisasMonos != null)
        {
            audioSourceRisasMonos.Stop();
        }
    }

    private void ReproducirClip(
        AudioSource source,
        AudioClip clip,
        float volumen)
    {
        if (source == null ||
            clip == null)
        {
            return;
        }

        float pitchMenor =
            Mathf.Min(
                pitchMinimo,
                pitchMaximo
            );

        float pitchMayor =
            Mathf.Max(
                pitchMinimo,
                pitchMaximo
            );

        source.pitch =
            Random.Range(
                pitchMenor,
                pitchMayor
            );

        source.PlayOneShot(
            clip,
            volumen
        );
    }

    private AudioClip ObtenerClipAleatorioSinRepetir(
        AudioClip[] clips,
        ref int ultimoIndice)
    {
        if (clips == null ||
            clips.Length == 0)
        {
            return null;
        }

        if (clips.Length == 1)
        {
            ultimoIndice = 0;
            return clips[0];
        }

        int nuevoIndice;
        int intentos = 0;

        do
        {
            nuevoIndice =
                Random.Range(
                    0,
                    clips.Length
                );

            intentos++;
        }
        while (
            nuevoIndice == ultimoIndice &&
            intentos < 10
        );

        ultimoIndice = nuevoIndice;

        return clips[nuevoIndice];
    }

    private void OnDisable()
    {
        FinalizarCinematica();
    }
}