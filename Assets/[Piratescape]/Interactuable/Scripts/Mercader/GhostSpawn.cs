using UnityEngine;

public class GhostSpawn : MonoBehaviour
{
    [SerializeField] private Transform[] puntosAparicion;

    private int ultimoPunto = -1;

    public void EmpezarNoche()
    {
        AparecerEnPuntoNocturno();
        gameObject.SetActive(true);
    }

    public void EmpezarDia()
    {
        gameObject.SetActive(false);
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

        Debug.Log("Fantasma apareció en: " + puntosAparicion[nuevoPunto].name);
    }
}