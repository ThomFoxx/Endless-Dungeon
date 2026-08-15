namespace EndlessDungeon.Dungeon;
using EndlessDungeon.Characters.Monsters;

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

        PlaceTestSlime(
            floor,
            rooms,
            random);

        return floor;
    }

    private void PlaceTestSlime(
     DungeonFloor floor,
     List<Room> rooms,
     Random random)
    {
        // Work backward through the rooms to favor
        // spawning away from the explorer's entrance.
        for (int i = rooms.Count - 1; i >= 1; i--)
        {
            Room room = rooms[i];

            int x = room.CenterX;
            int y = room.CenterY;

            Tile tile = floor.GetTile(
                x,
                y);

            if (tile.Type != TileType.Floor)
            {
                continue;
            }

            int distanceFromStart =
                Math.Abs(x - floor.StartX) +
                Math.Abs(y - floor.StartY);

            if (distanceFromStart < 4)
            {
                continue;
            }

            // Every Slime gets its own permanent chance
            // between 30% and 50% of doing nothing.
            double inactivityChance =
                0.30 +
                random.NextDouble() * 0.20;

            floor.AddMonster(
                new Slime(
                    x,
                    y,
                    inactivityChance));

            return;
        }
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

        // Floor 1 has no stairs leading upward.
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

        Room downRoom = FindFarthestRoom(
            startRoom,
            rooms);

        if (rooms.Count == 1)
        {
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

        // Every fifth floor provides an opportunity
        // to safely leave the dungeon.
        if (floor.FloorNumber % 5 == 0)
        {
            PlaceExitPortal(floor);
        }
    }

    private void PlaceExitPortal(
    DungeonFloor floor)
    {
        (int X, int Y)[] directions =
        {
        (1, 0),
        (-1, 0),
        (0, 1),
        (0, -1)
    };

        foreach ((int xOffset, int yOffset) in directions)
        {
            int portalX =
                floor.StairsDownX + xOffset;

            int portalY =
                floor.StairsDownY + yOffset;

            if (!floor.IsInsideBounds(
                portalX,
                portalY))
            {
                continue;
            }

            Tile tile = floor.GetTile(
                portalX,
                portalY);

            // Only replace ordinary floor.
            if (tile.Type != TileType.Floor)
            {
                continue;
            }

            floor.HasExitPortal = true;

            floor.ExitPortalX = portalX;
            floor.ExitPortalY = portalY;

            floor.SetTile(
                portalX,
                portalY,
                TileType.ExitPortal);

            return;
        }

        throw new InvalidOperationException(
            $"Could not place an exit portal beside the downstairs on Floor {floor.FloorNumber}.");
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
                rooms.Count,
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
            floor.AddRoom(newRoom);
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

                floor.GetTile(x, y).RegionId = room.Id;
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