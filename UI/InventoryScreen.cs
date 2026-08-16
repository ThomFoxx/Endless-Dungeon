using EndlessDungeon.Characters;
using EndlessDungeon.Input;
using EndlessDungeon.Items;
using EndlessDungeon.Rendering;

namespace EndlessDungeon.UI;

public class InventoryScreen
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;
    private readonly ActionLog _actionLog;

    public InventoryScreen(
        ConsoleRenderer renderer,
        InputManager inputManager,
        ActionLog actionLog)
    {
        _renderer = renderer;
        _inputManager = inputManager;
        _actionLog = actionLog;
    }

    public bool Show(Explorer explorer, bool isInDungeon)
    {
        int selectedIndex = 0;
        bool isViewingInventory = true;
        bool actionTaken = false;

        string statusMessage = string.Empty;

        while (isViewingInventory)
        {
            _renderer.Clear();
            _renderer.WriteTitle("INVENTORY");

            WriteExplorerStats(explorer);

            if (explorer.Inventory.Count == 0)
            {
                WriteEmptyInventory(statusMessage);

                ConsoleKey emptyKey = _inputManager.ReadKey();

                if (emptyKey == ConsoleKey.I || emptyKey == ConsoleKey.Escape)
                {
                    isViewingInventory = false;
                }

                continue;
            }

            selectedIndex = Math.Clamp(
                selectedIndex,
                0,
                explorer.Inventory.Count - 1);

            WriteInventoryList(explorer, selectedIndex);

            Item selectedItem = explorer.Inventory[selectedIndex];

            WriteItemDetails(explorer, selectedItem);

            if (!string.IsNullOrEmpty(statusMessage))
            {
                Console.WriteLine();
                Console.WriteLine(statusMessage);
            }

            WriteControls(selectedItem);

            ConsoleKey key = _inputManager.ReadKey();

            switch (key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = explorer.Inventory.Count - 1;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    selectedIndex++;

                    if (selectedIndex >= explorer.Inventory.Count)
                    {
                        selectedIndex = 0;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.E:
                case ConsoleKey.Enter:
                    actionTaken = HandleInventoryAction(
                        explorer,
                        selectedItem,
                        out statusMessage);

                    if (actionTaken && isInDungeon)
                    {
                        isViewingInventory = false;
                    }

                    if (explorer.Inventory.Count > 0)
                    {
                        selectedIndex = Math.Clamp(
                            selectedIndex,
                            0,
                            explorer.Inventory.Count - 1);
                    }

                    break;

                case ConsoleKey.I:
                case ConsoleKey.Escape:
                    isViewingInventory = false;
                    break;
            }
        }

        _renderer.Clear();

        return actionTaken;
    }

    private void WriteExplorerStats(Explorer explorer)
    {
        Console.WriteLine();
        Console.WriteLine(explorer.Name);
        Console.WriteLine($"Attack: {explorer.Attack}");
        Console.WriteLine($"Defense: {explorer.Defense}");
        Console.WriteLine();

        Console.Write("Weapon: ");

        if (explorer.EquippedWeapon != null)
        {
            Console.ForegroundColor = explorer.EquippedWeapon.Color;
            Console.Write(explorer.EquippedWeapon.Glyph);
            Console.ResetColor();

            Console.WriteLine(
                $"  {explorer.EquippedWeapon.Name} " +
                $"(+{explorer.EquippedWeapon.AttackBonus} Attack)");
        }
        else
        {
            Console.WriteLine("None");
        }

        Console.Write("Armor:  ");

        if (explorer.EquippedArmor != null)
        {
            Console.ForegroundColor = explorer.EquippedArmor.Color;
            Console.Write(explorer.EquippedArmor.Glyph);
            Console.ResetColor();

            Console.WriteLine(
                $"  {explorer.EquippedArmor.Name} " +
                $"(+{explorer.EquippedArmor.DefenseBonus} Defense)");
        }
        else
        {
            Console.WriteLine("None");
        }

        Console.WriteLine();
        Console.WriteLine($"Backpack Items: {explorer.Inventory.Count}");
        Console.WriteLine();
    }

    private void WriteEmptyInventory(string statusMessage)
    {
        Console.WriteLine("Your inventory is empty.");

        if (!string.IsNullOrEmpty(statusMessage))
        {
            Console.WriteLine();
            Console.WriteLine(statusMessage);
        }

        Console.WriteLine();
        Console.WriteLine("I / Escape - Return");
    }

    private void WriteInventoryList(Explorer explorer, int selectedIndex)
    {
        for (int i = 0; i < explorer.Inventory.Count; i++)
        {
            Item item = explorer.Inventory[i];

            Console.Write(i == selectedIndex ? "> " : "  ");

            Console.ForegroundColor = item.Color;
            Console.Write(item.Glyph);
            Console.ResetColor();

            Console.WriteLine($"  {item.Name}");
        }
    }

    private void WriteItemDetails(Explorer explorer, Item item)
    {
        Console.WriteLine();
        Console.WriteLine("────────────────────────────────────────");
        Console.WriteLine(item.Name);
        Console.WriteLine(item.Description);

        switch (item)
        {
            case Weapon weapon:
                Console.WriteLine($"Attack Bonus: +{weapon.AttackBonus}");
                Console.WriteLine(
                    $"Attack if Equipped: {explorer.BaseAttack + weapon.AttackBonus}");
                break;

            case Armor armor:
                Console.WriteLine($"Defense Bonus: +{armor.DefenseBonus}");
                Console.WriteLine(
                    $"Defense if Equipped: {explorer.BaseDefense + armor.DefenseBonus}");
                break;

            case HealingPotion potion:
                Console.WriteLine($"Healing: {potion.HealingAmount} HP");
                break;
        }
    }

    private void WriteControls(Item selectedItem)
    {
        Console.WriteLine();
        Console.WriteLine("Up / Down - Select");

        if (selectedItem is Consumable)
        {
            Console.WriteLine("E / Enter - Use");
        }
        else
        {
            Console.WriteLine("E / Enter - Equip");
        }

        Console.WriteLine("I / Escape - Return");
    }

    private bool HandleInventoryAction(
        Explorer explorer,
        Item item,
        out string statusMessage)
    {
        switch (item)
        {
            case Weapon weapon:
                Weapon? oldWeapon = explorer.EquippedWeapon;

                if (!explorer.EquipWeapon(weapon))
                {
                    statusMessage = "Unable to equip that weapon.";
                    return false;
                }

                statusMessage = $"Equipped {weapon.Name}.";

                if (oldWeapon != null)
                {
                    _actionLog.Add(
                        $"You equip {weapon.Name} and stow {oldWeapon.Name}.");
                }
                else
                {
                    _actionLog.Add($"You equip {weapon.Name}.");
                }

                return true;

            case Armor armor:
                Armor? oldArmor = explorer.EquippedArmor;

                if (!explorer.EquipArmor(armor))
                {
                    statusMessage = "Unable to equip that armor.";
                    return false;
                }

                statusMessage = $"Equipped {armor.Name}.";

                if (oldArmor != null)
                {
                    _actionLog.Add(
                        $"You equip {armor.Name} and stow {oldArmor.Name}.");
                }
                else
                {
                    _actionLog.Add($"You equip {armor.Name}.");
                }

                return true;

            case Consumable consumable:
                if (!consumable.TryUse(explorer, out statusMessage))
                {
                    return false;
                }

                explorer.RemoveItem(consumable);
                _actionLog.Add(statusMessage);

                return true;

            default:
                statusMessage = "You cannot use that item.";
                return false;
        }
    }
}