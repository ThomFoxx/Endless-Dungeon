using EndlessDungeon.Input;
using EndlessDungeon.Rendering;
using EndlessDungeon.Dungeon;

namespace EndlessDungeon;

public class Game
{
    private readonly ConsoleRenderer _renderer;
    private readonly InputManager _inputManager;
    private DungeonRun _dungeonRun;

    private bool _isRunning = true;

    public Game()
    {
        _renderer = new ConsoleRenderer();
        _inputManager = new InputManager();

        // Temporary explorer seed until character creation exists.
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

        int playerX = floor.StartX;
        int playerY = floor.StartY;

        bool isExploring = true;

        _renderer.Clear();

        while (isExploring)
        {
            visibilityManager.UpdateVisibility(
                floor,
                playerX,
                playerY);

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
                        floor = _dungeonRun.Descend();

                        playerX = floor.StairsUpX;
                        playerY = floor.StairsUpY;
                    }
                    else if (
                        currentTile.Type == TileType.StairsUp &&
                        _dungeonRun.CurrentFloorNumber > 1)
                    {
                        floor = _dungeonRun.Ascend();

                        playerX = floor.StairsDownX;
                        playerY = floor.StairsDownY;
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

            TryMovePlayer(
                floor,
                ref playerX,
                ref playerY,
                moveX,
                moveY);
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
}