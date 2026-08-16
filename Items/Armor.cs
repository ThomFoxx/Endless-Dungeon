namespace EndlessDungeon.Items;

public class Armor : Item
{
    public int DefenseBonus { get; }

    public Armor(string id, string name, string glyph, ConsoleColor color,
        string description, int defenseBonus)
        : base(id, name, glyph, color, description)
    {
        DefenseBonus = defenseBonus;
    }
}