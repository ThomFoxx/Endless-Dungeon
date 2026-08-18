using EndlessDungeon.Items;

namespace EndlessDungeon.Dungeon;

public class Chest
{
    private readonly List<Item> _items = new();

    public int X { get; }
    public int Y { get; }

    public bool IsOpened { get; private set; }

    public string Glyph => IsOpened ? "□" : "▣";
    public ConsoleColor Color => ConsoleColor.DarkYellow;

    public IReadOnlyList<Item> Items => _items;

    public Chest(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public List<Item> Open()
    {
        if (IsOpened)
        {
            return new List<Item>();
        }

        IsOpened = true;

        List<Item> contents = new(_items);
        _items.Clear();

        return contents;
    }

    public void RestoreOpenedState(bool isOpened)
    {
        IsOpened = isOpened;

        if (IsOpened)
        {
            _items.Clear();
        }
    }
}