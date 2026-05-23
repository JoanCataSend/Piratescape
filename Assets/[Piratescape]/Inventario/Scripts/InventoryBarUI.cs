using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventorySlotUI[] slotUIs;
    [SerializeField] private bool buscarSlotsAutomaticamente = true;

    private bool suscrito;

    private void Awake()
    {
        BuscarReferencias();
    }

    private void OnEnable()
    {
        BuscarReferencias();
        SuscribirseInventario();
        RefreshUI();
    }

    private void OnDisable()
    {
        DesuscribirseInventario();
    }

    private void BuscarReferencias()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (buscarSlotsAutomaticamente && (slotUIs == null || slotUIs.Length == 0))
        {
            slotUIs = GetComponentsInChildren<InventorySlotUI>(true);
        }
    }

    private void SuscribirseInventario()
    {
        if (playerInventory == null || suscrito)
        {
            return;
        }

        playerInventory.OnInventoryChanged += RefreshUI;
        suscrito = true;
    }

    private void DesuscribirseInventario()
    {
        if (playerInventory == null || !suscrito)
        {
            return;
        }

        playerInventory.OnInventoryChanged -= RefreshUI;
        suscrito = false;
    }

    public void RefreshUI()
    {
        if (playerInventory == null)
        {
            BuscarReferencias();
        }

        if (playerInventory == null || slotUIs == null)
        {
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
            {
                continue;
            }

            InventorySlot slot = playerInventory.GetSlot(i);
            bool slotTieneItem = slot != null && !slot.IsEmpty();

            if (!slotTieneItem)
            {
                slotUIs[i].SetEmpty();
            }
            else
            {
                slotUIs[i].SetSlot(slot.itemData, slot.amount);
            }

            slotUIs[i].SetSelected(slotTieneItem && i == playerInventory.SelectedSlotIndex);
        }
    }
}
