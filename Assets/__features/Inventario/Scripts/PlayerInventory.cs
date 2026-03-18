using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerInventory : MonoBehaviour, IItemReceiver
{
    public event Action OnInventoryChanged;

    [Header("Configuracion del inventario")]
    [SerializeField] private int inventorySize = 5;

    private List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        slots = new List<InventorySlot>();

        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot());
        }

        NotifyInventoryChanged();
    }

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.LogWarning("PlayerInventory: itemData es null.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("PlayerInventory: amount debe ser mayor que 0.");
            return false;
        }

        int remainingAmount = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].CanStack(itemData))
            {
                int amountToAdd = Mathf.Min(remainingAmount, slots[i].FreeSpace());
                slots[i].amount += amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
            {
                int amountToAdd = Mathf.Min(remainingAmount, InventorySlot.MaxStack);
                slots[i].itemData = itemData;
                slots[i].amount = amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        return false;
    }

    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}