using EndlessDungeon.Characters;

namespace EndlessDungeon.Items;

public abstract class Consumable : Item
{
    public override bool IsStackable => true;

    protected Consumable(string id, string name, string glyph, ConsoleColor color,
        string description)
        : base(id, name, glyph, color, description)
    {
    }

    public abstract bool TryUse(Explorer explorer, out string message);
}