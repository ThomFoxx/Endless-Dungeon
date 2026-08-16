namespace EndlessDungeon.Items;

public abstract class Item
{
    public string Id { get; }
    public string Name { get; }
    public string Glyph { get; }
    public ConsoleColor Color { get; }
    public string Description { get; }

    public virtual bool IsStackable => false;

    protected Item(string id, string name, string glyph, ConsoleColor color, string description)
    {
        Id = id;
        Name = name;
        Glyph = glyph;
        Color = color;
        Description = description;
    }
}