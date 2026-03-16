public interface IItemReceiver
{
    bool TryAddItem(ItemData itemData, int amount);
}