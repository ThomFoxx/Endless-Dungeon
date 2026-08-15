using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon.Characters;

public class Explorer
{
    public string Name { get; set; }

    public char Glyph { get; set; } = '₽';

    public int DungeonSeed { get; }

    public int X { get; set; }
    public int Y { get; set; }

    public int MaxHealth { get; set; } = 20;
    public int CurrentHealth { get; set; } = 20;

    public int Attack { get; set; } = 5;
    public int Defense { get; set; } = 1;

    public int Level { get; set; } = 1;
    public int Experience { get; set; }

    public int DeepestFloorReached { get; private set; } = 1;

    public string LastDamageSource { get; private set; } =
        "Unknown";

    public bool IsAlive => CurrentHealth > 0;

    public Explorer(
        string name,
        int dungeonSeed)
    {
        Name = name;
        DungeonSeed = dungeonSeed;
    }

    public void SetPosition(
        int x,
        int y)
    {
        X = x;
        Y = y;
    }

    public void RecordFloorReached(
        int floorNumber)
    {
        DeepestFloorReached = Math.Max(
            DeepestFloorReached,
            floorNumber);
    }

    public void TakeDamage(
        int damage,
        string source)
    {
        CurrentHealth = Math.Max(
            0,
            CurrentHealth - damage);

        LastDamageSource = source;
    }

    public int AttackMonster(
    Monster monster)
    {
        int damage = Math.Max(
            1,
            Attack - monster.Defense);

        monster.TakeDamage(damage);

        return damage;
    }
}