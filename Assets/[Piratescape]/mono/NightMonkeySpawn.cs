using System.Collections;
using UnityEngine;

public sealed class NightMonkeySpawn : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform[] puntosAparicion;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animacionIdle = "Crouch Idle";
    [SerializeField] private string animacionCaminar = "Crouched Walking";

    [Header("Comportamiento")]
    [SerializeField] private float tiempoIdleMin = 2f;
    [SerializeField] private float tiempoIdleMax = 5f;
    [SerializeField] private float tiempoCaminarMin = 3f;
    [SerializeField] private float tiempoCaminarMax = 6f;

    [Header("Movimiento en círculo")]
    [SerializeField] private float radioMovimiento = 1.2f;
    [SerializeField] private float velocidadMovimiento = 0.7f;
    [SerializeField] private bool sentidoHorario = true;

    private int ultimoPunto = -1;
    private Vector3 centroMovimiento;
    private Coroutine rutinaComportamiento;
    private bool estaDeNoche;

    private float anguloActual;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        gameObject.SetActive(false);
    }

    public void EmpezarNoche()
    {
        estaDeNoche = true;

        AparecerEnPuntoNocturno();

        gameObject.SetActive(true);

        if (rutinaComportamiento != null)
        {
            StopCoroutine(rutinaComportamiento);
        }

        rutinaComportamiento = StartCoroutine(ComportamientoNocturno());
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;

        if (rutinaComportamiento != null)
        {
            StopCoroutine(rutinaComportamiento);
            rutinaComportamiento = null;
        }

        gameObject.SetActive(false);
    }

    private void AparecerEnPuntoNocturno()
    {
        if (puntosAparicion == null || puntosAparicion.Length == 0)
        {
            Debug.LogWarning("No hay puntos asignados al mono nocturno.", this);
            return;
        }

        int nuevoPunto;

        do
        {
            nuevoPunto = Random.Range(0, puntosAparicion.Length);
        }
        while (nuevoPunto == ultimoPunto && puntosAparicion.Length > 1);

        ultimoPunto = nuevoPunto;

        transform.position = puntosAparicion[nuevoPunto].position;
        transform.rotation = puntosAparicion[nuevoPunto].rotation;

        centroMovimiento = transform.position;
        anguloActual = Random.Range(0f, 360f);
        sentidoHorario = Random.value > 0.5f;
    }

    private IEnumerator ComportamientoNocturno()
    {
        while (estaDeNoche)
        {
            yield return IdleRoutine();

            if (!estaDeNoche)
            {
                yield break;
            }

            yield return CaminarEnCirculoRoutine();
        }
    }

    private IEnumerator IdleRoutine()
    {
        if (animator != null && !string.IsNullOrEmpty(animacionIdle))
        {
            animator.speed = 1f;
            animator.Play(animacionIdle, 0, 0f);
        }

        float duracion = Random.Range(tiempoIdleMin, tiempoIdleMax);
        float timer = 0f;

        while (timer < duracion)
        {
            if (!estaDeNoche)
            {
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CaminarEnCirculoRoutine()
    {
        if (animator != null && !string.IsNullOrEmpty(animacionCaminar))
        {
            animator.speed = 1f;
            animator.Play(animacionCaminar, 0, 0f);
        }

        float duracion = Random.Range(tiempoCaminarMin, tiempoCaminarMax);
        float timer = 0f;

        while (timer < duracion)
        {
            if (!estaDeNoche)
            {
                yield break;
            }

            timer += Time.deltaTime;

            float direccion = sentidoHorario ? 1f : -1f;
            anguloActual += direccion * velocidadMovimiento * 60f * Time.deltaTime;

            float radianes = anguloActual * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(radianes) * radioMovimiento,
                0f,
                Mathf.Sin(radianes) * radioMovimiento
            );

            Vector3 nuevaPosicion = centroMovimiento + offset;

            Vector3 direccionMovimiento = nuevaPosicion - transform.position;
            direccionMovimiento.y = 0f;

            if (direccionMovimiento.sqrMagnitude > 0.001f)
            {
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 8f);
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                nuevaPosicion,
                velocidadMovimiento * Time.deltaTime
            );

            yield return null;
        }
    }
}