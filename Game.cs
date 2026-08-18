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
    private readonly InventoryScreen _inventoryScreen;
    private readonly StorageChestScreen _storageChestScreen;
    private readonly GlyphTestScreen _glyphTestScreen;

    private Explorer _explorer;
    private DungeonRun _dungeonRun;

    private bool _isRunning = true;
    private bool _saveOverwriteAcknowledged;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();
        _honorBoard = new HonorBoard();
        _actionLog = new ActionLog();
        _storageChest = new StorageChest();

        _inventoryScreen = new InventoryScreen(_renderer, _inputManager, _actionLog);
        _storageChestScreen = new StorageChestScreen(_renderer, _inputManager);
        _glyphTestScreen = new GlyphTestScreen(_renderer, _inputManager);

        _explorer = CreateNewExplorer();
        _dungeonRun = new DungeonRun(_explorer.DungeonSeed);
        _saveManager = new SaveManager();

        _saveOverwriteAcknowledged = !_saveManager.HasSaveFile;
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
                if (ConfirmDungeonEntry())
                {
                    RunDungeon();
                }
                break;

            case ConsoleKey.I:
                _inventoryScreen.Show(_explorer, isInDungeon: false);
                break;

            case ConsoleKey.C:
                _storageChestScreen.Show(_explorer, _storageChest);
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
                _glyphTestScreen.Show();
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
                    turnTaken = _inventoryScreen.Show(_explorer, isInDungeon: true);

                    if (turnTaken)
                    {
                        BeginMonsterRound(floor);
                    }
                    break;

                case ConsoleKey.E:
                    Tile currentTile = floor.GetTile(_explorer.X, _explorer.Y);

                    if (TryOpenAdjacentChest(floor))
                    {
                        BeginMonsterRound(floor);
                        RunMonsterTurns(floor);
                        continue;
                    }

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

                        SaveGame();
                        ShowDungeonExitMessage();

                        continue;
                    }

                    continue;

                case ConsoleKey.F2:
                    _renderer.DebugRevealDungeon =
                        !_renderer.DebugRevealDungeon;

                    _actionLog.Add(
                        _renderer.DebugRevealDungeon
                            ? "DEBUG: Dungeon reveal enabled."
                            : "DEBUG: Dungeon reveal disabled.");

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
        Console.WriteLine("Your progress has been saved.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return to camp.");

        _inputManager.ReadKey();
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

        Chest? chest = floor.GetChestAt(targetX, targetY);

        if (chest != null && !chest.IsOpened)
        {
            _actionLog.Add("A closed chest blocks your path.");
            return false;
        }

        BeginMonsterRound(floor);

        Monster? targetMonster = floor.GetMonsterAt(targetX, targetY);

        if (targetMonster != null)
        {
            int damage = _explorer.AttackMonster(targetMonster);

            _actionLog.Add($"You hit {targetMonster.Name} for {damage} damage.");

            // Give immediate visual feedback for the player's hit.
            _renderer.FlashMonsterHit(targetMonster);

            if (targetMonster.IsAlive)
            {
                targetMonster.Retaliate(_explorer, _actionLog);
            }
            else
            {
                HandleMonsterDeath(floor, targetMonster);
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

    private void HandleMonsterDeath( DungeonFloor floor, Monster monster)
    {
        int deathX = monster.X;
        int deathY = monster.Y;

        List<Item> loot = monster.TakeAllLoot();

        _actionLog.Add($"{monster.Name} is defeated.");

        // Remove the monster first so its death tile becomes available.
        floor.RemoveMonster(monster);

        foreach (Item item in loot)
        {
            if (TryPlaceMonsterDrop(
                floor,
                item,
                deathX,
                deathY))
            {
                _actionLog.Add(
                    $"{monster.Name} drops {item.Name}.");
            }
        }
    }

    private bool TryPlaceMonsterDrop( DungeonFloor floor, Item item, int originX, int originY)
    {
        int bestX = -1;
        int bestY = -1;
        int bestDistance = int.MaxValue;

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                Tile tile = floor.GetTile(x, y);

                if (tile.Type != TileType.Floor)
                {
                    continue;
                }

                if (floor.GetMonsterAt(x, y) != null)
                {
                    continue;
                }

                if (floor.GetGroundItemAt(x, y) != null)
                {
                    continue;
                }

                if (x == _explorer.X &&
                    y == _explorer.Y)
                {
                    continue;
                }

                int distance =
                    Math.Abs(x - originX) +
                    Math.Abs(y - originY);

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestX = x;
                bestY = y;
            }
        }

        if (bestX < 0 || bestY < 0)
        {
            return false;
        }

        floor.AddGroundItem(new GroundItem(
            item,
            bestX,
            bestY));

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
        _honorBoard.AddExplorer(_explorer);

        // The dead explorer and their dungeon are gone.
        _explorer = CreateNewExplorer();
        _dungeonRun = new DungeonRun(_explorer.DungeonSeed);

        // Death is permanent once it has occurred.
        SaveGame();

        _renderer.Clear();
        _renderer.WriteTitle("EXPLORER SLAIN");

        Console.WriteLine();
        Console.WriteLine(
            $"{explorerName} has fallen.");

        Console.WriteLine();
        Console.WriteLine($"Deepest Floor Reached: {deepestFloor}");

        Console.WriteLine($"Cause of Death: Slain by {cause}");

        Console.WriteLine();
        Console.WriteLine("Their dungeon fades away with them.");

        Console.WriteLine();
        Console.WriteLine($"{_explorer.Name} will be the next to enter.");

        Console.WriteLine();
        Console.WriteLine("Press any key to return to camp.");

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

        foreach (DungeonFloor floor in _dungeonRun.GeneratedFloors.OrderBy(floor => floor.FloorNumber))
        {
            DungeonFloorSaveData floorData = new()
            {
                FloorNumber = floor.FloorNumber
            };

            for (int y = 0; y < floor.Height; y++)
            {
                for (int x = 0; x < floor.Width; x++)
                {
                    Tile tile = floor.GetTile(x, y);

                    if (tile.Visibility == VisibilityState.Unseen)
                    {
                        continue;
                    }

                    floorData.ExploredTiles.Add(new TilePositionSaveData
                    {
                        X = x,
                        Y = y
                    });
                }
            }

            foreach (Monster monster in floor.Monsters)
            {
                MonsterSaveData monsterData = new()
                {
                    MonsterId = monster.Id,
                    X = monster.X,
                    Y = monster.Y,
                    CurrentHealth = monster.CurrentHealth
                };

                foreach (Item item in monster.Loot)
                {
                    monsterData.LootItemIds.Add(item.Id);
                }

                if (monster is Slime slime)
                {
                    monsterData.InactivityChance = slime.InactivityChance;
                }

                if (monster is Goblin goblin)
                {
                    monsterData.HomeRegionId = goblin.HomeRegionId;
                    monsterData.LastSeenX = goblin.LastSeenX;
                    monsterData.LastSeenY = goblin.LastSeenY;
                }

                floorData.Monsters.Add(monsterData);
            }

            foreach (GroundItem groundItem in floor.GroundItems)
            {
                floorData.GroundItems.Add(new GroundItemSaveData
                {
                    ItemId = groundItem.Item.Id,
                    X = groundItem.X,
                    Y = groundItem.Y
                });
            }

            foreach (Chest chest in floor.Chests)
            {
                ChestSaveData chestData = new()
                {
                    X = chest.X,
                    Y = chest.Y,
                    IsOpened = chest.IsOpened
                };

                foreach (Item item in chest.Items)
                {
                    chestData.ItemIds.Add(item.Id);
                }

                floorData.Chests.Add(chestData);
            }

            saveData.DungeonFloors.Add(floorData);
        }

        return saveData;
    }

    private void SaveGame()
    {
        SaveData saveData = CreateSaveData();

        _saveManager.Save(saveData);
        _saveOverwriteAcknowledged = true;

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

        foreach (DungeonFloorSaveData floorData in saveData.DungeonFloors)
        {
            DungeonFloor floor = _dungeonRun.GetOrCreateFloor(floorData.FloorNumber);

            // Dynamic content generated from the seed will be replaced
            // by the state that existed when the game was saved.
            floor.ClearMonsters();
            floor.ClearGroundItems();
            floor.ClearChests();

            foreach (TilePositionSaveData tileData in floorData.ExploredTiles)
            {
                if (!floor.IsInsideBounds(tileData.X, tileData.Y))
                {
                    continue;
                }

                Tile tile = floor.GetTile(tileData.X, tileData.Y);

                if (tile.Type != TileType.Empty)
                {
                    tile.Visibility = VisibilityState.Explored;
                }
            }

            foreach (MonsterSaveData monsterData in floorData.Monsters)
            {
                Monster monster = CreateMonsterFromSave(monsterData);
                floor.AddMonster(monster);
            }

            foreach (GroundItemSaveData itemData in floorData.GroundItems)
            {
                if (!floor.IsInsideBounds(itemData.X, itemData.Y))
                {
                    continue;
                }

                Item item = ItemFactory.Create(itemData.ItemId);

                floor.AddGroundItem(new GroundItem(
                    item,
                    itemData.X,
                    itemData.Y));
            }

            foreach (ChestSaveData chestData in floorData.Chests)
            {
                Chest chest = new(chestData.X, chestData.Y);

                foreach (string itemId in chestData.ItemIds)
                {
                    chest.AddItem(ItemFactory.Create(itemId));
                }

                chest.RestoreOpenedState(chestData.IsOpened);

                floor.AddChest(chest);
            }
        }

        _saveOverwriteAcknowledged = true;

        return true;
    }

    private bool ConfirmDungeonEntry()
    {
        if (_saveOverwriteAcknowledged || !_saveManager.HasSaveFile)
        {
            return true;
        }

        _renderer.Clear();
        _renderer.WriteTitle("EXISTING SAVE DATA");

        Console.WriteLine();
        Console.WriteLine("An existing save file was found, but it has not");
        Console.WriteLine("been loaded during this session.");
        Console.WriteLine();
        Console.WriteLine("Entering the dungeon with the current Explorer may");
        Console.WriteLine("overwrite that save when you successfully exit.");
        Console.WriteLine();
        Console.WriteLine("You can return to Camp and load the existing save");
        Console.WriteLine("if you want to keep playing from it.");
        Console.WriteLine();
        Console.WriteLine("Continue with the current game? [Y/N]");

        while (true)
        {
            ConsoleKey key = _inputManager.ReadKey();

            switch (key)
            {
                case ConsoleKey.Y:
                    _saveOverwriteAcknowledged = true;
                    _renderer.Clear();
                    return true;

                case ConsoleKey.N:
                case ConsoleKey.Escape:
                    _renderer.Clear();
                    return false;
            }
        }
    }

    private Monster CreateMonsterFromSave(MonsterSaveData monsterData)
    {
        Monster monster = monsterData.MonsterId switch
        {
            MonsterIds.Slime => new Slime(
                monsterData.X,
                monsterData.Y,
                monsterData.InactivityChance ?? 0.40),

            MonsterIds.Goblin => new Goblin(
                monsterData.X,
                monsterData.Y,
                monsterData.HomeRegionId ?? -1),

            _ => throw new InvalidOperationException(
                $"Unknown monster ID in save file: {monsterData.MonsterId}")
        };

        if (monster is Goblin goblin)
        {
            goblin.RestoreLastSeenPosition(
                monsterData.LastSeenX,
                monsterData.LastSeenY);
        }

        foreach (string itemId in monsterData.LootItemIds)
        {
            monster.AddLoot(ItemFactory.Create(itemId));
        }

        monster.RestoreHealth(monsterData.CurrentHealth);

        return monster;
    }

    private Chest? GetAdjacentClosedChest(DungeonFloor floor)
    {
        (int X, int Y)[] directions =
        {
        (0, -1),
        (1, 0),
        (0, 1),
        (-1, 0)
    };

        foreach ((int X, int Y) direction in directions)
        {
            int x = _explorer.X + direction.X;
            int y = _explorer.Y + direction.Y;

            if (!floor.IsInsideBounds(x, y))
            {
                continue;
            }

            Chest? chest = floor.GetChestAt(x, y);

            if (chest != null && !chest.IsOpened)
            {
                return chest;
            }
        }

        return null;
    }

    private bool TryOpenAdjacentChest(DungeonFloor floor)
    {
        Chest? chest = GetAdjacentClosedChest(floor);

        if (chest == null)
        {
            return false;
        }

        List<Item> contents = chest.Open();

        _actionLog.Add("You open the chest.");

        if (contents.Count == 0)
        {
            _actionLog.Add("The chest is empty.");
            return true;
        }

        foreach (Item item in contents)
        {
            _explorer.AddItem(item);
            _actionLog.Add($"You find {item.Name}.");
        }

        return true;
    }
}