using UnityEngine;
// Archivo provisional mientras no existe el inventario
public sealed class DebugItemReceiver : MonoBehaviour, IItemReceiver
{
    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.LogWarning("DebugItemReceiver: itemData is null.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("DebugItemReceiver: amount must be greater than 0.");
            return false;
        }

        Debug.Log($"Item received: {itemData.DisplayName} x{amount}");
        return true;
    }
}