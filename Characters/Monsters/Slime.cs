using EndlessDungeon.Dungeon;

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
            glyph: '●',
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
        Explorer explorer)
    {
        int distance =
            GetDistanceToExplorer(explorer);

        // The Slime remains dormant until the explorer
        // comes reasonably close.
        if (distance > AwarenessRange)
        {
            return;
        }

        // Slimes are unintelligent and sometimes simply
        // fail to respond even when they notice something.
        if (Random.Shared.NextDouble() < InactivityChance)
        {
            return;
        }

        if (distance == 1)
        {
            AttackExplorer(explorer);
            return;
        }

        MoveTowardExplorer(
            floor,
            explorer);
    }
}