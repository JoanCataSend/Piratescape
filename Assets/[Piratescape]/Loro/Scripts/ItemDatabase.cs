using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Gameplay/Guardado/Item Database")]
public sealed class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    public ItemData BuscarPorId(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            ItemData item = items[i];

            if (item == null)
            {
                continue;
            }

            if (item.ItemId == itemId)
            {
                return item;
            }
        }

        Debug.LogWarning("ItemDatabase: no se encontro ningun item con id " + itemId + ".");
        return null;
    }
}
