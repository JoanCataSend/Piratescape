using System.Collections.Generic;
using UnityEngine;

public sealed class GestorRespawnRecursos : MonoBehaviour
{
    [SerializeField] private List<GrupoSpawnRecursos> gruposSpawn = new List<GrupoSpawnRecursos>();
    [SerializeField] private Transform playerTransform;

    public void GestionarInicioNuevoDia()
    {
        LimpiarRecogiblesSueltosDelMapa();

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

    private void LimpiarRecogiblesSueltosDelMapa()
    {
        ObjetoRecogibleInteractuable[] recogibles = FindObjectsByType<ObjetoRecogibleInteractuable>(FindObjectsSortMode.None);

        for (int i = 0; i < recogibles.Length; i++)
        {
            ObjetoRecogibleInteractuable recogible = recogibles[i];

            if (recogible == null)
            {
                continue;
            }

            if (playerTransform != null && recogible.transform.IsChildOf(playerTransform))
            {
                continue;
            }

            Destroy(recogible.gameObject);
        }
    }
}