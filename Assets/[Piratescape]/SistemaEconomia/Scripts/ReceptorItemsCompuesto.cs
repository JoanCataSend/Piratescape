using UnityEngine;

public sealed class ReceptorItemsCompuesto : MonoBehaviour, IItemReceiver
{
    [Header("Receptores")]
    [SerializeField] private MonoBehaviour receptorEconomiaSource;
    [SerializeField] private MonoBehaviour receptorInventarioSource;

    private IItemReceiver receptorEconomia;
    private IItemReceiver receptorInventario;

    private void Awake()
    {
        receptorEconomia = receptorEconomiaSource as IItemReceiver;
        receptorInventario = receptorInventarioSource as IItemReceiver;

        if (receptorEconomiaSource != null && receptorEconomia == null)
        {
            Debug.LogError("ReceptorItemsCompuesto: receptorEconomiaSource no implementa IItemReceiver.", this);
        }

        if (receptorInventarioSource != null && receptorInventario == null)
        {
            Debug.LogError("ReceptorItemsCompuesto: receptorInventarioSource no implementa IItemReceiver.", this);
        }
    }

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            return false;
        }

        if (itemData is MonedaItemData)
        {
            if (receptorEconomia != null)
            {
                return receptorEconomia.TryAddItem(itemData, amount);
            }

            return false;
        }

        if (receptorInventario != null)
        {
            return receptorInventario.TryAddItem(itemData, amount);
        }

        return false;
    }
}