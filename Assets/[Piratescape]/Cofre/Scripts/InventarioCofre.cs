using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventarioCofre : MonoBehaviour
{
    public event Action OnInventarioCofreCambiado;

    [Header("Configuracion")]
    [SerializeField] private int numeroSlots = 10;

    [Header("Slots del cofre")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    private void Awake()
    {
        InicializarSlots();
    }

    private void OnValidate()
    {
        if (numeroSlots < 1)
        {
            numeroSlots = 1;
        }
    }

    private void InicializarSlots()
    {
        while (slots.Count < numeroSlots)
        {
            slots.Add(new InventorySlot());
        }

        while (slots.Count > numeroSlots)
        {
            slots.RemoveAt(slots.Count - 1);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new InventorySlot();
            }
        }
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    public int GetNumeroSlots()
    {
        return slots.Count;
    }

    public int IntentarAnadirItem(ItemData itemData, int cantidad)
    {
        if (itemData == null || cantidad <= 0)
        {
            return 0;
        }

        InicializarSlots();

        int cantidadPendiente = cantidad;
        int cantidadAnadida = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemData != itemData || slot.IsFull())
            {
                continue;
            }

            int cantidadParaEsteSlot = Mathf.Min(cantidadPendiente, slot.FreeSpace());
            slot.amount += cantidadParaEsteSlot;
            cantidadPendiente -= cantidadParaEsteSlot;
            cantidadAnadida += cantidadParaEsteSlot;

            if (cantidadPendiente <= 0)
            {
                NotificarCambio();
                return cantidadAnadida;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || !slot.IsEmpty())
            {
                continue;
            }

            int cantidadParaEsteSlot = Mathf.Min(cantidadPendiente, InventorySlot.MaxStack);
            slot.itemData = itemData;
            slot.amount = cantidadParaEsteSlot;
            cantidadPendiente -= cantidadParaEsteSlot;
            cantidadAnadida += cantidadParaEsteSlot;

            if (cantidadPendiente <= 0)
            {
                NotificarCambio();
                return cantidadAnadida;
            }
        }

        if (cantidadAnadida > 0)
        {
            NotificarCambio();
        }

        return cantidadAnadida;
    }

    public int QuitarDelSlot(int index, int cantidad)
    {
        if (cantidad <= 0)
        {
            return 0;
        }

        InventorySlot slot = GetSlot(index);

        if (slot == null || slot.IsEmpty())
        {
            return 0;
        }

        int cantidadQuitada = Mathf.Min(cantidad, slot.amount);
        slot.amount -= cantidadQuitada;

        if (slot.amount <= 0)
        {
            slot.Clear();
        }

        NotificarCambio();
        return cantidadQuitada;
    }

    public void NotificarCambio()
    {
        OnInventarioCofreCambiado?.Invoke();
    }
}
