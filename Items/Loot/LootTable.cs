namespace EndlessDungeon.Items.Loot;

public class LootTable
{
    private readonly List<LootEntry> _entries = new();

    public int NoDropWeight { get; }

    public LootTable(int noDropWeight = 0)
    {
        NoDropWeight = Math.Max(0, noDropWeight);
    }

    public LootTable Add(string itemId, int weight)
    {
        _entries.Add(new LootEntry(itemId, weight));
        return this;
    }

    public Item? Roll(Random random)
    {
        int totalWeight = NoDropWeight;

        foreach (LootEntry entry in _entries)
        {
            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = random.Next(totalWeight);

        if (roll < NoDropWeight)
        {
            return null;
        }

        roll -= NoDropWeight;

        foreach (LootEntry entry in _entries)
        {
            if (roll < entry.Weight)
            {
                return ItemFactory.Create(entry.ItemId);
            }

            roll -= entry.Weight;
        }

        return null;
    }

    public List<Item> Roll(Random random, int rolls)
    {
        List<Item> items = new();

        for (int i = 0; i < rolls; i++)
        {
            Item? item = Roll(random);

            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }
}