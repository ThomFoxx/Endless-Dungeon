using EndlessDungeon.Dungeon;
using EndlessDungeon.Characters;
using System.Text;
using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon.Rendering;

public class ConsoleRenderer
{
    private int _lastRenderWidth;
    private int _lastRenderHeight;

    public void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Title = "Endless Dungeon";
    }

    public void Clear()
    {
        Console.Clear();
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

    public void DrawDungeon(
    DungeonFloor floor,
    Explorer explorer)
    {
        string floorHeader =
            $"Dungeon Floor {floor.FloorNumber}  |  Seed: {floor.Seed}";

        string explorerHeader =
            $"{explorer.Name}  |  HP: {explorer.CurrentHealth}/{explorer.MaxHealth}";

        const string movementText =
            "WASD / Arrow Keys - Move / Attack";

        const string interactText =
            "E - Interact / Use Stairs / Exit";

        const string escapeText =
            "Escape - Return to test camp";

        // Determine how wide this frame needs to be.
        int currentRenderWidth = new[]
        {
        floor.Width,
        floorHeader.Length,
        explorerHeader.Length,
        movementText.Length,
        interactText.Length,
        escapeText.Length
    }.Max();

        // If the previous frame was wider, keep using that width
        // so leftover characters are overwritten.
        int renderWidth = Math.Max(
            currentRenderWidth,
            _lastRenderWidth);

        int row = 0;

        // Header
        WritePaddedLine(
            floorHeader,
            row++,
            renderWidth);

        WritePaddedLine(
            explorerHeader,
            row++,
            renderWidth);

        WritePaddedLine(
            string.Empty,
            row++,
            renderWidth);

        // Dungeon map
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

                if (x == explorer.X && y == explorer.Y)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(explorer.Glyph);
                    continue;
                }

                Monster? monster =
                    floor.GetMonsterAt(x, y);

                if (
                    tile.Visibility == VisibilityState.Visible &&
                    monster != null)
                {
                    Console.ForegroundColor = monster.Color;
                    Console.Write(monster.Glyph);
                    continue;
                }

                bool isExplored =
                    tile.Visibility == VisibilityState.Explored;

                switch (tile.Type)
                {
                    case TileType.Wall:
                        Console.ForegroundColor =
                            isExplored
                                ? ConsoleColor.DarkGray
                                : ConsoleColor.Gray;

                        Console.Write(
                            GetWallCharacter(floor, x, y));
                        break;

                    case TileType.Floor:
                        Console.ForegroundColor =
                            isExplored
                                ? ConsoleColor.DarkGray
                                : ConsoleColor.Gray;

                        Console.Write('·');
                        break;

                    case TileType.StairsUp:
                        Console.ForegroundColor =
                            isExplored
                                ? ConsoleColor.DarkGray
                                : ConsoleColor.White;

                        Console.Write('▲');
                        break;

                    case TileType.StairsDown:
                        Console.ForegroundColor =
                            isExplored
                                ? ConsoleColor.DarkGray
                                : ConsoleColor.White;

                        Console.Write('▼');
                        break;

                    case TileType.ExitPortal:
                        Console.ForegroundColor =
                            isExplored
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

            // Clear anything remaining from a previously wider frame.
            if (renderWidth > floor.Width)
            {
                Console.Write(
                    new string(
                        ' ',
                        renderWidth - floor.Width));
            }

            row++;
        }

        // Controls
        WritePaddedLine(
            string.Empty,
            row++,
            renderWidth);

        WritePaddedLine(
            movementText,
            row++,
            renderWidth);

        WritePaddedLine(
            interactText,
            row++,
            renderWidth);

        WritePaddedLine(
            escapeText,
            row++,
            renderWidth);

        // If the previous frame had more rows than this one,
        // erase those old rows as well.
        for (
            int clearRow = row;
            clearRow < _lastRenderHeight;
            clearRow++)
        {
            WritePaddedLine(
                string.Empty,
                clearRow,
                renderWidth);
        }

        _lastRenderWidth = currentRenderWidth;
        _lastRenderHeight = row;

        // Keep the cursor away from the dungeon itself.
        Console.SetCursorPosition(
            0,
            row);
    }

    private char GetWallCharacter(
    DungeonFloor floor,
    int x,
    int y)
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
            (true, false, false, false) => '│',
            (false, true, false, false) => '│',
            (false, false, true, false) => '─',
            (false, false, false, true) => '─',

            // Unusual isolated geometry
            _ => GetIsolatedWallCharacter(floor, x, y)
        };
    }

    private bool HasNorthWallConnection(
    DungeonFloor floor,
    int x,
    int y)
    {
        if (!IsWall(floor, x, y - 1))
        {
            return false;
        }

        // Look for floor alongside the pair of vertical wall tiles.
        return
            IsOpenSpace(floor, x - 1, y) ||
            IsOpenSpace(floor, x - 1, y - 1) ||
            IsOpenSpace(floor, x + 1, y) ||
            IsOpenSpace(floor, x + 1, y - 1);
    }

    private bool HasSouthWallConnection(
        DungeonFloor floor,
        int x,
        int y)
    {
        if (!IsWall(floor, x, y + 1))
        {
            return false;
        }

        return
            IsOpenSpace(floor, x - 1, y) ||
            IsOpenSpace(floor, x - 1, y + 1) ||
            IsOpenSpace(floor, x + 1, y) ||
            IsOpenSpace(floor, x + 1, y + 1);
    }

    private bool HasEastWallConnection(
        DungeonFloor floor,
        int x,
        int y)
    {
        if (!IsWall(floor, x + 1, y))
        {
            return false;
        }

        // Look for floor above or below the pair of horizontal wall tiles.
        return
            IsOpenSpace(floor, x, y - 1) ||
            IsOpenSpace(floor, x + 1, y - 1) ||
            IsOpenSpace(floor, x, y + 1) ||
            IsOpenSpace(floor, x + 1, y + 1);
    }

    private bool HasWestWallConnection(
        DungeonFloor floor,
        int x,
        int y)
    {
        if (!IsWall(floor, x - 1, y))
        {
            return false;
        }

        return
            IsOpenSpace(floor, x, y - 1) ||
            IsOpenSpace(floor, x - 1, y - 1) ||
            IsOpenSpace(floor, x, y + 1) ||
            IsOpenSpace(floor, x - 1, y + 1);
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


}