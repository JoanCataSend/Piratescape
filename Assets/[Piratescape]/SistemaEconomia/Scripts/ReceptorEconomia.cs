using UnityEngine;

public sealed class ReceptorEconomia : MonoBehaviour, IItemReceiver
{
    [Header("Referencias")]
    [SerializeField] private SistemaEconomia sistemaEconomia;

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            return false;
        }

        if (sistemaEconomia == null)
        {
            Debug.LogWarning("ReceptorEconomia: falta referencia a SistemaEconomia.", this);
            return false;
        }

        MonedaItemData monedaItemData = itemData as MonedaItemData;

        if (monedaItemData == null)
        {
            return false;
        }

        int cantidadFinal = monedaItemData.Valor * amount;
        sistemaEconomia.AnadirMoneda(monedaItemData.TipoMoneda, cantidadFinal);
        return true;
    }
}