using EndlessDungeon.Input;
using EndlessDungeon.Rendering;
using EndlessDungeon.Dungeon;
using EndlessDungeon.Characters;
using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.Records;
using EndlessDungeon.UI;
using EndlessDungeon.Items;
using EndlessDungeon.Storage;
using EndlessDungeon.Saving;
using Endless_Dungeon.Storage;

namespace EndlessDungeon;

public class Game
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;
    private readonly HonorBoard _honorBoard;
    private readonly ActionLog _actionLog;
    private readonly StorageChest _storageChest;
    private readonly SaveManager _saveManager;

    private Explorer _explorer;
    private DungeonRun _dungeonRun;

    private bool _isRunning = true;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();
        _honorBoard = new HonorBoard();
        _actionLog = new ActionLog();
        _storageChest = new StorageChest();

        _explorer = CreateNewExplorer();
        _dungeonRun = new DungeonRun(_explorer.DungeonSeed);
        _saveManager = new SaveManager();
    }

    private Explorer CreateNewExplorer()
    {
        int explorerNumber = _honorBoard.Records.Count + 1;
        string name = $"Explorer {explorerNumber}";

        int dungeonSeed = Random.Shared.Next(int.MinValue, int.MaxValue);

        Explorer explorer = new(name, dungeonSeed);

        Weapon chippedSword = (Weapon)ItemFactory.Create(ItemIds.ChippedSword);

        explorer.EquipStartingWeapon(chippedSword);

        return explorer;
    }

    public void Run()
    {
        _renderer.Initialize();

        while (_isRunning)
        {
            ShowCamp();
        }
    }

    private void ShowCamp()
    {
        _renderer.Clear();
        _renderer.WriteTitle("EXPLORER'S CAMP");

        Console.WriteLine();
        Console.WriteLine($"  Explorer: {_explorer.Name}");
        Console.WriteLine($"  HP: {_explorer.CurrentHealth}/{_explorer.MaxHealth}");
        Console.WriteLine($"  Deepest Floor: {_explorer.DeepestFloorReached}");

        Console.WriteLine();
        Console.WriteLine("  [E] Enter Dungeon");
        Console.WriteLine("  [I] Inventory");
        Console.WriteLine("  [C] Storage Chest");
        Console.WriteLine("  [H] Honor Board");
        Console.WriteLine("  [S] Save Game");
        Console.WriteLine("  [L] Load Game");
        Console.WriteLine("  [Q] Save & Quit");

        Console.WriteLine();
        Console.WriteLine("  [F1] Help");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  [X] DEBUG - Simulate Explorer Death");
        Console.WriteLine("  [T] DEBUG - Glyph Test");
        Console.WriteLine("  [D] DEBUG - Load Dungeon Seed");
        Console.ResetColor();

        ConsoleKey key = _inputManager.ReadKey();

        switch (key)
        {
            case ConsoleKey.E:
                RunDungeon();
                break;

            case ConsoleKey.I:
                ShowInventory(isInDungeon: false);
                break;

            case ConsoleKey.C:
                ShowStorageChest();
                break;

            case ConsoleKey.H:
                ShowHonorBoard();
                break;

            case ConsoleKey.S:
                SaveGame();
                break;

            case ConsoleKey.Q:
                SaveGame();
                _isRunning = false;
                break;

            case ConsoleKey.F1:
                ShowHelp();
                break;

            case ConsoleKey.X:
                HandleDebugDeath();
                break;

            case ConsoleKey.T:
                ShowGlyphTest();
                break;

            case ConsoleKey.L:
                if (LoadGame())
                {
                    _actionLog.Add("Game loaded.");
                }
                else
                {
                    _actionLog.Add("No save file found.");
                }
                break;

            case ConsoleKey.D:
                ShowDungeonSeedLoader();
                break;
        }
    }

    private void ShowDungeonSeedLoader()
    {
        _renderer.Clear();
        _renderer.WriteTitle("LOAD DUNGEON SEED");

        Console.WriteLine();
        Console.WriteLine($"Current Seed: {_explorer.DungeonSeed}");
        Console.WriteLine();
        Console.WriteLine("Enter a signed 32-bit dungeon seed.");
        Console.WriteLine("Leave blank to cancel.");
        Console.WriteLine();

        Console.Write("Seed: ");

        Console.CursorVisible = true;
        string? input = Console.ReadLine();
        Console.CursorVisible = false;

        if (string.IsNullOrWhiteSpace(input))
        {
            _renderer.Clear();
            return;
        }

        if (!int.TryParse(input, out int seed))
        {
            Console.WriteLine();
            Console.WriteLine("Invalid seed.");
            Console.WriteLine("Press any key to return.");

            _inputManager.ReadKey();
            _renderer.Clear();
            return;
        }

        _explorer.SetDungeonSeed(seed);

        // Discard any previously generated dungeon and start fresh
        // with the supplied seed.
        _dungeonRun = new DungeonRun(seed);

        Console.WriteLine();
        Console.WriteLine($"Dungeon seed changed to {seed}.");
        Console.WriteLine("The next expedition will generate a fresh dungeon.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return.");

        _inputManager.ReadKey();
        _renderer.Clear();
    }

    private void ShowHelp()
    {
        _renderer.Clear();
        _renderer.WriteTitle("HELP");

        Console.WriteLine();
        Console.WriteLine("Surface Controls");
        Console.WriteLine();
        Console.WriteLine("  E     Enter Dungeon");
        Console.WriteLine("  I     Inventory");
        Console.WriteLine("  C     Storage Chest");
        Console.WriteLine("  H     Honor Board");
        Console.WriteLine("  S     Save");
        Console.WriteLine("  Q     Save & Quit");
        Console.WriteLine("  F1    Help");
        Console.WriteLine();
        Console.WriteLine("Press any key to return.");

        _inputManager.ReadKey();
    }

    private void ShowHonorBoard()
    {
        _renderer.Clear();
        _renderer.WriteTitle("HONOR BOARD");

        Console.WriteLine();

        if (_honorBoard.Records.Count == 0)
        {
            Console.WriteLine(
                "No explorers have yet fallen in the Endless Dungeon.");
        }
        else
        {
            foreach (HonorRecord record in _honorBoard.Records)
            {
                Console.WriteLine(
                    $"{record.ExplorerName}");

                Console.WriteLine(
                    $"  Level: {record.Level}");

                Console.WriteLine(
                    $"  Deepest Floor: {record.DeepestFloor}");

                Console.WriteLine(
                    $"  {record.CauseOfDeath}");

                Console.WriteLine();
            }
        }

        Console.WriteLine(
            "Press any key to return to camp.");

        _inputManager.ReadKey();
    }

    private void RunDungeon()
    {
        VisibilityManager visibilityManager = new();

        DungeonFloor floor = _dungeonRun.BeginExpedition();

        _explorer.SetPosition(floor.StartX, floor.StartY);

        _actionLog.Clear();
        _actionLog.Add("You enter the Endless Dungeon.");

        bool isExploring = true;

        _renderer.Clear();

        while (isExploring)
        {
            visibilityManager.UpdateVisibility(floor, _explorer.X, _explorer.Y);
            _renderer.DrawDungeon(floor, _explorer, _actionLog);

            ConsoleKey key = _inputManager.ReadKey();

            int moveX = 0;
            int moveY = 0;
            bool turnTaken = false;

            int healthBeforeAction = _explorer.CurrentHealth;

            switch (key)
            {
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    moveY = -1;
                    break;

                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    moveY = 1;
                    break;

                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    moveX = -1;
                    break;

                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    moveX = 1;
                    break;

                case ConsoleKey.G:
                    turnTaken = TryPickUpItem(floor);
                    break;

                case ConsoleKey.I:
                    turnTaken = ShowInventory(isInDungeon: true);

                    if (turnTaken)
                    {
                        BeginMonsterRound(floor);
                    }

                    break;

                case ConsoleKey.E:
                    Tile currentTile = floor.GetTile(_explorer.X, _explorer.Y);

                    if (currentTile.Type == TileType.StairsDown)
                    {
                        floor = _dungeonRun.Descend();

                        _explorer.RecordFloorReached(floor.FloorNumber);
                        _explorer.SetPosition(floor.StairsUpX, floor.StairsUpY);

                        _actionLog.Add($"You descend to Floor {floor.FloorNumber}.");
                    }
                    else if (currentTile.Type == TileType.StairsUp &&
                             _dungeonRun.CurrentFloorNumber > 1)
                    {
                        floor = _dungeonRun.Ascend();

                        _explorer.SetPosition(floor.StairsDownX, floor.StairsDownY);

                        _actionLog.Add($"You ascend to Floor {floor.FloorNumber}.");
                    }
                    else if (currentTile.Type == TileType.ExitPortal)
                    {
                        isExploring = false;

                        ShowDungeonExitMessage();
                        continue;
                    }

                    continue;

                case ConsoleKey.Escape:
                    isExploring = false;
                    continue;
            }

            if (moveX != 0 || moveY != 0)
            {
                turnTaken = TryMoveExplorer(floor, moveX, moveY);
            }

            if (turnTaken && _explorer.IsAlive)
            {
                RunMonsterTurns(floor);
            }

            if (_explorer.CurrentHealth < healthBeforeAction)
            {
                visibilityManager.UpdateVisibility(floor, _explorer.X, _explorer.Y);
                _renderer.DrawDungeon(floor, _explorer, _actionLog);
                _renderer.FlashExplorerHit(_explorer);
            }

            if (!_explorer.IsAlive)
            {
                isExploring = false;
                HandleExplorerDeath();
            }
        }
    }

    private void ShowDungeonExitMessage()
    {
        _renderer.Clear();
        _renderer.WriteTitle("EXPEDITION COMPLETE");

        Console.WriteLine();
        Console.WriteLine("You step through the portal and return safely.");
        Console.WriteLine();
        Console.WriteLine("Your expedition has been completed.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return to camp.");

        _inputManager.ReadKey();
    }

    private bool ShowInventory(bool isInDungeon)
    {
        int selectedIndex = 0;
        bool isViewingInventory = true;
        bool actionTaken = false;

        string statusMessage = string.Empty;

        while (isViewingInventory)
        {
            _renderer.Clear();
            _renderer.WriteTitle("INVENTORY");

            Console.WriteLine();
            Console.WriteLine(_explorer.Name);
            Console.WriteLine($"Attack: {_explorer.Attack}");
            Console.WriteLine($"Defense: {_explorer.Defense}");
            Console.WriteLine();

            Console.Write("Weapon: ");

            if (_explorer.EquippedWeapon != null)
            {
                Console.ForegroundColor = _explorer.EquippedWeapon.Color;
                Console.Write(_explorer.EquippedWeapon.Glyph);
                Console.ResetColor();

                Console.WriteLine(
                    $"  {_explorer.EquippedWeapon.Name} " +
                    $"(+{_explorer.EquippedWeapon.AttackBonus} Attack)");
            }
            else
            {
                Console.WriteLine("None");
            }

            Console.Write("Armor:  ");

            if (_explorer.EquippedArmor != null)
            {
                Console.ForegroundColor = _explorer.EquippedArmor.Color;
                Console.Write(_explorer.EquippedArmor.Glyph);
                Console.ResetColor();

                Console.WriteLine(
                    $"  {_explorer.EquippedArmor.Name} " +
                    $"(+{_explorer.EquippedArmor.DefenseBonus} Defense)");
            }
            else
            {
                Console.WriteLine("None");
            }

            Console.WriteLine();
            Console.WriteLine($"Backpack Items: {_explorer.Inventory.Count}");
            Console.WriteLine();

            if (_explorer.Inventory.Count == 0)
            {
                Console.WriteLine("Your inventory is empty.");

                if (!string.IsNullOrEmpty(statusMessage))
                {
                    Console.WriteLine();
                    Console.WriteLine(statusMessage);
                }

                Console.WriteLine();
                Console.WriteLine("I / Escape - Return");

                ConsoleKey emptyKey = _inputManager.ReadKey();

                if (emptyKey == ConsoleKey.I || emptyKey == ConsoleKey.Escape)
                {
                    isViewingInventory = false;
                }

                continue;
            }

            selectedIndex = Math.Clamp(selectedIndex, 0, _explorer.Inventory.Count - 1);

            for (int i = 0; i < _explorer.Inventory.Count; i++)
            {
                Item item = _explorer.Inventory[i];

                Console.Write(i == selectedIndex ? "> " : "  ");

                Console.ForegroundColor = item.Color;
                Console.Write(item.Glyph);
                Console.ResetColor();

                Console.WriteLine($"  {item.Name}");
            }

            Item selectedItem = _explorer.Inventory[selectedIndex];

            Console.WriteLine();
            Console.WriteLine("────────────────────────────────────────");
            Console.WriteLine(selectedItem.Name);
            Console.WriteLine(selectedItem.Description);

            switch (selectedItem)
            {
                case Weapon weapon:
                    Console.WriteLine($"Attack Bonus: +{weapon.AttackBonus}");
                    Console.WriteLine(
                        $"Attack if Equipped: {_explorer.BaseAttack + weapon.AttackBonus}");
                    break;

                case Armor armor:
                    Console.WriteLine($"Defense Bonus: +{armor.DefenseBonus}");
                    Console.WriteLine(
                        $"Defense if Equipped: {_explorer.BaseDefense + armor.DefenseBonus}");
                    break;

                case HealingPotion potion:
                    Console.WriteLine($"Healing: {potion.HealingAmount} HP");
                    break;
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                Console.WriteLine();
                Console.WriteLine(statusMessage);
            }

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

            ConsoleKey key = _inputManager.ReadKey();

            switch (key)
            {
                case ConsoleKey.UpArrow:
                case ConsoleKey.W:
                    selectedIndex--;

                    if (selectedIndex < 0)
                    {
                        selectedIndex = _explorer.Inventory.Count - 1;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.DownArrow:
                case ConsoleKey.S:
                    selectedIndex++;

                    if (selectedIndex >= _explorer.Inventory.Count)
                    {
                        selectedIndex = 0;
                    }

                    statusMessage = string.Empty;
                    break;

                case ConsoleKey.E:
                case ConsoleKey.Enter:
                    actionTaken = HandleInventoryAction(selectedItem, out statusMessage);

                    if (actionTaken && isInDungeon)
                    {
                        isViewingInventory = false;
                    }

                    if (_explorer.Inventory.Count > 0)
                    {
                        selectedIndex = Math.Clamp(
                            selectedIndex,
                            0,
                            _explorer.Inventory.Count - 1);
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

    private void ShowStorageChest()
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
            Console.WriteLine($"{_explorer.Name}");
            Console.WriteLine();

            Console.WriteLine("Equipped:");
            Console.WriteLine($"  Weapon: {_explorer.EquippedWeapon?.Name ?? "None"}");
            Console.WriteLine($"  Armor:  {_explorer.EquippedArmor?.Name ?? "None"}");

            Console.WriteLine();

            backpackIndex = ClampSelection(backpackIndex, _explorer.Inventory.Count);
            storageIndex = ClampSelection(storageIndex, _storageChest.Stacks.Count);

            Console.ForegroundColor = selectingBackpack
                ? ConsoleColor.Cyan
                : ConsoleColor.Gray;

            Console.WriteLine($"BACKPACK ({_explorer.Inventory.Count})");
            Console.ResetColor();

            WriteStorageItemList(
                _explorer.Inventory,
                backpackIndex,
                selectingBackpack);

            Console.WriteLine();

            Console.ForegroundColor = !selectingBackpack
                ? ConsoleColor.Cyan
                : ConsoleColor.Gray;

            Console.WriteLine($"STORAGE CHEST ({_storageChest.ItemCount})");
            Console.ResetColor();

            WriteStorageStackList(
                _storageChest.Stacks,
                storageIndex,
                !selectingBackpack);

            Item? selectedItem = selectingBackpack
                ? GetItemAt(_explorer.Inventory, backpackIndex)
                : GetStackAt(_storageChest.Stacks, storageIndex)?.Item;

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
                        statusMessage = MoveItemToStorage(backpackIndex);
                    }
                    else
                    {
                        statusMessage = MoveItemToBackpack(storageIndex);
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

    private bool TryMoveExplorer(DungeonFloor floor, int moveX, int moveY)
    {
        if (moveX == 0 && moveY == 0)
        {
            return false;
        }

        int targetX = _explorer.X + moveX;
        int targetY = _explorer.Y + moveY;

        if (!floor.IsWalkable(targetX, targetY))
        {
            return false;
        }

        BeginMonsterRound(floor);

        Monster? targetMonster = floor.GetMonsterAt(targetX, targetY);

        if (targetMonster != null)
        {
            int damage = _explorer.AttackMonster(targetMonster);

            _actionLog.Add(
                $"You hit {targetMonster.Name} for {damage} damage.");

            // Give immediate visual feedback for the player's hit.
            _renderer.FlashMonsterHit(targetMonster);

            if (targetMonster.IsAlive)
            {
                targetMonster.Retaliate(_explorer, _actionLog);
            }
            else
            {
                _actionLog.Add(
                    $"{targetMonster.Name} is defeated.");

                floor.RemoveMonster(targetMonster);
            }

            return true;
        }

        List<Monster> adjacentMonsters = GetAdjacentMonsters(
            floor,
            _explorer.X,
            _explorer.Y);

        _explorer.SetPosition(targetX, targetY);

        foreach (Monster monster in adjacentMonsters)
        {
            if (!_explorer.IsAlive)
            {
                break;
            }

            int newDistance =
                Math.Abs(monster.X - _explorer.X) +
                Math.Abs(monster.Y - _explorer.Y);

            if (newDistance > 1)
            {
                monster.MakeOpportunityAttack(
                    _explorer,
                    _actionLog);
            }
        }

        return true;
    }

    private void BeginMonsterRound( DungeonFloor floor)
    {
        foreach (Monster monster in floor.Monsters)
        {
            monster.BeginRound();
        }
    }

    private void RunMonsterTurns( DungeonFloor floor)
    {
        foreach (Monster monster in floor.Monsters.ToList())
        {
            if (!_explorer.IsAlive)
            {
                break;
            }

            monster.TakeTurn(
                floor,
                _explorer,
                _actionLog);
        }
    }

    private void HandleDebugDeath()
    {
        string explorerName =
            _explorer.Name;

        int oldSeed =
            _explorer.DungeonSeed;

        _honorBoard.AddDebugExplorer(
            _explorer);

        _explorer = CreateNewExplorer();

        _dungeonRun = new DungeonRun(
            _explorer.DungeonSeed);

        _renderer.Clear();
        _renderer.WriteTitle("DEBUG - EXPLORER DEATH");

        Console.WriteLine();
        Console.WriteLine(
            $"{explorerName} has been removed.");

        Console.WriteLine();
        Console.WriteLine(
            $"Old Dungeon Seed: {oldSeed}");

        Console.WriteLine(
            $"New Dungeon Seed: {_explorer.DungeonSeed}");

        Console.WriteLine();
        Console.WriteLine(
            "The previous dungeon has been destroyed.");

        Console.WriteLine();
        Console.WriteLine(
            "Press any key to return to camp.");

        _inputManager.ReadKey();
    }

    private List<Monster> GetAdjacentMonsters(DungeonFloor floor, int x, int y)
    {
        return floor.Monsters
            .Where(monster =>
                monster.IsAlive &&
                Math.Abs(monster.X - x) +
                Math.Abs(monster.Y - y) == 1)
            .ToList();
    }

    private void HandleExplorerDeath()
    {
        string explorerName =
            _explorer.Name;

        int deepestFloor =
            _explorer.DeepestFloorReached;

        string cause =
            _explorer.LastDamageSource;

        // Preserve the dead explorer's record.
        _honorBoard.AddExplorer(
            _explorer);

        // The dead explorer's dungeon is completely discarded.
        _explorer = CreateNewExplorer();

        _dungeonRun = new DungeonRun(
            _explorer.DungeonSeed);

        _renderer.Clear();
        _renderer.WriteTitle("EXPLORER SLAIN");

        Console.WriteLine();
        Console.WriteLine(
            $"{explorerName} has fallen.");

        Console.WriteLine();
        Console.WriteLine(
            $"Deepest Floor Reached: {deepestFloor}");

        Console.WriteLine(
            $"Cause of Death: Slain by {cause}");

        Console.WriteLine();
        Console.WriteLine(
            "Their dungeon fades away with them.");

        Console.WriteLine();
        Console.WriteLine(
            $"{_explorer.Name} will be the next to enter.");

        Console.WriteLine();
        Console.WriteLine(
            "Press any key to return to camp.");

        _inputManager.ReadKey();
    }

    private bool TryPickUpItem(DungeonFloor floor)
    {
        GroundItem? groundItem = floor.GetGroundItemAt(_explorer.X, _explorer.Y);

        if (groundItem == null)
        {
            _actionLog.Add("There is nothing here to pick up.");
            return false;
        }

        BeginMonsterRound(floor);

        _explorer.AddItem(groundItem.Item);
        floor.RemoveGroundItem(groundItem);

        _actionLog.Add($"You pick up {groundItem.Item.Name}.");

        return true;
    }

    private bool HandleInventoryAction(Item item, out string statusMessage)
    {
        switch (item)
        {
            case Weapon weapon:
                {
                    Weapon? oldWeapon = _explorer.EquippedWeapon;

                    if (!_explorer.EquipWeapon(weapon))
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
                }

            case Armor armor:
                {
                    Armor? oldArmor = _explorer.EquippedArmor;

                    if (!_explorer.EquipArmor(armor))
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
                }

            case Consumable consumable:
                {
                    if (!consumable.TryUse(_explorer, out statusMessage))
                    {
                        return false;
                    }

                    _explorer.RemoveItem(consumable);
                    _actionLog.Add(statusMessage);

                    return true;
                }

            default:
                statusMessage = "You cannot use that item.";
                return false;
        }
    }

    private void ShowGlyphTest()
    {
        bool isViewing = true;

        while (isViewing)
        {
            _renderer.Clear();
            _renderer.WriteTitle("GLYPH TEST");

            Console.WriteLine();
            Console.WriteLine("Symbols should remain one terminal cell wide.");
            Console.WriteLine("The | characters should line up vertically.");
            Console.WriteLine();

            WriteGlyphTest("Explorer", "₽", "U+20BD", ConsoleColor.Cyan);
            WriteGlyphTest("Slime", "●", "U+25CF", ConsoleColor.Green);

            Console.WriteLine();

            WriteGlyphTest("Potion", "¡", "U+00A1", ConsoleColor.Magenta);
            WriteGlyphTest("Weapon", "†", "U+2020", ConsoleColor.White);
            WriteGlyphTest("Armor", "◈", "U+25C8", ConsoleColor.DarkYellow);

            Console.WriteLine();

            WriteGlyphTest("Stairs Up", "▲", "U+25B2", ConsoleColor.White);
            WriteGlyphTest("Stairs Down", "▼", "U+25BC", ConsoleColor.White);
            WriteGlyphTest("Exit Portal", "֍", "U+058D", ConsoleColor.Cyan);

            Console.WriteLine();
            Console.WriteLine("Candidate Explorer / Job Glyphs");
            Console.WriteLine();

            WriteGlyphTest("Fighter A", "‡", "U+2021", ConsoleColor.Red);
            WriteGlyphTest("Fighter B", "╬", "U+256C", ConsoleColor.Red);
            WriteGlyphTest("Fighter C", "Ϯ", "U+03EE", ConsoleColor.Red);
            WriteGlyphTest("Fighter D", "Ӿ", "U+04FE", ConsoleColor.Red);
            WriteGlyphTest("Rogue", "⚿", "U+26BF", ConsoleColor.DarkYellow);
            WriteGlyphTest("Ranger", "⌖", "U+2316", ConsoleColor.Green);
            WriteGlyphTest("Mage", "✦", "U+2726", ConsoleColor.Magenta);
            WriteGlyphTest("Scout", "♠", "U+2660", ConsoleColor.DarkGreen);

            Console.WriteLine();
            Console.WriteLine("Candidate Creature / Item Glyphs");
            Console.WriteLine();

            WriteGlyphTest("Candidate", "♣", "U+2663", ConsoleColor.Green);
            WriteGlyphTest("Candidate", "¤", "U+00A4", ConsoleColor.Yellow);
            WriteGlyphTest("Candidate", "◆", "U+25C6", ConsoleColor.Cyan);
            WriteGlyphTest("Candidate", "Ψ", "U+03A8", ConsoleColor.Red);
            WriteGlyphTest("Candidate", "Ѻ", "U+047A", ConsoleColor.DarkMagenta);
            WriteGlyphTest("Candidate", "Ӝ", "U+04DC", ConsoleColor.DarkYellow);

            Console.WriteLine();
            Console.WriteLine("Escape / T - Return");

            ConsoleKey key = _inputManager.ReadKey();

            if (key == ConsoleKey.Escape || key == ConsoleKey.T)
            {
                isViewing = false;
            }
        }

        _renderer.Clear();
    }

    private void WriteGlyphTest( string label, string glyph, string codePoint, ConsoleColor color)
    {
        Console.Write($"{label,-14} ");

        Console.ForegroundColor = color;
        Console.Write(glyph);
        Console.ResetColor();

        Console.WriteLine($"|  {codePoint}");
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

    private void WriteStorageItemList( IReadOnlyList<Item> items, int selectedIndex, bool isActive)
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

    private string MoveItemToStorage(int index)
    {
        Item? item = GetItemAt(_explorer.Inventory, index);

        if (item == null)
        {
            return "There is nothing in your backpack to store.";
        }

        if (!_explorer.RemoveItem(item))
        {
            return "Unable to move that item.";
        }

        _storageChest.AddItem(item);

        return $"{item.Name} placed in storage.";
    }

    private string MoveItemToBackpack(int index)
    {
        ItemStack? stack = GetStackAt(_storageChest.Stacks, index);

        if (stack == null)
        {
            return "There is nothing in storage to take.";
        }

        Item? item = _storageChest.TakeOne(stack);

        if (item == null)
        {
            return "Unable to remove that item from storage.";
        }

        _explorer.AddItem(item);

        return $"{item.Name} added to your backpack.";
    }

    private void WriteStorageStackList( IReadOnlyList<ItemStack> stacks, int selectedIndex, bool isActive)
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

    private ItemStack? GetStackAt(IReadOnlyList<ItemStack> stacks, int index)
    {
        if (index < 0 || index >= stacks.Count)
        {
            return null;
        }

        return stacks[index];
    }

    private SaveData CreateSaveData()
    {
        SaveData saveData = new();

        saveData.Explorer = new ExplorerSaveData
        {
            Name = _explorer.Name,
            DungeonSeed = _explorer.DungeonSeed,
            Level = _explorer.Level,
            Experience = _explorer.Experience,
            CurrentHealth = _explorer.CurrentHealth,
            DeepestFloorReached = _explorer.DeepestFloorReached,
            EquippedWeaponId = _explorer.EquippedWeapon?.Id,
            EquippedArmorId = _explorer.EquippedArmor?.Id,
            InventoryItemIds = _explorer.Inventory
                .Select(item => item.Id)
                .ToList()
        };

        foreach (ItemStack stack in _storageChest.Stacks)
        {
            saveData.Storage.Add(new StorageStackSaveData
            {
                ItemId = stack.Item.Id,
                Quantity = stack.Quantity
            });
        }

        foreach (HonorRecord record in _honorBoard.Records)
        {
            saveData.HonorBoard.Add(new HonorRecordSaveData
            {
                ExplorerName = record.ExplorerName,
                Level = record.Level,
                DeepestFloor = record.DeepestFloor,
                CauseOfDeath = record.CauseOfDeath
            });
        }

        return saveData;
    }

    private void SaveGame()
    {
        SaveData saveData = CreateSaveData();

        _saveManager.Save(saveData);

        _actionLog.Add("Game saved.");
    }

    private bool LoadGame()
    {
        SaveData? saveData = _saveManager.Load();

        if (saveData == null)
        {
            return false;
        }

        ExplorerSaveData explorerData = saveData.Explorer;

        Explorer explorer = new(
            explorerData.Name,
            explorerData.DungeonSeed);

        explorer.RestoreProgress(
            explorerData.Level,
            explorerData.Experience,
            explorerData.CurrentHealth,
            explorerData.DeepestFloorReached);

        if (explorerData.EquippedWeaponId != null)
        {
            Item item = ItemFactory.Create(explorerData.EquippedWeaponId);

            if (item is Weapon weapon)
            {
                explorer.RestoreWeapon(weapon);
            }
        }

        if (explorerData.EquippedArmorId != null)
        {
            Item item = ItemFactory.Create(explorerData.EquippedArmorId);

            if (item is Armor armor)
            {
                explorer.RestoreArmor(armor);
            }
        }

        foreach (string itemId in explorerData.InventoryItemIds)
        {
            explorer.AddItem(ItemFactory.Create(itemId));
        }

        _storageChest.Clear();

        foreach (StorageStackSaveData stackData in saveData.Storage)
        {
            for (int i = 0; i < stackData.Quantity; i++)
            {
                _storageChest.AddItem(ItemFactory.Create(stackData.ItemId));
            }
        }

        _honorBoard.Clear();

        foreach (HonorRecordSaveData recordData in saveData.HonorBoard)
        {
            _honorBoard.AddRecord(new HonorRecord(
                recordData.ExplorerName,
                recordData.DeepestFloor,
                recordData.Level,
                recordData.CauseOfDeath));
        }

        _explorer = explorer;
        _dungeonRun = new DungeonRun(_explorer.DungeonSeed);

        return true;
    }
}