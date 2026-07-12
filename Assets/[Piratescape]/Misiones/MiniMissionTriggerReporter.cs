using UnityEngine;

public sealed class MiniMissionTriggerReporter : MonoBehaviour
{
    [Header("Evento")]
    [SerializeField] private string eventoAReportar = "entrar_cueva_con_luz";
    [SerializeField] private bool reportarUnaSolaVez = true;

    [Header("Jugador")]
    [SerializeField] private string tagJugador = "Player";

    [Header("Requisitos opcionales")]
    [SerializeField] private bool requiereItemEnInventario = false;
    [SerializeField] private ItemData itemRequerido;
    [SerializeField] private string itemIdRequerido = "farol";
    [SerializeField] private int cantidadRequerida = 1;

    [Header("Depuracion")]
    [SerializeField] private bool mostrarLogs = false;

    private bool yaReportado;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            return;
        }

        if (!EsJugador(other, out PlayerInventory inventario))
        {
            return;
        }

        if (requiereItemEnInventario && !MiniMissionManager.InventarioTieneItem(inventario, itemRequerido, itemIdRequerido, cantidadRequerida))
        {
            if (mostrarLogs)
            {
                Debug.Log("[MiniMissionTriggerReporter] El jugador entro, pero no tiene el item requerido: " + itemIdRequerido, this);
            }

            return;
        }

        MiniMissionManager.ReportarEventoGlobal(eventoAReportar);
        yaReportado = true;

        if (mostrarLogs)
        {
            Debug.Log("[MiniMissionTriggerReporter] Evento reportado: " + eventoAReportar, this);
        }
    }

    public void ReportarManual()
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            return;
        }

        MiniMissionManager.ReportarEventoGlobal(eventoAReportar);
        yaReportado = true;
    }

    private bool EsJugador(Collider other, out PlayerInventory inventario)
    {
        inventario = null;

        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(tagJugador) && other.CompareTag(tagJugador))
        {
            inventario = other.GetComponent<PlayerInventory>();

            if (inventario == null)
            {
                inventario = other.GetComponentInParent<PlayerInventory>();
            }

            return true;
        }

        inventario = other.GetComponentInParent<PlayerInventory>();
        return inventario != null;
    }
}
