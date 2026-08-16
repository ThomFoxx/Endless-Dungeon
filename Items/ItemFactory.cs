namespace EndlessDungeon.Items;

public static class ItemFactory
{
    public static Item Create(string id)
    {
        return id switch
        {
            ItemIds.ChippedSword => new Weapon(
                ItemIds.ChippedSword,
                "Chipped Sword",
                "†",
                ConsoleColor.DarkGray,
                "A battered sword with several chips along its edge.",
                1),

            ItemIds.IronSword => new Weapon(
                ItemIds.IronSword,
                "Iron Sword",
                "†",
                ConsoleColor.White,
                "A dependable iron sword with a well-worn grip.",
                3),

            ItemIds.LeatherArmor => new Armor(
                ItemIds.LeatherArmor,
                "Leather Armor",
                "◈",
                ConsoleColor.DarkYellow,
                "Simple leather armor offering modest protection.",
                1),

            ItemIds.HealingPotion => new HealingPotion(8),

            _ => throw new ArgumentException(
                $"Unknown item ID: {id}",
                nameof(id))
        };
    }
}