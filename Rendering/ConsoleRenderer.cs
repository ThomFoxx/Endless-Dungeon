using EndlessDungeon.Dungeon;
using EndlessDungeon.Characters;
using System.Text;
using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.UI;
using EndlessDungeon.Items;

namespace EndlessDungeon.Rendering;

public class ConsoleRenderer
{
    private int _lastRenderWidth;
    private int _lastRenderHeight;
    private const int DungeonMapStartRow = 3;

    public void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Title = "Endless Dungeon";
    }

    public void Clear()
    {
        Console.Clear();

        // A full clear means there is no previous frame
        // for the renderer to clean up.
        _lastRenderWidth = 0;
        _lastRenderHeight = 0;
    }

    public void WriteTitle(string title)
    {
        int width = Math.Max(title.Length + 4, 40);

        Console.WriteLine($"╔{new string('═', width - 2)}╗");

        int padding = width - title.Length - 2;
        int leftPadding = padding / 2;
        int rightPadding = padding - leftPadding;

        Console.WriteLine(
            $"║{new string(' ', leftPadding)}{title}{new string(' ', rightPadding)}║");

        Console.WriteLine($"╚{new string('═', width - 2)}╝");
    }

    public void DrawDungeon(DungeonFloor floor, Explorer explorer, ActionLog actionLog)
    {
        string floorHeader = $"Dungeon Floor {floor.FloorNumber}  |  Seed: {floor.Seed}";
        string explorerHeader = $"{explorer.Name}  |  HP: {explorer.CurrentHealth}/{explorer.MaxHealth}";

        const string movementText = "WASD / Arrow Keys - Move / Attack";
        const string pickupText = "G - Pick Up Item";
        const string interactText = "E - Interact / Use Stairs / Exit";
        const string escapeText = "Escape - Return to test camp";
        const string actionHeader = "Recent Actions:";
        const string inventoryText = "I - Inventory";

        int logWidth = actionLog.Entries.Count > 0
            ? actionLog.Entries.Max(entry => entry.Length + 2)
            : 0;

        int currentRenderWidth = new[] {
            floor.Width,
            floorHeader.Length,
            explorerHeader.Length,
            movementText.Length,
            pickupText.Length,
            inventoryText.Length,
            interactText.Length,
            escapeText.Length,
            actionHeader.Length,
            logWidth
        }.Max();

        int renderWidth = Math.Max(currentRenderWidth, _lastRenderWidth);

        // Header + map + action log + four control lines.
        int requiredHeight = floor.Height + 15;

        bool hasEnoughBuffer = EnsureConsoleBuffer(renderWidth + 1, requiredHeight + 1);

        if (!hasEnoughBuffer)
        {
            Console.Clear();
            Console.WriteLine("The terminal window is too small to display the game.");
            Console.WriteLine();
            Console.WriteLine("Please enlarge the terminal window and try again.");
            return;
        }

        int row = 0;

        WritePaddedLine(floorHeader, row++, renderWidth);
        WritePaddedLine(explorerHeader, row++, renderWidth);
        WritePaddedLine(string.Empty, row++, renderWidth);

        for (int y = 0; y < floor.Height; y++)
        {
            Console.SetCursorPosition(0, row);

            for (int x = 0; x < floor.Width; x++)
            {
                Tile tile = floor.GetTile(x, y);

                if (tile.Visibility == VisibilityState.Unseen)
                {
                    Console.ResetColor();
                    Console.Write(' ');
                    continue;
                }

                // Explorer has highest rendering priority.
                if (x == explorer.X && y == explorer.Y)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(explorer.Glyph);
                    continue;
                }

                Monster? monster = floor.GetMonsterAt(x, y);

                // Monsters are only shown while currently visible.
                if (tile.Visibility == VisibilityState.Visible && monster != null)
                {
                    Console.ForegroundColor = monster.Color;
                    Console.Write(monster.Glyph);
                    continue;
                }

                GroundItem? groundItem = floor.GetGroundItemAt(x, y);

                // Like monsters, items are only rendered while currently visible.
                if (tile.Visibility == VisibilityState.Visible && groundItem != null)
                {
                    Console.ForegroundColor = groundItem.Item.Color;
                    Console.Write(groundItem.Item.Glyph);
                    continue;
                }

                bool isExplored = tile.Visibility == VisibilityState.Explored;

                switch (tile.Type)
                {
                    case TileType.Wall:
                        Console.ForegroundColor = isExplored
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.Gray;

                        Console.Write(GetWallCharacter(floor, x, y));
                        break;

                    case TileType.Floor:
                        Console.ForegroundColor = isExplored
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.Gray;

                        Console.Write('·');
                        break;

                    case TileType.StairsUp:
                        Console.ForegroundColor = isExplored
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.White;

                        Console.Write('▲');
                        break;

                    case TileType.StairsDown:
                        Console.ForegroundColor = isExplored
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.White;

                        Console.Write('▼');
                        break;

                    case TileType.ExitPortal:
                        Console.ForegroundColor = isExplored
                            ? ConsoleColor.DarkGray
                            : ConsoleColor.Cyan;

                        Console.Write('֍');
                        break;

                    default:
                        Console.ResetColor();
                        Console.Write(' ');
                        break;
                }
            }

            Console.ResetColor();

            if (renderWidth > floor.Width)
            {
                Console.Write(new string(' ', renderWidth - floor.Width));
            }

            row++;
        }

        WritePaddedLine(string.Empty, row++, renderWidth);
        WritePaddedLine(actionHeader, row++, renderWidth);

        for (int i = 0; i < 4; i++)
        {
            string message = i < actionLog.Entries.Count
                ? $"  {actionLog.Entries[i]}"
                : string.Empty;

            WritePaddedLine(message, row++, renderWidth);
        }

        WritePaddedLine(string.Empty, row++, renderWidth);
        WritePaddedLine(movementText, row++, renderWidth);
        WritePaddedLine(pickupText, row++, renderWidth);
        WritePaddedLine(inventoryText, row++, renderWidth);
        WritePaddedLine(interactText, row++, renderWidth);
        WritePaddedLine(escapeText, row++, renderWidth);

        for (int clearRow = row; clearRow < _lastRenderHeight; clearRow++)
        {
            WritePaddedLine(string.Empty, clearRow, renderWidth);
        }

        _lastRenderWidth = currentRenderWidth;
        _lastRenderHeight = row;

        Console.SetCursorPosition(0, Math.Min(row, Console.BufferHeight - 1));
    }

    private char GetWallCharacter(DungeonFloor floor, int x, int y)
    {
        bool north = HasNorthWallConnection(floor, x, y);
        bool south = HasSouthWallConnection(floor, x, y);
        bool east = HasEastWallConnection(floor, x, y);
        bool west = HasWestWallConnection(floor, x, y);

        return (north, south, east, west) switch
        {
            // Four-way junction
            (true, true, true, true) => '┼',

            // Three-way junctions
            (true, true, true, false) => '├',
            (true, true, false, true) => '┤',
            (true, false, true, true) => '┴',
            (false, true, true, true) => '┬',

            // Straight walls
            (true, true, false, false) => '│',
            (false, false, true, true) => '─',

            // Corners
            (false, true, true, false) => '┌',
            (false, true, false, true) => '┐',
            (true, false, true, false) => '└',
            (true, false, false, true) => '┘',

            // Wall ends
            (true, false, false, false) => '╵',
            (false, true, false, false) => '╷',
            (false, false, true, false) => '╶',
            (false, false, false, true) => '╴',

            // Unusual isolated geometry
            _ => GetIsolatedWallCharacter(floor, x, y)
        };
    }

    private bool FormsOpenCorner( DungeonFloor floor, int x, int y, int directionX, int directionY)
    {
        return IsOpenSpace(
            floor,
            x + directionX,
            y + directionY);
    }

    private bool HasNorthWallConnection(DungeonFloor floor, int x, int y)
    {
        if (!IsWall(floor, x, y - 1))
        {
            return false;
        }

        if (HasVerticalDiagonalPinch(floor, x, y - 1, y))
        {
            bool formsWestCorner =
                IsWall(floor, x - 1, y) &&
                FormsOpenCorner(floor, x, y, -1, -1);

            bool formsEastCorner =
                IsWall(floor, x + 1, y) &&
                FormsOpenCorner(floor, x, y, 1, -1);

            if (!formsWestCorner && !formsEastCorner)
            {
                return false;
            }
        }

        return
            IsOpenSpace(floor, x - 1, y) ||
            IsOpenSpace(floor, x - 1, y - 1) ||
            IsOpenSpace(floor, x + 1, y) ||
            IsOpenSpace(floor, x + 1, y - 1);
    }

    private bool HasSouthWallConnection(DungeonFloor floor, int x, int y)
    {
        if (!IsWall(floor, x, y + 1))
        {
            return false;
        }

        if (HasVerticalDiagonalPinch(floor, x, y, y + 1))
        {
            bool formsWestCorner =
                IsWall(floor, x - 1, y) &&
                FormsOpenCorner(floor, x, y, -1, 1);

            bool formsEastCorner =
                IsWall(floor, x + 1, y) &&
                FormsOpenCorner(floor, x, y, 1, 1);

            if (!formsWestCorner && !formsEastCorner)
            {
                return false;
            }
        }

        return
            IsOpenSpace(floor, x - 1, y) ||
            IsOpenSpace(floor, x - 1, y + 1) ||
            IsOpenSpace(floor, x + 1, y) ||
            IsOpenSpace(floor, x + 1, y + 1);
    }

    private bool HasEastWallConnection(DungeonFloor floor, int x, int y)
    {
        if (!IsWall(floor, x + 1, y))
        {
            return false;
        }

        if (HasHorizontalDiagonalPinch(floor, x, x + 1, y))
        {
            bool formsNorthCorner =
                IsWall(floor, x, y - 1) &&
                FormsOpenCorner(floor, x, y, 1, -1);

            bool formsSouthCorner =
                IsWall(floor, x, y + 1) &&
                FormsOpenCorner(floor, x, y, 1, 1);

            if (!formsNorthCorner && !formsSouthCorner)
            {
                return false;
            }
        }

        return
            IsOpenSpace(floor, x, y - 1) ||
            IsOpenSpace(floor, x + 1, y - 1) ||
            IsOpenSpace(floor, x, y + 1) ||
            IsOpenSpace(floor, x + 1, y + 1);
    }

    private bool HasWestWallConnection(DungeonFloor floor, int x, int y)
    {
        if (!IsWall(floor, x - 1, y))
        {
            return false;
        }

        if (HasHorizontalDiagonalPinch(floor, x - 1, x, y))
        {
            bool formsNorthCorner =
                IsWall(floor, x, y - 1) &&
                FormsOpenCorner(floor, x, y, -1, -1);

            bool formsSouthCorner =
                IsWall(floor, x, y + 1) &&
                FormsOpenCorner(floor, x, y, -1, 1);

            if (!formsNorthCorner && !formsSouthCorner)
            {
                return false;
            }
        }

        return
            IsOpenSpace(floor, x, y - 1) ||
            IsOpenSpace(floor, x - 1, y - 1) ||
            IsOpenSpace(floor, x, y + 1) ||
            IsOpenSpace(floor, x - 1, y + 1);
    }

    private bool HasVerticalDiagonalPinch(DungeonFloor floor, int x, int upperY, int lowerY)
    {
        bool upperLeftFloor = IsOpenSpace(floor, x - 1, upperY);
        bool upperRightFloor = IsOpenSpace(floor, x + 1, upperY);
        bool lowerLeftFloor = IsOpenSpace(floor, x - 1, lowerY);
        bool lowerRightFloor = IsOpenSpace(floor, x + 1, lowerY);

        bool fallsRight =
            IsWall(floor, x - 1, upperY) &&
            upperRightFloor &&
            lowerLeftFloor &&
            IsWall(floor, x + 1, lowerY);

        bool fallsLeft =
            upperLeftFloor &&
            IsWall(floor, x + 1, upperY) &&
            IsWall(floor, x - 1, lowerY) &&
            lowerRightFloor;

        return fallsRight || fallsLeft;
    }

    private bool HasHorizontalDiagonalPinch(DungeonFloor floor, int leftX, int rightX, int y)
    {
        bool upperLeftFloor = IsOpenSpace(floor, leftX, y - 1);
        bool upperRightFloor = IsOpenSpace(floor, rightX, y - 1);
        bool lowerLeftFloor = IsOpenSpace(floor, leftX, y + 1);
        bool lowerRightFloor = IsOpenSpace(floor, rightX, y + 1);

        bool fallsRight =
            IsWall(floor, leftX, y - 1) &&
            upperRightFloor &&
            lowerLeftFloor &&
            IsWall(floor, rightX, y + 1);

        bool fallsLeft =
            upperLeftFloor &&
            IsWall(floor, rightX, y - 1) &&
            IsWall(floor, leftX, y + 1) &&
            lowerRightFloor;

        return fallsRight || fallsLeft;
    }

    private bool IsWall(
    DungeonFloor floor,
    int x,
    int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        return floor.GetTile(x, y).Type == TileType.Wall;
    }

    private bool IsOpenSpace(
        DungeonFloor floor,
        int x,
        int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        return floor.GetTile(x, y).IsWalkable;
    }

    private char GetIsolatedWallCharacter(
    DungeonFloor floor,
    int x,
    int y)
    {
        bool north = IsOpenSpace(floor, x, y - 1);
        bool south = IsOpenSpace(floor, x, y + 1);
        bool east = IsOpenSpace(floor, x + 1, y);
        bool west = IsOpenSpace(floor, x - 1, y);

        // A wall separating spaces horizontally.
        if (east || west)
        {
            return '│';
        }

        // A wall separating spaces vertically.
        if (north || south)
        {
            return '─';
        }

        // Truly isolated solid geometry.
        // This should be uncommon and is deliberately obvious if it occurs.
        return '■';
    }

    private void WritePaddedLine(
    string text,
    int row,
    int width)
    {
        Console.SetCursorPosition(
            0,
            row);

        Console.ResetColor();

        Console.Write(text);

        int remainingSpace =
            width - text.Length;

        if (remainingSpace > 0)
        {
            Console.Write(
                new string(
                    ' ',
                    remainingSpace));
        }
    }

    public void FlashExplorerHit(Explorer explorer)
    {
        string explorerHeader =
            $"{explorer.Name}  |  HP: {explorer.CurrentHealth}/{explorer.MaxHealth}";

        // Flash the HP line red.
        Console.SetCursorPosition(
            0,
            1);

        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.Write(
            explorerHeader.PadRight(
                _lastRenderWidth));

        // Flash the explorer's map cell.
        Console.SetCursorPosition(
            explorer.X,
            DungeonMapStartRow + explorer.Y);

        Console.ForegroundColor =
            ConsoleColor.White;

        Console.BackgroundColor =
            ConsoleColor.DarkRed;

        Console.Write(explorer.Glyph);

        // Brief pulse without clearing or redrawing the screen.
        System.Threading.Thread.Sleep(90);

        // Restore the HP line.
        WritePaddedLine(
            explorerHeader,
            1,
            _lastRenderWidth);

        // Restore the explorer glyph.
        Console.SetCursorPosition(
            explorer.X,
            DungeonMapStartRow + explorer.Y);

        Console.ForegroundColor =
            ConsoleColor.Cyan;

        Console.BackgroundColor =
            ConsoleColor.Black;

        Console.Write(explorer.Glyph);

        Console.ResetColor();

        Console.SetCursorPosition(
            0,
            Math.Min(
                _lastRenderHeight,
                Console.BufferHeight - 1));
    }

    public void FlashMonsterHit(Monster monster)
    {
        int screenX = monster.X;
        int screenY = DungeonMapStartRow + monster.Y;

        // Briefly flash the monster with a red background.
        Console.SetCursorPosition(screenX, screenY);

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write(monster.Glyph);

        Thread.Sleep(90);

        // Restore the monster's normal appearance.
        Console.SetCursorPosition(screenX, screenY);

        Console.ForegroundColor = monster.Color;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(monster.Glyph);

        Console.ResetColor();

        Console.SetCursorPosition(
            0,
            Math.Min(_lastRenderHeight, Console.BufferHeight - 1));
    }

    private bool EnsureConsoleBuffer(int requiredWidth, int requiredHeight)
    {
        if (requiredWidth <= Console.BufferWidth &&
            requiredHeight <= Console.BufferHeight)
        {
            return true;
        }

        // Explicitly guarded because SetBufferSize is Windows-only.
        if (OperatingSystem.IsWindows())
        {
            int newWidth = Math.Max(Console.BufferWidth, requiredWidth);
            int newHeight = Math.Max(Console.BufferHeight, requiredHeight);

            Console.SetBufferSize(newWidth, newHeight);
            return true;
        }

        return false;
    }
}