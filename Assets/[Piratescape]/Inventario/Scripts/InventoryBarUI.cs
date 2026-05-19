using UnityEngine;

public class InventoryBarUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventorySlotUI[] slotUIs;

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (playerInventory == null)
        {
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
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