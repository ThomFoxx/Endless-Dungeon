namespace EndlessDungeon.Dungeon;

public class DungeonGenerator
{
    private const int MinRoomSize = 3;
    private const int MaxRoomSize = 5;

    private const int TargetRoomCount = 6;
    private const int MaxPlacementAttempts = 100;

    public DungeonFloor GenerateFloor(
     int floorNumber,
     int width,
     int height,
     int seed)
    {
        Random random = new(seed);

        DungeonFloor floor = new(
            floorNumber,
            width,
            height,
            seed);

        List<Room> rooms = GenerateRooms(
            floor,
            random);

        ConnectRooms(
            floor,
            rooms,
            random);

        GenerateWalls(floor);

        PlaceStairs(
            floor,
            rooms);

        return floor;
    }

    private void PlaceStairs(
    DungeonFloor floor,
    List<Room> rooms)
    {
        if (rooms.Count == 0)
        {
            throw new InvalidOperationException(
                "Cannot place stairs on a floor with no rooms.");
        }

        Room startRoom = rooms[0];

        floor.StartX = startRoom.CenterX;
        floor.StartY = startRoom.CenterY;

        // Floor 1 is the dungeon entrance, so it has no stairs up.
        if (floor.FloorNumber > 1)
        {
            floor.HasStairsUp = true;

            floor.StairsUpX = startRoom.CenterX;
            floor.StairsUpY = startRoom.CenterY;

            floor.SetTile(
                floor.StairsUpX,
                floor.StairsUpY,
                TileType.StairsUp);
        }

        // Try to put the downward stairs in a room far from
        // the room where the explorer entered.
        Room downRoom = FindFarthestRoom(
            startRoom,
            rooms);

        if (rooms.Count == 1)
        {
            // Unlikely, but keeps the two stairs separate
            // if generation only managed to create one room.
            floor.StairsDownX = startRoom.X;
            floor.StairsDownY = startRoom.Y;
        }
        else
        {
            floor.StairsDownX = downRoom.CenterX;
            floor.StairsDownY = downRoom.CenterY;
        }

        floor.SetTile(
            floor.StairsDownX,
            floor.StairsDownY,
            TileType.StairsDown);
    }

    private Room FindFarthestRoom(
        Room startingRoom,
        List<Room> rooms)
    {
        Room farthestRoom = startingRoom;
        int greatestDistance = -1;

        foreach (Room room in rooms)
        {
            int distance =
                Math.Abs(room.CenterX - startingRoom.CenterX) +
                Math.Abs(room.CenterY - startingRoom.CenterY);

            if (distance > greatestDistance)
            {
                greatestDistance = distance;
                farthestRoom = room;
            }
        }

        return farthestRoom;
    }

    private List<Room> GenerateRooms(
        DungeonFloor floor,
        Random random)
    {
        List<Room> rooms = new();

        int attempts = 0;

        while (
            rooms.Count < TargetRoomCount &&
            attempts < MaxPlacementAttempts)
        {
            attempts++;

            int width = random.Next(
                MinRoomSize,
                MaxRoomSize + 1);

            int height = random.Next(
                MinRoomSize,
                MaxRoomSize + 1);

            // Leave space around the outer edge of the map.
            int x = random.Next(
                2,
                floor.Width - width - 1);

            int y = random.Next(
                2,
                floor.Height - height - 1);

            Room newRoom = new(
                x,
                y,
                width,
                height);

            if (OverlapsExistingRoom(
                newRoom,
                rooms))
            {
                continue;
            }

            CarveRoom(
                floor,
                newRoom);

            rooms.Add(newRoom);
        }

        return rooms;
    }

    private bool OverlapsExistingRoom(
        Room room,
        List<Room> rooms)
    {
        foreach (Room existingRoom in rooms)
        {
            if (room.Overlaps(existingRoom))
            {
                return true;
            }
        }

        return false;
    }

    private void CarveRoom(
        DungeonFloor floor,
        Room room)
    {
        for (
            int y = room.Y;
            y < room.Y + room.Height;
            y++)
        {
            for (
                int x = room.X;
                x < room.X + room.Width;
                x++)
            {
                floor.SetTile(
                    x,
                    y,
                    TileType.Floor);
            }
        }
    }

    private void ConnectRooms(
        DungeonFloor floor,
        List<Room> rooms,
        Random random)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Room previousRoom = rooms[i - 1];
            Room currentRoom = rooms[i];

            bool horizontalFirst =
                random.Next(0, 2) == 0;

            if (horizontalFirst)
            {
                CarveHorizontalCorridor(
                    floor,
                    previousRoom.CenterX,
                    currentRoom.CenterX,
                    previousRoom.CenterY);

                CarveVerticalCorridor(
                    floor,
                    previousRoom.CenterY,
                    currentRoom.CenterY,
                    currentRoom.CenterX);
            }
            else
            {
                CarveVerticalCorridor(
                    floor,
                    previousRoom.CenterY,
                    currentRoom.CenterY,
                    previousRoom.CenterX);

                CarveHorizontalCorridor(
                    floor,
                    previousRoom.CenterX,
                    currentRoom.CenterX,
                    currentRoom.CenterY);
            }
        }
    }

    private void CarveHorizontalCorridor(
        DungeonFloor floor,
        int startX,
        int endX,
        int y)
    {
        int minimumX = Math.Min(startX, endX);
        int maximumX = Math.Max(startX, endX);

        for (int x = minimumX; x <= maximumX; x++)
        {
            floor.SetTile(
                x,
                y,
                TileType.Floor);
        }
    }

    private void CarveVerticalCorridor(
        DungeonFloor floor,
        int startY,
        int endY,
        int x)
    {
        int minimumY = Math.Min(startY, endY);
        int maximumY = Math.Max(startY, endY);

        for (int y = minimumY; y <= maximumY; y++)
        {
            floor.SetTile(
                x,
                y,
                TileType.Floor);
        }
    }

    private void GenerateWalls(DungeonFloor floor)
    {
        List<(int X, int Y)> wallPositions = new();

        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                if (floor.GetTile(x, y).Type != TileType.Floor)
                {
                    continue;
                }

                CheckNeighborsForWalls(
                    floor,
                    x,
                    y,
                    wallPositions);
            }
        }

        foreach ((int x, int y) in wallPositions)
        {
            if (floor.GetTile(x, y).Type == TileType.Empty)
            {
                floor.SetTile(
                    x,
                    y,
                    TileType.Wall);
            }
        }
    }

    private void CheckNeighborsForWalls(
        DungeonFloor floor,
        int centerX,
        int centerY,
        List<(int X, int Y)> wallPositions)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = centerX + x;
                int checkY = centerY + y;

                if (!floor.IsInsideBounds(
                    checkX,
                    checkY))
                {
                    continue;
                }

                if (floor.GetTile(
                    checkX,
                    checkY).Type == TileType.Empty)
                {
                    wallPositions.Add(
                        (checkX, checkY));
                }
            }
        }
    }
}