using System.Collections.Generic;
using UnityEngine;

public static class InventarioGuardadoUtil
{
    public static List<DatosSlotInventario> CrearDesdePlayer(PlayerInventory inventario)
    {
        List<DatosSlotInventario> datos = new List<DatosSlotInventario>();

        if (inventario == null)
        {
            return datos;
        }

        List<InventorySlot> slots = inventario.GetSlots();
        CrearDesdeListaSlots(slots, datos);
        return datos;
    }

    public static List<DatosSlotInventario> CrearDesdeCofre(InventarioCofre inventario)
    {
        List<DatosSlotInventario> datos = new List<DatosSlotInventario>();

        if (inventario == null)
        {
            return datos;
        }

        for (int i = 0; i < inventario.GetNumeroSlots(); i++)
        {
            InventorySlot slot = inventario.GetSlot(i);
            datos.Add(CrearDatoSlot(slot));
        }

        return datos;
    }

    public static void CargarEnPlayer(PlayerInventory inventario, List<DatosSlotInventario> datos, ItemDatabase itemDatabase)
    {
        if (inventario == null)
        {
            return;
        }

        List<InventorySlot> slots = inventario.GetSlots();
        CargarEnListaSlots(slots, datos, itemDatabase);
        inventario.NotifyInventoryChanged();
    }

    public static void CargarEnCofre(InventarioCofre inventario, List<DatosSlotInventario> datos, ItemDatabase itemDatabase)
    {
        if (inventario == null)
        {
            return;
        }

        int numeroSlots = inventario.GetNumeroSlots();

        for (int i = 0; i < numeroSlots; i++)
        {
            InventorySlot slot = inventario.GetSlot(i);

            if (slot == null)
            {
                continue;
            }

            DatosSlotInventario dato = datos != null && i < datos.Count ? datos[i] : null;
            CargarSlot(slot, dato, itemDatabase);
        }

        inventario.NotificarCambio();
    }

    private static void CrearDesdeListaSlots(List<InventorySlot> slots, List<DatosSlotInventario> datos)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            datos.Add(CrearDatoSlot(slots[i]));
        }
    }

    private static DatosSlotInventario CrearDatoSlot(InventorySlot slot)
    {
        DatosSlotInventario dato = new DatosSlotInventario();

        if (slot == null || slot.IsEmpty() || slot.itemData == null)
        {
            dato.itemId = "";
            dato.cantidad = 0;
            return dato;
        }

        dato.itemId = slot.itemData.ItemId;
        dato.cantidad = slot.amount;
        return dato;
    }

    private static void CargarEnListaSlots(List<InventorySlot> slots, List<DatosSlotInventario> datos, ItemDatabase itemDatabase)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new InventorySlot();
            }

            DatosSlotInventario dato = datos != null && i < datos.Count ? datos[i] : null;
            CargarSlot(slots[i], dato, itemDatabase);
        }
    }

    private static void CargarSlot(InventorySlot slot, DatosSlotInventario dato, ItemDatabase itemDatabase)
    {
        if (slot == null)
        {
            return;
        }

        if (dato == null || string.IsNullOrWhiteSpace(dato.itemId) || dato.cantidad <= 0)
        {
            slot.Clear();
            return;
        }

        if (itemDatabase == null)
        {
            Debug.LogWarning("No se puede cargar el item " + dato.itemId + " porque falta ItemDatabase.");
            slot.Clear();
            return;
        }

        ItemData item = itemDatabase.BuscarPorId(dato.itemId);

        if (item == null)
        {
            slot.Clear();
            return;
        }

        slot.itemData = item;
        slot.amount = Mathf.Clamp(dato.cantidad, 1, InventorySlot.DefaultMaxStack);
    }
}
