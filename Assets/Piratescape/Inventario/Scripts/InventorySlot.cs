using System;

[Serializable]
public class InventorySlot
{
    public const int MaxStack = 24; // Máximo de objetos por slot

    public ItemData itemData;
    public int amount;

    public bool IsEmpty()
    {
        return itemData == null || amount <= 0;
    }

    public bool IsFull()
    {
        return amount >= MaxStack;
    }

    public bool CanStack(ItemData otherItem)
    {
        return !IsEmpty() && itemData == otherItem && amount < MaxStack;
    }

    public int FreeSpace()
    {
        return MaxStack - amount;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }
}
