using System.Collections.Generic;
using UnityEngine;

public class NightThreatSystem : MonoBehaviour
{
    [Header("Probabilidad")]
    [Range(0f, 1f)]
    [SerializeField] private float probability = 0.4f;

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

        // 🔹 Probabilidad de que ocurra el evento
        if (Random.value > probability)
        {
            Debug.Log("🌙 Noche tranquila... no pasó nada.");
            MostrarMensaje("Has dormido tranquilo.");
            return;
        }

        var slots = playerInventory.GetSlots();

        // 🔹 Buscar el item más valioso
        InventorySlot bestSlot = null;
        int highestValue = -1;

        foreach (var slot in slots)
        {
            if (slot.IsEmpty()) continue;

            int value = slot.itemData.Value;

            if (value > highestValue)
            {
                highestValue = value;
                bestSlot = slot;
            }
        }

        // 🔹 Si no hay nada
        if (bestSlot == null)
        {
            Debug.Log("🐒 Han venido monos... pero no tenías nada.");
            MostrarMensaje("Han venido monos... pero no tenías nada.");
            return;
        }

        // 🔹 Cantidad a robar (pero SOLO de ese item)
        int amountAvailable = bestSlot.amount;
        int amountToSteal = Random.Range(minItemsToSteal, maxItemsToSteal + 1);

        // 👇 CLAVE: no robar más de lo que tienes
        int finalAmount = Mathf.Min(amountToSteal, amountAvailable);

        string itemName = bestSlot.itemData.DisplayName;

        // 🔹 Quitar items
        bestSlot.amount -= finalAmount;

        if (bestSlot.amount <= 0)
        {
            bestSlot.Clear();
        }

        playerInventory.ForceUpdateUI();

        // 🔹 Mensaje bonito
        string mensaje = $"Te han robado {finalAmount} {itemName}";
        if (finalAmount > 1) mensaje += "s";

        Debug.Log("🐒 " + mensaje);
        MostrarMensaje(mensaje);
    }

    // 🔹 Método para UI (puedes conectarlo luego)
    private void MostrarMensaje(string texto)
    {
        Debug.Log("📢 " + texto);

        if (NightMessageUI.Instance != null)
        {
            NightMessageUI.Instance.ShowMessage(texto);
        }
    }
}