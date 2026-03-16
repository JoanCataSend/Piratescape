using UnityEngine;

[CreateAssetMenu(fileName = "ConsumibleItem", menuName = "Gameplay/Items/Consumible Item")]
public class ConsumibleItemData : ItemData
{
    [Header("Consumible properties")]

    [SerializeField] private int healthRecovery;
    [SerializeField] private int energyRecovery;

    public int HealthRecovery => healthRecovery;
    public int EnergyRecovery => energyRecovery;
}