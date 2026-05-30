using UnityEngine;

public class CinematicSeagullsMovement : MonoBehaviour
{
    [Header("Objetivo a seguir")]
    [SerializeField] private Transform target;

    [Header("Órbita alrededor del barco")]
    [SerializeField] private float radio = 8f;
    [SerializeField] private float velocidadAngular = 35f;
    [SerializeField] private float altura = 5f;
    [SerializeField] private Vector3 offsetCentro = Vector3.zero;

    [Header("Movimiento vertical suave")]
    [SerializeField] private float amplitudSubidaBajada = 0.6f;
    [SerializeField] private float velocidadSubidaBajada = 2f;

    [Header("Rotación")]
    [SerializeField] private bool mirarSegunMovimiento = true;
    [SerializeField] private float suavizadoRotacion = 6f;

    [Tooltip("Inclinación lateral durante el giro. Si se inclina al revés, cambia 25 por -25.")]
    [SerializeField] private float inclinacionGiro = 25f;

    [Tooltip("Ajuste extra por si el modelo de la gaviota no mira hacia delante correctamente. Si sigue yendo de culo, prueba Y = 180.")]
    [SerializeField] private Vector3 rotacionOffsetModelo = Vector3.zero;

    [Header("Inicio")]
    [SerializeField] private float anguloInicial = 0f;

    private bool movimientoActivo;
    private float anguloActual;

    private void Awake()
    {
        anguloActual = anguloInicial;
    }

    private void Update()
    {
        if (!movimientoActivo || target == null)
        {
            return;
        }

        MoverGaviotas();
    }

    public void IniciarMovimiento(Transform nuevoTarget)
    {
        target = nuevoTarget;
        movimientoActivo = true;
        anguloActual = anguloInicial;
    }

    public void DetenerMovimiento()
    {
        movimientoActivo = false;
    }

    private void MoverGaviotas()
    {
        anguloActual += velocidadAngular * Time.deltaTime;

        float anguloRad = anguloActual * Mathf.Deg2Rad;

        Vector3 centro = target.position + offsetCentro;

        float x = Mathf.Cos(anguloRad) * radio;
        float z = Mathf.Sin(anguloRad) * radio;
        float y = altura + Mathf.Sin(Time.time * velocidadSubidaBajada) * amplitudSubidaBajada;

        Vector3 nuevaPosicion = centro + new Vector3(x, y, z);
        transform.position = nuevaPosicion;

        if (!mirarSegunMovimiento)
        {
            return;
        }

        // Dirección desde el centro (barco) hacia la gaviota
        Vector3 direccionRadial = new Vector3(x, 0f, z).normalized;

        if (direccionRadial.sqrMagnitude < 0.001f)
        {
            return;
        }

        // Tangente calculada para que la gaviota orbite
        // y el ala izquierda quede mirando hacia el barco
        Vector3 direccionTangencial = Vector3.Cross(direccionRadial, Vector3.up).normalized;

        if (direccionTangencial.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion rotacionBase = Quaternion.LookRotation(direccionTangencial, Vector3.up);

        // Inclinación lateral para simular el giro
        Quaternion rotacionConInclinacion = rotacionBase * Quaternion.Euler(0f, 0f, inclinacionGiro);

        // Offset por si el modelo importado no tiene su "frente" bien orientado
        Quaternion offsetModelo = Quaternion.Euler(rotacionOffsetModelo);

        Quaternion rotacionFinal = rotacionConInclinacion * offsetModelo;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionFinal,
            suavizadoRotacion * Time.deltaTime
        );
    }
}