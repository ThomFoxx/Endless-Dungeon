namespace EndlessDungeon.Items;

public class Weapon : Item
{
    public int AttackBonus { get; }

    public Weapon(string id, string name, string glyph, ConsoleColor color,
        string description, int attackBonus)
        : base(id, name, glyph, color, description)
    {
        AttackBonus = attackBonus;
    }
}