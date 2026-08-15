using EndlessDungeon.Dungeon;
using System.Text;

namespace EndlessDungeon.Rendering;

public class ConsoleRenderer
{
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
    int playerX,
    int playerY)
    {
        Console.SetCursorPosition(0, 0);

        Console.WriteLine(
            $"Dungeon Floor {floor.FloorNumber}  |  Seed: {floor.Seed}");

        Console.WriteLine();

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                Tile tile = floor.GetTile(x, y);

                // Tiles the explorer has never seen are completely hidden.
                if (tile.Visibility == VisibilityState.Unseen)
                {
                    Console.Write(' ');
                    continue;
                }

                // Draw the explorer over the tile they are standing on.
                if (x == playerX && y == playerY)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write('@');
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
                        Console.Write(' ');
                        break;
                }
            }

            Console.WriteLine();
        }

        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine("WASD / Arrow Keys - Move");
        Console.WriteLine("E - Interact / Use Stairs / Exit");
        Console.WriteLine("Escape - Return to test camp");
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
}