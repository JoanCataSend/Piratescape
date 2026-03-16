using UnityEngine;

public class moviimientoplayer : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Configuración de Movimiento")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float sprintMultiplier = 1.8f; //Valor que muliplica al correr
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Configuración de Rotación")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform; //Moviminto depende de la camara

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Oculta ratón y lo bloquea
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            // Se pone a -2 (y no a 0) para asegurar que el CharacterController detecte bien el suelo en pendientes
            playerVelocity.y = -2f;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized; // Normalized evita que corra más en diagonal

        //Sprint
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Fire3");
        float currentSpeed = isSprinting ? playerSpeed * sprintMultiplier : playerSpeed;


        //Calculos raros que no entiendo
        if (direction.magnitude >= 0.1f)
        {
            // Calcula el ángulo hacia el que debe mirar sumando la rotación "Y" de la cámara
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // Suaviza la rotación para que el pirata no gire de forma robótica/instantánea
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calcula el vector de movimiento real basándose en la rotación final
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Aplica el movimiento horizontal
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        //Salto
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        //Gravedad
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}