using EndlessDungeon.Input;
using EndlessDungeon.Rendering;
using EndlessDungeon.Dungeon;

namespace EndlessDungeon;

public class Game
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;

    private bool _isRunning = true;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();
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
        const int testExplorerSeed = 12345;

        DungeonRun dungeon = new(testExplorerSeed);

        DungeonFloor floor = dungeon.GetCurrentFloor();

        int playerX = floor.StartX;
        int playerY = floor.StartY;

        bool isExploring = true;

        _renderer.Clear();

        while (isExploring)
        {
            _renderer.DrawDungeon(
                floor,
                playerX,
                playerY);

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
                        playerX,
                        playerY);

                    if (currentTile.Type == TileType.StairsDown)
                    {
                        floor = dungeon.Descend();

                        playerX = floor.StairsUpX;
                        playerY = floor.StairsUpY;
                    }
                    else if (
                        currentTile.Type == TileType.StairsUp &&
                        dungeon.CurrentFloorNumber > 1)
                    {
                        floor = dungeon.Ascend();

                        playerX = floor.StairsDownX;
                        playerY = floor.StairsDownY;
                    }

                    continue;

                case ConsoleKey.Escape:
                    isExploring = false;
                    continue;
            }

            TryMovePlayer(
                floor,
                ref playerX,
                ref playerY,
                moveX,
                moveY);
        }
    }

    private void TryMovePlayer(DungeonFloor floor, ref int playerX, ref int playerY, int moveX, int moveY)
    {
        int targetX = playerX + moveX;
        int targetY = playerY + moveY;

        if (!floor.IsWalkable(targetX, targetY))
        {
            return;
        }

        playerX = targetX;
        playerY = targetY;
    }
}