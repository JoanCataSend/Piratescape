using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class BarcoNavegable : MonoBehaviour, Interactuable
{
    [Header("Referencias del barco")]
    [SerializeField] private Transform agua;
    [SerializeField] private string nombreObjetoAgua = "agua";
    [SerializeField] private Transform puntoInteraccion;
    [SerializeField] private Transform asientoJugador;
    [SerializeField] private Transform puntoSalida;

    [Header("Referencias del jugador")]
    [SerializeField] private Transform jugadorTransform;
    [SerializeField] private movimientoplayer movimientoJugador;
    [SerializeField] private CharacterController characterControllerJugador;
    [SerializeField] private Interactuador interactuadorJugador;
    [SerializeField] private PlayerPickupController recogidaJugador;
    [SerializeField] private PlayerInventoryInput inventarioInputJugador;
    [SerializeField] private Animator animatorJugador;

    [Header("Interaccion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;
    [SerializeField] private string promptMontar = "Montar";
    [SerializeField] private string promptBajar = "Bajar";

    [Header("Movimiento")]
    [SerializeField] private float velocidadAdelante = 7f;
    [SerializeField] private float velocidadAtras = 3f;
    [SerializeField] private float aceleracion = 6f;
    [SerializeField] private float frenado = 4f;
    [SerializeField] private float velocidadGiro = 85f;
    [SerializeField] private float giroMinimoSinMovimiento = 0.35f;

    [Header("Flotacion")]
    [SerializeField] private float alturaSobreAgua = 0.35f;
    [SerializeField] private float suavizadoFlotacion = 8f;
    [SerializeField] private float balanceoVertical = 0.08f;
    [SerializeField] private float velocidadBalanceo = 1.5f;
    [SerializeField] private bool mantenerBarcoVertical = true;


    [Header("Bloqueo contra isla/suelo")]
    [SerializeField] private bool bloquearMovimientoEnTierra = true;
    [SerializeField] private LayerMask capasTierra;
    [SerializeField] private bool autoConfigurarCapasTierraSiEstaVacio = true;
    [SerializeField] private bool detectarObstaculosConSphereCast = true;
    [SerializeField] private bool detectarTierraDebajoDelBarco = true;
    [SerializeField] private float radioDeteccionTierra = 0.75f;
    [SerializeField] private float distanciaAnticipacionTierra = 0.75f;
    [SerializeField] private float alturaCentroSphereCast = 0.35f;
    [SerializeField] private float alturaOrigenRaycastTierra = 3f;
    [SerializeField] private float distanciaRaycastTierra = 7f;
    [SerializeField] private float margenTierraRespectoAgua = -0.12f;
    [SerializeField] private float distanciaChequeoProaPopa = 1.7f;
    [SerializeField] private float distanciaChequeoLateral = 0.8f;
    [SerializeField] private bool ignorarTriggersTierra = true;
    [SerializeField] private bool mostrarGizmosBloqueoTierra = true;

    [Header("Animacion jugador opcional")]
    [SerializeField] private string boolSentado = "isSitting";

    private Rigidbody rb;
    private JugadorActivador jugadorActivador;
    private Transform padreOriginalJugador;

    private bool activo;
    private bool montado;
    private bool promptVisible;
    private string mensajePromptActual;

    private float velocidadActual;
    private int frameUltimaInteraccion = -1;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => interactuable && (activo || montado);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PrepararRigidbody();
        CachearAgua();
        AutoConfigurarCapasTierra();
        CachearJugador();
    }

    private void Start()
    {
        ActualizarPrompt();
    }

    private void Update()
    {
        CachearJugador();
        ActualizarEstadoInteraccion();
        ActualizarPrompt();

        if (!montado)
        {
            return;
        }

        MantenerJugadorEnAsiento();

        if (Time.frameCount == frameUltimaInteraccion)
        {
            return;
        }

        if (SeHaPulsadoInteraccion())
        {
            DesmontarJugador();
        }
    }

    private void FixedUpdate()
    {
        Vector3 posicionNueva = rb.position;
        Quaternion rotacionNueva = rb.rotation;

        if (montado)
        {
            CalcularMovimientoBarco(ref posicionNueva, ref rotacionNueva);
        }
        else
        {
            velocidadActual = Mathf.MoveTowards(
                velocidadActual,
                0f,
                frenado * Time.fixedDeltaTime
            );
        }

        AplicarFlotacion(ref posicionNueva, ref rotacionNueva);

        rb.MoveRotation(rotacionNueva);
        rb.MovePosition(posicionNueva);
    }

    private void OnDisable()
    {
        OcultarPrompt();

        if (montado)
        {
            DesmontarJugador();
        }
    }

    public void Interactuar()
    {
        if (!interactuable)
        {
            return;
        }

        frameUltimaInteraccion = Time.frameCount;

        if (montado)
        {
            DesmontarJugador();
            return;
        }

        if (!activo)
        {
            return;
        }

        MontarJugador();
    }

    private void PrepararRigidbody()
    {
        if (rb == null)
        {
            return;
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void CachearAgua()
    {
        if (agua != null)
        {
            return;
        }

        GameObject aguaEncontrada = GameObject.Find(nombreObjetoAgua);

        if (aguaEncontrada == null)
        {
            aguaEncontrada = GameObject.Find("Agua");
        }

        if (aguaEncontrada != null)
        {
            agua = aguaEncontrada.transform;
        }
    }

    private void CachearJugador()
    {
        if (jugadorTransform == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");

            if (jugador != null)
            {
                jugadorTransform = jugador.transform;
            }
        }

        if (jugadorTransform == null)
        {
            return;
        }

        if (movimientoJugador == null)
        {
            movimientoJugador = jugadorTransform.GetComponent<movimientoplayer>();
        }

        if (characterControllerJugador == null)
        {
            characterControllerJugador = jugadorTransform.GetComponent<CharacterController>();
        }

        if (interactuadorJugador == null)
        {
            interactuadorJugador = jugadorTransform.GetComponent<Interactuador>();
        }

        if (recogidaJugador == null)
        {
            recogidaJugador = jugadorTransform.GetComponent<PlayerPickupController>();
        }

        if (inventarioInputJugador == null)
        {
            inventarioInputJugador = jugadorTransform.GetComponent<PlayerInventoryInput>();
        }

        if (animatorJugador == null)
        {
            animatorJugador = jugadorTransform.GetComponentInChildren<Animator>();
        }

        if (jugadorActivador == null)
        {
            jugadorActivador = jugadorTransform.GetComponent<JugadorActivador>();
        }
    }

    private void ActualizarEstadoInteraccion()
    {
        if (montado)
        {
            activo = true;
            return;
        }

        if (!interactuable || jugadorTransform == null)
        {
            activo = false;
            return;
        }

        Vector3 origen = puntoInteraccion != null
            ? puntoInteraccion.position
            : transform.position;

        Vector3 posicionJugador = jugadorActivador != null
            ? jugadorActivador.Position
            : jugadorTransform.position;

        activo = Vector3.Distance(origen, posicionJugador) <= rango;
    }

    private void ActualizarPrompt()
    {
        bool debeMostrarPrompt = interactuable && (activo || montado);
        string mensaje = montado ? promptBajar : promptMontar;

        if (!debeMostrarPrompt)
        {
            OcultarPrompt();
            return;
        }

        if (promptVisible && mensajePromptActual == mensaje)
        {
            return;
        }

        if (InteractionUI.Instance == null)
        {
            return;
        }

        mensajePromptActual = mensaje;
        promptVisible = true;
        InteractionUI.Instance.Show(this, mensaje);
    }

    private void OcultarPrompt()
    {
        if (!promptVisible)
        {
            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide(this);
        }

        promptVisible = false;
        mensajePromptActual = null;
    }

    private void MontarJugador()
    {
        CachearJugador();

        if (jugadorTransform == null)
        {
            Debug.LogWarning("BarcoNavegable: no se ha encontrado el jugador.", this);
            return;
        }

        Transform asiento = asientoJugador != null ? asientoJugador : transform;

        padreOriginalJugador = jugadorTransform.parent;
        montado = true;
        velocidadActual = 0f;

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = false;
        }

        if (interactuadorJugador != null)
        {
            interactuadorJugador.enabled = false;
        }

        if (recogidaJugador != null)
        {
            recogidaJugador.enabled = false;
        }

        if (inventarioInputJugador != null)
        {
            inventarioInputJugador.enabled = false;
        }

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = false;
        }

        jugadorTransform.SetParent(asiento, true);
        jugadorTransform.position = asiento.position;
        jugadorTransform.rotation = asiento.rotation;

        PonerBoolAnimador(boolSentado, true);
        PonerBoolAnimador("isWalking", false);
        PonerBoolAnimador("isRunning", false);

        ActualizarPrompt();
    }

    private void DesmontarJugador()
    {
        if (jugadorTransform == null)
        {
            montado = false;
            return;
        }

        Transform salida = puntoSalida != null ? puntoSalida : null;
        Vector3 posicionSalida = salida != null
            ? salida.position
            : transform.position - transform.right * 1.6f + Vector3.up * 0.1f;

        Quaternion rotacionSalida = salida != null
            ? salida.rotation
            : Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = false;
        }

        jugadorTransform.SetParent(padreOriginalJugador, true);
        jugadorTransform.position = posicionSalida;
        jugadorTransform.rotation = rotacionSalida;

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = true;
        }

        if (movimientoJugador != null)
        {
            movimientoJugador.enabled = true;
        }

        if (interactuadorJugador != null)
        {
            interactuadorJugador.enabled = true;
        }

        if (recogidaJugador != null)
        {
            recogidaJugador.enabled = true;
        }

        if (inventarioInputJugador != null)
        {
            inventarioInputJugador.enabled = true;
        }

        PonerBoolAnimador(boolSentado, false);

        montado = false;
        velocidadActual = 0f;
        activo = false;

        OcultarPrompt();
    }

    private void MantenerJugadorEnAsiento()
    {
        if (jugadorTransform == null || asientoJugador == null)
        {
            return;
        }

        jugadorTransform.position = asientoJugador.position;
        jugadorTransform.rotation = asientoJugador.rotation;
    }

    private void CalcularMovimientoBarco(ref Vector3 posicionNueva, ref Quaternion rotacionNueva)
    {
        Vector2 input = LeerInputNavegacion();

        float objetivoVelocidad = 0f;

        if (input.y > 0.05f)
        {
            objetivoVelocidad = input.y * velocidadAdelante;
        }
        else if (input.y < -0.05f)
        {
            objetivoVelocidad = input.y * velocidadAtras;
        }

        float cambioVelocidad = Mathf.Abs(objetivoVelocidad) > 0.01f
            ? aceleracion
            : frenado;

        velocidadActual = Mathf.MoveTowards(
            velocidadActual,
            objetivoVelocidad,
            cambioVelocidad * Time.fixedDeltaTime
        );

        float factorGiro = Mathf.Max(
            Mathf.Abs(velocidadActual) / Mathf.Max(velocidadAdelante, 0.01f),
            giroMinimoSinMovimiento
        );

        float giro = input.x * velocidadGiro * factorGiro * Time.fixedDeltaTime;
        rotacionNueva *= Quaternion.Euler(0f, giro, 0f);

        Vector3 direccionAvance = rotacionNueva * Vector3.forward;
        Vector3 desplazamiento = direccionAvance * velocidadActual * Time.fixedDeltaTime;
        Vector3 posicionObjetivo = posicionNueva + desplazamiento;

        if (MovimientoBloqueadoPorTierra(posicionNueva, posicionObjetivo, rotacionNueva, Mathf.Sign(velocidadActual)))
        {
            velocidadActual = 0f;
            return;
        }

        posicionNueva = posicionObjetivo;
    }

    private void AutoConfigurarCapasTierra()
    {
        if (!autoConfigurarCapasTierraSiEstaVacio || capasTierra.value != 0)
        {
            return;
        }

        string[] nombresCapasProbables =
        {
            "Suelo",
            "Ground",
            "Isla",
            "Island",
            "Terrain",
            "Terreno",
            "Mapa",
            "Arena",
            "Grass",
            "Cesped"
        };

        int mascara = 0;

        for (int i = 0; i < nombresCapasProbables.Length; i++)
        {
            int capa = LayerMask.NameToLayer(nombresCapasProbables[i]);

            if (capa >= 0)
            {
                mascara |= 1 << capa;
            }
        }

        if (mascara != 0)
        {
            capasTierra = mascara;
        }
    }

    private bool MovimientoBloqueadoPorTierra(
        Vector3 posicionActual,
        Vector3 posicionObjetivo,
        Quaternion rotacionObjetivo,
        float signoAvance
    )
    {
        if (!bloquearMovimientoEnTierra || capasTierra.value == 0)
        {
            return false;
        }

        Vector3 desplazamientoHorizontal = posicionObjetivo - posicionActual;
        desplazamientoHorizontal.y = 0f;

        if (desplazamientoHorizontal.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        if (detectarObstaculosConSphereCast && HayObstaculoTierraEnTrayecto(posicionActual, desplazamientoHorizontal))
        {
            return true;
        }

        if (detectarTierraDebajoDelBarco && HayTierraDebajoDeLaHuella(posicionObjetivo, rotacionObjetivo, signoAvance))
        {
            return true;
        }

        return false;
    }

    private bool HayObstaculoTierraEnTrayecto(Vector3 posicionActual, Vector3 desplazamientoHorizontal)
    {
        Vector3 origen = posicionActual + Vector3.up * alturaCentroSphereCast;
        Vector3 direccion = desplazamientoHorizontal.normalized;
        float distancia = desplazamientoHorizontal.magnitude + distanciaAnticipacionTierra;

        RaycastHit[] impactos = Physics.SphereCastAll(
            origen,
            radioDeteccionTierra,
            direccion,
            distancia,
            capasTierra,
            ObtenerModoTriggersTierra()
        );

        for (int i = 0; i < impactos.Length; i++)
        {
            Collider colliderImpactado = impactos[i].collider;

            if (ColliderDebeIgnorarse(colliderImpactado))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool HayTierraDebajoDeLaHuella(Vector3 posicionObjetivo, Quaternion rotacionObjetivo, float signoAvance)
    {
        Vector3 adelante = rotacionObjetivo * Vector3.forward;
        Vector3 derecha = rotacionObjetivo * Vector3.right;

        Vector3 centro = posicionObjetivo;
        Vector3 extremo = signoAvance >= 0f
            ? posicionObjetivo + adelante * distanciaChequeoProaPopa
            : posicionObjetivo - adelante * distanciaChequeoProaPopa;

        if (HayTierraDebajoDelPunto(centro) || HayTierraDebajoDelPunto(extremo))
        {
            return true;
        }

        if (HayTierraDebajoDelPunto(extremo + derecha * distanciaChequeoLateral) ||
            HayTierraDebajoDelPunto(extremo - derecha * distanciaChequeoLateral))
        {
            return true;
        }

        return false;
    }

    private bool HayTierraDebajoDelPunto(Vector3 punto)
    {
        Vector3 origen = punto + Vector3.up * alturaOrigenRaycastTierra;

        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            Vector3.down,
            distanciaRaycastTierra,
            capasTierra,
            ObtenerModoTriggersTierra()
        );

        float alturaAgua = agua != null ? agua.position.y : transform.position.y;
        float alturaMinimaBloqueo = alturaAgua + margenTierraRespectoAgua;

        for (int i = 0; i < impactos.Length; i++)
        {
            Collider colliderImpactado = impactos[i].collider;

            if (ColliderDebeIgnorarse(colliderImpactado))
            {
                continue;
            }

            if (impactos[i].point.y >= alturaMinimaBloqueo)
            {
                return true;
            }
        }

        return false;
    }

    private QueryTriggerInteraction ObtenerModoTriggersTierra()
    {
        return ignorarTriggersTierra
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;
    }

    private bool ColliderDebeIgnorarse(Collider colliderImpactado)
    {
        if (colliderImpactado == null)
        {
            return true;
        }

        Transform transformImpactado = colliderImpactado.transform;

        if (transformImpactado == transform || transformImpactado.IsChildOf(transform))
        {
            return true;
        }

        if (jugadorTransform != null &&
            (transformImpactado == jugadorTransform || transformImpactado.IsChildOf(jugadorTransform)))
        {
            return true;
        }

        if (agua != null &&
            (transformImpactado == agua || transformImpactado.IsChildOf(agua)))
        {
            return true;
        }

        return false;
    }

    private Vector2 LeerInputNavegacion()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick.sqrMagnitude > input.sqrMagnitude)
            {
                input = stick;
            }
        }

        return Vector2.ClampMagnitude(input, 1f);
    }

    private void AplicarFlotacion(ref Vector3 posicionNueva, ref Quaternion rotacionNueva)
    {
        float alturaAgua = agua != null ? agua.position.y : transform.position.y;
        float balanceo = Mathf.Sin(Time.time * velocidadBalanceo) * balanceoVertical;
        float alturaObjetivo = alturaAgua + alturaSobreAgua + balanceo;

        posicionNueva.y = Mathf.Lerp(
            posicionNueva.y,
            alturaObjetivo,
            suavizadoFlotacion * Time.fixedDeltaTime
        );

        if (mantenerBarcoVertical)
        {
            rotacionNueva = Quaternion.Euler(0f, rotacionNueva.eulerAngles.y, 0f);
        }
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void PonerBoolAnimador(string nombreParametro, bool valor)
    {
        if (animatorJugador == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        AnimatorControllerParameter[] parametros = animatorJugador.parameters;

        for (int i = 0; i < parametros.Length; i++)
        {
            if (parametros[i].type == AnimatorControllerParameterType.Bool &&
                parametros[i].name == nombreParametro)
            {
                animatorJugador.SetBool(nombreParametro, valor);
                return;
            }
        }
    }

    private void DibujarGizmosBloqueoTierra()
    {
        Quaternion rotacion = Application.isPlaying && rb != null ? rb.rotation : transform.rotation;
        Vector3 posicion = Application.isPlaying && rb != null ? rb.position : transform.position;
        Vector3 adelante = rotacion * Vector3.forward;
        Vector3 derecha = rotacion * Vector3.right;
        Vector3 proa = posicion + adelante * distanciaChequeoProaPopa;
        Vector3 popa = posicion - adelante * distanciaChequeoProaPopa;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(posicion + Vector3.up * alturaCentroSphereCast, radioDeteccionTierra);
        Gizmos.DrawLine(
            posicion + Vector3.up * alturaCentroSphereCast,
            posicion + Vector3.up * alturaCentroSphereCast + adelante * distanciaAnticipacionTierra
        );

        Gizmos.color = Color.magenta;
        DibujarPuntoRaycastTierra(posicion);
        DibujarPuntoRaycastTierra(proa);
        DibujarPuntoRaycastTierra(popa);
        DibujarPuntoRaycastTierra(proa + derecha * distanciaChequeoLateral);
        DibujarPuntoRaycastTierra(proa - derecha * distanciaChequeoLateral);
        DibujarPuntoRaycastTierra(popa + derecha * distanciaChequeoLateral);
        DibujarPuntoRaycastTierra(popa - derecha * distanciaChequeoLateral);
    }

    private void DibujarPuntoRaycastTierra(Vector3 punto)
    {
        Vector3 origen = punto + Vector3.up * alturaOrigenRaycastTierra;
        Gizmos.DrawLine(origen, origen + Vector3.down * distanciaRaycastTierra);
        Gizmos.DrawWireSphere(punto, 0.12f);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centro = puntoInteraccion != null ? puntoInteraccion.position : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centro, rango);

        if (mostrarGizmosBloqueoTierra)
        {
            DibujarGizmosBloqueoTierra();
        }

        if (asientoJugador != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(asientoJugador.position, 0.12f);
        }

        if (puntoSalida != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(puntoSalida.position, 0.12f);
        }
    }
}
