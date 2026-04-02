using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInventory : MonoBehaviour, IItemReceiver
{
    public event Action OnInventoryChanged;

    [Header("Configuracion del inventario")]
    [SerializeField] private int inventorySize = 5;

    [Header("Seleccion")]
    [SerializeField] private int selectedSlotIndex = 0;

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

    private void Update()
    {
        // TECLADO: seleccionar slots 1-5
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectSlot(4);

            // Q para consumir/usar
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                UseSelectedItem();
            }
        }

        // MANDO
        if (Gamepad.current != null)
        {
            // L1
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                SelectPreviousSlot();
            }

            // R1
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                SelectNextSlot();
            }

            // TRIANGULO / Y para consumir/usar
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                UseSelectedItem();
            }
        }
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
        if (slots == null || slots.Count == 0)
        {
            return;
        }

        selectedSlotIndex++;

        if (selectedSlotIndex >= slots.Count)
        {
            selectedSlotIndex = 0;
        }

        NotifyInventoryChanged();
    }

    public void SelectPreviousSlot()
    {
        if (slots == null || slots.Count == 0)
        {
            return;
        }

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

        if (item is not ConsumibleItemData consumibleItem)
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

    /* Métodos añadidos para la construcción del barco */
    public int ObtenerCantidad(ItemData itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        int cantidadTotal = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemData != itemData)
            {
                continue;
            }

            cantidadTotal += slot.amount;
        }

        return cantidadTotal;
    }

    public int RemoverHasta(ItemData itemData, int cantidadSolicitada)
    {
        if (itemData == null || cantidadSolicitada <= 0)
        {
            return 0;
        }

        int cantidadDisponible = ObtenerCantidad(itemData);
        int cantidadARemover = Mathf.Min(cantidadDisponible, cantidadSolicitada);

        if (cantidadARemover <= 0)
        {
            return 0;
        }

        RemoveItem(itemData, cantidadARemover);
        return cantidadARemover;
    }
}