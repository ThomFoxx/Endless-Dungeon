using EndlessDungeon.Dungeon;
using EndlessDungeon.UI;

namespace EndlessDungeon.Characters.Monsters;

public class Slime : Monster
{
    private const int AwarenessRange = 6;

    public double InactivityChance { get; }

    public Slime(
        int x,
        int y,
        double inactivityChance)
        : base(
            name: "Slime",
            glyph: "●",
            color: ConsoleColor.Green,
            x: x,
            y: y,
            maxHealth: 6,
            attack: 2,
            defense: 0)
    {
        InactivityChance = Math.Clamp(
            inactivityChance,
            0.30,
            0.50);
    }

    protected override void PerformTurn(
        DungeonFloor floor,
        Explorer explorer,
        ActionLog actionLog)
    {
        int distance =
            GetDistanceToExplorer(explorer);

        if (distance > AwarenessRange)
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