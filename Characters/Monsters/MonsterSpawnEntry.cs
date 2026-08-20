namespace EndlessDungeon.Characters.Monsters;

public class MonsterSpawnEntry
{
    public string MonsterId { get; }
    public int Weight { get; }

    public MonsterSpawnEntry(string monsterId, int weight)
    {
        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight));
        }

        MonsterId = monsterId;
        Weight = weight;
    }
}