using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;


[RequireComponent(typeof(CharacterController))]
public class movimientoplayer : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Configuración de Rotación")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animación")]
    [SerializeField] private Animator animator;

    [Header("Audio de salto")]
    [SerializeField] private PlayerJumpAudio playerJumpAudio;

    [Header("Natación / Agua")]
    [SerializeField] private bool activarNatacion = true;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private bool autoConfigurarWaterLayerSiEstaVacio = true;
    [SerializeField] private float swimSpeed = 3.2f;
    [SerializeField] private bool permitirSprintNadando = false;
    [SerializeField] private float sprintMultiplierNadando = 1.25f;
    [SerializeField] private float alturaPivotSobreAgua = 0.05f;
    [Tooltip("Valor extra que solo se aplica cuando el jugador esta quieto flotando. Usa valores negativos para hundir un poco mas el cuerpo sin afectar a la animacion de nadar.")]
    [SerializeField] private float hundimientoExtraAlFlotarQuieto = -0.18f;
    [SerializeField] private float suavizadoAlturaAgua = 14f;
    [SerializeField] private float margenEntradaAgua = 0.25f;
    [Range(0.1f, 0.95f)]
    [SerializeField] private float porcentajeAlturaParaEmpezarANadar = 0.5f;
    [SerializeField] private float margenActivacionNatacion = 0.05f;
    [SerializeField] private float alturaOrigenRaycastAgua = 2.5f;
    [SerializeField] private float distanciaRaycastAgua = 5f;
    [SerializeField] private float tiempoSinAguaParaSalir = 0.15f;
    [SerializeField] private bool detectarTriggersAgua = true;
    [SerializeField] private bool ignorarColisionFisicaConAgua = true;
    [SerializeField] private bool mantenerMitadCuerpoEnSuperficieAlNadar = true;
    [SerializeField] private float velocidadVerticalMaximaNatacion = 2.5f;

    [Header("Suelo que bloquea el agua")]
    [SerializeField] private LayerMask capasSueloBloqueanAgua;
    [SerializeField] private bool autoConfigurarCapasSueloBloqueanAgua = true;
    [SerializeField] private bool ignorarTriggersSueloBloqueoAgua = true;

    [Header("Animator natación directo")]
    [SerializeField] private bool forzarEstadosAnimatorNatacion = true;
    [SerializeField] private string estadoAnimatorFlotar = "Treading Water";
    [SerializeField] private string estadoAnimatorNadar = "Swimming";
    [SerializeField] private string estadoAnimatorIdle = "Idle";
    [SerializeField] private string estadoAnimatorWalk = "Walk";
    [SerializeField] private string estadoAnimatorRun = "Running";
    [SerializeField] private float duracionCrossFadeNatacion = 0.08f;

    [Header("Animación de natación")]
    [SerializeField] private string parametroAnimacionNadando = "isSwimming";
    [SerializeField] private string parametroAnimacionNadandoMovimiento = "isSwimmingMoving";
    [SerializeField] private string triggerEntrarAgua = "enterWater";
    [SerializeField] private string triggerSalirAgua = "exitWater";

    [Header("Audio de agua / natación")]
    [SerializeField] private AudioSource audioSourceEntradaAgua;
    [SerializeField] private AudioSource audioSourceLoopNatacion;
    [SerializeField] private AudioMixerGroup outputAguaJugador;
    [SerializeField] private AudioClip[] sonidosEntradaAgua;
    [SerializeField] private AudioClip sonidoLoopNatacion;
    [Range(0f, 1f)]
    [SerializeField] private float volumenEntradaAgua = 0.75f;
    [Range(0f, 1f)]
    [SerializeField] private float volumenLoopNatacion = 0.45f;
    [SerializeField] private bool loopNatacionSoloSiSeMueve = true;
    [SerializeField] private float pitchAguaMinimo = 0.96f;
    [SerializeField] private float pitchAguaMaximo = 1.04f;

    [Header("Estado afectado por energia")]
    [SerializeField] private float energySpeedMultiplier = 1f;
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool isTired = false;

    [Header("Estado especial")]
    [SerializeField] private bool isFainted = false;
    [SerializeField] private bool isPickingUp = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector2 moveInput;
    private bool jumpPressed;

    private bool isSwimming;
    private bool estaTocandoAgua;
    private float alturaSuperficieAguaActual;
    private float tiempoSinDetectarAgua;
    private Collider aguaActual;
    private int ultimoIndiceEntradaAgua = -1;
    private int ultimoEstadoAnimatorForzado;
    private bool ultimoFrameAnimatorEnNatacion;
    private bool forzarRefrescoAnimatorNatacion;

    private bool saltoEnCurso;
    private bool estuvoEnElAire;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null &&
            Camera.main != null)
        {
            cameraTransform =
                Camera.main.transform;
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (playerJumpAudio == null)
        {
            playerJumpAudio =
                GetComponent<PlayerJumpAudio>();
        }

        ConfigurarLayerAgua();
        ConfigurarCapasSueloBloqueanAgua();
        PrepararAudioAgua();

        CrearInputs();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();

        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJumpPerformed;

        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();

        DetenerLoopNatacion();
    }

    private void Start()
    {
        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        if (isFainted)
        {
            moveInput = Vector2.zero;
            GestionarLoopNatacion();
            ActualizarAnimaciones();
            return;
        }

        LeerMovimientoActual();
        ActualizarEstadoAgua();

        bool estabaEnSuelo =
            groundedPlayer;

        groundedPlayer =
            controller.isGrounded;

        if (!isSwimming)
        {
            ComprobarAterrizaje(
                estabaEnSuelo,
                groundedPlayer
            );

            if (groundedPlayer &&
                playerVelocity.y < 0f)
            {
                playerVelocity.y = -2f;
            }
        }

        if (isPickingUp)
        {
            moveInput = Vector2.zero;
            jumpPressed = false;

            if (isSwimming)
            {
                MantenerAlturaNatacion();
                GestionarLoopNatacion();
                ActualizarAnimaciones();
                return;
            }

            AplicarGravedad();
            ActualizarAnimaciones();
            return;
        }

        if (isSwimming)
        {
            MoverNadando();
            MantenerAlturaNatacion();
            jumpPressed = false;
            GestionarLoopNatacion();
            ActualizarAnimaciones();
            return;
        }

        MoverJugador();
        Saltar();
        AplicarGravedad();
        GestionarLoopNatacion();
        ActualizarAnimaciones();
    }

    private void CrearInputs()
    {
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value
        );

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding(
            "<Gamepad>/leftStick"
        );

        moveAction.AddBinding(
            "<Gamepad>/dpad"
        );

        jumpAction = new InputAction(
            name: "Jump",
            type: InputActionType.Button
        );

        jumpAction.AddBinding(
            "<Keyboard>/space"
        );

        jumpAction.AddBinding(
            "<Gamepad>/buttonSouth"
        );

        sprintAction = new InputAction(
            name: "Sprint",
            type: InputActionType.Value
        );

        sprintAction.AddBinding(
            "<Keyboard>/leftShift"
        );

        sprintAction.AddBinding(
            "<Gamepad>/leftStickPress"
        );

        sprintAction.AddBinding(
            "<Gamepad>/rightTrigger"
        );
    }

    private void LeerMovimientoActual()
    {
        if (moveAction != null &&
            moveAction.enabled)
        {
            moveInput =
                moveAction.ReadValue<Vector2>();
        }
    }

    private void OnMovePerformed(
        InputAction.CallbackContext context)
    {
        if (isFainted || isPickingUp)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput =
            context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(
        InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(
        InputAction.CallbackContext context)
    {
        if (isFainted || isPickingUp || isSwimming)
        {
            return;
        }

        jumpPressed = true;
    }

    private void MoverJugador()
    {
        Vector3 direction =
            new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            );

        if (direction.magnitude < 0.1f)
        {
            return;
        }

        direction.Normalize();

        float cameraY =
            cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : 0f;

        float targetAngle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg +
            cameraY;

        float smoothAngle =
            Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

        transform.rotation =
            Quaternion.Euler(
                0f,
                smoothAngle,
                0f
            );

        Vector3 moveDir =
            Quaternion.Euler(
                0f,
                targetAngle,
                0f
            ) *
            Vector3.forward;

        bool isTryingToSprint =
            sprintAction != null &&
            sprintAction.ReadValue<float>() > 0.5f;

        bool isSprinting =
            isTryingToSprint &&
            canSprint &&
            !isTired;

        float currentSpeed =
            isSprinting
                ? playerSpeed * sprintMultiplier
                : playerSpeed;

        currentSpeed *=
            energySpeedMultiplier;

        controller.Move(
            moveDir.normalized *
            currentSpeed *
            Time.deltaTime
        );
    }

    private void MoverNadando()
    {
        Vector3 direction =
            new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            );

        if (direction.magnitude < 0.1f)
        {
            return;
        }

        direction.Normalize();

        float cameraY =
            cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : 0f;

        float targetAngle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) *
            Mathf.Rad2Deg +
            cameraY;

        float smoothAngle =
            Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );

        transform.rotation =
            Quaternion.Euler(
                0f,
                smoothAngle,
                0f
            );

        Vector3 moveDir =
            Quaternion.Euler(
                0f,
                targetAngle,
                0f
            ) *
            Vector3.forward;

        bool isTryingToSprint =
            sprintAction != null &&
            sprintAction.ReadValue<float>() > 0.5f;

        bool isSprinting =
            permitirSprintNadando &&
            isTryingToSprint &&
            canSprint &&
            !isTired;

        float currentSpeed =
            isSprinting
                ? swimSpeed * sprintMultiplierNadando
                : swimSpeed;

        currentSpeed *=
            energySpeedMultiplier;

        controller.Move(
            moveDir.normalized *
            currentSpeed *
            Time.deltaTime
        );
    }

    private void Saltar()
    {
        if (jumpPressed &&
            groundedPlayer &&
            !isSwimming)
        {
            playerVelocity.y =
                Mathf.Sqrt(
                    jumpHeight *
                    -2f *
                    gravityValue
                );

            saltoEnCurso = true;
            estuvoEnElAire = false;

            if (playerJumpAudio != null)
            {
                playerJumpAudio
                    .ReproducirInicioSalto();
            }

            if (animator != null)
            {
                animator.SetTrigger("jump");
            }
        }

        jumpPressed = false;
    }

    private void ComprobarAterrizaje(
        bool estabaEnSuelo,
        bool estaEnSueloAhora)
    {
        if (!saltoEnCurso)
        {
            return;
        }

        if (!estaEnSueloAhora)
        {
            estuvoEnElAire = true;
            return;
        }

        if (estuvoEnElAire &&
            !estabaEnSuelo &&
            estaEnSueloAhora)
        {
            saltoEnCurso = false;
            estuvoEnElAire = false;

            if (playerJumpAudio != null)
            {
                playerJumpAudio
                    .ReproducirAterrizaje();
            }
        }
    }

    private void AplicarGravedad()
    {
        if (isSwimming)
        {
            playerVelocity.y = 0f;
            return;
        }

        playerVelocity.y +=
            gravityValue *
            Time.deltaTime;

        controller.Move(
            playerVelocity *
            Time.deltaTime
        );
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null)
        {
            return;
        }

        bool isWalking =
            moveInput.magnitude > 0.1f &&
            !isSwimming &&
            !isFainted &&
            !isPickingUp;

        bool isTryingToSprint =
            sprintAction != null &&
            sprintAction.ReadValue<float>() > 0.5f;

        bool isRunning =
            isWalking &&
            isTryingToSprint &&
            canSprint &&
            !isTired &&
            !isFainted &&
            !isPickingUp &&
            !isSwimming;

        bool isSwimmingMoving =
            isSwimming &&
            moveInput.magnitude > 0.1f &&
            !isFainted &&
            !isPickingUp;

        animator.SetBool(
            "isWalking",
            isWalking
        );

        animator.SetBool(
            "isRunning",
            isRunning
        );

        animator.SetBool(
            "isTired",
            isTired
        );

        animator.SetBool(
            "isFainted",
            isFainted
        );

        SetAnimatorBoolIfExists(
            parametroAnimacionNadando,
            isSwimming
        );

        SetAnimatorBoolIfExists(
            parametroAnimacionNadandoMovimiento,
            isSwimmingMoving
        );

        ActualizarEstadosAnimatorNatacionForzados(
            isSwimmingMoving,
            isWalking,
            isRunning
        );
    }

    private void ActualizarEstadoAgua()
    {
        if (!activarNatacion)
        {
            if (isSwimming)
            {
                SalirDeModoNatacion(true);
            }
            else if (estaTocandoAgua)
            {
                SalirDeContactoAgua();
            }

            return;
        }

        bool aguaDetectada =
            DetectarSuperficieAgua(
                out float alturaAgua,
                out Collider colliderAgua
            );

        if (aguaDetectada)
        {
            tiempoSinDetectarAgua = 0f;
            alturaSuperficieAguaActual = alturaAgua;
            aguaActual = colliderAgua;

            if (!estaTocandoAgua)
            {
                EntrarEnContactoAgua();
            }

            bool profundidadSuficienteParaNadar =
                AguaSuperaMitadDelJugador(alturaAgua);

            if (profundidadSuficienteParaNadar)
            {
                if (!isSwimming)
                {
                    EntrarEnModoNatacion();
                }
            }
            else if (isSwimming)
            {
                SalirDeModoNatacion(false);
            }

            return;
        }

        if (!estaTocandoAgua &&
            !isSwimming)
        {
            return;
        }

        tiempoSinDetectarAgua +=
            Time.deltaTime;

        if (tiempoSinDetectarAgua <
            tiempoSinAguaParaSalir)
        {
            return;
        }

        if (isSwimming)
        {
            SalirDeModoNatacion(true);
        }
        else
        {
            SalirDeContactoAgua();
        }
    }

    private bool DetectarSuperficieAgua(
        out float alturaAgua,
        out Collider colliderAgua)
    {
        alturaAgua = 0f;
        colliderAgua = null;

        if (waterLayer.value == 0)
        {
            return false;
        }

        Vector3 origen =
            transform.position +
            Vector3.up * alturaOrigenRaycastAgua;

        QueryTriggerInteraction triggerMode =
            detectarTriggersAgua
                ? QueryTriggerInteraction.Collide
                : QueryTriggerInteraction.Ignore;

        RaycastHit? hitAgua =
            ObtenerPrimerHitAguaSinSueloDelante(
                origen,
                triggerMode
            );

        if (!hitAgua.HasValue)
        {
            return false;
        }

        RaycastHit hit = hitAgua.Value;

        float piesJugador =
            controller != null
                ? controller.bounds.min.y
                : transform.position.y;

        bool tocaAgua =
            hit.point.y >=
            piesJugador - margenEntradaAgua;

        if (!tocaAgua)
        {
            return false;
        }

        if (ignorarColisionFisicaConAgua &&
            controller != null &&
            hit.collider != null)
        {
            Physics.IgnoreCollision(
                controller,
                hit.collider,
                true
            );
        }

        alturaAgua = hit.point.y;
        colliderAgua = hit.collider;

        return true;
    }

    private RaycastHit? ObtenerPrimerHitAguaSinSueloDelante(
        Vector3 origen,
        QueryTriggerInteraction triggerMode)
    {
        int mascaraBusqueda =
            waterLayer.value | capasSueloBloqueanAgua.value;

        if (capasSueloBloqueanAgua.value == 0)
        {
            if (Physics.Raycast(
                    origen,
                    Vector3.down,
                    out RaycastHit hitSoloAgua,
                    distanciaRaycastAgua,
                    waterLayer,
                    triggerMode))
            {
                return hitSoloAgua;
            }

            return null;
        }

        RaycastHit[] hits =
            Physics.RaycastAll(
                origen,
                Vector3.down,
                distanciaRaycastAgua,
                mascaraBusqueda,
                triggerMode
            );

        if (hits == null ||
            hits.Length == 0)
        {
            return null;
        }

        Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider =
                hits[i].collider;

            if (hitCollider == null ||
                EsColliderDelPropioJugador(hitCollider))
            {
                continue;
            }

            bool esAgua =
                CapaEstaEnMascara(
                    hitCollider.gameObject.layer,
                    waterLayer
                );

            bool esSueloBloqueante =
                CapaEstaEnMascara(
                    hitCollider.gameObject.layer,
                    capasSueloBloqueanAgua
                );

            if (esAgua)
            {
                return hits[i];
            }

            if (esSueloBloqueante)
            {
                if (ignorarTriggersSueloBloqueoAgua &&
                    hitCollider.isTrigger)
                {
                    continue;
                }

                return null;
            }
        }

        return null;
    }

    private bool EsColliderDelPropioJugador(
        Collider hitCollider)
    {
        if (hitCollider == null)
        {
            return false;
        }

        Transform hitTransform =
            hitCollider.transform;

        return
            hitTransform == transform ||
            hitTransform.IsChildOf(transform) ||
            transform.IsChildOf(hitTransform);
    }

    private bool CapaEstaEnMascara(
        int layer,
        LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private bool AguaSuperaMitadDelJugador(
        float alturaAgua)
    {
        if (controller == null)
        {
            return alturaAgua >=
                   transform.position.y;
        }

        Bounds boundsJugador =
            controller.bounds;

        float alturaReferenciaNatacion =
            boundsJugador.min.y +
            boundsJugador.size.y *
            porcentajeAlturaParaEmpezarANadar;

        float margenSalida =
            isSwimming
                ? Mathf.Abs(margenActivacionNatacion)
                : 0f;

        return alturaAgua >=
               alturaReferenciaNatacion -
               margenSalida;
    }

    private void EntrarEnContactoAgua()
    {
        estaTocandoAgua = true;
        ReproducirEntradaAgua();
    }

    private void SalirDeContactoAgua()
    {
        estaTocandoAgua = false;
        tiempoSinDetectarAgua = 0f;
        aguaActual = null;
        DetenerLoopNatacion();
    }

    private void EntrarEnModoNatacion()
    {
        isSwimming = true;
        jumpPressed = false;
        playerVelocity = Vector3.zero;
        saltoEnCurso = false;
        estuvoEnElAire = false;

        ReiniciarControlAnimatorNatacion();

        bool estaMoviendoseNadando =
            moveInput.magnitude > 0.1f &&
            !isFainted &&
            !isPickingUp;

        AplicarParametrosAnimatorNatacionInmediatos(
            true,
            estaMoviendoseNadando
        );

        SetAnimatorTriggerIfExists(
            triggerEntrarAgua
        );

        ForzarEstadoAnimatorNatacionActual(
            estaMoviendoseNadando
        );

        GestionarLoopNatacion();
    }

    private void SalirDeModoNatacion(
        bool limpiarContactoAgua)
    {
        isSwimming = false;
        tiempoSinDetectarAgua = 0f;
        playerVelocity = Vector3.zero;

        AplicarParametrosAnimatorNatacionInmediatos(
            false,
            false
        );

        ReiniciarControlAnimatorNatacion();

        SetAnimatorTriggerIfExists(
            triggerSalirAgua
        );

        ForzarEstadoAnimatorSalidaAguaActual();

        DetenerLoopNatacion();

        if (limpiarContactoAgua)
        {
            estaTocandoAgua = false;
            aguaActual = null;
        }
    }

    private void EntrarEnAgua()
    {
        if (!estaTocandoAgua)
        {
            EntrarEnContactoAgua();
        }

        EntrarEnModoNatacion();
    }

    private void SalirDelAgua()
    {
        SalirDeModoNatacion(true);
    }

    private void MantenerAlturaNatacion()
    {
        if (controller == null)
        {
            return;
        }

        Bounds boundsJugador =
            controller.bounds;

        float alturaActualReferencia;

        if (mantenerMitadCuerpoEnSuperficieAlNadar)
        {
            alturaActualReferencia =
                boundsJugador.min.y +
                boundsJugador.size.y *
                porcentajeAlturaParaEmpezarANadar;
        }
        else
        {
            alturaActualReferencia =
                transform.position.y;
        }

        bool estaMoviendoseNadando =
            moveInput.sqrMagnitude > 0.01f &&
            !isFainted &&
            !isPickingUp;

        float offsetFlotandoQuieto =
            estaMoviendoseNadando
                ? 0f
                : hundimientoExtraAlFlotarQuieto;

        float alturaObjetivoReferencia =
            alturaSuperficieAguaActual +
            alturaPivotSobreAgua +
            offsetFlotandoQuieto;

        float diferencia =
            alturaObjetivoReferencia -
            alturaActualReferencia;

        float factorSuavizado =
            1f - Mathf.Exp(
                -suavizadoAlturaAgua * Time.deltaTime
            );

        float paso =
            diferencia * factorSuavizado;

        if (velocidadVerticalMaximaNatacion > 0f)
        {
            float maxPaso =
                velocidadVerticalMaximaNatacion *
                Time.deltaTime;

            paso =
                Mathf.Clamp(
                    paso,
                    -maxPaso,
                    maxPaso
                );
        }

        if (Mathf.Abs(paso) < 0.001f)
        {
            return;
        }

        controller.Move(
            Vector3.up * paso
        );

        playerVelocity.y = 0f;
    }

    private void ConfigurarLayerAgua()
    {
        if (!autoConfigurarWaterLayerSiEstaVacio ||
            waterLayer.value != 0)
        {
            return;
        }

        AgregarLayerAguaSiExiste("Agua");
        AgregarLayerAguaSiExiste("Water");
        AgregarLayerAguaSiExiste("agua");
        AgregarLayerAguaSiExiste("water");
    }

    private void AgregarLayerAguaSiExiste(
        string nombreLayer)
    {
        int layer =
            LayerMask.NameToLayer(nombreLayer);

        if (layer < 0)
        {
            return;
        }

        waterLayer.value |=
            1 << layer;
    }

    private void ConfigurarCapasSueloBloqueanAgua()
    {
        if (!autoConfigurarCapasSueloBloqueanAgua ||
            capasSueloBloqueanAgua.value != 0)
        {
            return;
        }

        AgregarLayerSueloBloqueoSiExiste("Suelo");
        AgregarLayerSueloBloqueoSiExiste("Ground");
        AgregarLayerSueloBloqueoSiExiste("Terrain");
        AgregarLayerSueloBloqueoSiExiste("Isla");
        AgregarLayerSueloBloqueoSiExiste("suelo");
        AgregarLayerSueloBloqueoSiExiste("ground");
        AgregarLayerSueloBloqueoSiExiste("terrain");
        AgregarLayerSueloBloqueoSiExiste("isla");
    }

    private void AgregarLayerSueloBloqueoSiExiste(
        string nombreLayer)
    {
        int layer =
            LayerMask.NameToLayer(nombreLayer);

        if (layer < 0)
        {
            return;
        }

        capasSueloBloqueanAgua.value |=
            1 << layer;
    }

    private void PrepararAudioAgua()
    {
        if (audioSourceEntradaAgua == null)
        {
            audioSourceEntradaAgua =
                CrearAudioSourceAgua(false);
        }

        if (audioSourceLoopNatacion == null)
        {
            audioSourceLoopNatacion =
                CrearAudioSourceAgua(true);
        }

        ConfigurarAudioSourceAgua(
            audioSourceEntradaAgua,
            false
        );

        ConfigurarAudioSourceAgua(
            audioSourceLoopNatacion,
            true
        );
    }

    private AudioSource CrearAudioSourceAgua(
        bool loop)
    {
        AudioSource source =
            gameObject.AddComponent<AudioSource>();

        source.loop = loop;

        return source;
    }

    private void ConfigurarAudioSourceAgua(
        AudioSource source,
        bool loop)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = 16f;
        source.outputAudioMixerGroup = outputAguaJugador;
    }

    private void ReproducirEntradaAgua()
    {
        AudioClip clip =
            ObtenerClipAguaAleatorioSinRepetir(
                sonidosEntradaAgua,
                ref ultimoIndiceEntradaAgua
            );

        ReproducirClipAgua(
            audioSourceEntradaAgua,
            clip,
            volumenEntradaAgua
        );
    }

    private void GestionarLoopNatacion()
    {
        if (audioSourceLoopNatacion == null ||
            sonidoLoopNatacion == null)
        {
            return;
        }

        bool debeSonar =
            isSwimming &&
            (!loopNatacionSoloSiSeMueve ||
             moveInput.magnitude > 0.1f) &&
            !isFainted;

        if (debeSonar)
        {
            if (audioSourceLoopNatacion.clip !=
                sonidoLoopNatacion)
            {
                audioSourceLoopNatacion.clip =
                    sonidoLoopNatacion;
            }

            audioSourceLoopNatacion.volume =
                volumenLoopNatacion;

            if (!audioSourceLoopNatacion.isPlaying)
            {
                audioSourceLoopNatacion.Play();
            }

            return;
        }

        DetenerLoopNatacion();
    }

    private void DetenerLoopNatacion()
    {
        if (audioSourceLoopNatacion == null ||
            !audioSourceLoopNatacion.isPlaying)
        {
            return;
        }

        audioSourceLoopNatacion.Stop();
    }

    private void ReproducirClipAgua(
        AudioSource source,
        AudioClip clip,
        float volumen)
    {
        if (source == null || clip == null)
        {
            return;
        }

        float pitchMenor =
            Mathf.Min(
                pitchAguaMinimo,
                pitchAguaMaximo
            );

        float pitchMayor =
            Mathf.Max(
                pitchAguaMinimo,
                pitchAguaMaximo
            );

        source.pitch =
            UnityEngine.Random.Range(
                pitchMenor,
                pitchMayor
            );

        source.PlayOneShot(
            clip,
            volumen
        );
    }

    private AudioClip ObtenerClipAguaAleatorioSinRepetir(
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
                UnityEngine.Random.Range(
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

    private void ActualizarEstadosAnimatorNatacionForzados(
        bool isSwimmingMoving,
        bool isWalking,
        bool isRunning)
    {
        if (!forzarEstadosAnimatorNatacion ||
            animator == null ||
            isFainted ||
            isPickingUp)
        {
            return;
        }

        if (isSwimming)
        {
            ultimoFrameAnimatorEnNatacion = true;
            ForzarEstadoAnimatorNatacionActual(
                isSwimmingMoving
            );
            return;
        }

        if (!ultimoFrameAnimatorEnNatacion &&
            !forzarRefrescoAnimatorNatacion)
        {
            return;
        }

        ultimoFrameAnimatorEnNatacion = false;
        ultimoEstadoAnimatorForzado = 0;
        forzarRefrescoAnimatorNatacion = false;

        string estadoSalida =
            isRunning
                ? estadoAnimatorRun
                : isWalking
                    ? estadoAnimatorWalk
                    : estadoAnimatorIdle;

        ForzarEstadoAnimatorSiExiste(
            estadoSalida
        );
    }

    private void ForzarEstadoAnimatorSiExiste(
        string nombreEstado)
    {
        if (animator == null ||
            string.IsNullOrEmpty(nombreEstado))
        {
            return;
        }

        if (!TryGetAnimatorStateHash(
                nombreEstado,
                out int estadoHash))
        {
            return;
        }

        AnimatorStateInfo estadoActual =
            animator.GetCurrentAnimatorStateInfo(0);

        bool yaEstaEnEstado =
            EstadoAnimatorCoincide(
                estadoActual,
                estadoHash
            );

        bool yaVaHaciaEstado = false;

        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo siguienteEstado =
                animator.GetNextAnimatorStateInfo(0);

            yaVaHaciaEstado =
                EstadoAnimatorCoincide(
                    siguienteEstado,
                    estadoHash
                );
        }

        if (!forzarRefrescoAnimatorNatacion &&
            ultimoEstadoAnimatorForzado == estadoHash &&
            (yaEstaEnEstado || yaVaHaciaEstado))
        {
            return;
        }

        animator.CrossFade(
            estadoHash,
            Mathf.Max(0f, duracionCrossFadeNatacion),
            0,
            0f
        );

        ultimoEstadoAnimatorForzado =
            estadoHash;
        forzarRefrescoAnimatorNatacion = false;
    }


    private void ReiniciarControlAnimatorNatacion()
    {
        ultimoFrameAnimatorEnNatacion = false;
        ultimoEstadoAnimatorForzado = 0;
        forzarRefrescoAnimatorNatacion = true;
    }

    private void AplicarParametrosAnimatorNatacionInmediatos(
        bool nadando,
        bool nadandoConMovimiento)
    {
        SetAnimatorBoolIfExists(
            parametroAnimacionNadando,
            nadando
        );

        SetAnimatorBoolIfExists(
            parametroAnimacionNadandoMovimiento,
            nadandoConMovimiento
        );

        if (!nadando)
        {
            SetAnimatorBoolIfExists(
                parametroAnimacionNadandoMovimiento,
                false
            );
        }
    }

    private void ForzarEstadoAnimatorNatacionActual(
        bool isSwimmingMoving)
    {
        if (!forzarEstadosAnimatorNatacion ||
            animator == null)
        {
            return;
        }

        string estadoObjetivo =
            isSwimmingMoving
                ? estadoAnimatorNadar
                : estadoAnimatorFlotar;

        ForzarEstadoAnimatorSiExiste(
            estadoObjetivo
        );
    }

    private void ForzarEstadoAnimatorSalidaAguaActual()
    {
        if (!forzarEstadosAnimatorNatacion ||
            animator == null ||
            isFainted ||
            isPickingUp)
        {
            return;
        }

        LeerMovimientoActual();

        bool isWalking =
            moveInput.magnitude > 0.1f;

        bool isTryingToSprint =
            sprintAction != null &&
            sprintAction.ReadValue<float>() > 0.5f;

        bool isRunning =
            isWalking &&
            isTryingToSprint &&
            canSprint &&
            !isTired;

        string estadoSalida =
            isRunning
                ? estadoAnimatorRun
                : isWalking
                    ? estadoAnimatorWalk
                    : estadoAnimatorIdle;

        ForzarEstadoAnimatorSiExiste(
            estadoSalida
        );
    }

    private bool EstadoAnimatorCoincide(
        AnimatorStateInfo estado,
        int estadoHash)
    {
        return
            estado.shortNameHash == estadoHash ||
            estado.fullPathHash == estadoHash;
    }

    private bool TryGetAnimatorStateHash(
        string nombreEstado,
        out int estadoHash)
    {
        estadoHash = 0;

        int hashSimple =
            Animator.StringToHash(nombreEstado);

        if (animator.HasState(0, hashSimple))
        {
            estadoHash = hashSimple;
            return true;
        }

        int hashBaseLayer =
            Animator.StringToHash(
                "Base Layer." + nombreEstado
            );

        if (animator.HasState(0, hashBaseLayer))
        {
            estadoHash = hashBaseLayer;
            return true;
        }

        return false;
    }

    private void SetAnimatorBoolIfExists(
        string parameterName,
        bool value)
    {
        if (animator == null ||
            string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter
                 in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(
                    parameterName,
                    value
                );

                return;
            }
        }
    }

    private void SetAnimatorTriggerIfExists(
        string parameterName)
    {
        if (animator == null ||
            string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter
                 in animator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(parameterName);
                return;
            }
        }
    }

    public bool IsSwimming()
    {
        return isSwimming;
    }

    public void PlayPickUpAnimation()
    {
        if (isFainted || isPickingUp)
        {
            return;
        }

        isPickingUp = true;
        moveInput = Vector2.zero;
        jumpPressed = false;

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );

            animator.SetBool(
                "isRunning",
                false
            );

            animator.SetTrigger("PickUp");
        }
    }

    public void EndPickUpAnimation()
    {
        isPickingUp = false;
        jumpPressed = false;

        LeerMovimientoActual();
        ActualizarAnimaciones();
    }

    public bool IsPickingUp()
    {
        return isPickingUp;
    }

    public bool IsActuallySprinting()
    {
        bool isTryingToSprint =
            sprintAction != null &&
            sprintAction.ReadValue<float>() > 0.5f;

        bool isMoving =
            moveInput.magnitude > 0.1f;

        return
            isTryingToSprint &&
            isMoving &&
            canSprint &&
            !isTired &&
            !isFainted &&
            !isPickingUp &&
            !isSwimming;
    }

    public void SetEnergySpeedMultiplier(
        float multiplier)
    {
        energySpeedMultiplier =
            Mathf.Clamp(
                multiplier,
                0.1f,
                1f
            );
    }

    public void SetCanSprint(bool value)
    {
        canSprint = value;
    }

    public void SetTired(bool value)
    {
        isTired = value;

        if (isTired)
        {
            canSprint = false;
        }

        ActualizarAnimaciones();
    }

    public bool IsTired()
    {
        return isTired;
    }

    public void PlayFaintAnimation()
    {
        if (isFainted)
        {
            return;
        }

        isFainted = true;
        isPickingUp = false;
        moveInput = Vector2.zero;
        jumpPressed = false;
        playerVelocity = Vector3.zero;

        saltoEnCurso = false;
        estuvoEnElAire = false;

        if (isSwimming)
        {
            SalirDelAgua();
        }

        if (animator != null)
        {
            animator.SetBool(
                "isWalking",
                false
            );

            animator.SetBool(
                "isRunning",
                false
            );

            animator.SetBool(
                "isFainted",
                true
            );

            animator.SetTrigger("faint");
        }
    }

    public void RecoverFromFaint()
    {
        isFainted = false;
        isPickingUp = false;
        jumpPressed = false;
        playerVelocity = Vector3.zero;

        saltoEnCurso = false;
        estuvoEnElAire = false;

        LeerMovimientoActual();

        if (animator != null)
        {
            animator.SetBool(
                "isFainted",
                false
            );
        }
    }

    public bool IsFainted()
    {
        return isFainted;
    }
}