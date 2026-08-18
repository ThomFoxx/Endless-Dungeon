using EndlessDungeon.Dungeon;
using EndlessDungeon.UI;

namespace EndlessDungeon.Characters.Monsters;

public class Goblin : Monster
{
    private const int AwarenessRange = 7;
    public int HomeRegionId { get; }

    public Goblin(int x, int y, int homeRegionId)
        : base(
            MonsterIds.Goblin,
            "Goblin",
            "g",
            ConsoleColor.DarkYellow,
            x,
            y,
            5,
            3,
            0)
    {
        HomeRegionId = homeRegionId;
    }

    public int? LastSeenX { get; private set; }
    public int? LastSeenY { get; private set; }

    private bool HasLastSeenPosition =>
        LastSeenX.HasValue && LastSeenY.HasValue;

    protected override void PerformTurn(DungeonFloor floor, Explorer explorer, ActionLog actionLog)
    {
        bool canDetectExplorer = CanDetectExplorer(
            floor,
            explorer,
            AwarenessRange);

        if (canDetectExplorer)
        {
            RememberExplorerPosition(explorer);

            int distance = GetDistanceToExplorer(explorer);

            if (distance == 1)
            {
                int damage = AttackExplorer(explorer);

                actionLog.Add(
                    $"Goblin hits you for {damage} damage.");

                return;
            }

            MoveTowardExplorer(floor, explorer);
            return;
        }

        Tile currentTile = floor.GetTile(X, Y);

        // Once a Goblin has chased an intruder out of its home room,
        // it investigates the last place where it saw them.
        if (currentTile.RegionId != HomeRegionId &&
            HasLastSeenPosition)
        {
            InvestigateLastSeenPosition(floor, explorer);
            return;
        }

        Patrol(floor, explorer);
    }

    private void Patrol(DungeonFloor floor, Explorer explorer)
    {
        Tile currentTile = floor.GetTile(X, Y);

        // If pursuit has taken the Goblin outside its home room,
        // wait here until the Explorer becomes detectable again.
        if (currentTile.RegionId != HomeRegionId)
        {
            return;
        }

        (int X, int Y)[] directions =
        {
        (0, -1),
        (1, 0),
        (0, 1),
        (-1, 0)
    };

        int startIndex = Random.Shared.Next(directions.Length);

        for (int i = 0; i < directions.Length; i++)
        {
            int index = (startIndex + i) % directions.Length;

            int targetX = X + directions[index].X;
            int targetY = Y + directions[index].Y;

            if (!floor.IsInsideBounds(targetX, targetY))
            {
                continue;
            }

            Tile targetTile = floor.GetTile(targetX, targetY);

            // Patrol movement stays inside the Goblin's home room.
            if (targetTile.RegionId != HomeRegionId)
            {
                continue;
            }

            if (TryMove(
                floor,
                explorer,
                directions[index].X,
                directions[index].Y))
            {
                return;
            }
        }
    }

    public void RestoreLastSeenPosition(int? x, int? y)
    {
        LastSeenX = x;
        LastSeenY = y;
    }

    private void RememberExplorerPosition(Explorer explorer)
    {
        LastSeenX = explorer.X;
        LastSeenY = explorer.Y;
    }

    private void ClearLastSeenPosition()
    {
        LastSeenX = null;
        LastSeenY = null;
    }

    protected void MoveTowardPosition(DungeonFloor floor, Explorer explorer, int targetX, int targetY)
    {
        int deltaX = targetX - X;
        int deltaY = targetY - Y;

        // Try the axis with the greatest distance first.
        if (Math.Abs(deltaX) >= Math.Abs(deltaY))
        {
            if (deltaX != 0 &&
                TryMove(floor, explorer, Math.Sign(deltaX), 0))
            {
                return;
            }

            if (deltaY != 0)
            {
                TryMove(floor, explorer, 0, Math.Sign(deltaY));
            }

            return;
        }

        if (deltaY != 0 &&
            TryMove(floor, explorer, 0, Math.Sign(deltaY)))
        {
            return;
        }

        if (deltaX != 0)
        {
            TryMove(floor, explorer, Math.Sign(deltaX), 0);
        }
    }

    private void InvestigateLastSeenPosition(DungeonFloor floor, Explorer explorer)
    {
        if (!LastSeenX.HasValue || !LastSeenY.HasValue)
        {
            return;
        }

        int targetX = LastSeenX.Value;
        int targetY = LastSeenY.Value;

        if (X == targetX && Y == targetY)
        {
            ClearLastSeenPosition();
            return;
        }

        MoveTowardPosition(
            floor,
            explorer,
            targetX,
            targetY);

        // The Goblin reached the location but still cannot see
        // the Explorer. Its investigation has ended for now.
        if (X == targetX && Y == targetY)
        {
            ClearLastSeenPosition();
        }
    }
}