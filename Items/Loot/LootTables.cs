using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon.Items.Loot;

public static class LootTables
{
    public static readonly LootTable ScatteredBasic = new LootTable()
        .Add(ItemIds.HealingPotion, 50)
        .Add(ItemIds.LeatherArmor, 30)
        .Add(ItemIds.IronSword, 20);

    public static readonly LootTable SlimeDrops = new LootTable(noDropWeight: 75)
        .Add(ItemIds.HealingPotion, 25);

    public static readonly LootTable GoblinDrops = new LootTable(noDropWeight: 55)
        .Add(ItemIds.HealingPotion, 25)
        .Add(ItemIds.IronSword, 20);

    public static readonly LootTable BasicChest = new LootTable()
        .Add(ItemIds.HealingPotion, 45)
        .Add(ItemIds.LeatherArmor, 30)
        .Add(ItemIds.IronSword, 25);

    public static LootTable? GetMonsterDrops(string monsterId)
    {
        return monsterId switch
        {
            MonsterIds.Slime => SlimeDrops,
            MonsterIds.Goblin => GoblinDrops,
            _ => null
        };
    }
}