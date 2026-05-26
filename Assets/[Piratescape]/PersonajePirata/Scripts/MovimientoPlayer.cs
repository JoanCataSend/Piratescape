using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

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
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isFainted)
        {
            moveInput = Vector2.zero;
            ActualizarAnimaciones();
            return;
        }

        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0f)
        {
            playerVelocity.y = -2f;
        }

        if (isPickingUp)
        {
            moveInput = Vector2.zero;
            jumpPressed = false;

            AplicarGravedad();
            ActualizarAnimaciones();
            return;
        }

        LeerMovimientoActual();

        MoverJugador();
        Saltar();
        AplicarGravedad();
        ActualizarAnimaciones();
    }

    private void CrearInputs()
    {
        moveAction = new InputAction(name: "Move", type: InputActionType.Value);

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

        moveAction.AddBinding("<Gamepad>/leftStick");
        moveAction.AddBinding("<Gamepad>/dpad");

        jumpAction = new InputAction(name: "Jump", type: InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        sprintAction = new InputAction(name: "Sprint", type: InputActionType.Value);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");
        sprintAction.AddBinding("<Gamepad>/rightTrigger");
    }

    private void LeerMovimientoActual()
    {
        if (moveAction != null && moveAction.enabled)
        {
            moveInput = moveAction.ReadValue<Vector2>();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (isFainted || isPickingUp)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isFainted || isPickingUp)
        {
            return;
        }

        jumpPressed = true;
    }

    private void MoverJugador()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        if (direction.magnitude < 0.1f)
        {
            return;
        }

        direction.Normalize();

        float cameraY = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraY;

        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref turnSmoothVelocity,
            turnSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        bool isTryingToSprint = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;
        bool isSprinting = isTryingToSprint && canSprint && !isTired;

        float currentSpeed = isSprinting ? playerSpeed * sprintMultiplier : playerSpeed;
        currentSpeed *= energySpeedMultiplier;

        controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
    }

    private void Saltar()
    {
        if (jumpPressed && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);

            if (animator != null)
            {
                animator.SetTrigger("jump");
            }
        }

        jumpPressed = false;
    }

    private void AplicarGravedad()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null)
        {
            return;
        }

        bool isWalking = moveInput.magnitude > 0.1f && !isFainted && !isPickingUp;

        bool isTryingToSprint = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;
        bool isRunning = isWalking && isTryingToSprint && canSprint && !isTired && !isFainted && !isPickingUp;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isTired", isTired);
        animator.SetBool("isFainted", isFainted);
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
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
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
        bool isTryingToSprint = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;
        bool isMoving = moveInput.magnitude > 0.1f;

        return isTryingToSprint && isMoving && canSprint && !isTired && !isFainted && !isPickingUp;
    }

    public void SetEnergySpeedMultiplier(float multiplier)
    {
        energySpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
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

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isFainted", true);
            animator.SetTrigger("faint");
        }
    }

    public void RecoverFromFaint()
    {
        isFainted = false;
        isPickingUp = false;
        jumpPressed = false;
        playerVelocity = Vector3.zero;

        LeerMovimientoActual();

        if (animator != null)
        {
            animator.SetBool("isFainted", false);
        }
    }

    public bool IsFainted()
    {
        return isFainted;
    }
}