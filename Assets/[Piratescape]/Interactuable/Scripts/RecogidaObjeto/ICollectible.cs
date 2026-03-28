public interface ICollectible
{
    ItemData ItemData { get; }
    int Amount { get; }

    void OnCollected();
}