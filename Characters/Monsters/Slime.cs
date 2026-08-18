using EndlessDungeon.Dungeon;
using EndlessDungeon.UI;

namespace EndlessDungeon.Characters.Monsters;

public class Slime : Monster
{
    private const int AwarenessRange = 6;

    public double InactivityChance { get; }

    public Slime(int x, int y, double inactivityChance)
    : base(
        MonsterIds.Slime,
        "Slime",
        "●",
        ConsoleColor.Green,
        x,
        y,
        6,
        2,
        0)
    {
        InactivityChance = Math.Clamp(inactivityChance, 0.30, 0.50);
    }

    protected override void PerformTurn(
        DungeonFloor floor,
        Explorer explorer,
        ActionLog actionLog)
    {
        int distance =
            GetDistanceToExplorer(explorer);

        if (!CanDetectExplorer(floor, explorer, AwarenessRange))
        {
            return;
        }

        // Slimes sometimes simply fail to act.
        if (Random.Shared.NextDouble() < InactivityChance)
        {
            return;
        }

        if (distance == 1)
        {
            int damage =
                AttackExplorer(explorer);

            actionLog.Add(
                $"Slime hits you for {damage} damage.");

            return;
        }

        MoveTowardExplorer(
            floor,
            explorer);
    }
}