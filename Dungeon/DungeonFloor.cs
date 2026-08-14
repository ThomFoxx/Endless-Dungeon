namespace EndlessDungeon.Dungeon;

public class DungeonFloor
{
    private readonly Tile[,] _tiles;

    public int Width { get; }
    public int Height { get; }

    public int StartX { get; set; }
    public int StartY { get; set; }

    public DungeonFloor(int width, int height)
    {
        Width = width;
        Height = height;

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
}