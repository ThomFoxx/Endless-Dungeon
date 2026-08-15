namespace EndlessDungeon.Dungeon;

public class Tile
{
    public TileType Type { get; set; }

    // -1 means this tile is not part of a room region.
    public int RegionId { get; set; } = -1;

    public VisibilityState Visibility { get; set; } =
        VisibilityState.Unseen;

    public bool IsWalkable =>
        Type == TileType.Floor ||
        Type == TileType.StairsUp ||
        Type == TileType.StairsDown ||
        Type == TileType.ExitPortal;

    public Tile(TileType type = TileType.Empty)
    {
        Type = type;
    }
}