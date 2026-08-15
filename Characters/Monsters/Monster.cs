using EndlessDungeon.Dungeon;

namespace EndlessDungeon.Characters.Monsters;

public abstract class Monster
{
    public string Name { get; }
    public char Glyph { get; }
    public ConsoleColor Color { get; }

    public int X { get; private set; }
    public int Y { get; private set; }

    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }

    public int Attack { get; }
    public int Defense { get; }

    public bool IsAlive => CurrentHealth > 0;

    // Reactions such as retaliation and opportunity attacks
    // consume the monster's normal action for this round.
    public bool HasActedThisRound { get; private set; }

    protected Monster(
        string name,
        char glyph,
        ConsoleColor color,
        int x,
        int y,
        int maxHealth,
        int attack,
        int defense)
    {
        Name = name;
        Glyph = glyph;
        Color = color;

        X = x;
        Y = y;

        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;

        Attack = attack;
        Defense = defense;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(
            0,
            CurrentHealth - damage);
    }

    public void BeginRound()
    {
        HasActedThisRound = false;
    }

    public void TakeTurn(
        DungeonFloor floor,
        Explorer explorer)
    {
        if (!IsAlive || HasActedThisRound)
        {
            return;
        }

        PerformTurn(
            floor,
            explorer);

        HasActedThisRound = true;
    }

    // Each monster type decides what it does on its normal turn.
    protected abstract void PerformTurn(
        DungeonFloor floor,
        Explorer explorer);

    public void Retaliate(Explorer explorer)
    {
        if (!IsAlive)
        {
            return;
        }

        AttackExplorer(explorer);

        HasActedThisRound = true;
    }

    public void MakeOpportunityAttack(
        Explorer explorer)
    {
        if (!IsAlive || HasActedThisRound)
        {
            return;
        }

        AttackExplorer(explorer);

        HasActedThisRound = true;
    }

    protected void AttackExplorer(
        Explorer explorer)
    {
        int damage = Math.Max(
            1,
            Attack - explorer.Defense);

        explorer.TakeDamage(damage);
    }

    protected int GetDistanceToExplorer(
        Explorer explorer)
    {
        return
            Math.Abs(explorer.X - X) +
            Math.Abs(explorer.Y - Y);
    }

    protected void MoveTowardExplorer(
        DungeonFloor floor,
        Explorer explorer)
    {
        int horizontalDirection =
            Math.Sign(explorer.X - X);

        int verticalDirection =
            Math.Sign(explorer.Y - Y);

        int horizontalDistance =
            Math.Abs(explorer.X - X);

        int verticalDistance =
            Math.Abs(explorer.Y - Y);

        // Try the axis with the greatest distance first.
        if (horizontalDistance >= verticalDistance)
        {
            if (TryMove(
                floor,
                explorer,
                horizontalDirection,
                0))
            {
                return;
            }

            TryMove(
                floor,
                explorer,
                0,
                verticalDirection);
        }
        else
        {
            if (TryMove(
                floor,
                explorer,
                0,
                verticalDirection))
            {
                return;
            }

            TryMove(
                floor,
                explorer,
                horizontalDirection,
                0);
        }
    }

    private bool TryMove(
        DungeonFloor floor,
        Explorer explorer,
        int moveX,
        int moveY)
    {
        if (moveX == 0 && moveY == 0)
        {
            return false;
        }

        int targetX = X + moveX;
        int targetY = Y + moveY;

        if (!floor.IsWalkable(
            targetX,
            targetY))
        {
            return false;
        }

        if (floor.GetMonsterAt(
            targetX,
            targetY) != null)
        {
            return false;
        }

        // Normal movement never enters the explorer's tile.
        if (
            targetX == explorer.X &&
            targetY == explorer.Y)
        {
            return false;
        }

        SetPosition(
            targetX,
            targetY);

        return true;
    }
}