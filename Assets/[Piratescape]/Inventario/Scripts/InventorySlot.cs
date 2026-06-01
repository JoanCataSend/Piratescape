using System;

[Serializable]
public class InventorySlot
{
    public const int DefaultMaxStack = 25;

    public ItemData itemData;
    public int amount;

    public bool IsEmpty()
    {
        return itemData == null || amount <= 0;
    }

    public int GetMaxStack()
    {
        if (itemData != null && EsGema(itemData))
        {
            return 1;
        }

        return DefaultMaxStack;
    }

    public bool IsFull()
    {
        return amount >= GetMaxStack();
    }

    public bool CanStack(ItemData otherItem)
    {
        return !IsEmpty()
            && itemData == otherItem
            && amount < GetMaxStack();
    }

    public int FreeSpace()
    {
        return GetMaxStack() - amount;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }

    private bool EsGema(ItemData item)
    {
        if (item == null)
            return false;

        string nombre = item.name.ToLower();

        return nombre.Contains("gema");
    }
}