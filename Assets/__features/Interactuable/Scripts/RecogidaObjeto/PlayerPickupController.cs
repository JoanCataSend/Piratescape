using UnityEngine;

public sealed class PlayerPickupController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour itemReceiverSource;

    private IItemReceiver itemReceiver;

    private void Awake()
    {
        itemReceiver = itemReceiverSource as IItemReceiver;

        if (itemReceiver == null)
        {
            Debug.LogError("PlayerPickupController: itemReceiverSource must implement IItemReceiver.", this);
        }
    }

    public bool TryCollect(ICollectible collectible)
    {
        if (collectible == null)
        {
            Debug.LogWarning("PlayerPickupController: collectible is null.");
            return false;
        }

        if (collectible.ItemData == null)
        {
            Debug.LogWarning("PlayerPickupController: collectible item data is null.");
            return false;
        }

        if (itemReceiver == null)
        {
            Debug.LogWarning("PlayerPickupController: item receiver is not assigned.");
            return false;
        }

        bool added = itemReceiver.TryAddItem(collectible.ItemData, collectible.Amount);

        if (!added)
        {
            return false;
        }

        collectible.OnCollected();
        return true;
    }
}