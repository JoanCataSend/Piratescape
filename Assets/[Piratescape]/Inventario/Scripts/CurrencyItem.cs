using UnityEngine;

public enum CurrencyType
{
    Concha,
    Tulipan,
    Pinya,
    Cristal
}

[CreateAssetMenu(fileName = "CurrencyItem", menuName = "Gameplay/Items/Currency Item")]
public class CurrencyItemData : ItemData
{
    [Header("Currency")]
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private int value = 1;

    public CurrencyType CurrencyType => currencyType;
    public int Value => value;
}