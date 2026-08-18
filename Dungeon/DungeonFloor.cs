using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.Items;

namespace EndlessDungeon.Dungeon;

public class DungeonFloor
{
    private readonly Tile[,] _tiles;
    private readonly List<Room> _rooms = new();
    private readonly List<Monster> _monsters = new();
    private readonly List<GroundItem> _groundItems = new();
    private readonly List<Chest> _chests = new();

    public int FloorNumber { get; }
    public int Seed { get; }

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; set; }
    public int StartY { get; set; }

    public bool HasStairsUp { get; set; }

    public int StairsUpX { get; set; }
    public int StairsUpY { get; set; }

    public int StairsDownX { get; set; }
    public int StairsDownY { get; set; }

    public bool HasExitPortal { get; set; }

    public int ExitPortalX { get; set; }
    public int ExitPortalY { get; set; }

    public IReadOnlyList<Room> Rooms => _rooms;
    public IReadOnlyList<Monster> Monsters => _monsters;
    public IReadOnlyList<GroundItem> GroundItems => _groundItems;
    public IReadOnlyList<Chest> Chests => _chests;
    

    public DungeonFloor(int floorNumber, int width, int height, int seed)
    {
        FloorNumber = floorNumber;
        Width = width;
        Height = height;
        Seed = seed;

        _tiles = new Tile[width, height];

        InitializeTiles();
    }

    public Tile GetTile(int x, int y)
    {
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (!IsInsideBounds(x, y))
        {
            return;
        }

        _tiles[x, y].Type = type;
    }

    public bool IsWalkable(int x, int y)
    {
        if (!IsInsideBounds(x, y))
        {
            return false;
        }

        return _tiles[x, y].IsWalkable;
    }

    public bool IsInsideBounds(int x, int y)
    {
        return
            x >= 0 &&
            x < Width &&
            y >= 0 &&
            y < Height;
    }

    private void InitializeTiles()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _tiles[x, y] = new Tile(TileType.Empty);
            }
        }
    }

    public void AddRoom(Room room)
    {
        _rooms.Add(room);
    }

    public void AddMonster(Monster monster)
    {
        _monsters.Add(monster);
    }

    public Monster? GetMonsterAt(int x, int y)
    {
        return _monsters.FirstOrDefault(
            monster =>
                monster.IsAlive &&
                monster.X == x &&
                monster.Y == y);
    }

    public void RemoveMonster(Monster monster)
    {
        _monsters.Remove(monster);
    }

    public void AddGroundItem(GroundItem groundItem)
    {
        _groundItems.Add(groundItem);
    }

    public GroundItem? GetGroundItemAt(int x, int y)
    {
        return _groundItems.FirstOrDefault(item => item.X == x && item.Y == y);
    }

    public void RemoveGroundItem(GroundItem groundItem)
    {
        _groundItems.Remove(groundItem);
    }

    public void ClearMonsters()
    {
        _monsters.Clear();
    }

    public void ClearGroundItems()
    {
        _groundItems.Clear();
    }

    public void AddChest(Chest chest)
    {
        _chests.Add(chest);
    }

    public Chest? GetChestAt(int x, int y)
    {
        foreach (Chest chest in _chests)
        {
            if (chest.X == x && chest.Y == y)
            {
                return chest;
            }
        }

        return null;
    }

    public void ClearChests()
    {
        _chests.Clear();
    }
}