using UnityEngine;

public class NightMonkeySpawn : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform[] puntosAparicion;

    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string nombreAnimacionNoche = "Idle";

    private int ultimoPunto = -1;
    private bool estaDeNoche;

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
        AparecerEnPuntoNocturno();
        gameObject.SetActive(true);
        estaDeNoche = true;

        if (animator != null && !string.IsNullOrEmpty(nombreAnimacionNoche))
        {
            animator.Play(nombreAnimacionNoche, 0, 0f);
        }
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;
        gameObject.SetActive(false);
    }

    private void AparecerEnPuntoNocturno()
    {
        if (puntosAparicion == null || puntosAparicion.Length == 0)
        {
            Debug.LogWarning("No hay puntos asignados al mono nocturno", this);
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

        Debug.Log("Mono nocturno apareció en: " + puntosAparicion[nuevoPunto].name);
    }
}