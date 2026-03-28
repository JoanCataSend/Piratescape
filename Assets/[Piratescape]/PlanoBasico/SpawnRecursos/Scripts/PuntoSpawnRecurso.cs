using UnityEngine;

public sealed class PuntoSpawnRecurso : MonoBehaviour
{
    [Header("Configuracion del punto")]
    [SerializeField] private float radioComprobacion = 0.6f;

    private RecursoGenerado recursoActual;

    public float RadioComprobacion => radioComprobacion;
    public bool EstaOcupado => recursoActual != null;
    public Vector3 Posicion => transform.position;

    public void AsignarRecursoActual(RecursoGenerado nuevoRecurso)
    {
        recursoActual = nuevoRecurso;
    }

    public void LimpiarRecursoActual()
    {
        recursoActual = null;
    }
}