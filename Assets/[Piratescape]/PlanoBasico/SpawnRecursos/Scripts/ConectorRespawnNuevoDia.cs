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
            sistemaTiempo.OnDayNightChanged += GestionarCambioDiaNoche;
        }
    }

    private void OnDisable()
    {
        if (sistemaTiempo != null)
        {
            sistemaTiempo.OnDayNightChanged -= GestionarCambioDiaNoche;
        }
    }

    private void GestionarCambioDiaNoche(bool esNoche)
    {
        if (esNoche)
        {
            return;
        }

        if (gestorRespawnRecursos == null)
        {
            return;
        }

        gestorRespawnRecursos.GestionarInicioNuevoDia();
    }
}