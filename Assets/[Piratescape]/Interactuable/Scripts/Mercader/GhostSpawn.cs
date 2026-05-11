using UnityEngine;

public class GhostSpawn : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform[] puntosAparicion;

    [Header("Movimiento en forma de 8")]
    [SerializeField] private float amplitudX = 0.8f;
    [SerializeField] private float amplitudZ = 0.4f;
    [SerializeField] private float velocidad = 1.2f;

    private int ultimoPunto = -1;
    private Vector3 posicionBase;
    private bool estaDeNoche;

    public void EmpezarNoche()
    {
        AparecerEnPuntoNocturno();
        gameObject.SetActive(true);
        estaDeNoche = true;
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;
        gameObject.SetActive(false);
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

        Debug.Log("Fantasma apareció en: " + puntosAparicion[nuevoPunto].name);
    }

    private void MoverEnFormaDeOcho()
    {
        float tiempo = Time.time * velocidad;

        float x = Mathf.Sin(tiempo) * amplitudX;
        float z = Mathf.Sin(tiempo * 2f) * amplitudZ;

        Vector3 nuevaPosicion = posicionBase + new Vector3(x, 0f, z);

        Vector3 direccion = (nuevaPosicion - transform.position).normalized;

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