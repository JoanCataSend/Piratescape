using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    [Header("Panel de tienda")]
    [SerializeField] private GameObject shopPanel;

    [Header("Inventario y moneda")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ItemData shellItem;

    [Header("UI")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private TMP_Text moneyText;

    [Header("Items en venta")]
    [SerializeField] private List<ItemData> itemsForSale;
    [SerializeField] private List<int> prices;

    private List<ShopItemUI> spawnedItems = new List<ShopItemUI>();
    private bool shopGenerated = false;

    private void Start()
    {
        shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleShop();
        }
    }

    private void ToggleShop()
    {
        bool abrir = !shopPanel.activeSelf;
        shopPanel.SetActive(abrir);

        if (abrir)
        {
            if (!shopGenerated)
            {
                GenerateShop();
                shopGenerated = true;
            }

            RefreshUI();
        }
    }

    public void GenerateShop()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        spawnedItems.Clear();

        for (int i = 0; i < itemsForSale.Count; i++)
        {
            ShopItemUI itemUI = Instantiate(itemPrefab, gridParent);
            itemUI.Setup(itemsForSale[i], prices[i], this);
            spawnedItems.Add(itemUI);
        }
    }

    public void RefreshUI()
    {
        int money = playerInventory.ObtenerCantidad(shellItem);

        moneyText.text = $"Conchas: {money}";

        foreach (ShopItemUI item in spawnedItems)
        {
            item.UpdateState(money);
        }
    }

    public void TryBuy(ItemData item, int price)
    {
        int money = playerInventory.ObtenerCantidad(shellItem);

        if (money < price)
        {
            Debug.Log("No tienes suficientes conchas");
            return;
        }

        playerInventory.RemoverHasta(shellItem, price);
        playerInventory.TryAddItem(item, 1);

        RefreshUI();
    }
}