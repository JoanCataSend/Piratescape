using UnityEngine;

[DisallowMultipleComponent]
public class TiburonAgua : MonoBehaviour
{
    private enum EstadoTiburon
    {
        Patrulla,
        PersigueJugadorNadando,
        SeAcercaAlBarco
    }

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private movimientoplayer movimientoJugador;
    [SerializeField] private JugadorDanioFeedback feedbackDanioJugador;
    [SerializeField] private Animator animatorTiburon;

    [Header("Agua")]
    [SerializeField] private Transform agua;
    [SerializeField] private string nombreObjetoAgua = "agua";
    [SerializeField] private LayerMask capasAgua;
    [SerializeField] private bool autoConfigurarCapasAguaSiEstaVacio = true;
    [SerializeField] private float alturaRespectoSuperficieAgua = -0.25f;
    [SerializeField] private float balanceoVertical = 0.05f;
    [SerializeField] private float velocidadBalanceo = 1.5f;
    [SerializeField] private float alturaOrigenRaycastAgua = 4f;
    [SerializeField] private float distanciaRaycastAgua = 10f;
    [SerializeField] private bool detectarTriggersAgua = true;

    [Header("Deteccion")]
    [SerializeField] private float rangoDeteccion = 18f;
    [SerializeField] private float rangoPerderObjetivo = 24f;
    [SerializeField] private bool perseguirBarcoSinAtacar = true;

    [Header("Movimiento")]
    [SerializeField] private float velocidadPatrulla = 2f;
    [SerializeField] private float velocidadPersecucion = 5f;
    [SerializeField] private float velocidadAcercarseAlBarco = 4f;
    [SerializeField] private float velocidadGiro = 8f;
    [SerializeField] private float distanciaPararCercaBarco = 3f;
    [SerializeField] private float distanciaPararCercaJugador = 0.65f;
    [SerializeField] private float correccionRotacionModeloY = 0f;

    [Header("Patrulla")]
    [SerializeField] private float radioPatrulla = 10f;
    [SerializeField] private float distanciaLlegadaPatrulla = 0.8f;
    [SerializeField] private float tiempoEntreCambiosPatrulla = 4f;

    [Header("Ataque")]
    [SerializeField] private int danioPorAtaque = 15;
    [SerializeField] private float distanciaAtaque = 1.35f;
    [SerializeField] private float tiempoEntreAtaques = 1.6f;
    [SerializeField] private bool atacarSoloSiJugadorEstaNadando = true;

    [Header("Animator opcional")]
    [SerializeField] private string boolPersiguiendo = "isChasing";
    [SerializeField] private string triggerAtaque = "attack";
    [SerializeField] private string parametroVelocidad = "speed";

    [Header("Debug")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private bool mostrarLogsAtaque;

    private Rigidbody rb;
    private Vector3 posicionInicial;
    private Vector3 destinoPatrulla;
    private float tiempoSiguienteCambioPatrulla;
    private float tiempoSiguienteAtaque;
    private EstadoTiburon estadoActual;
    private float velocidadActualAnimator;

    private void Awake()
    {
        posicionInicial = transform.position;

        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (animatorTiburon == null)
        {
            animatorTiburon = GetComponentInChildren<Animator>();
        }

        AutoConfigurarCapasAgua();
        CachearAgua();
        CachearJugador();
        ElegirNuevoDestinoPatrulla();
    }

    private void Update()
    {
        CachearJugador();

        bool jugadorNadando = JugadorEstaNadando();
        bool jugadorEnBarco = JugadorEstaEnBarco();

        ActualizarEstado(
            jugadorNadando,
            jugadorEnBarco);

        ActualizarMovimiento();
        ActualizarAtaque(jugadorNadando);
        AjustarAlturaAgua();
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

    private void CachearAgua()
    {
        if (agua != null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(nombreObjetoAgua))
        {
            GameObject aguaGO = GameObject.Find(nombreObjetoAgua);

            if (aguaGO == null && nombreObjetoAgua != "Agua")
            {
                aguaGO = GameObject.Find("Agua");
            }

            if (aguaGO == null && nombreObjetoAgua != "Water")
            {
                aguaGO = GameObject.Find("Water");
            }

            if (aguaGO != null)
            {
                agua = aguaGO.transform;
            }
        }
    }

    private void AutoConfigurarCapasAgua()
    {
        if (!autoConfigurarCapasAguaSiEstaVacio || capasAgua.value != 0)
        {
            return;
        }

        int layerAgua = LayerMask.NameToLayer("Agua");

        if (layerAgua < 0)
        {
            layerAgua = LayerMask.NameToLayer("Water");
        }

        if (layerAgua >= 0)
        {
            capasAgua = 1 << layerAgua;
        }
    }

    private void ActualizarEstado(
        bool jugadorNadando,
        bool jugadorEnBarco)
    {
        if (jugador == null)
        {
            estadoActual = EstadoTiburon.Patrulla;
            return;
        }

        float distancia = DistanciaHorizontal(
            transform.position,
            jugador.position);

        bool cercaParaDetectar = distancia <= rangoDeteccion;
        bool demasiadoLejos = distancia > rangoPerderObjetivo;

        if (estadoActual != EstadoTiburon.Patrulla && demasiadoLejos)
        {
            estadoActual = EstadoTiburon.Patrulla;
            return;
        }

        if (jugadorNadando && cercaParaDetectar)
        {
            estadoActual = EstadoTiburon.PersigueJugadorNadando;
            return;
        }

        if (perseguirBarcoSinAtacar && jugadorEnBarco && cercaParaDetectar)
        {
            estadoActual = EstadoTiburon.SeAcercaAlBarco;
            return;
        }

        if (estadoActual == EstadoTiburon.PersigueJugadorNadando && jugadorNadando)
        {
            return;
        }

        if (estadoActual == EstadoTiburon.SeAcercaAlBarco && jugadorEnBarco)
        {
            return;
        }

        estadoActual = EstadoTiburon.Patrulla;
    }

    private void ActualizarMovimiento()
    {
        switch (estadoActual)
        {
            case EstadoTiburon.PersigueJugadorNadando:
                MoverHacia(
                    jugador.position,
                    velocidadPersecucion,
                    distanciaPararCercaJugador);
                break;

            case EstadoTiburon.SeAcercaAlBarco:
                MoverHacia(
                    jugador.position,
                    velocidadAcercarseAlBarco,
                    distanciaPararCercaBarco);
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

        MoverHacia(
            destinoPatrulla,
            velocidadPatrulla,
            distanciaLlegadaPatrulla);
    }

    private void ElegirNuevoDestinoPatrulla()
    {
        Vector2 circulo = UnityEngine.Random.insideUnitCircle * radioPatrulla;

        destinoPatrulla = posicionInicial + new Vector3(
            circulo.x,
            0f,
            circulo.y);

        tiempoSiguienteCambioPatrulla = Time.time + tiempoEntreCambiosPatrulla;
    }

    private void MoverHacia(
        Vector3 destino,
        float velocidad,
        float distanciaParada)
    {
        Vector3 posicionActual = transform.position;
        Vector3 destinoPlano = new Vector3(
            destino.x,
            posicionActual.y,
            destino.z);

        Vector3 direccion = destinoPlano - posicionActual;
        direccion.y = 0f;

        float distancia = direccion.magnitude;

        if (distancia <= 0.001f)
        {
            velocidadActualAnimator = 0f;
            return;
        }

        Vector3 direccionNormalizada = direccion / distancia;

        RotarHacia(direccionNormalizada);

        if (distancia <= distanciaParada)
        {
            velocidadActualAnimator = 0f;
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

    private void RotarHacia(Vector3 direccion)
    {
        if (direccion.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(
            direccion,
            Vector3.up) * Quaternion.Euler(0f, correccionRotacionModeloY, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadGiro * Time.deltaTime);
    }

    private void ActualizarAtaque(bool jugadorNadando)
    {
        if (jugador == null)
        {
            return;
        }

        if (atacarSoloSiJugadorEstaNadando && !jugadorNadando)
        {
            return;
        }

        if (JugadorEstaEnBarco())
        {
            return;
        }

        if (estadoActual != EstadoTiburon.PersigueJugadorNadando)
        {
            return;
        }

        if (Time.time < tiempoSiguienteAtaque)
        {
            return;
        }

        float distancia = DistanciaHorizontal(
            transform.position,
            jugador.position);

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

        if (feedbackDanioJugador != null)
        {
            feedbackDanioJugador.RecibirDanio(
                danioPorAtaque,
                transform.position);
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
            Debug.Log(
                "TiburonAgua: ataque al jugador. Daño = " + danioPorAtaque,
                this);
        }
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

    private void AjustarAlturaAgua()
    {
        float alturaAgua = ObtenerAlturaAgua(transform.position);
        float balanceo = balanceoVertical > 0f
            ? Mathf.Sin(Time.time * velocidadBalanceo + posicionInicial.x) * balanceoVertical
            : 0f;

        Vector3 posicion = transform.position;
        posicion.y = alturaAgua + alturaRespectoSuperficieAgua + balanceo;
        transform.position = posicion;
    }

    private float ObtenerAlturaAgua(Vector3 posicion)
    {
        if (capasAgua.value != 0)
        {
            Vector3 origen = posicion + Vector3.up * alturaOrigenRaycastAgua;
            QueryTriggerInteraction triggerInteraction = detectarTriggersAgua
                ? QueryTriggerInteraction.Collide
                : QueryTriggerInteraction.Ignore;

            if (Physics.Raycast(
                    origen,
                    Vector3.down,
                    out RaycastHit hit,
                    distanciaRaycastAgua,
                    capasAgua,
                    triggerInteraction))
            {
                return hit.point.y;
            }
        }

        if (agua != null)
        {
            return agua.position.y;
        }

        return posicionInicial.y;
    }

    private float DistanciaHorizontal(
        Vector3 a,
        Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void ActualizarAnimator()
    {
        bool persiguiendo = estadoActual == EstadoTiburon.PersigueJugadorNadando ||
                            estadoActual == EstadoTiburon.SeAcercaAlBarco;

        SetBoolAnimatorSiExiste(boolPersiguiendo, persiguiendo);
        SetFloatAnimatorSiExiste(parametroVelocidad, velocidadActualAnimator);
    }

    private void SetBoolAnimatorSiExiste(
        string nombreParametro,
        bool valor)
    {
        if (animatorTiburon == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorTiburon.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Bool)
            {
                animatorTiburon.SetBool(nombreParametro, valor);
                return;
            }
        }
    }

    private void SetFloatAnimatorSiExiste(
        string nombreParametro,
        float valor)
    {
        if (animatorTiburon == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorTiburon.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Float)
            {
                animatorTiburon.SetFloat(nombreParametro, valor);
                return;
            }
        }
    }

    private void SetTriggerAnimatorSiExiste(string nombreParametro)
    {
        if (animatorTiburon == null || string.IsNullOrEmpty(nombreParametro))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animatorTiburon.parameters)
        {
            if (parametro.name == nombreParametro &&
                parametro.type == AnimatorControllerParameterType.Trigger)
            {
                animatorTiburon.SetTrigger(nombreParametro);
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        Gizmos.color = Color.cyan;
        Vector3 centroPatrulla = Application.isPlaying
            ? posicionInicial
            : transform.position;
        Gizmos.DrawWireSphere(centroPatrulla, radioPatrulla);
    }
}
