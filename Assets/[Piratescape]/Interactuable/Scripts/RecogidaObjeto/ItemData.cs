using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Gameplay/Items/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject worldPrefab;

    [Header("Visual en mano")]
    [SerializeField] private GameObject handPrefab;
    [SerializeField] private Vector3 handLocalPosition;
    [SerializeField] private Vector3 handLocalRotation;
    [SerializeField] private Vector3 handLocalScale = Vector3.one;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject WorldPrefab => worldPrefab;

    public GameObject HandPrefab => handPrefab;
    public Vector3 HandLocalPosition => handLocalPosition;
    public Vector3 HandLocalRotation => handLocalRotation;
    public Vector3 HandLocalScale => handLocalScale;
}