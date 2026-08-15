namespace EndlessDungeon.Dungeon;

public class VisibilityManager
{
    private const int CorridorVisionRange = 6;

    public void UpdateVisibility(
    DungeonFloor floor,
    int playerX,
    int playerY)
    {
        // Tiles visible on the previous turn become remembered.
        MoveVisibleTilesToExplored(floor);

        // Always reveal the explorer and their eight
        // immediately surrounding tiles.
        RevealImmediateArea(
            floor,
            playerX,
            playerY);

        Tile playerTile = floor.GetTile(
            playerX,
            playerY);

        // Standing inside a room reveals the entire room.
        if (playerTile.RegionId >= 0)
        {
            RevealRoom(
                floor,
                playerTile.RegionId);
        }

        // Always cast straight cardinal sight lines.
        // This allows an explorer standing in a room to
        // see through a doorway when aligned with it.
        RevealCardinalSightLines(
            floor,
            playerX,
            playerY);
    }

    private void MoveVisibleTilesToExplored(
        DungeonFloor floor)
    {
        for (int y = 0; y < floor.Height; y++)
        {
            for (int x = 0; x < floor.Width; x++)
            {
                Tile tile = floor.GetTile(x, y);

                if (tile.Visibility == VisibilityState.Visible)
                {
                    tile.Visibility = VisibilityState.Explored;
                }
            }
        }
    }

    private void RevealImmediateArea(
        DungeonFloor floor,
        int playerX,
        int playerY)
    {
        // Reveal the player's tile plus all 8 surrounding tiles.
        for (int yOffset = -1; yOffset <= 1; yOffset++)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                int x = playerX + xOffset;
                int y = playerY + yOffset;

                RevealTile(
                    floor,
                    x,
                    y);
            }
        }
    }

    private void RevealRoom(
        DungeonFloor floor,
        int regionId)
    {
        Room? room = floor.Rooms.FirstOrDefault(
            room => room.Id == regionId);

        if (room == null)
        {
            return;
        }

        // Include one tile around the room so its walls
        // are visible along with the room interior.
        for (
            int y = room.Y - 1;
            y <= room.Y + room.Height;
            y++)
        {
            for (
                int x = room.X - 1;
                x <= room.X + room.Width;
                x++)
            {
                RevealTile(
                    floor,
                    x,
                    y);
            }
        }
    }

    private void RevealCardinalSightLines(
        DungeonFloor floor,
        int playerX,
        int playerY)
    {
        RevealDirection(
            floor,
            playerX,
            playerY,
            0,
            -1);

        RevealDirection(
            floor,
            playerX,
            playerY,
            0,
            1);

        RevealDirection(
            floor,
            playerX,
            playerY,
            -1,
            0);

        RevealDirection(
            floor,
            playerX,
            playerY,
            1,
            0);
    }

    private void RevealDirection(
        DungeonFloor floor,
        int startX,
        int startY,
        int directionX,
        int directionY)
    {
        for (
            int distance = 1;
            distance <= CorridorVisionRange;
            distance++)
        {
            int x =
                startX + directionX * distance;

            int y =
                startY + directionY * distance;

            if (!floor.IsInsideBounds(x, y))
            {
                break;
            }

            Tile tile = floor.GetTile(x, y);

            RevealTile(
                floor,
                x,
                y);

            // The wall itself is visible,
            // but nothing beyond it is.
            if (tile.Type == TileType.Wall)
            {
                break;
            }
        }
    }

    private void RevealTile(
        DungeonFloor floor,
        int x,
        int y)
    {
        if (!floor.IsInsideBounds(x, y))
        {
            return;
        }

        Tile tile = floor.GetTile(x, y);

        // Empty void outside the generated dungeon
        // should remain invisible.
        if (tile.Type == TileType.Empty)
        {
            return;
        }

        tile.Visibility = VisibilityState.Visible;
    }
}