using System.Collections.Generic;
using UnityEngine;

public sealed class GestorRespawnRecursos : MonoBehaviour
{
    [SerializeField] private List<GrupoSpawnRecursos> gruposSpawn = new List<GrupoSpawnRecursos>();
    [SerializeField] private Transform playerTransform;

    public void GestionarInicioNuevoDia()
    {
        LimpiarSoloRecursosGeneradosDelMapa();

        for (int i = 0; i < gruposSpawn.Count; i++)
        {
            GrupoSpawnRecursos grupoActual = gruposSpawn[i];

            if (grupoActual == null)
            {
                continue;
            }

            grupoActual.ReiniciarRecursosDelGrupo();
        }
    }

    private void LimpiarSoloRecursosGeneradosDelMapa()
    {
        RecursoGenerado[] recursosGenerados = FindObjectsByType<RecursoGenerado>(FindObjectsSortMode.None);

        for (int i = 0; i < recursosGenerados.Length; i++)
        {
            RecursoGenerado recursoGenerado = recursosGenerados[i];

            if (recursoGenerado == null)
            {
                continue;
            }

            if (playerTransform != null && recursoGenerado.transform.IsChildOf(playerTransform))
            {
                continue;
            }

            Destroy(recursoGenerado.gameObject);
        }
    }
}