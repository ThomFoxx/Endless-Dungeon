using Endless_Dungeon.Storage;
using EndlessDungeon.Characters;
using EndlessDungeon.Input;
using EndlessDungeon.Items;
using EndlessDungeon.Rendering;
using EndlessDungeon.Storage;

namespace EndlessDungeon.UI;

public class StorageChestScreen
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;

    public StorageChestScreen(ConsoleRenderer renderer, InputManager inputManager)
    {
        _renderer = renderer;
        _inputManager = inputManager;
    }

    public void Show(Explorer explorer, StorageChest storageChest)
    {
        int backpackIndex = 0;
        int storageIndex = 0;

        bool selectingBackpack = true;
        bool isViewingStorage = true;

        string statusMessage = string.Empty;

        while (isViewingStorage)
        {
            _renderer.Clear();
            _renderer.WriteTitle("STORAGE CHEST");

            Console.WriteLine();
            Console.WriteLine(explorer.Name);
            Console.WriteLine();

            Console.WriteLine("Equipped:");
            Console.WriteLine($"  Weapon: {explorer.EquippedWeapon?.Name ?? "None"}");
            Console.WriteLine($"  Armor:  {explorer.EquippedArmor?.Name ?? "None"}");

            Console.WriteLine();

            backpackIndex = ClampSelection(backpackIndex, explorer.Inventory.Count);
            storageIndex = ClampSelection(storageIndex, storageChest.Stacks.Count);

            WriteBackpack(
                explorer,
                backpackIndex,
                selectingBackpack);

            Console.WriteLine();

            WriteStorage(
                storageChest,
                storageIndex,
                !selectingBackpack);

            Item? selectedItem = selectingBackpack
                ? GetItemAt(explorer.Inventory, backpackIndex)
                : GetStackAt(storageChest.Stacks, storageIndex)?.Item;

            if (selectedItem != null)
            {
                Console.WriteLine();
                Console.WriteLine("────────────────────────────────────────");
                WriteItemDetails(selectedItem);
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                Console.WriteLine();
                Console.WriteLine(statusMessage);
            }

            Console.WriteLine();
            Console.WriteLine("Up / Down       Select");
            Console.WriteLine("Tab             Switch Container");
            Console.WriteLine("E / Enter       Transfer Item");
            Console.WriteLine("C / Escape      Return to Camp");

            ConsoleKey key = _inputManager.ReadKey();

            switch (key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    if (selectingBackpack)
                    {
                        backpackIndex--;
                    }
                    else
                    {
                        storageIndex--;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    if (selectingBackpack)
                    {
                        backpackIndex++;
                    }
                    else
                    {
                        storageIndex++;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.Tab:
                    selectingBackpack = !selectingBackpack;
                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.E:
                case ConsoleKey.Enter:
                    if (selectingBackpack)
                    {
                        statusMessage = MoveItemToStorage(
                            explorer,
                            storageChest,
                            backpackIndex);
                    }
                    else
                    {
                        statusMessage = MoveItemToBackpack(
                            explorer,
                            storageChest,
                            storageIndex);
                    }

                    break;

                case ConsoleKey.C:
                case ConsoleKey.Escape:
                    isViewingStorage = false;
                    break;
            }
        }

        _renderer.Clear();
    }

    private void WriteBackpack(Explorer explorer, int selectedIndex, bool isActive)
    {
        Console.ForegroundColor = isActive
            ? ConsoleColor.Cyan
            : ConsoleColor.Gray;

        Console.WriteLine($"BACKPACK ({explorer.Inventory.Count})");
        Console.ResetColor();

        WriteItemList(explorer.Inventory, selectedIndex, isActive);
    }

    private void WriteStorage(StorageChest storageChest, int selectedIndex, bool isActive)
    {
        Console.ForegroundColor = isActive
            ? ConsoleColor.Cyan
            : ConsoleColor.Gray;

        Console.WriteLine($"STORAGE CHEST ({storageChest.ItemCount})");
        Console.ResetColor();

        WriteStackList(storageChest.Stacks, selectedIndex, isActive);
    }

    private void WriteItemList(IReadOnlyList<Item> items, int selectedIndex, bool isActive)
    {
        if (items.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Empty");
            Console.ResetColor();
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            Item item = items[i];

            if (isActive && i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("> ");
            }
            else
            {
                Console.Write("  ");
            }

            Console.ForegroundColor = item.Color;
            Console.Write(item.Glyph);

            Console.ResetColor();
            Console.WriteLine($"  {item.Name}");
        }
    }

    private void WriteStackList(IReadOnlyList<ItemStack> stacks, int selectedIndex, bool isActive)
    {
        if (stacks.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  Empty");
            Console.ResetColor();
            return;
        }

        for (int i = 0; i < stacks.Count; i++)
        {
            ItemStack stack = stacks[i];
            Item item = stack.Item;

            if (isActive && i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("> ");
            }
            else
            {
                Console.Write("  ");
            }

            Console.ForegroundColor = item.Color;
            Console.Write(item.Glyph);

            Console.ResetColor();
            Console.Write($"  {item.Name}");

            if (stack.Quantity > 1)
            {
                Console.Write($" x{stack.Quantity}");
            }

            Console.WriteLine();
        }
    }

    private void WriteItemDetails(Item item)
    {
        Console.WriteLine(item.Name);
        Console.WriteLine(item.Description);

        switch (item)
        {
            case Weapon weapon:
                Console.WriteLine($"Attack Bonus: +{weapon.AttackBonus}");
                break;

            case Armor armor:
                Console.WriteLine($"Defense Bonus: +{armor.DefenseBonus}");
                break;

            case HealingPotion potion:
                Console.WriteLine($"Healing: {potion.HealingAmount} HP");
                break;
        }
    }

    private string MoveItemToStorage(Explorer explorer, StorageChest storageChest, int index)
    {
        Item? item = GetItemAt(explorer.Inventory, index);

        if (item == null)
        {
            return "There is nothing in your backpack to store.";
        }

        if (!explorer.RemoveItem(item))
        {
            return "Unable to move that item.";
        }

        storageChest.AddItem(item);

        return $"{item.Name} placed in storage.";
    }

    private string MoveItemToBackpack(Explorer explorer, StorageChest storageChest, int index)
    {
        ItemStack? stack = GetStackAt(storageChest.Stacks, index);

        if (stack == null)
        {
            return "There is nothing in storage to take.";
        }

        Item? item = storageChest.TakeOne(stack);

        if (item == null)
        {
            return "Unable to remove that item from storage.";
        }

        explorer.AddItem(item);

        return $"{item.Name} added to your backpack.";
    }

    private int ClampSelection(int index, int itemCount)
    {
        if (itemCount == 0)
        {
            return 0;
        }

        if (index < 0)
        {
            return itemCount - 1;
        }

        if (index >= itemCount)
        {
            return 0;
        }

        return index;
    }

    private Item? GetItemAt(IReadOnlyList<Item> items, int index)
    {
        if (index < 0 || index >= items.Count)
        {
            return null;
        }

        return items[index];
    }

    private ItemStack? GetStackAt(IReadOnlyList<ItemStack> stacks, int index)
    {
        if (index < 0 || index >= stacks.Count)
        {
            return null;
        }

        return stacks[index];
    }
}