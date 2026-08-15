namespace EndlessDungeon.Dungeon;

public class Room
{
    public int Id { get; }

    public int X { get; }
    public int Y { get; }

    public int Width { get; }
    public int Height { get; }

    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public Room(
        int id,
        int x,
        int y,
        int width,
        int height)
    {
        Id = id;

        X = x;
        Y = y;

        Width = width;
        Height = height;
    }

    public bool Overlaps(Room other, int padding = 1)
    {
        return
            X - padding < other.X + other.Width &&
            X + Width + padding > other.X &&
            Y - padding < other.Y + other.Height &&
            Y + Height + padding > other.Y;
    }
}