using EndlessDungeon.Input;
using EndlessDungeon.Rendering;
using EndlessDungeon.Dungeon;
using EndlessDungeon.Characters;
using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon;

public class Game
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;
    private DungeonRun _dungeonRun;
    private Explorer _explorer;

    private bool _isRunning = true;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();

        // Temporary explorer until character creation exists.
        _explorer = new Explorer("Test Explorer");

        // Temporary seed until Explorer owns dungeon generation data.
        _dungeonRun = new DungeonRun(12345);
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
        Console.WriteLine("  [X] DEBUG - Destroy Dungeon");
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
                ShowPlaceholder("Honor Board");
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
                DestroyDungeonDebug();
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

    private void RunDungeonTest()
    {
        VisibilityManager visibilityManager = new();

        DungeonFloor floor = _dungeonRun.BeginExpedition();

        _explorer.SetPosition(
            floor.StartX,
            floor.StartY);

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
                _explorer);

            ConsoleKey key = _inputManager.ReadKey();

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
                    Tile currentTile = floor.GetTile(
                        _explorer.X,
                        _explorer.Y);

                    if (currentTile.Type == TileType.StairsDown)
                    {
                        floor = _dungeonRun.Descend();

                        _explorer.SetPosition(
                            floor.StairsUpX,
                            floor.StairsUpY);
                    }
                    else if (
                        currentTile.Type == TileType.StairsUp &&
                        _dungeonRun.CurrentFloorNumber > 1)
                    {
                        floor = _dungeonRun.Ascend();

                        _explorer.SetPosition(
                            floor.StairsDownX,
                            floor.StairsDownY);
                    }
                    else if (
                        currentTile.Type == TileType.ExitPortal)
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

            bool turnTaken = TryMoveExplorer(
                floor,
                moveX,
                moveY);

            if (turnTaken)
            {
                RunMonsterTurns(floor);
            }

            if (!_explorer.IsAlive)
            {
                isExploring = false;

                ShowTestDeathMessage();
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

        // This is a valid explorer action, so begin
        // a new monster round.
        BeginMonsterRound(floor);

        Monster? targetMonster =
            floor.GetMonsterAt(
                targetX,
                targetY);

        // Moving into a monster attacks it.
        if (targetMonster != null)
        {
            _explorer.AttackMonster(
                targetMonster);

            if (targetMonster.IsAlive)
            {
                // Being attacked always provokes a response.
                // The Slime's inactivity chance does not apply.
                targetMonster.Retaliate(
                    _explorer);
            }
            else
            {
                floor.RemoveMonster(
                    targetMonster);
            }

            return true;
        }

        // Remember which monsters threatened the explorer
        // before the movement occurred.
        List<Monster> adjacentMonsters =
            GetAdjacentMonsters(
                floor,
                _explorer.X,
                _explorer.Y);

        _explorer.SetPosition(
            targetX,
            targetY);

        // Any monster that was adjacent but is no longer
        // adjacent gets an opportunity attack.
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
                    _explorer);
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
                _explorer);
        }
    }

    private void ShowTestDeathMessage()
    {
        _renderer.Clear();
        _renderer.WriteTitle("EXPLORER SLAIN");

        Console.WriteLine();
        Console.WriteLine("The explorer has fallen in the dungeon.");
        Console.WriteLine();
        Console.WriteLine(
            "The full death and Honor Board system will be added next.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return to camp.");

        _inputManager.ReadKey();
    }

    private void DestroyDungeonDebug()
    {
        int oldSeed = _dungeonRun.ExplorerSeed;

        // Simulate a new explorer receiving a completely new dungeon.
        int newSeed = Random.Shared.Next(
            int.MinValue,
            int.MaxValue);

        _dungeonRun = new DungeonRun(newSeed);

        _renderer.Clear();
        _renderer.WriteTitle("DEBUG - EXPLORER DEATH");

        Console.WriteLine();
        Console.WriteLine("The current dungeon has been destroyed.");
        Console.WriteLine();
        Console.WriteLine($"Old Explorer Seed: {oldSeed}");
        Console.WriteLine($"New Explorer Seed: {newSeed}");
        Console.WriteLine();
        Console.WriteLine("All dungeon floors and exploration data");
        Console.WriteLine("from the previous explorer are gone.");
        Console.WriteLine();
        Console.WriteLine("Press any key to return to camp.");

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
}