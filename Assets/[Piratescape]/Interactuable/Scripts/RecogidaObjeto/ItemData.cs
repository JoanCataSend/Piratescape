using UnityEngine;
[CreateAssetMenu(fileName = "ItemData", menuName = "Gameplay/Items/Item Data")]

public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Valor del item")]
    [SerializeField] private int value = 1;
    public int Value => value;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;
}