using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.Items;
using EndlessDungeon.Items.Loot;

namespace EndlessDungeon.Dungeon;

public class DungeonGenerator
{
    private const int MaxPlacementAttempts = 100;

    public DungeonFloor GenerateFloor(int floorNumber, int seed, FloorProfile profile)
    {
        Random random = new(seed);

        DungeonFloor floor = new(floorNumber, profile.Width, profile.Height, seed);

        List<Room> rooms = GenerateRooms(floor, random, profile);

        ConnectRooms(floor, rooms, random);
        GenerateWalls(floor);
        PlaceStairs(floor, rooms);
        PlaceMonsters(floor, rooms, random, profile);
        PlaceChests(floor, rooms, random, profile.ChestCount);
        PlaceScatteredLoot(floor, rooms, random, profile.ScatteredLootCount);

        return floor;
    }

    private void PlaceScatteredLoot(DungeonFloor floor, List<Room> rooms, Random random, int itemCount)
    {
        int placedItems = 0;
        int attemptsRemaining = itemCount * 20;

        while (placedItems < itemCount &&
               attemptsRemaining > 0)
        {
            attemptsRemaining--;

            Item? item = LootTables.ScatteredBasic.Roll(random);

            if (item == null)
            {
                continue;
            }

            if (TryPlaceGroundItem(
                floor,
                rooms,
                random,
                item))
            {
                placedItems++;
            }
        }
    }

    private bool TryPlaceGroundItem(DungeonFloor floor, List<Room> rooms, Random random, Item item)
    {
        if (rooms.Count <= 1)
        {
            return false;
        }

        const int MaxAttempts = 30;

        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            // Skip the starting room for loose loot.
            Room room = rooms[random.Next(1, rooms.Count)];

            int x = random.Next(
                room.X,
                room.X + room.Width);

            int y = random.Next(
                room.Y,
                room.Y + room.Height);

            if (!floor.IsInsideBounds(x, y))
            {
                continue;
            }

            Tile tile = floor.GetTile(x, y);

            // Don't replace stairs, portals, etc.
            if (tile.Type != TileType.Floor)
            {
                continue;
            }

            if (floor.GetMonsterAt(x, y) != null)
            {
                continue;
            }

            if (floor.GetGroundItemAt(x, y) != null)
            {
                continue;
            }

            if (floor.GetChestAt(x, y) != null)
            {
                continue;
            }

            floor.AddGroundItem(new GroundItem(
                item,
                x,
                y));

            return true;
        }

        return false;
    }

    private void PlaceMonsters(DungeonFloor floor, List<Room> rooms, Random random, FloorProfile profile)
    {
        int placedMonsters = 0;
        int attemptsRemaining = profile.MonsterCount * 30;

        while (placedMonsters < profile.MonsterCount && attemptsRemaining > 0)
        {
            attemptsRemaining--;

            if (rooms.Count <= 1)
            {
                return;
            }

            Room room = rooms[random.Next(1, rooms.Count)];

            int x = random.Next(room.X, room.X + room.Width);
            int y = random.Next(room.Y, room.Y + room.Height);

            if (!CanPlaceMonster(floor, x, y))
            {
                continue;
            }

            MonsterThreatTier tier = profile.RollThreatTier(random);
            string monsterId = MonsterSpawnTables.Roll(tier, random);

            Monster monster = CreateGeneratedMonster(monsterId, x, y, room.Id, random);
            AddGeneratedMonsterLoot(monster, random);

            floor.AddMonster(monster);
            placedMonsters++;
        }
    }

    private bool CanPlaceMonster(DungeonFloor floor, int x, int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return false;
        }

        if (floor.GetTile(x, y).Type != TileType.Floor)
        {
            return false;
        }

        if (floor.GetMonsterAt(x, y) != null)
        {
            return false;
        }

        if (floor.GetGroundItemAt(x, y) != null)
        {
            return false;
        }

        if (floor.GetChestAt(x, y) != null)
        {
            return false;
        }

        int distanceFromStart = Math.Abs(x - floor.StartX) + Math.Abs(y - floor.StartY);

        return distanceFromStart >= 4;
    }

    private Monster CreateGeneratedMonster(string monsterId, int x, int y, int roomId, Random random)
    {
        return monsterId switch
        {
            MonsterIds.Slime => new Slime(x, y, 0.30 + random.NextDouble() * 0.20),
            MonsterIds.Goblin => new Goblin(x, y, roomId),

            _ => throw new InvalidOperationException($"Unknown generated monster ID: {monsterId}")
        };
    }

    private void AddGeneratedMonsterLoot(Monster monster, Random random)
    {
        LootTable? lootTable = LootTables.GetMonsterDrops(monster.Id);

        if (lootTable == null)
        {
            return;
        }

        Item? item = lootTable.Roll(random);

        if (item != null)
        {
            monster.AddLoot(item);
        }
    }

    private void PlaceChests(DungeonFloor floor, List<Room> rooms, Random random, int chestCount)
    {
        int placedChests = 0;
        int attemptsRemaining = chestCount * 30;

        while (placedChests < chestCount && attemptsRemaining > 0)
        {
            attemptsRemaining--;

            if (rooms.Count <= 1)
            {
                return;
            }

            Room room = rooms[random.Next(1, rooms.Count)];

            int x = random.Next(room.X, room.X + room.Width);
            int y = random.Next(room.Y, room.Y + room.Height);

            if (!floor.IsInsideBounds(x, y))
            {
                continue;
            }

            Tile tile = floor.GetTile(x, y);

            if (tile.Type != TileType.Floor)
            {
                continue;
            }

            if (floor.GetMonsterAt(x, y) != null)
            {
                continue;
            }

            if (floor.GetGroundItemAt(x, y) != null)
            {
                continue;
            }

            if (floor.GetChestAt(x, y) != null)
            {
                continue;
            }

            Chest chest = new(x, y);

            List<Item> contents = LootTables.BasicChest.Roll(random, 2);

            foreach (Item item in contents)
            {
                chest.AddItem(item);
            }

            floor.AddChest(chest);
            placedChests++;
        }
    }

    private void PlaceStairs(DungeonFloor floor, List<Room> rooms)
    {
        if (rooms.Count == 0)
        {
            throw new InvalidOperationException("Cannot place stairs on a floor with no rooms.");
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

            floor.SetTile(floor.StairsUpX, floor.StairsUpY, TileType.StairsUp);
        }

        Room downRoom = FindFarthestRoom(startRoom, rooms);

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

        floor.SetTile(floor.StairsDownX, floor.StairsDownY, TileType.StairsDown);

        // Every fifth floor provides an opportunity
        // to safely leave the dungeon.
        if (floor.FloorNumber % 5 == 0)
        {
            PlaceExitPortal(floor);
        }
    }

    private void PlaceExitPortal(DungeonFloor floor)
    {
        (int X, int Y)[] directions = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        foreach ((int xOffset, int yOffset) in directions)
        {
            int portalX = floor.StairsDownX + xOffset;

            int portalY = floor.StairsDownY + yOffset;

            if (!floor.IsInsideBounds(portalX, portalY))
            {
                continue;
            }

            Tile tile = floor.GetTile(portalX, portalY);

            // Only replace ordinary floor.
            if (tile.Type != TileType.Floor)
            {
                continue;
            }

            floor.HasExitPortal = true;

            floor.ExitPortalX = portalX;
            floor.ExitPortalY = portalY;

            floor.SetTile(portalX, portalY, TileType.ExitPortal);

            return;
        }

        throw new InvalidOperationException(
            $"Could not place an exit portal beside the downstairs on Floor {floor.FloorNumber}.");
    }

    private Room FindFarthestRoom(Room startingRoom, List<Room> rooms)
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

    private List<Room> GenerateRooms(DungeonFloor floor, Random random, FloorProfile profile)
    {
        List<Room> rooms = new();

        int attempts = 0;

        while (rooms.Count < profile.TargetRoomCount && attempts < MaxPlacementAttempts)
        {
            attempts++;

            int width = random.Next(profile.MinRoomSize, profile.MaxRoomSize + 1);
            int height = random.Next(profile.MinRoomSize, profile.MaxRoomSize + 1);

            // Leave space around the outer edge of the map.
            int x = random.Next(2, floor.Width - width - 1);

            int y = random.Next(2, floor.Height - height - 1);

            Room newRoom = new(rooms.Count, x, y, width, height);

            if (OverlapsExistingRoom(newRoom, rooms))
            {
                continue;
            }

            CarveRoom(floor, newRoom);

            rooms.Add(newRoom);
            floor.AddRoom(newRoom);
        }

        return rooms;
    }

    private bool OverlapsExistingRoom(Room room, List<Room> rooms)
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

    private void CarveRoom(DungeonFloor floor, Room room)
    {
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                floor.SetTile(x, y, TileType.Floor);

                floor.GetTile(x, y).RegionId = room.Id;
            }
        }
    }

    private void ConnectRooms(DungeonFloor floor, List<Room> rooms, Random random)
    {
        for (int i = 1; i < rooms.Count; i++)
        {
            Room previousRoom = rooms[i - 1];
            Room currentRoom = rooms[i];

            bool horizontalFirst = random.Next(0, 2) == 0;

            if (horizontalFirst)
            {
                CarveHorizontalCorridor(floor, previousRoom.CenterX, currentRoom.CenterX, previousRoom.CenterY);

                CarveVerticalCorridor(floor, previousRoom.CenterY, currentRoom.CenterY, currentRoom.CenterX);
            }
            else
            {
                CarveVerticalCorridor(floor, previousRoom.CenterY, currentRoom.CenterY, previousRoom.CenterX);

                CarveHorizontalCorridor(floor, previousRoom.CenterX, currentRoom.CenterX, currentRoom.CenterY);
            }
        }
    }

    private void CarveHorizontalCorridor(DungeonFloor floor, int startX, int endX, int y)
    {
        int minimumX = Math.Min(startX, endX);
        int maximumX = Math.Max(startX, endX);

        for (int x = minimumX; x <= maximumX; x++)
        {
            floor.SetTile(x, y, TileType.Floor);
        }
    }

    private void CarveVerticalCorridor(DungeonFloor floor, int startY, int endY, int x)
    {
        int minimumY = Math.Min(startY, endY);
        int maximumY = Math.Max(startY, endY);

        for (int y = minimumY; y <= maximumY; y++)
        {
            floor.SetTile(x, y, TileType.Floor);
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

                CheckNeighborsForWalls(floor, x, y, wallPositions);
            }
        }

        foreach ((int x, int y) in wallPositions)
        {
            if (floor.GetTile(x, y).Type == TileType.Empty)
            {
                floor.SetTile(x, y, TileType.Wall);
            }
        }
    }

    private void CheckNeighborsForWalls(DungeonFloor floor, int centerX, int centerY, List<(int X, int Y)> wallPositions)
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

                if (!floor.IsInsideBounds(checkX, checkY))
                {
                    continue;
                }

                if (floor.GetTile(checkX, checkY).Type == TileType.Empty)
                {
                    wallPositions.Add((checkX, checkY));
                }
            }
        }
    }

    private void AddMonsterLoot(Monster monster, LootTable lootTable, Random random)
    {
        Item? item = lootTable.Roll(random);

        if (item != null)
        {
            monster.AddLoot(item);
        }
    }
}