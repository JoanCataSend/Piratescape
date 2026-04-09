using UnityEngine;

public class NightThreatSystem : MonoBehaviour
{
    [Header("Probabilidad")]
    [Range(0f, 1f)]
    [SerializeField] private float probability = 0.4f; // 40% de probabilidad

    [Header("Cantidad a robar")]
    [SerializeField] private int minItemsToSteal = 1;
    [SerializeField] private int maxItemsToSteal = 3;

    [Header("Referencias")]
    [SerializeField] private PlayerInventory playerInventory;

    public void ResolveNightEvent()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("NightThreatSystem: falta PlayerInventory");
            return;
        }

        float roll = Random.value;

        if (roll > probability)
        {
            Debug.Log("🌙 Noche tranquila... no pasó nada.");
            return;
        }

        Debug.Log("🐒 ¡Monos ladrones han aparecido!");

        int itemsToSteal = Random.Range(minItemsToSteal, maxItemsToSteal + 1);

        int stolen = 0; // 👈 IMPORTANTE

        for (int i = 0; i < itemsToSteal; i++)
        {
            if (StealMostValuableItem()) // 👈 ahora devuelve bool
            {
                stolen++;
            }
        }

        if (stolen > 0)
        {
            Debug.Log($"⚠️ Te han robado {stolen} objetos.");
        }
        else
        {
            Debug.Log("😴 Han venido monos... pero no tenías nada.");
        }
    }

    private bool StealMostValuableItem()
    {
        var slots = playerInventory.GetSlots();

        InventorySlot bestSlot = null;
        int highestValue = -1;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty()) continue;

            int itemValue = slot.itemData.Value;

            if (itemValue > highestValue)
            {
                highestValue = itemValue;
                bestSlot = slot;
            }
        }

        if (bestSlot == null)
        {
            return false; // no hay nada
        }

        // quitar 1 unidad del item más valioso
        bestSlot.amount--;

        if (bestSlot.amount <= 0)
        {
            bestSlot.Clear();
        }

        playerInventory.ForceUpdateUI();

        return true;
    }
}