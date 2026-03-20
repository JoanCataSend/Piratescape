using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Gameplay/Items/Item Data")]
public class ItemData : ScriptableObject

{
    [Header("Basic data")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Consumible")]
    [SerializeField] private bool isConsumable;
    [SerializeField] private int healthRestore;
    [SerializeField] private int energyRestore;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public bool IsConsumable => isConsumable;
    public int HealthRestore => healthRestore;
    public int EnergyRestore => energyRestore;
}