using UnityEngine;

public class GhostSpawn : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform[] puntosAparicion;

    [Header("Visual / FX")]
    [SerializeField] private GhostVisualEffect ghostVisualEffect;

    [Header("Movimiento en forma de 8")]
    [SerializeField] private float amplitudX = 0.8f;
    [SerializeField] private float amplitudZ = 0.4f;
    [SerializeField] private float velocidad = 1.2f;

    private int ultimoPunto = -1;
    private Vector3 posicionBase;
    private bool estaDeNoche;

    private void Awake()
    {
        if (ghostVisualEffect == null)
        {
            ghostVisualEffect = GetComponent<GhostVisualEffect>();
        }
    }

    public void EmpezarNoche()
    {
        gameObject.SetActive(true);

        AparecerEnPuntoNocturno();

        estaDeNoche = true;

        if (ghostVisualEffect != null)
        {
            ghostVisualEffect.PlaySpawn();
        }
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;

        if (ghostVisualEffect != null)
        {
            ghostVisualEffect.PlayDespawn();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!estaDeNoche)
            return;

        MoverEnFormaDeOcho();
    }

    private void AparecerEnPuntoNocturno()
    {
        if (puntosAparicion == null || puntosAparicion.Length == 0)
        {
            Debug.LogWarning("No hay puntos asignados al fantasma");
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
        posicionBase = transform.position;
    }

    private void MoverEnFormaDeOcho()
    {
        float tiempo = Time.time * velocidad;

        float x = Mathf.Sin(tiempo) * amplitudX;
        float z = Mathf.Sin(tiempo * 2f) * amplitudZ;
        float y = Mathf.Sin(Time.time * 2f) * 0.25f;

        Vector3 nuevaPosicion = posicionBase + new Vector3(x, y, z);

        Vector3 direccion = nuevaPosicion - transform.position;
        direccion.y = 0f;
        direccion = direccion.normalized;

        if (direccion.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                Time.deltaTime * 8f
            );
        }

        transform.position = nuevaPosicion;
    }
}