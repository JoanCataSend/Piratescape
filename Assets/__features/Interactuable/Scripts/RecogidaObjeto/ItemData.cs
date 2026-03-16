using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Gameplay/Items/Item Data")]
public class ItemData : ScriptableObject

{
    [Header("Basic data")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
}