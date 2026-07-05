using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class CangrejoAtaque : MonoBehaviour
{
    private enum EstadoCangrejo
    {
        Escondido,
        SaliendoDeArena,
        EntrandoEnArena,
        Patrulla,
        PersigueJugador
    }

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private movimientoplayer movimientoJugador;
    [SerializeField] private JugadorDanioFeedback feedbackDanioJugador;
    [SerializeField] private Animator animatorCangrejo;

    [Header("Deteccion")]
    [SerializeField] private float rangoDeteccion = 8f;
    [SerializeField] private float rangoPerderObjetivo = 12f;
    [SerializeField] private bool noAtacarSiJugadorEstaNadando = true;
    [SerializeField] private bool noAtacarSiJugadorEstaEnBarco = true;

    [Header("Movimiento")]
    [SerializeField] private bool usarNavMeshAgentSiExiste = true;
    [SerializeField] private float velocidadPatrulla = 1.2f;
    [SerializeField] private float velocidadPersecucion = 3.2f;
    [SerializeField] private float velocidadGiro = 10f;
    [SerializeField] private float distanciaPararCercaJugador = 0.75f;
    [SerializeField] private float correccionRotacionModeloY = 0f;

    [Header("Patrulla")]
    [SerializeField] private float radioPatrulla = 5f;
    [SerializeField] private float distanciaLlegadaPatrulla = 0.45f;
    [SerializeField] private float tiempoEntreCambiosPatrulla = 3.5f;

    [Header("Suelo")]
    [SerializeField] private bool ajustarAlturaAlSuelo = true;
    [SerializeField] private LayerMask capasSuelo;
    [SerializeField] private bool autoConfigurarCapasSueloSiEstaVacio = true;
    [SerializeField] private float alturaSobreSuelo = 0.03f;
    [SerializeField] private float alturaOrigenRaycastSuelo = 2f;
    [SerializeField] private float distanciaRaycastSuelo = 6f;
    [SerializeField] private float suavizadoAlturaSuelo = 18f;
    [SerializeField] private bool ignorarTriggersSuelo = true;

    [Header("Arena / esconderse")]
    [SerializeField] private bool empezarEscondidoBajoArena = true;
    [SerializeField] private float rangoActivarSalidaArena = 5f;
    [SerializeField] private bool volverBajoArenaAlAlejarse = true;
    [SerializeField] private float rangoVolverBajoArena = 12f;
    [SerializeField] private bool congelarPrimerFrameMientrasEstaEscondido = true;
    [SerializeField] private bool reproducirAnimacionConPlayDirecto = true;
    [SerializeField] private string estadoAnimatorSalidaArena = "SalirDeArena";
    [SerializeField] private string triggerSalirArena = "salirArena";
    [SerializeField] private float duracionAnimacionSalidaArena = 4.95f;
    [SerializeField] private string estadoAnimatorEntradaArena = "EntrarArena";
    [SerializeField] private string triggerEntrarArena = "entrarArena";
    [SerializeField] private float duracionAnimacionEntradaArena = 3f;
    [SerializeField] private bool reproducirEstadoActivoAlTerminar = true;
    [SerializeField] private string estadoAnimatorActivo = "Idle";
    [SerializeField] private float retardoAntesDeAtacarTrasSalir = 0.35f;

    [Header("Arena / control directo")]
    [Tooltip("Si esta activo, el cangrejo NO sale si el jugador ya empieza dentro del rango. Tiene que salir del rango y volver a entrar.")]
    [SerializeField] private bool activarSalidaSoloAlEntrarEnRango = true;
    [SerializeField] private bool noAjustarAlturaSueloMientrasEstaEscondidoOSaliendo = true;
    [SerializeField] private bool desactivarNavMeshAgentMientrasEstaEnArena = true;
    [SerializeField] private bool usarRootMotionSoloDuranteArena = false;
    [Range(0f, 1f)]
    [SerializeField] private float frameNormalizadoOcultoSalidaArena = 0f;
    [SerializeField] private bool forzarEvaluacionAnimatorAlCambiarEstado = true;
    [SerializeField] private bool congelarAnimatorConSpeedCero = true;
    [Tooltip("Congela de verdad el cangrejo escondido apagando el Animator. Esto evita que el estado por defecto SalirDeArena avance al empezar el juego.")]
    [SerializeField] private bool desactivarAnimatorMientrasEstaEscondido = true;
    [Tooltip("Al desactivar esto puedes probar Root Motion en las animaciones de arena. Recomendado dejarlo activo salvo que la animacion no suba nada.")]
    [SerializeField] private bool forzarRootMotionOffSiNoSeUsaDuranteArena = true;

    [Header("Ataque")]
    [SerializeField] private int danioPorAtaque = 8;
    [SerializeField] private float distanciaAtaque = 1.05f;
    [SerializeField] private float tiempoEntreAtaques = 1.25f;

    [Header("Audio opcional")]
    [SerializeField] private AudioSource audioSourceCangrejo;
    [SerializeField] private AudioClip[] sonidosAtaqueCangrejo;
    [Range(0f, 1f)]
    [SerializeField] private float volumenAtaqueCangrejo = 0.7f;

    [Header("Animator opcional")]
    [SerializeField] private string boolCaminando = "isWalking";
    [SerializeField] private string boolPersiguiendo = "isChasing";
    [SerializeField] private string triggerAtaque = "attack";
    [SerializeField] private string parametroVelocidad = "speed";

    [Header("Debug")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private bool mostrarLogsAtaque;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private Vector3 posicionInicial;
    private Vector3 destinoPatrulla;
    private float tiempoSiguienteCambioPatrulla;
    private float tiempoSiguienteAtaque;
    private EstadoCangrejo estadoActual;
    private float velocidadActualAnimator;
    private int ultimoIndiceAtaque = -1;
    private bool haSalidoDeLaArena;
    private float tiempoTerminarAnimacionArena;
    private float velocidadAnimatorOriginal = 1f;
    private bool rootMotionOriginal;
    private bool navMeshDesactivadoPorArena;
    private bool sensorRangoSalidaInicializado;
    private bool jugadorEstabaDentroRangoSalida;

    private void Awake()
    {
        posicionInicial = transform.position;

        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.speed = velocidadPatrulla;
            agent.stoppingDistance = distanciaLlegadaPatrulla;
        }

        if (animatorCangrejo == null)
        {
            animatorCangrejo = GetComponentInChildren<Animator>();
        }

        if (animatorCangrejo != null)
        {
            rootMotionOriginal = animatorCangrejo.applyRootMotion;
            velocidadAnimatorOriginal = Mathf.Approximately(animatorCangrejo.speed, 0f)
                ? 1f
                : animatorCangrejo.speed;
        }

        PrepararAudio();
        AutoConfigurarCapasSuelo();
        CachearJugador();

        haSalidoDeLaArena = !empezarEscondidoBajoArena;
        estadoActual = empezarEscondidoBajoArena
            ? EstadoCangrejo.Escondido
            : EstadoCangrejo.Patrulla;

        if (empezarEscondidoBajoArena)
        {
            ForzarCangrejoEscondido();
        }
        else
        {
            ConfigurarControlArena(false);
        }

        InicializarSensorRangoSalida();
        ElegirNuevoDestinoPatrulla();
    }

    private void Start()
    {
        // Lo repetimos en Start para ganar a la evaluacion inicial del Animator.
        // Asi el estado por defecto del Animator no reproduce SalirDeArena al comenzar.
        if (empezarEscondidoBajoArena && estadoActual == EstadoCangrejo.Escondido)
        {
            ForzarCangrejoEscondido();
            InicializarSensorRangoSalida();
        }
    }

    private void OnEnable()
    {
        // Si el objeto se reactiva, vuelve a quedarse realmente congelado si su estado es escondido.
        if (Application.isPlaying && empezarEscondidoBajoArena && estadoActual == EstadoCangrejo.Escondido)
        {
            if (animatorCangrejo == null)
            {
                animatorCangrejo = GetComponentInChildren<Animator>();
            }

            ForzarCangrejoEscondido();
        }
    }

    private void Update()
    {
        CachearJugador();

        if (!haSalidoDeLaArena ||
            estadoActual == EstadoCangrejo.SaliendoDeArena ||
            estadoActual == EstadoCangrejo.EntrandoEnArena)
        {
            ActualizarCicloArena();

            if (!noAjustarAlturaSueloMientrasEstaEscondidoOSaliendo)
            {
                AjustarAlturaSuelo();
            }

            return;
        }

        if (DebeVolverBajoArena())
        {
            IniciarEntradaArena();
            return;
        }

        bool jugadorNadando = JugadorEstaNadando();
        bool jugadorEnBarco = JugadorEstaEnBarco();

        ActualizarEstado(jugadorNadando, jugadorEnBarco);
        ActualizarMovimiento();
        ActualizarAtaque(jugadorNadando, jugadorEnBarco);
        AjustarAlturaSuelo();
        ActualizarAnimator();
    }

    private void CachearJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorGO = GameObject.FindGameObjectWithTag("Player");

            if (jugadorGO != null)
            {
                jugador = jugadorGO.transform;
            }
        }

        if (jugador == null)
        {
            return;
        }

        if (movimientoJugador == null)
        {
            movimientoJugador = jugador.GetComponent<movimientoplayer>();
        }

        if (feedbackDanioJugador == null)
        {
            feedbackDanioJugador = jugador.GetComponent<JugadorDanioFeedback>();
        }

        if (feedbackDanioJugador == null)
        {
            feedbackDanioJugador = FindFirstObjectByType<JugadorDanioFeedback>();
        }
    }

    private void PrepararAudio()
    {
        if (audioSourceCangrejo == null)
        {
            audioSourceCangrejo = GetComponent<AudioSource>();
        }

        if (audioSourceCangrejo == null && sonidosAtaqueCangrejo != null && sonidosAtaqueCangrejo.Length > 0)
        {
            audioSourceCangrejo = gameObject.AddComponent<AudioSource>();
        }

        if (audioSourceCangrejo != null)
        {
            audioSourceCangrejo.playOnAwake = false;
            audioSourceCangrejo.loop = false;
            audioSourceCangrejo.spatialBlend = 1f;
        }
    }

    private void AutoConfigurarCapasSuelo()
    {
        if (!autoConfigurarCapasSueloSiEstaVacio || capasSuelo.value != 0)
        {
            return;
        }

        int layerSuelo = LayerMask.NameToLayer("Suelo");

        if (layerSuelo < 0)
        {
            layerSuelo = LayerMask.NameToLayer("Isla");
        }

        if (layerSuelo < 0)
        {
            layerSuelo = LayerMask.NameToLayer("Ground");
        }

        if (layerSuelo >= 0)
        {
            capasSuelo = 1 << layerSuelo;
        }
    }

    private void InicializarSensorRangoSalida()
    {
        sensorRangoSalidaInicializado = true;
        jugadorEstabaDentroRangoSalida = JugadorDentroRangoSalida();
    }

    private void ForzarCangrejoEscondido()
    {
        haSalidoDeLaArena = false;
        estadoActual = EstadoCangrejo.Escondido;
        velocidadActualAnimator = 0f;

        if (animatorCangrejo != null)
        {
            animatorCangrejo.enabled = true;
        }

        ConfigurarControlArena(true);
        DetenerAgent();

        SetBoolAnimatorSiExiste(boolCaminando, false);
        SetBoolAnimatorSiExiste(boolPersiguiendo, false);
        SetFloatAnimatorSiExiste(parametroVelocidad, 0f);

        if (animatorCangrejo == null || !congelarPrimerFrameMientrasEstaEscondido)
        {
            return;
        }

        ReproducirEstadoAnimator(
            estadoAnimatorSalidaArena,
            frameNormalizadoOcultoSalidaArena,
            false,
            true);

        if (congelarAnimatorConSpeedCero)
        {
            animatorCangrejo.speed = 0f;
        }

        if (forzarEvaluacionAnimatorAlCambiarEstado)
        {
            animatorCangrejo.Update(0f);
        }

        if (desactivarAnimatorMientrasEstaEscondido)
        {
            animatorCangrejo.enabled = false;
        }
    }

    private void ConfigurarControlArena(bool enArena)
    {
        if (agent != null && desactivarNavMeshAgentMientrasEstaEnArena)
        {
            if (enArena && agent.enabled)
            {
                navMeshDesactivadoPorArena = true;
                agent.enabled = false;
            }
            else if (!enArena && navMeshDesactivadoPorArena)
            {
                agent.enabled = true;
                navMeshDesactivadoPorArena = false;
            }
        }
        else if (enArena)
        {
            DetenerAgent();
        }

        if (animatorCangrejo != null)
        {
            if (enArena)
            {
                animatorCangrejo.applyRootMotion = usarRootMotionSoloDuranteArena;
            }
            else
            {
                animatorCangrejo.applyRootMotion = forzarRootMotionOffSiNoSeUsaDuranteArena
                    ? false
                    : rootMotionOriginal;
            }
        }
    }

    private void ReproducirEstadoAnimator(
        string nombreEstado,
        float tiempoNormalizado,
        bool resetearVelocidad,
        bool congelarTrasEvaluar = false)
    {
        if (animatorCangrejo == null || string.IsNullOrEmpty(nombreEstado))
        {
            return;
        }

        if (!EstadoExisteEnAnimator(nombreEstado))
        {
            return;
        }

        if (!animatorCangrejo.enabled)
        {
            animatorCangrejo.enabled = true;
        }

        if (resetearVelocidad)
        {
            animatorCangrejo.speed = Mathf.Approximately(velocidadAnimatorOriginal, 0f)
                ? 1f
                : velocidadAnimatorOriginal;
        }

        animatorCangrejo.Play(nombreEstado, 0, Mathf.Clamp01(tiempoNormalizado));

        if (forzarEvaluacionAnimatorAlCambiarEstado)
        {
            animatorCangrejo.Update(0f);
        }

        if (congelarTrasEvaluar && congelarAnimatorConSpeedCero)
        {
            animatorCangrejo.speed = 0f;
        }
    }

    private void ActualizarCicloArena()
    {
        DetenerAgent();
        velocidadActualAnimator = 0f;

        switch (estadoActual)
        {
            case EstadoCangrejo.Escondido:
                ActualizarEstadoEscondido();
                break;

            case EstadoCangrejo.SaliendoDeArena:
                RotarSuavementeHaciaJugadorMientrasSale();

                if (Time.time >= tiempoTerminarAnimacionArena)
                {
                    TerminarSalidaArena();
                }
                break;

            case EstadoCangrejo.EntrandoEnArena:
                if (Time.time >= tiempoTerminarAnimacionArena)
                {
                    TerminarEntradaArena();
                }
                break;
        }
    }

    private void ActualizarEstadoEscondido()
    {
        bool dentroAhora = JugadorDentroRangoSalida();

        if (!sensorRangoSalidaInicializado)
        {
            sensorRangoSalidaInicializado = true;
            jugadorEstabaDentroRangoSalida = dentroAhora;
            return;
        }

        bool debeSalir = activarSalidaSoloAlEntrarEnRango
            ? dentroAhora && !jugadorEstabaDentroRangoSalida
            : dentroAhora;

        jugadorEstabaDentroRangoSalida = dentroAhora;

        if (debeSalir)
        {
            IniciarSalidaArena();
        }
    }

    private bool JugadorDentroRangoSalida()
    {
        if (jugador == null)
        {
            return false;
        }

        float distancia = DistanciaHorizontal(transform.position, jugador.position);
        return distancia <= rangoActivarSalidaArena;
    }

    private bool DebeVolverBajoArena()
    {
        if (!empezarEscondidoBajoArena ||
            !volverBajoArenaAlAlejarse ||
            !haSalidoDeLaArena ||
            estadoActual == EstadoCangrejo.SaliendoDeArena ||
            estadoActual == EstadoCangrejo.EntrandoEnArena ||
            jugador == null)
        {
            return false;
        }

        float rango = rangoVolverBajoArena > 0f
            ? rangoVolverBajoArena
            : rangoPerderObjetivo;

        float distancia = DistanciaHorizontal(transform.position, jugador.position);
        return distancia > rango;
    }

    private void IniciarSalidaArena()
    {
        estadoActual = EstadoCangrejo.SaliendoDeArena;
        tiempoTerminarAnimacionArena = Time.time + Mathf.Max(0.05f, duracionAnimacionSalidaArena);
        ConfigurarControlArena(true);
        DetenerAgent();

        SetBoolAnimatorSiExiste(boolCaminando, false);
        SetBoolAnimatorSiExiste(boolPersiguiendo, false);
        SetFloatAnimatorSiExiste(parametroVelocidad, 0f);

        if (animatorCangrejo != null)
        {
            animatorCangrejo.enabled = true;
            animatorCangrejo.applyRootMotion = usarRootMotionSoloDuranteArena;

            if (reproducirAnimacionConPlayDirecto && EstadoExisteEnAnimator(estadoAnimatorSalidaArena))
            {
                ReproducirEstadoAnimator(estadoAnimatorSalidaArena, 0f, true);
            }
            else
            {
                animatorCangrejo.speed = Mathf.Approximately(velocidadAnimatorOriginal, 0f)
                    ? 1f
                    : velocidadAnimatorOriginal;
                SetTriggerAnimatorSiExiste(triggerSalirArena);
            }
        }

        if (mostrarLogsAtaque)
        {
            Debug.Log("CangrejoAtaque: el jugador entro en rango, saliendo de arena.", this);
        }
    }

    private void IniciarEntradaArena()
    {
        estadoActual = EstadoCangrejo.EntrandoEnArena;
        haSalidoDeLaArena = false;
        tiempoTerminarAnimacionArena = Time.time + Mathf.Max(0.05f, duracionAnimacionEntradaArena);
        ConfigurarControlArena(true);
        DetenerAgent();
        velocidadActualAnimator = 0f;

        SetBoolAnimatorSiExiste(boolCaminando, false);
        SetBoolAnimatorSiExiste(boolPersiguiendo, false);
        SetFloatAnimatorSiExiste(parametroVelocidad, 0f);

        if (animatorCangrejo != null)
        {
            animatorCangrejo.enabled = true;
            animatorCangrejo.applyRootMotion = usarRootMotionSoloDuranteArena;

            if (reproducirAnimacionConPlayDirecto && EstadoExisteEnAnimator(estadoAnimatorEntradaArena))
            {
                ReproducirEstadoAnimator(estadoAnimatorEntradaArena, 0f, true);
            }
            else
            {
                animatorCangrejo.speed = Mathf.Approximately(velocidadAnimatorOriginal, 0f)
                    ? 1f
                    : velocidadAnimatorOriginal;
                SetTriggerAnimatorSiExiste(triggerEntrarArena);
            }
        }

        if (mostrarLogsAtaque)
        {
            Debug.Log("CangrejoAtaque: jugador lejos, entrando en arena.", this);
        }
    }

    private void RotarSuavementeHaciaJugadorMientrasSale()
    {
        if (jugador == null)
        {
            return;
        }

        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;
        RotarHacia(direccion);
    }

    private void TerminarSalidaArena()
    {
        haSalidoDeLaArena = true;
        estadoActual = EstadoCangrejo.Patrulla;
        ConfigurarControlArena(false);
        AjustarAlturaSuelo();
        posicionInicial = transform.position;
        tiempoSiguienteAtaque = Time.time + retardoAntesDeAtacarTrasSalir;
        sensorRangoSalidaInicializado = false;
        ElegirNuevoDestinoPatrulla();

        if (animatorCangrejo != null)
        {
            animatorCangrejo.enabled = true;
            animatorCangrejo.applyRootMotion = forzarRootMotionOffSiNoSeUsaDuranteArena
                ? false
                : rootMotionOriginal;
            animatorCangrejo.speed = Mathf.Approximately(velocidadAnimatorOriginal, 0f)
                ? 1f
                : velocidadAnimatorOriginal;

            if (reproducirEstadoActivoAlTerminar && EstadoExisteEnAnimator(estadoAnimatorActivo))
            {
                ReproducirEstadoAnimator(estadoAnimatorActivo, 0f, true);
            }
        }

        if (mostrarLogsAtaque)
        {
            Debug.Log("CangrejoAtaque: salida terminada, cangrejo activo.", this);
        }
    }

    private void TerminarEntradaArena()
    {
        ForzarCangrejoEscondido();
        InicializarSensorRangoSalida();

        if (mostrarLogsAtaque)
        {
            Debug.Log("CangrejoAtaque: entrada terminada, cangrejo congelado bajo arena.", this);
        }
    }

    private bool EstadoExisteEnAnimator(string nombreEstado)
    {
        if (animatorCangrejo == null || string.IsNullOrEmpty(nombreEstado))
        {
            return false;
        }

        int hashEstado = Animator.StringToHash(nombreEstado);

        if (animatorCangrejo.HasState(0, hashEstado))
        {
            return true;
        }

        if (mostrarLogsAtaque)
        {
            Debug.LogWarning(
                "CangrejoAtaque: no encuentro el estado de Animator '" + nombreEstado + "'. " +
                "El nombre del ESTADO del Animator tiene que coincidir exactamente.",
                this);
        }

        return false;
    }

    private void ActualizarEstado(bool jugadorNadando, bool jugadorEnBarco)
    {
        if (jugador == null)
        {
            estadoActual = EstadoCangrejo.Patrulla;
            return;
        }

        if (noAtacarSiJugadorEstaEnBarco && jugadorEnBarco)
        {
            estadoActual = EstadoCangrejo.Patrulla;
            return;
        }

        if (noAtacarSiJugadorEstaNadando && jugadorNadando)
        {
            estadoActual = EstadoCangrejo.Patrulla;
            return;
        }

        float distancia = DistanciaHorizontal(transform.position, jugador.position);
        bool cercaParaDetectar = distancia <= rangoDeteccion;
        bool demasiadoLejos = distancia > rangoPerderObjetivo;

        if (estadoActual != EstadoCangrejo.Patrulla && demasiadoLejos)
        {
            estadoActual = EstadoCangrejo.Patrulla;
            return;
        }

        estadoActual = cercaParaDetectar
            ? EstadoCangrejo.PersigueJugador
            : EstadoCangrejo.Patrulla;
    }

    private void ActualizarMovimiento()
    {
        switch (estadoActual)
        {
            case EstadoCangrejo.PersigueJugador:
                MoverHacia(jugador.position, velocidadPersecucion, distanciaPararCercaJugador);
                break;

            default:
                Patrullar();
                break;
        }
    }

    private void Patrullar()
    {
        if (Time.time >= tiempoSiguienteCambioPatrulla ||
            DistanciaHorizontal(transform.position, destinoPatrulla) <= distanciaLlegadaPatrulla)
        {
            ElegirNuevoDestinoPatrulla();
        }

        MoverHacia(destinoPatrulla, velocidadPatrulla, distanciaLlegadaPatrulla);
    }

    private void ElegirNuevoDestinoPatrulla()
    {
        Vector2 circulo = UnityEngine.Random.insideUnitCircle * radioPatrulla;

        destinoPatrulla = posicionInicial + new Vector3(circulo.x, 0f, circulo.y);
        tiempoSiguienteCambioPatrulla = Time.time + tiempoEntreCambiosPatrulla;
    }

    private void MoverHacia(Vector3 destino, float velocidad, float distanciaParada)
    {
        if (destino == Vector3.zero && jugador == null)
        {
            velocidadActualAnimator = 0f;
            DetenerAgent();
            return;
        }

        Vector3 posicionActual = transform.position;
        Vector3 destinoPlano = new Vector3(destino.x, posicionActual.y, destino.z);
        Vector3 direccion = destinoPlano - posicionActual;
        direccion.y = 0f;

        float distancia = direccion.magnitude;

        if (distancia <= 0.001f)
        {
            velocidadActualAnimator = 0f;
            DetenerAgent();
            return;
        }

        Vector3 direccionNormalizada = direccion / distancia;
        RotarHacia(direccionNormalizada);

        if (distancia <= distanciaParada)
        {
            velocidadActualAnimator = 0f;
            DetenerAgent();
            return;
        }

        if (PuedeUsarNavMeshAgent())
        {
            agent.speed = velocidad;
            agent.stoppingDistance = distanciaParada;
            agent.isStopped = false;
            agent.SetDestination(destino);
            velocidadActualAnimator = agent.velocity.magnitude;
            return;
        }

        Vector3 nuevaPosicion = posicionActual + direccionNormalizada * velocidad * Time.deltaTime;

        if (rb != null)
        {
            rb.MovePosition(nuevaPosicion);
        }
        else
        {
            transform.position = nuevaPosicion;
        }

        velocidadActualAnimator = velocidad;
    }

    private bool PuedeUsarNavMeshAgent()
    {
        return usarNavMeshAgentSiExiste &&
               agent != null &&
               agent.enabled &&
               agent.isOnNavMesh;
    }

    private void DetenerAgent()
    {
        if (PuedeUsarNavMeshAgent())
        {
            agent.isStopped = true;
        }
    }

    private void RotarHacia(Vector3 direccion)
    {
        if (direccion.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion, Vector3.up) *
                                      Quaternion.Euler(0f, correccionRotacionModeloY, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadGiro * Time.deltaTime);
    }

    private void ActualizarAtaque(bool jugadorNadando, bool jugadorEnBarco)
    {
        if (jugador == null)
        {
            return;
        }

        if (noAtacarSiJugadorEstaEnBarco && jugadorEnBarco)
        {
            return;
        }

        if (noAtacarSiJugadorEstaNadando && jugadorNadando)
        {
            return;
        }

        if (estadoActual != EstadoCangrejo.PersigueJugador)
        {
            return;
        }

        if (Time.time < tiempoSiguienteAtaque)
        {
            return;
        }

        float distancia = DistanciaHorizontal(transform.position, jugador.position);

        if (distancia > distanciaAtaque)
        {
            return;
        }

        AtacarJugador();
    }

    private void AtacarJugador()
    {
        tiempoSiguienteAtaque = Time.time + tiempoEntreAtaques;

        SetTriggerAnimatorSiExiste(triggerAtaque);
        ReproducirSonidoAtaqueCangrejo();

        if (feedbackDanioJugador != null)
        {
            feedbackDanioJugador.RecibirDanio(danioPorAtaque, transform.position, transform);
        }
        else
        {
            PlayerHealth playerHealth = jugador != null
                ? jugador.GetComponent<PlayerHealth>()
                : null;

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(danioPorAtaque);
            }
        }

        if (mostrarLogsAtaque)
        {
            Debug.Log("CangrejoAtaque: ataque al jugador. Danio = " + danioPorAtaque, this);
        }
    }

    private void ReproducirSonidoAtaqueCangrejo()
    {
        if (audioSourceCangrejo == null || sonidosAtaqueCangrejo == null || sonidosAtaqueCangrejo.Length == 0)
        {
            return;
        }

        AudioClip clip = ObtenerClipAtaqueAleatorio();

        if (clip == null)
        {
            return;
        }

        audioSourceCangrejo.PlayOneShot(clip, volumenAtaqueCangrejo);
    }

    private AudioClip ObtenerClipAtaqueAleatorio()
    {
        if (sonidosAtaqueCangrejo.Length == 1)
        {
            ultimoIndiceAtaque = 0;
            return sonidosAtaqueCangrejo[0];
        }

        int indice;

        do
        {
            indice = UnityEngine.Random.Range(0, sonidosAtaqueCangrejo.Length);
        }
        while (indice == ultimoIndiceAtaque);

        ultimoIndiceAtaque = indice;
        return sonidosAtaqueCangrejo[indice];
    }

    private bool JugadorEstaNadando()
    {
        if (movimientoJugador == null)
        {
            return false;
        }

        return movimientoJugador.enabled && movimientoJugador.IsSwimming();
    }

    private bool JugadorEstaEnBarco()
    {
        if (jugador == null)
        {
            return false;
        }

        return jugador.GetComponentInParent<BarcoNavegable>() != null;
    }

    private void AjustarAlturaSuelo()
    {
        if (!ajustarAlturaAlSuelo || capasSuelo.value == 0)
        {
            return;
        }

        if (PuedeUsarNavMeshAgent())
        {
            return;
        }

        Vector3 origen = transform.position + Vector3.up * alturaOrigenRaycastSuelo;
        QueryTriggerInteraction triggerInteraction = ignorarTriggersSuelo
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        if (!Physics.Raycast(origen, Vector3.down, out RaycastHit hit, distanciaRaycastSuelo, capasSuelo, triggerInteraction))
        {
            return;
        }

        Vector3 posicion = transform.position;
        float yObjetivo = hit.point.y + alturaSobreSuelo;
        posicion.y = Mathf.Lerp(posicion.y, yObjetivo, suavizadoAlturaSuelo * Time.deltaTime);
        transform.position = posicion;
    }

    private float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void ActualizarAnimator()
    {
        bool moviendose = velocidadActualAnimator > 0.05f;
        bool persiguiendo = estadoActual == EstadoCangrejo.PersigueJugador;

        SetBoolAnimatorSiExiste(boolCaminando, moviendose);
        SetBoolAnimatorSiExiste(boolPersiguiendo, persiguiendo);
        SetFloatAnimatorSiExiste(parametroVelocidad, velocidadActualAnimator);
    }

    private void SetBoolAnimatorSiExiste(string nombreParametro, bool valor)
    {
        if (animatorCangrejo == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorCangrejo.parameters)
        {
            if (parametro.name == nombreParametro && parametro.type == AnimatorControllerParameterType.Bool)
            {
                animatorCangrejo.SetBool(nombreParametro, valor);
                return;
            }
        }
    }

    private void SetFloatAnimatorSiExiste(string nombreParametro, float valor)
    {
        if (animatorCangrejo == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorCangrejo.parameters)
        {
            if (parametro.name == nombreParametro && parametro.type == AnimatorControllerParameterType.Float)
            {
                animatorCangrejo.SetFloat(nombreParametro, valor);
                return;
            }
        }
    }

    private void SetTriggerAnimatorSiExiste(string nombreParametro)
    {
        if (animatorCangrejo == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorCangrejo.parameters)
        {
            if (parametro.name == nombreParametro && parametro.type == AnimatorControllerParameterType.Trigger)
            {
                animatorCangrejo.SetTrigger(nombreParametro);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, rangoActivarSalidaArena);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, rangoVolverBajoArena);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        Gizmos.color = Color.cyan;
        Vector3 centroPatrulla = Application.isPlaying ? posicionInicial : transform.position;
        Gizmos.DrawWireSphere(centroPatrulla, radioPatrulla);
    }
}
