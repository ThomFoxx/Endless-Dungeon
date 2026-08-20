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
    private const int ExplorerHeaderRow = 2;
    private const int DungeonMapStartRow = 4;
    private const int DungeonMapStartColumn = 1;
    private const int PreferredConsoleWidth = 80;
    private const int PreferredConsoleHeight = 40;

    private int _viewportX;
    private int _viewportY;
    private int _viewportWidth;
    private int _viewportHeight;
    private int _mapOffsetX;
    private int _mapOffsetY;

    public bool DebugRevealDungeon { get; set; }

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
        UpdateViewport(floor, explorer);

        if (_viewportWidth < 10 || _viewportHeight < 5)
        {
            Console.Clear();
            Console.WriteLine("The terminal window is too small to display the game.");
            Console.WriteLine();
            Console.WriteLine("Please enlarge the terminal window and try again.");
            return;
        }

        string floorHeader = $"Dungeon Floor {floor.FloorNumber}  |  Seed: {floor.Seed}";
        string explorerHeader = $"{explorer.Name}  |  HP: {explorer.CurrentHealth}/{explorer.MaxHealth}";

        const string movementText = "WASD / Arrow Keys - Move / Attack";
        const string pickupText = "G - Pick Up Item";
        const string inventoryText = "I - Inventory";
        const string interactText = "E - Interact / Use Stairs / Exit";
        const string escapeText = "Escape - Return to test camp";
        const string actionHeader = "Recent Actions:";

        int logWidth = actionLog.Entries.Count > 0
            ? actionLog.Entries.Max(entry => entry.Length + 2)
            : 0;

        // Leave room for the two vertical frame characters and one
        // safety column so Windows Terminal does not wrap the line.
        int maxContentWidth = Math.Max(1, Console.WindowWidth - 3);

        int currentRenderWidth = new[]
        {
            _viewportWidth,
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

        currentRenderWidth = Math.Min(currentRenderWidth, maxContentWidth);

        // Preserve the previous width where possible so shorter frames
        // do not leave stale characters behind.
        int renderWidth = Math.Min(
            maxContentWidth,
            Math.Max(currentRenderWidth, _lastRenderWidth));

        // Fixed screen rows:
        // 1 top border
        // 2 status rows
        // 3 separators
        // 5 action rows
        // 5 control rows
        // 1 bottom border
        // = 17 rows plus the viewport.
        int requiredHeight = _viewportHeight + 17;

        bool hasEnoughBuffer = EnsureConsoleBuffer(renderWidth + 2, requiredHeight + 1);

        if (!hasEnoughBuffer)
        {
            Console.Clear();
            Console.WriteLine("The terminal window is too small to display the game.");
            Console.WriteLine();
            Console.WriteLine("Please enlarge the terminal window and try again.");
            return;
        }

        int row = 0;

        // ─────────────────────────────────────────────
        // Status
        // ─────────────────────────────────────────────

        WriteFrameTop(row++, renderWidth);
        WriteFrameLine(CenterFrameText(floorHeader, renderWidth), row++, renderWidth);
        WriteFrameLine(CenterFrameText(explorerHeader, renderWidth), row++, renderWidth);
        WriteFrameSeparator(row++, renderWidth);

        // ─────────────────────────────────────────────
        // Dungeon viewport
        // ─────────────────────────────────────────────

        int endX = _viewportX + _viewportWidth;
        int endY = _viewportY + _viewportHeight;

        for (int screenY = 0; screenY < _viewportHeight; screenY++)
        {
            Console.SetCursorPosition(0, row);
            Console.ResetColor();
            Console.Write('║');

            for (int screenX = 0; screenX < _viewportWidth; screenX++)
            {
                int x = _viewportX + screenX - _mapOffsetX;
                int y = _viewportY + screenY - _mapOffsetY;

                if (!floor.IsInsideBounds(x, y))
                {
                    Console.ResetColor();
                    Console.Write(' ');
                    continue;
                }

                Tile tile = floor.GetTile(x, y);

                if (!DebugRevealDungeon && tile.Visibility == VisibilityState.Unseen)
                {
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

                if (monster != null &&
                    (DebugRevealDungeon || tile.Visibility == VisibilityState.Visible))
                {
                    Console.ForegroundColor = monster.Color;
                    Console.Write(monster.Glyph);
                    continue;
                }

                Chest? chest = floor.GetChestAt(x, y);

                if (chest != null &&
                    (DebugRevealDungeon || tile.Visibility == VisibilityState.Visible))
                {
                    Console.ForegroundColor = chest.Color;
                    Console.Write(chest.Glyph);
                    continue;
                }

                GroundItem? groundItem = floor.GetGroundItemAt(x, y);

                if (groundItem != null &&
                    (DebugRevealDungeon || tile.Visibility == VisibilityState.Visible))
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

                        Console.Write('Ѻ');
                        break;

                    default:
                        Console.ResetColor();
                        Console.Write(' ');
                        break;
                }
            }

            Console.ResetColor();
            Console.Write('║');

            row++;
        }

        // Shared dungeon / action-log border.
        WriteFrameSeparator(row++, renderWidth);

        // ─────────────────────────────────────────────
        // Action log
        // ─────────────────────────────────────────────

        WriteFrameLine(actionHeader, row++, renderWidth);

        for (int i = 0; i < 4; i++)
        {
            string message = i < actionLog.Entries.Count
                ? $"  {actionLog.Entries[i]}"
                : string.Empty;

            WriteFrameLine(message, row++, renderWidth);
        }

        // Shared action-log / controls border.
        WriteFrameSeparator(row++, renderWidth);

        // ─────────────────────────────────────────────
        // Controls
        // ─────────────────────────────────────────────

        int controlBlockWidth = new[]
        {
            movementText.Length,
            pickupText.Length,
            inventoryText.Length,
            interactText.Length,
            escapeText.Length
        }.Max();

        int controlPadding = Math.Max(0, (renderWidth - controlBlockWidth) / 2);
        string controlIndent = new string(' ', controlPadding);

        WriteFrameLine(controlIndent + movementText, row++, renderWidth);
        WriteFrameLine(controlIndent + pickupText, row++, renderWidth);
        WriteFrameLine(controlIndent + inventoryText, row++, renderWidth);
        WriteFrameLine(controlIndent + interactText, row++, renderWidth);
        WriteFrameLine(controlIndent + escapeText, row++, renderWidth);

        WriteFrameBottom(row++, renderWidth);

        // Clear any rows left over from a previously taller frame.
        for (int clearRow = row; clearRow < _lastRenderHeight; clearRow++)
        {
            ClearRenderedLine(clearRow, renderWidth);
        }

        _lastRenderWidth = renderWidth;
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

    private bool FormsOpenCorner(DungeonFloor floor, int x, int y, int directionX, int directionY)
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

    private bool IsWall(DungeonFloor floor, int x, int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        return floor.GetTile(x, y).Type == TileType.Wall;
    }

    private bool IsOpenSpace(DungeonFloor floor, int x, int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        return floor.GetTile(x, y).IsWalkable;
    }

    private char GetIsolatedWallCharacter(DungeonFloor floor, int x, int y)
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

    private void WritePaddedLine(string text, int row, int width)
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

    private void WriteFrameTop(int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.ResetColor();
        Console.Write($"╔{new string('═', width)}╗");
    }

    private void WriteFrameSeparator(int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.ResetColor();
        Console.Write($"╠{new string('═', width)}╣");
    }

    private void WriteFrameBottom(int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.ResetColor();
        Console.Write($"╚{new string('═', width)}╝");
    }

    private void WriteFrameLine(string text, int row, int width)
    {
        if (text.Length > width)
        {
            text = text[..width];
        }

        Console.SetCursorPosition(0, row);
        Console.ResetColor();

        Console.Write('║');
        Console.Write(text.PadRight(width));
        Console.Write('║');
    }

    private void ClearRenderedLine(int row, int width)
    {
        Console.SetCursorPosition(0, row);
        Console.ResetColor();
        Console.Write(new string(' ', width + 2));
    }

    private string FitFrameText(string text, int width)
    {
        if (text.Length > width)
        {
            text = text[..width];
        }

        return text.PadRight(width);
    }

    public void FlashExplorerHit(Explorer explorer)
    {
        string explorerHeader = $"{explorer.Name}  |  HP: {explorer.CurrentHealth}/{explorer.MaxHealth}";
        string centeredHeader = CenterFrameText(explorerHeader, _lastRenderWidth);
        string fittedHeader = FitFrameText(centeredHeader, _lastRenderWidth);

        bool explorerOnScreen = TryGetScreenPosition(
            explorer.X,
            explorer.Y,
            out int screenX,
            out int screenY);

        // Flash the Explorer information red without overwriting the frame.
        Console.SetCursorPosition(1, ExplorerHeaderRow);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(fittedHeader);

        // Flash the Explorer's map cell.
        if (explorerOnScreen)
        {
            Console.SetCursorPosition(screenX, screenY);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(explorer.Glyph);
        }

        Thread.Sleep(100);

        // Restore the centered Explorer information.
        WriteFrameLine(
            CenterFrameText(explorerHeader, _lastRenderWidth),
            ExplorerHeaderRow,
            _lastRenderWidth);

        // Restore the Explorer glyph.
        if (explorerOnScreen)
        {
            Console.SetCursorPosition(screenX, screenY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(explorer.Glyph);
        }

        Console.ResetColor();

        Console.SetCursorPosition(
            0,
            Math.Min(_lastRenderHeight, Console.BufferHeight - 1));
    }

    public void FlashMonsterHit(Monster monster)
    {
        if (!TryGetScreenPosition(monster.X, monster.Y, out int screenX, out int screenY))
        {
            return;
        }

        Console.SetCursorPosition(screenX, screenY);
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write(monster.Glyph);

        Thread.Sleep(90);

        Console.SetCursorPosition(screenX, screenY);
        Console.ForegroundColor = monster.Color;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(monster.Glyph);

        Console.ResetColor();
        Console.SetCursorPosition(0, Math.Min(_lastRenderHeight, Console.BufferHeight - 1));
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

    private void UpdateViewport(DungeonFloor floor, Explorer explorer)
    {
        const int ReservedUiRows = 18;

        int usableWindowWidth = Math.Min(Console.WindowWidth, PreferredConsoleWidth);
        int usableWindowHeight = Math.Min(Console.WindowHeight, PreferredConsoleHeight);

        int availableWidth = Math.Max(1, usableWindowWidth - 3);
        int availableHeight = Math.Max(1, usableWindowHeight - ReservedUiRows);

        _viewportWidth = availableWidth;
        _viewportHeight = availableHeight;

        int desiredX = explorer.X - _viewportWidth / 2;
        int desiredY = explorer.Y - _viewportHeight / 2;

        int maxX = Math.Max(0, floor.Width - _viewportWidth);
        int maxY = Math.Max(0, floor.Height - _viewportHeight);

        _viewportX = Math.Clamp(desiredX, 0, maxX);
        _viewportY = Math.Clamp(desiredY, 0, maxY);

        // Center floors that are smaller than the viewport.
        _mapOffsetX = floor.Width < _viewportWidth
            ? (_viewportWidth - floor.Width) / 2
            : 0;

        _mapOffsetY = floor.Height < _viewportHeight
            ? (_viewportHeight - floor.Height) / 2
            : 0;
    }

    private bool TryGetScreenPosition(int worldX, int worldY, out int screenX, out int screenY)
    {
        int viewportScreenX = worldX - _viewportX + _mapOffsetX;
        int viewportScreenY = worldY - _viewportY + _mapOffsetY;

        screenX = DungeonMapStartColumn + viewportScreenX;
        screenY = DungeonMapStartRow + viewportScreenY;

        return
            viewportScreenX >= 0 &&
            viewportScreenX < _viewportWidth &&
            viewportScreenY >= 0 &&
            viewportScreenY < _viewportHeight;
    }

    private string CenterFrameText(string text, int width)
    {
        if (text.Length > width)
        {
            text = text[..width];
        }

        int leftPadding = Math.Max(0, (width - text.Length) / 2);

        return new string(' ', leftPadding) + text;
    }
}