using EndlessDungeon.Dungeon;
using EndlessDungeon.UI;
using EndlessDungeon.Items;

namespace EndlessDungeon.Characters.Monsters;

public abstract class Monster
{
    public string Id { get; }
    public string Name { get; }
    public string Glyph { get; }
    public ConsoleColor Color { get; }

    public int X { get; private set; }
    public int Y { get; private set; }

    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }

    public int Attack { get; }
    public int Defense { get; }

    public bool IsAlive => CurrentHealth > 0;

    public bool HasActedThisRound { get; private set; }

    private readonly List<Item> _loot = new();

    public IReadOnlyList<Item> Loot => _loot;

    protected Monster(string id, string name, string glyph, ConsoleColor color, int x, int y, int maxHealth, int attack, int defense)
    {
        Id = id;
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

    public void RestoreHealth(int currentHealth)
    {
        CurrentHealth = Math.Clamp(currentHealth, 0, MaxHealth);
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

    public void TakeTurn(DungeonFloor floor, Explorer explorer, ActionLog actionLog)
    {
        if (!IsAlive || HasActedThisRound)
        {
            return;
        }

        PerformTurn(
            floor,
            explorer,
            actionLog);

        HasActedThisRound = true;
    }

    protected abstract void PerformTurn(DungeonFloor floor, Explorer explorer, ActionLog actionLog);

    public void Retaliate(Explorer explorer, ActionLog actionLog)
    {
        if (!IsAlive)
        {
            return;
        }

        int damage =
            AttackExplorer(explorer);

        actionLog.Add(
            $"{Name} retaliates for {damage} damage.");

        HasActedThisRound = true;
    }

    public void MakeOpportunityAttack(Explorer explorer, ActionLog actionLog)
    {
        if (!IsAlive || HasActedThisRound)
        {
            return;
        }

        int damage =
            AttackExplorer(explorer);

        actionLog.Add(
            $"{Name} strikes as you retreat for {damage} damage.");

        HasActedThisRound = true;
    }

    protected int AttackExplorer(Explorer explorer)
    {
        int damage = Math.Max(
            1,
            Attack - explorer.Defense);

        explorer.TakeDamage(
            damage,
            Name);

        return damage;
    }

    protected int GetDistanceToExplorer(Explorer explorer)
    {
        return
            Math.Abs(explorer.X - X) +
            Math.Abs(explorer.Y - Y);
    }

    protected bool CanDetectExplorer(DungeonFloor floor, Explorer explorer, int awarenessRange)
    {
        int distance = GetDistanceToExplorer(explorer);

        if (distance > awarenessRange)
        {
            return false;
        }

        Tile monsterTile = floor.GetTile(X, Y);
        Tile explorerTile = floor.GetTile(explorer.X, explorer.Y);

        // Creatures in the same room can see one another.
        if (monsterTile.RegionId >= 0 &&
            monsterTile.RegionId == explorerTile.RegionId)
        {
            return true;
        }

        // Outside rooms, detection requires a clear cardinal sight line.
        if (X == explorer.X)
        {
            return HasClearVerticalSightLine(
                floor,
                X,
                Y,
                explorer.Y);
        }

        if (Y == explorer.Y)
        {
            return HasClearHorizontalSightLine(
                floor,
                Y,
                X,
                explorer.X);
        }

        return false;
    }

    protected void MoveTowardExplorer(DungeonFloor floor, Explorer explorer)
    {
        int horizontalDirection =
            Math.Sign(explorer.X - X);

        int verticalDirection =
            Math.Sign(explorer.Y - Y);

        int horizontalDistance =
            Math.Abs(explorer.X - X);

        int verticalDistance =
            Math.Abs(explorer.Y - Y);

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

    protected bool TryMove(DungeonFloor floor, Explorer explorer, int moveX, int moveY)
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

    private bool HasClearVerticalSightLine(DungeonFloor floor, int x, int startY, int endY)
    {
        int direction = Math.Sign(endY - startY);

        for (int y = startY + direction;
             y != endY;
             y += direction)
        {
            if (!floor.GetTile(x, y).IsWalkable)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasClearHorizontalSightLine(DungeonFloor floor, int y, int startX, int endX)
    {
        int direction = Math.Sign(endX - startX);

        for (int x = startX + direction;
             x != endX;
             x += direction)
        {
            if (!floor.GetTile(x, y).IsWalkable)
            {
                return false;
            }
        }

        return true;
    }

    public void AddLoot(Item item)
    {
        _loot.Add(item);
    }

    public List<Item> TakeAllLoot()
    {
        List<Item> loot = new(_loot);
        _loot.Clear();

        return loot;
    }
}