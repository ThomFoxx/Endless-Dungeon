namespace EndlessDungeon.Items;

public class GroundItem
{
    public Item Item { get; }
    public int X { get; }
    public int Y { get; }

    public GroundItem(Item item, int x, int y)
    {
        Item = item;
        X = x;
        Y = y;
    }
}