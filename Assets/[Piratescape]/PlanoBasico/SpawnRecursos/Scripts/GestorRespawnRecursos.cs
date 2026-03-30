using System.Collections.Generic;
using UnityEngine;

public sealed class GestorRespawnRecursos : MonoBehaviour
{
    [SerializeField] private List<GrupoSpawnRecursos> gruposSpawn = new List<GrupoSpawnRecursos>();

    public void GestionarInicioNuevoDia()
    {
        for (int i = 0; i < gruposSpawn.Count; i++)
        {
            GrupoSpawnRecursos grupoActual = gruposSpawn[i];

            if (grupoActual == null)
            {
                continue;
            }

            grupoActual.RepoblarRecursosFaltantes();
        }
    }
}