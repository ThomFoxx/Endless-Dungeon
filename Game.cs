using EndlessDungeon.Input;
using EndlessDungeon.Rendering;
using EndlessDungeon.Dungeon;
using EndlessDungeon.Characters;
using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.Records;
using EndlessDungeon.UI;

namespace EndlessDungeon;

public class Game
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;
    private readonly HonorBoard _honorBoard;
    private readonly ActionLog _actionLog;

    private Explorer _explorer;
    private DungeonRun _dungeonRun;

    private bool _isRunning = true;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();
        _honorBoard = new HonorBoard();
        _actionLog = new ActionLog();

        _explorer = CreateNewExplorer();

        _dungeonRun = new DungeonRun(
            _explorer.DungeonSeed);
    }

    private Explorer CreateNewExplorer()
    {
        int explorerNumber =
            _honorBoard.Records.Count + 1;

        string name =
            $"Explorer {explorerNumber}";

        int dungeonSeed = Random.Shared.Next(
            int.MinValue,
            int.MaxValue);

        return new Explorer(
            name,
            dungeonSeed);
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
        Console.WriteLine(
            $"  Explorer: {_explorer.Name}");

        Console.WriteLine(
            $"  HP: {_explorer.CurrentHealth}/{_explorer.MaxHealth}");

        Console.WriteLine(
            $"  Deepest Floor: {_explorer.DeepestFloorReached}");

        Console.WriteLine();
        Console.WriteLine("  [E] Enter Dungeon");
        Console.WriteLine("  [I] Inventory");
        Console.WriteLine("  [C] Storage Chest");
        Console.WriteLine("  [H] Honor Board");
        Console.WriteLine("  [S] Save Game");
        Console.WriteLine("  [Q] Save & Quit");
        Console.WriteLine();
        Console.WriteLine("  [F1] Help");

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(
            "  [X] DEBUG - Simulate Explorer Death");
        Console.ResetColor();

        ConsoleKey key = _inputManager.ReadKey();

        switch (key)
        {
            case ConsoleKey.E:
                RunDungeonTest();
                break;

            case ConsoleKey.I:
                ShowPlaceholder("Inventory");
                break;

            case ConsoleKey.C:
                ShowPlaceholder("Storage Chest");
                break;

            case ConsoleKey.H:
                ShowHonorBoard();
                break;

            case ConsoleKey.S:
                ShowPlaceholder("Game Saved");
                break;

            case ConsoleKey.Q:
                _isRunning = false;
                break;

            case ConsoleKey.F1:
                ShowHelp();
                break;

            case ConsoleKey.X:
                HandleDebugDeath();
                break;
        }
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

    private void ShowPlaceholder(string message)
    {
        _renderer.Clear();
        _renderer.WriteTitle(message);

        Console.WriteLine();
        Console.WriteLine("This feature will be added shortly.");
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

    private void RunDungeonTest()
    {
        VisibilityManager visibilityManager = new();

        DungeonFloor floor =
            _dungeonRun.BeginExpedition();

        _explorer.SetPosition(
            floor.StartX,
            floor.StartY);

        _actionLog.Clear();
        _actionLog.Add(
            "You enter the Endless Dungeon.");

        bool isExploring = true;

        _renderer.Clear();

        while (isExploring)
        {
            visibilityManager.UpdateVisibility(
                floor,
                _explorer.X,
                _explorer.Y);

            _renderer.DrawDungeon(
                floor,
                _explorer,
                _actionLog);

            ConsoleKey key =
                _inputManager.ReadKey();

            int moveX = 0;
            int moveY = 0;

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

                case ConsoleKey.E:
                    Tile currentTile =
                        floor.GetTile(
                            _explorer.X,
                            _explorer.Y);

                    if (
                        currentTile.Type ==
                        TileType.StairsDown)
                    {
                        floor =
                            _dungeonRun.Descend();

                        _explorer.RecordFloorReached(
                            floor.FloorNumber);

                        _explorer.SetPosition(
                            floor.StairsUpX,
                            floor.StairsUpY);

                        _actionLog.Add(
                            $"You descend to Floor {floor.FloorNumber}.");
                    }
                    else if (
                        currentTile.Type ==
                        TileType.StairsUp &&
                        _dungeonRun.CurrentFloorNumber > 1)
                    {
                        floor =
                            _dungeonRun.Ascend();

                        _explorer.SetPosition(
                            floor.StairsDownX,
                            floor.StairsDownY);

                        _actionLog.Add(
                            $"You ascend to Floor {floor.FloorNumber}.");
                    }
                    else if (
                        currentTile.Type ==
                        TileType.ExitPortal)
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

            int healthBeforeTurn =
                _explorer.CurrentHealth;

            bool turnTaken =
                TryMoveExplorer(
                    floor,
                    moveX,
                    moveY);

            if (turnTaken)
            {
                RunMonsterTurns(floor);
            }

            // Give immediate visual feedback if anything
            // damaged the explorer during this turn.
            if (
                _explorer.CurrentHealth <
                healthBeforeTurn)
            {
                _renderer.FlashExplorerHit(
                    _explorer);
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

    private bool TryMoveExplorer(
    DungeonFloor floor,
    int moveX,
    int moveY)
    {
        if (moveX == 0 && moveY == 0)
        {
            return false;
        }

        int targetX =
            _explorer.X + moveX;

        int targetY =
            _explorer.Y + moveY;

        if (!floor.IsWalkable(
            targetX,
            targetY))
        {
            return false;
        }

        BeginMonsterRound(floor);

        Monster? targetMonster =
            floor.GetMonsterAt(
                targetX,
                targetY);

        if (targetMonster != null)
        {
            int damage =
                _explorer.AttackMonster(
                    targetMonster);

            _actionLog.Add(
                $"You hit {targetMonster.Name} for {damage} damage.");

            if (targetMonster.IsAlive)
            {
                targetMonster.Retaliate(
                    _explorer,
                    _actionLog);
            }
            else
            {
                _actionLog.Add(
                    $"{targetMonster.Name} is defeated.");

                floor.RemoveMonster(
                    targetMonster);
            }

            return true;
        }

        List<Monster> adjacentMonsters =
            GetAdjacentMonsters(
                floor,
                _explorer.X,
                _explorer.Y);

        _explorer.SetPosition(
            targetX,
            targetY);

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

    private void BeginMonsterRound(
    DungeonFloor floor)
    {
        foreach (Monster monster in floor.Monsters)
        {
            monster.BeginRound();
        }
    }

    private void RunMonsterTurns(
    DungeonFloor floor)
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
    
    private List<Monster> GetAdjacentMonsters(
    DungeonFloor floor,
    int x,
    int y)
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
}