using Endless_Dungeon.Storage;
using EndlessDungeon.Items;

namespace EndlessDungeon.Storage;

public class StorageChest
{
    private readonly List<ItemStack> _stacks = new();

    public IReadOnlyList<ItemStack> Stacks => _stacks;

    public int ItemCount
    {
        get
        {
            int count = 0;

            foreach (ItemStack stack in _stacks)
            {
                count += stack.Quantity;
            }

            return count;
        }
    }

    public void AddItem(Item item)
    {
        if (item.IsStackable)
        {
            foreach (ItemStack stack in _stacks)
            {
                if (stack.CanAccept(item))
                {
                    stack.AddItem(item);
                    return;
                }
            }
        }

        _stacks.Add(new ItemStack(item));
    }

    public Item? TakeOne(ItemStack stack)
    {
        if (!_stacks.Contains(stack))
        {
            return null;
        }

        Item? item = stack.TakeOne();

        if (stack.Quantity == 0)
        {
            _stacks.Remove(stack);
        }

        return item;
    }

    public void Clear()
    {
        _stacks.Clear();
    }
}