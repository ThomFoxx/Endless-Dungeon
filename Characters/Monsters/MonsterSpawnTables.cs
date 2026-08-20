namespace EndlessDungeon.Characters.Monsters;

public static class MonsterSpawnTables
{
    public static readonly MonsterSpawnTable Easy = new MonsterSpawnTable()
        .Add(MonsterIds.Slime, 60)
        .Add(MonsterIds.Goblin, 40);

    public static readonly MonsterSpawnTable Medium = new();
    public static readonly MonsterSpawnTable Hard = new();

    public static string Roll(MonsterThreatTier tier, Random random)
    {
        MonsterSpawnTable requestedTable = GetTable(tier);

        if (requestedTable.HasEntries)
        {
            return requestedTable.Roll(random)
                ?? throw new InvalidOperationException("Monster spawn table unexpectedly returned no monster.");
        }

        // Development fallback while some threat tiers are empty.
        MonsterSpawnTable fallback = FindNearestPopulatedTable(tier);

        return fallback.Roll(random)
            ?? throw new InvalidOperationException("No populated monster spawn tables exist.");
    }

    private static MonsterSpawnTable GetTable(MonsterThreatTier tier)
    {
        return tier switch
        {
            MonsterThreatTier.Easy => Easy,
            MonsterThreatTier.Medium => Medium,
            MonsterThreatTier.Hard => Hard,
            _ => Easy
        };
    }

    private static MonsterSpawnTable FindNearestPopulatedTable(MonsterThreatTier tier)
    {
        MonsterSpawnTable[] preferredOrder = tier switch
        {
            MonsterThreatTier.Easy => new[] { Easy, Medium, Hard },
            MonsterThreatTier.Medium => new[] { Medium, Easy, Hard },
            MonsterThreatTier.Hard => new[] { Hard, Medium, Easy },
            _ => new[] { Easy, Medium, Hard }
        };

        foreach (MonsterSpawnTable table in preferredOrder)
        {
            if (table.HasEntries)
            {
                return table;
            }
        }

        throw new InvalidOperationException("No monster spawn tables contain any monsters.");
    }
}