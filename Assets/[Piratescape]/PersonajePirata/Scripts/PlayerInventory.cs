using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInventory : MonoBehaviour, IItemReceiver
{
    public event Action OnInventoryChanged;
    public event Action<string> OnInventoryMessageRequested;

    [Header("Configuracion del inventario")]
    [SerializeField] private int inventorySize = 5;

    [Header("Seleccion")]
    [SerializeField] private int selectedSlotIndex = 0;

    [Header("Drop")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropDistance = 1.5f;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private PlayerHealth playerHealth;
    private PlayerEnergy playerEnergy;

    public int SelectedSlotIndex => selectedSlotIndex;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerEnergy = GetComponent<PlayerEnergy>();
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

    private bool CanAddItem(ItemData itemData, int amount)
{
    if (itemData == null || amount <= 0)
    {
        return false;
    }

    int freeSpaceTotal = 0;

    for (int i = 0; i < slots.Count; i++)
    {
        if (slots[i] == null)
        {
            continue;
        }

        if (!slots[i].IsEmpty() && slots[i].itemData == itemData && slots[i].amount < InventorySlot.MaxStack)
        {
            freeSpaceTotal += InventorySlot.MaxStack - slots[i].amount;
        }
        else if (slots[i].IsEmpty())
        {
            freeSpaceTotal += InventorySlot.MaxStack;
        }
    }

    return freeSpaceTotal >= amount;
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

    if (!CanAddItem(itemData, amount))
    {
        OnInventoryMessageRequested?.Invoke("¡Oh no! Mis bolsillos están llenos");
        return false;
    }

    int remainingAmount = amount;

    // Primero apilar en slots existentes
    for (int i = 0; i < slots.Count; i++)
    {
        if (slots[i] == null)
        {
            continue;
        }

        if (!slots[i].IsEmpty() && slots[i].itemData == itemData && slots[i].amount < InventorySlot.MaxStack)
        {
            int freeSpace = InventorySlot.MaxStack - slots[i].amount;
            int amountToAdd = Mathf.Min(remainingAmount, freeSpace);

            slots[i].amount += amountToAdd;

            remainingAmount -= amountToAdd;

            if (remainingAmount <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }
    }

    // Luego meter en slots vacíos
    for (int i = 0; i < slots.Count; i++)
    {
        if (slots[i] == null)
        {
            continue;
        }

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

    NotifyInventoryChanged();
    return false;
}

    public bool RemoveItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        int remainingAmount = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].itemData == itemData)
            {
                int amountToRemove = Mathf.Min(remainingAmount, slots[i].amount);
                slots[i].amount -= amountToRemove;
                remainingAmount -= amountToRemove;

                if (slots[i].amount <= 0)
                {
                    slots[i].Clear();
                }

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        return remainingAmount <= 0;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return;
        }

        selectedSlotIndex = index;
        NotifyInventoryChanged();
    }

    public void SelectNextSlot()
    {
        selectedSlotIndex++;

        if (selectedSlotIndex >= slots.Count)
        {
            selectedSlotIndex = 0;
        }

        NotifyInventoryChanged();
    }

    public void SelectPreviousSlot()
    {
        selectedSlotIndex--;

        if (selectedSlotIndex < 0)
        {
            selectedSlotIndex = slots.Count - 1;
        }

        NotifyInventoryChanged();
    }

    public void UseSelectedItem()
    {
        InventorySlot slot = GetSlot(selectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("No hay item en el slot seleccionado.");
            return;
        }

        ItemData item = slot.itemData;
        ConsumibleItemData consumibleItem = item as ConsumibleItemData;

        if (consumibleItem == null)
        {
            Debug.Log(item.DisplayName + " no es consumible.");
            return;
        }

        bool used = ConsumeItem(consumibleItem);

        if (used)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.Clear();
            }

            Debug.Log("Consumido: " + item.DisplayName);
            NotifyInventoryChanged();
        }
    }

    public void DropSelectedItem()
    {
        InventorySlot slot = GetSlot(selectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("No hay item para tirar.");
            return;
        }

        ItemData item = slot.itemData;

        if (item.WorldPrefab == null)
        {
            Debug.LogWarning("El item " + item.DisplayName + " no tiene WorldPrefab asignado.");
            return;
        }

        Vector3 spawnPosition = GetDropPosition();
        Quaternion spawnRotation = Quaternion.identity;

        GameObject droppedObject = Instantiate(item.WorldPrefab, spawnPosition, spawnRotation);

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = transform.forward + Vector3.up * 0.2f;
            rb.AddForce(throwDirection.normalized * 2f, ForceMode.Impulse);
        }

        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
        }

        Debug.Log("Tirado al suelo: " + item.DisplayName);
        NotifyInventoryChanged();
    }

    private Vector3 GetDropPosition()
    {
        if (dropPoint != null)
        {
            return dropPoint.position;
        }

        return transform.position + transform.forward * dropDistance + Vector3.up * 0.5f;
    }

    private bool ConsumeItem(ConsumibleItemData item)
    {
        if (item == null)
        {
            return false;
        }

        bool usedSomething = false;

        if (item.HealthRestore > 0)
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(item.HealthRestore);
                usedSomething = true;
            }
            else
            {
                Debug.LogWarning("No hay PlayerHealth en el jugador.");
            }
        }

        if (item.EnergyRestore > 0)
        {
            if (playerEnergy != null)
            {
                playerEnergy.RestoreEnergy(item.EnergyRestore);
                usedSomething = true;
            }
            else
            {
                Debug.LogWarning("No hay PlayerEnergy en el jugador.");
            }
        }

        return usedSomething;
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