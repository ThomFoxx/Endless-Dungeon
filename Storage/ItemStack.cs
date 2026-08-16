using EndlessDungeon.Items;

namespace Endless_Dungeon.Storage;

public class ItemStack
{
    private readonly List<Item> _items = new();

    public Item Item => _items[0];
    public int Quantity => _items.Count;

    public ItemStack(Item item)
    {
        _items.Add(item);
    }

    public bool CanAccept(Item item)
    {
        return Item.IsStackable &&
               item.IsStackable &&
               Item.Id == item.Id;
    }

    public bool AddItem(Item item)
    {
        if (!CanAccept(item))
        {
            return false;
        }

        _items.Add(item);
        return true;
    }

    public Item? TakeOne()
    {
        if (_items.Count == 0)
        {
            return null;
        }

        int index = _items.Count - 1;
        Item item = _items[index];

        _items.RemoveAt(index);

        return item;
    }
}