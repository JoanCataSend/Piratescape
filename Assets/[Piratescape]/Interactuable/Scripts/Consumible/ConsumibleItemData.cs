    using UnityEngine;

    [CreateAssetMenu(fileName = "ConsumibleItem", menuName = "Gameplay/Items/Consumible Item")]
    public class ConsumibleItemData : ItemData
    {
        [SerializeField] private int healthRestore;
        [SerializeField] private int energyRestore;

        public int HealthRestore => healthRestore;
        public int EnergyRestore => energyRestore;
    }