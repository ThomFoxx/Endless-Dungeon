using EndlessDungeon.Characters;

namespace EndlessDungeon.Items;

public class HealingPotion : Consumable
{
    public int HealingAmount { get; }

    public HealingPotion(int healingAmount)
        : base(
            ItemIds.HealingPotion,
            "Healing Potion",
            "¡",
            ConsoleColor.Magenta,
            $"A restorative potion that heals up to {healingAmount} HP.")
    {
        HealingAmount = healingAmount;
    }

    public override bool TryUse(Explorer explorer, out string message)
    {
        int healed = explorer.Heal(HealingAmount);

        if (healed <= 0)
        {
            message = "You are already at full health.";
            return false;
        }

        message = $"You drink the Healing Potion and recover {healed} HP.";
        return true;
    }
}