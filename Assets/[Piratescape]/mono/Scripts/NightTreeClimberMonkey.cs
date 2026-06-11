using System.Collections;
using UnityEngine;

public sealed class NightTreeClimberMonkey : MonoBehaviour
{
    [Header("Puntos del árbol")]
    [SerializeField] private Transform puntoInicio;
    [SerializeField] private Transform puntoFinal;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string animacionEscalar = "Climbing Up Wall";
    [SerializeField] private string animacionArriba = "Idle Crouching";

    [Header("Movimiento")]
    [SerializeField] private float duracionSubida = 8f;
    [SerializeField] private float tiempoArriba = 2f;
    [SerializeField] private float duracionBajada = 5f;
    [SerializeField] private bool mirarHaciaPuntoFinal = true;

    private Coroutine rutinaActual;
    private bool estaActivoDeNoche;

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
        estaActivoDeNoche = true;
    }

    public void EmpezarDia()
    {
        estaActivoDeNoche = false;

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = null;
        }

        gameObject.SetActive(false);
    }

    public IEnumerator EjecutarSubidaYBajada()
    {
        if (puntoInicio == null || puntoFinal == null)
        {
            Debug.LogWarning("Faltan puntoInicio o puntoFinal en el mono escalador.", this);
            yield break;
        }

        estaActivoDeNoche = true;
        gameObject.SetActive(true);

        transform.position = puntoInicio.position;
        transform.rotation = puntoInicio.rotation;

        if (mirarHaciaPuntoFinal)
        {
            MirarHacia(puntoFinal.position);
        }

        if (animator != null && !string.IsNullOrEmpty(animacionEscalar))
        {
            animator.speed = 0.75f;
            animator.Play(animacionEscalar, 0, 0f);
        }

        yield return MoverEntrePuntos(puntoInicio.position, puntoFinal.position, puntoInicio.rotation, puntoFinal.rotation, duracionSubida);

        if (!estaActivoDeNoche)
        {
            yield break;
        }

        if (animator != null && !string.IsNullOrEmpty(animacionArriba))
        {
            animator.speed = 1f;
            animator.Play(animacionArriba, 0, 0f);
        }

        yield return new WaitForSeconds(tiempoArriba);

        if (!estaActivoDeNoche)
        {
            yield break;
        }

        if (animator != null && !string.IsNullOrEmpty(animacionEscalar))
        {
            animator.speed = -0.65f;
            animator.Play(animacionEscalar, 0, 1f);
        }

        yield return MoverEntrePuntos(puntoFinal.position, puntoInicio.position, puntoFinal.rotation, puntoInicio.rotation, duracionBajada);

        if (animator != null)
        {
            animator.speed = 1f;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator MoverEntrePuntos(
        Vector3 posicionInicial,
        Vector3 posicionFinal,
        Quaternion rotacionInicial,
        Quaternion rotacionFinal,
        float duracion)
    {
        float timer = 0f;

        while (timer < duracion)
        {
            if (!estaActivoDeNoche)
            {
                yield break;
            }

            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duracion);
            float smoothT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(posicionInicial, posicionFinal, smoothT);
            transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionFinal, smoothT);

            yield return null;
        }

        transform.position = posicionFinal;
        transform.rotation = rotacionFinal;
    }

    private void MirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direccion.normalized);
    }
}