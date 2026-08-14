namespace EndlessDungeon.Dungeon;

public class Tile
{
    public TileType Type { get; set; }

    public bool IsWalkable => Type == TileType.Floor;

    public Tile(TileType type = TileType.Empty)
    {
        Type = type;
    }
}