using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject bloqueoVisual;

    private ItemData itemData;
    private int price;
    private ShopSystem shopSystem;

    public void Setup(ItemData item, int itemPrice, ShopSystem system)
    {
        itemData = item;
        price = itemPrice;
        shopSystem = system;

        icon.sprite = itemData.Icon;
        nameText.text = itemData.DisplayName;
        priceText.text = price + " Conchas";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(Buy);
    }

    public void UpdateState(int playerMoney)
    {
        bool canBuy = playerMoney >= price;

        buyButton.interactable = canBuy;

        if (bloqueoVisual != null)
        {
            bloqueoVisual.SetActive(!canBuy);
        }
    }

    private void Buy()
    {
        shopSystem.TryBuy(itemData, price);
    }
}