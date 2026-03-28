using UnityEngine;

public sealed class ConectorRespawnNuevoDia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem sistemaTiempo;
    [SerializeField] private GestorRespawnRecursos gestorRespawnRecursos;

    private void OnEnable()
    {
        if (sistemaTiempo != null)
        {
            sistemaTiempo.OnDayChanged += GestionarCambioDeDia;
        }
    }

    private void OnDisable()
    {
        if (sistemaTiempo != null)
        {
            sistemaTiempo.OnDayChanged -= GestionarCambioDeDia;
        }
    }

    private void GestionarCambioDeDia(int nuevoDia)
    {
        if (gestorRespawnRecursos == null)
        {
            return;
        }

        gestorRespawnRecursos.GestionarInicioNuevoDia();
    }
}