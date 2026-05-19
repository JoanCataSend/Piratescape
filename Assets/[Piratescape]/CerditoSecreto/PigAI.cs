using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PigAI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Zona de movimiento")]
    [SerializeField] private float radioZona = 8f;

    [Header("Movimiento")]
    [SerializeField] private float velocidadPaseo = 1.5f;
    [SerializeField] private float velocidadHuida = 4f;
    [SerializeField] private float velocidadRotacion = 8f;

    [Header("Huida")]
    [SerializeField] private float distanciaDetectarPlayer = 5f;
    [SerializeField] private float distanciaPararHuida = 7f;

    [Header("Gravedad")]
    [SerializeField] private float gravedad = -20f;
    [SerializeField] private float fuerzaPegadoSuelo = -2f;

    [Header("Animación")]
    [SerializeField] private string animRun = "Run Forward In Place";

    private CharacterController controller;

    private Vector3 centroZona;
    private Vector3 destinoActual;

    private bool huyendo;
    private float velocidadVertical;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        centroZona = transform.position;
        ElegirNuevoDestino();

        if (animator != null)
        {
            animator.Play(animRun);
        }
    }

    private void Update()
    {
        if (player == null) return;

        MantenerAnimacionRun();

        float distanciaPlayer = Vector3.Distance(transform.position, player.position);

        if (!huyendo && distanciaPlayer <= distanciaDetectarPlayer)
        {
            huyendo = true;
        }
        else if (huyendo && distanciaPlayer >= distanciaPararHuida)
        {
            huyendo = false;
            ElegirNuevoDestino();
        }

        Vector3 direccionMovimiento;

        if (huyendo)
        {
            direccionMovimiento = CalcularDireccionHuida();
        }
        else
        {
            direccionMovimiento = CalcularDireccionPaseo();
        }

        MoverCerdo(direccionMovimiento);
    }

    private Vector3 CalcularDireccionPaseo()
    {
        Vector3 direccion = destinoActual - transform.position;
        direccion.y = 0f;

        if (direccion.magnitude <= 0.4f)
        {
            ElegirNuevoDestino();
            return Vector3.zero;
        }

        return direccion.normalized;
    }

    private Vector3 CalcularDireccionHuida()
    {
        Vector3 direccionHuida = transform.position - player.position;
        direccionHuida.y = 0f;

        if (direccionHuida.magnitude < 0.1f)
        {
            direccionHuida = transform.forward;
        }

        Vector3 posiblePosicion = transform.position + direccionHuida.normalized * velocidadHuida * Time.deltaTime;

        Vector3 desdeCentro = posiblePosicion - centroZona;
        desdeCentro.y = 0f;

        if (desdeCentro.magnitude > radioZona)
        {
            Vector3 direccionAlCentro = centroZona - transform.position;
            direccionAlCentro.y = 0f;

            Vector3 direccionFinal = direccionHuida.normalized + direccionAlCentro.normalized;
            direccionFinal.y = 0f;

            return direccionFinal.normalized;
        }

        return direccionHuida.normalized;
    }

    private void MoverCerdo(Vector3 direccion)
    {
        float velocidadActual = huyendo ? velocidadHuida : velocidadPaseo;

        if (direccion.magnitude > 0.1f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidadRotacion * Time.deltaTime
            );
        }

        if (controller.isGrounded && velocidadVertical < 0f)
        {
            velocidadVertical = fuerzaPegadoSuelo;
        }

        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimientoHorizontal = direccion * velocidadActual;
        Vector3 movimientoVertical = Vector3.up * velocidadVertical;

        Vector3 movimientoFinal = movimientoHorizontal + movimientoVertical;

        controller.Move(movimientoFinal * Time.deltaTime);

        LimitarDentroDelCirculo();
    }

    private void LimitarDentroDelCirculo()
    {
        Vector3 desdeCentro = transform.position - centroZona;
        desdeCentro.y = 0f;

        if (desdeCentro.magnitude > radioZona)
        {
            Vector3 posicionLimite = centroZona + desdeCentro.normalized * radioZona;
            posicionLimite.y = transform.position.y;

            Vector3 correccion = posicionLimite - transform.position;
            controller.Move(correccion);
        }
    }

    private void ElegirNuevoDestino()
    {
        Vector2 puntoAleatorio = Random.insideUnitCircle * radioZona;

        destinoActual = new Vector3(
            centroZona.x + puntoAleatorio.x,
            transform.position.y,
            centroZona.z + puntoAleatorio.y
        );
    }

    private void MantenerAnimacionRun()
    {
        if (animator == null) return;

        AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);

        if (!estado.IsName(animRun))
        {
            animator.Play(animRun);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centro = Application.isPlaying ? centroZona : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centro, radioZona);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDetectarPlayer);
    }
}