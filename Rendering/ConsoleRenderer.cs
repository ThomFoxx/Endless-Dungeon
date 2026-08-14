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

    public void DrawDungeon(DungeonFloor floor, int playerX, int playerY)
    {
        Console.SetCursorPosition(0, 0);

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                if (x == playerX && y == playerY)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write('@');
                    continue;
                }

                Tile tile = floor.GetTile(x, y);

                switch (tile.Type)
                {
                    case TileType.Wall:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write(GetWallCharacter(floor, x, y));
                        break;

                    case TileType.Floor:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write('·');
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
        Console.WriteLine("Escape - Return to test camp");
    }

    private char GetWallCharacter(DungeonFloor floor, int x, int y)
    {
        // Check for open dungeon space around this wall.
        bool north = IsOpenSpace(floor, x, y - 1);
        bool south = IsOpenSpace(floor, x, y + 1);
        bool east = IsOpenSpace(floor, x + 1, y);
        bool west = IsOpenSpace(floor, x - 1, y);

        int cardinalCount =
            (north ? 1 : 0) +
            (south ? 1 : 0) +
            (east ? 1 : 0) +
            (west ? 1 : 0);

        // A solid tile almost surrounded by floor is better
        // represented as a pillar than a misleading wall junction.
        if (cardinalCount >= 3)
        {
            return '█';
        }

        // Floor on opposite sides means this wall separates
        // two nearby open areas.
        if (north && south)
        {
            return '─';
        }

        if (east && west)
        {
            return '│';
        }

        // Concave corners.
        if (north && east)
        {
            return '┐';
        }

        if (north && west)
        {
            return '┌';
        }

        if (south && east)
        {
            return '┘';
        }

        if (south && west)
        {
            return '└';
        }

        // Normal straight room/corridor walls.
        if (north || south)
        {
            return '─';
        }

        if (east || west)
        {
            return '│';
        }

        // No cardinal floor means this may be an outer corner.
        return GetDiagonalWallCharacter(
            floor,
            x,
            y);
    }

    private char GetDiagonalWallCharacter(
        DungeonFloor floor,
        int x,
        int y)
    {
        bool northEast = IsOpenSpace(floor, x + 1, y - 1);
        bool northWest = IsOpenSpace(floor, x - 1, y - 1);
        bool southEast = IsOpenSpace(floor, x + 1, y + 1);
        bool southWest = IsOpenSpace(floor, x - 1, y + 1);

        int diagonalCount =
            (northEast ? 1 : 0) +
            (northWest ? 1 : 0) +
            (southEast ? 1 : 0) +
            (southWest ? 1 : 0);

        if (diagonalCount == 1)
        {
            if (southEast)
            {
                return '┌';
            }

            if (southWest)
            {
                return '┐';
            }

            if (northEast)
            {
                return '└';
            }

            if (northWest)
            {
                return '┘';
            }
        }

        if (northEast && southEast &&
            !northWest && !southWest)
        {
            return '│';
        }

        if (northWest && southWest &&
            !northEast && !southWest)
        {
            return '│';
        }

        if (northEast && northWest &&
            !southEast && !southWest)
        {
            return '─';
        }

        if (southEast && southWest &&
            !northEast && !northWest)
        {
            return '─';
        }

        return '█';
    }

    private bool IsOpenSpace(DungeonFloor floor, int x, int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        return floor.GetTile(x, y).Type == TileType.Floor;
    }
}