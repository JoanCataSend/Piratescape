using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    public void SetEmpty()
    {
        iconImage.enabled = false;
        amountText.text = "";
    }

    public void SetSlot(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            SetEmpty();
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = itemData.Icon;
        amountText.text = amount.ToString();
    }
}