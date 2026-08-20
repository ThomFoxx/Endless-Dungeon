using EndlessDungeon.Characters.Monsters;
using EndlessDungeon.Items;

namespace EndlessDungeon.Characters;

public class Explorer
{
    private readonly List<Item> _inventory = new();

    public string Name { get; set; }
    public string Glyph { get; set; } = "₽";
    public long Id { get; }

    public int DungeonSeed { get; private set; }

    public int X { get; set; }
    public int Y { get; set; }

    public int MaxHealth { get; set; } = 20;
    public int CurrentHealth { get; set; } = 20;

    public int BaseAttack { get; set; } = 2;
    public int Attack => BaseAttack + (EquippedWeapon?.AttackBonus ?? 0);

    public int BaseDefense { get; set; } = 1;
    public int Defense => BaseDefense + (EquippedArmor?.DefenseBonus ?? 0);
    public Armor? EquippedArmor { get; private set; }

    public int Level { get; set; } = 1;
    public int Experience { get; set; }

    public int DeepestFloorReached { get; private set; } = 1;
    public string LastDamageSource { get; private set; } = "Unknown";

    public Weapon? EquippedWeapon { get; private set; }

    public IReadOnlyList<Item> Inventory => _inventory;

    public bool IsAlive => CurrentHealth > 0;

    public Explorer(long id, string name, int dungeonSeed)
    {
        Id = id;
        Name = name;
        DungeonSeed = dungeonSeed;
    }

    public void SetDungeonSeed(int dungeonSeed)
    {
        DungeonSeed = dungeonSeed;
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void RecordFloorReached(int floorNumber)
    {
        DeepestFloorReached = Math.Max(DeepestFloorReached, floorNumber);
    }

    public void TakeDamage(int damage, string source)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        LastDamageSource = source;
    }

    public int AttackMonster(Monster monster)
    {
        int damage = Math.Max(1, Attack - monster.Defense);

        monster.TakeDamage(damage);

        return damage;
    }

    public void AddItem(Item item)
    {
        item.AssignOriginalExplorer(Id);
        _inventory.Add(item);
    }

    public bool RemoveItem(Item item)
    {
        return _inventory.Remove(item);
    }

    public void EquipStartingWeapon(Weapon weapon)
    {
        weapon.AssignOriginalExplorer(Id);
        EquippedWeapon = weapon;
    }

    public bool EquipWeapon(Weapon weapon)
    {
        if (!_inventory.Contains(weapon))
        {
            return false;
        }

        _inventory.Remove(weapon);

        if (EquippedWeapon != null)
        {
            _inventory.Add(EquippedWeapon);
        }

        EquippedWeapon = weapon;

        return true;
    }

    public bool EquipArmor(Armor armor)
    {
        if (!_inventory.Contains(armor))
        {
            return false;
        }

        _inventory.Remove(armor);

        if (EquippedArmor != null)
        {
            _inventory.Add(EquippedArmor);
        }

        EquippedArmor = armor;
        return true;
    }

    public int Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth >= MaxHealth)
        {
            return 0;
        }

        int healthBefore = CurrentHealth;

        CurrentHealth = Math.Min(
            MaxHealth,
            CurrentHealth + amount);

        return CurrentHealth - healthBefore;
    }

    public void RestoreProgress( int level, int experience, int currentHealth, int deepestFloorReached)
    {
        Level = Math.Max(1, level);
        Experience = Math.Max(0, experience);
        CurrentHealth = Math.Clamp(currentHealth, 1, MaxHealth);
        DeepestFloorReached = Math.Max(1, deepestFloorReached);
    }

    public void ClearInventory()
    {
        _inventory.Clear();
    }

    public void RestoreWeapon(Weapon? weapon)
    {
        EquippedWeapon = weapon;
    }

    public void RestoreArmor(Armor? armor)
    {
        EquippedArmor = armor;
    }
}