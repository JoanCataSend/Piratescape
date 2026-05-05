using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NightThreatSystem : MonoBehaviour
{
    [Header("Probabilidad de robo al dormir normal")]
    [Range(0f, 1f)]
    [SerializeField] private float probability = 0.4f;

    [Header("Cantidad a robar en sueño normal")]
    [SerializeField] private int minItemsToSteal = 1;
    [SerializeField] private int maxItemsToSteal = 3;

    [Header("Referencias")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private SistemaEspantamonos sistemaEspantamonos;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (sistemaEspantamonos == null)
        {
            sistemaEspantamonos = FindFirstObjectByType<SistemaEspantamonos>();
        }
    }

    public void ResolveNightEvent()
    {
        if (HayEspantamonosActivo())
        {
            MostrarMensaje("El espantamonos ha ahuyentado a los monos.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("NightThreatSystem: falta referencia a PlayerInventory.", this);
            MostrarMensaje("Has dormido tranquilo.");
            return;
        }

        float roll = Random.value;

        if (roll > probability)
        {
            MostrarMensaje("Has dormido tranquilo.");
            return;
        }

        List<int> occupiedSlots = ObtenerIndicesSlotsOcupados();

        if (occupiedSlots.Count == 0)
        {
            MostrarMensaje("Han venido monos... pero no tenías nada.");
            return;
        }

        int itemsToSteal = Random.Range(minItemsToSteal, maxItemsToSteal + 1);
        Dictionary<ItemData, int> stolenByItem = new Dictionary<ItemData, int>();
        int totalStolen = 0;

        for (int i = 0; i < itemsToSteal; i++)
        {
            occupiedSlots = ObtenerIndicesSlotsOcupados();

            if (occupiedSlots.Count == 0)
            {
                break;
            }

            int randomSlotIndex = occupiedSlots[Random.Range(0, occupiedSlots.Count)];
            InventorySlot slot = playerInventory.GetSlot(randomSlotIndex);

            if (slot == null || slot.IsEmpty() || slot.itemData == null)
            {
                continue;
            }

            ItemData robbedItem = slot.itemData;

            int amountToSteal = Mathf.Max(1, Mathf.CeilToInt(slot.amount * 0.2f));
            amountToSteal = Mathf.Min(amountToSteal, slot.amount);

            slot.amount -= amountToSteal;
            totalStolen += amountToSteal;

            if (stolenByItem.ContainsKey(robbedItem))
            {
                stolenByItem[robbedItem] += amountToSteal;
            }
            else
            {
                stolenByItem.Add(robbedItem, amountToSteal);
            }

            if (slot.amount <= 0)
            {
                slot.Clear();
            }
        }

        playerInventory.NotifyInventoryChanged();

        if (totalStolen > 0)
        {
            MostrarMensaje("Te han robado " + ConstruirResumenRobos(stolenByItem));
        }
        else
        {
            MostrarMensaje("Han venido monos... pero no pudieron robarte nada.");
        }
    }

    public void ResolveFaintEvent()
    {
        if (HayEspantamonosActivo())
        {
            MostrarMensaje("Te has desmayado, pero el espantamonos ha protegido la base.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("NightThreatSystem: falta referencia a PlayerInventory.", this);
            MostrarMensaje("Te has desmayado, los monos te han robado todo");
            return;
        }

        List<InventorySlot> slots = playerInventory.GetSlots();

        if (slots == null || slots.Count == 0)
        {
            MostrarMensaje("Te has desmayado, los monos te han robado todo");
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            slot.Clear();
        }

        playerInventory.NotifyInventoryChanged();
        MostrarMensaje("Te has desmayado, los monos te han robado todo");
    }

    private List<int> ObtenerIndicesSlotsOcupados()
    {
        List<int> occupiedSlots = new List<int>();

        if (playerInventory == null)
        {
            return occupiedSlots;
        }

        List<InventorySlot> slots = playerInventory.GetSlots();

        if (slots == null)
        {
            return occupiedSlots;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot != null && !slot.IsEmpty())
            {
                occupiedSlots.Add(i);
            }
        }

        return occupiedSlots;
    }

    private string ConstruirResumenRobos(Dictionary<ItemData, int> stolenByItem)
    {
        if (stolenByItem == null || stolenByItem.Count == 0)
        {
            return "nada";
        }

        List<string> partes = new List<string>();

        foreach (KeyValuePair<ItemData, int> pair in stolenByItem)
        {
            ItemData item = pair.Key;
            int amount = pair.Value;

            if (item == null || amount <= 0)
            {
                continue;
            }

            partes.Add(amount + " " + item.DisplayName);
        }

        if (partes.Count == 0)
        {
            return "nada";
        }

        if (partes.Count == 1)
        {
            return partes[0];
        }

        if (partes.Count == 2)
        {
            return partes[0] + " y " + partes[1];
        }

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < partes.Count; i++)
        {
            if (i == partes.Count - 1)
            {
                sb.Append("y ").Append(partes[i]);
            }
            else
            {
                sb.Append(partes[i]).Append(", ");
            }
        }

        return sb.ToString();
    }

    private bool HayEspantamonosActivo()
    {
        if (sistemaEspantamonos == null)
        {
            sistemaEspantamonos = SistemaEspantamonos.Instance;
        }

        return sistemaEspantamonos != null && sistemaEspantamonos.EstaActivo;
    }

    private void MostrarMensaje(string texto)
    {
        if (NightMessageUI.Instance != null)
        {
            NightMessageUI.Instance.ShowMessage(texto);
        }
        else
        {
            Debug.LogWarning("NightThreatSystem: no existe NightMessageUI en la escena.");
        }
    }
}