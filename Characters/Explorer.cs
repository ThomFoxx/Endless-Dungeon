using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon.Characters;

public class Explorer
{
    public string Name { get; set; }

    public char Glyph { get; set; } = '₽';

    public int X { get; set; }
    public int Y { get; set; }

    public int MaxHealth { get; set; } = 20;
    public int CurrentHealth { get; set; } = 20;

    public int Attack { get; set; } = 5;
    public int Defense { get; set; } = 1;

    public int Level { get; set; } = 1;
    public int Experience { get; set; }

    public bool IsAlive => CurrentHealth > 0;

    public Explorer(string name)
    {
        Name = name;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(
            0,
            CurrentHealth - damage);
    }

    public void AttackMonster(
    Monster monster)
    {
        int damage = Math.Max(
            1,
            Attack - monster.Defense);

        monster.TakeDamage(damage);
    }
}