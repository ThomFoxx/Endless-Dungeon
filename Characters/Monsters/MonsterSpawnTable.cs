namespace EndlessDungeon.Characters.Monsters;

public class MonsterSpawnTable
{
    private readonly List<MonsterSpawnEntry> _entries = new();

    public bool HasEntries => _entries.Count > 0;

    public MonsterSpawnTable Add(string monsterId, int weight)
    {
        _entries.Add(new MonsterSpawnEntry(monsterId, weight));
        return this;
    }

    public string? Roll(Random random)
    {
        if (_entries.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;

        foreach (MonsterSpawnEntry entry in _entries)
        {
            totalWeight += entry.Weight;
        }

        int roll = random.Next(totalWeight);

        foreach (MonsterSpawnEntry entry in _entries)
        {
            if (roll < entry.Weight)
            {
                return entry.MonsterId;
            }

            roll -= entry.Weight;
        }

        return null;
    }
}