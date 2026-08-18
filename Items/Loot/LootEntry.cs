namespace EndlessDungeon.Items.Loot;

public class LootEntry
{
    public string ItemId { get; }
    public int Weight { get; }

    public LootEntry(string itemId, int weight)
    {
        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weight),
                "Loot weight must be greater than zero.");
        }

        ItemId = itemId;
        Weight = weight;
    }
}