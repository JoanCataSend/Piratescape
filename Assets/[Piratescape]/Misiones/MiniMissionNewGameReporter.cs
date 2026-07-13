using UnityEngine;

public sealed class MiniMissionNewGameReporter : MonoBehaviour
{
    [Header("Nueva partida")]
    [SerializeField] private bool mostrarLogs = false;

    public void MarcarNuevaPartida()
    {
        if (MiniMissionManager.Instance != null)
        {
            MiniMissionManager.Instance.PrepararNuevaPartida();
        }
        else
        {
            MiniMissionManager.MarcarNuevaPartidaGlobal();
        }

        if (mostrarLogs)
        {
            Debug.Log("[MiniMissionNewGameReporter] Nueva partida marcada. Las minimisiones se resetearan y esperaran al tutorial principal.", this);
        }
    }

    [ContextMenu("Debug/Marcar nueva partida")]
    public void DebugMarcarNuevaPartida()
    {
        MarcarNuevaPartida();
    }
}
