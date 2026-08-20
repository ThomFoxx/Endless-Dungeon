using EndlessDungeon.Characters.Monsters;

namespace EndlessDungeon.Dungeon;

public class FloorProfile
{
    public int Width { get; }
    public int Height { get; }

    public int MinRoomSize { get; }
    public int MaxRoomSize { get; }
    public int TargetRoomCount { get; }

    public int MonsterCount { get; }

    public int EasyThreatWeight { get; }
    public int MediumThreatWeight { get; }
    public int HardThreatWeight { get; }

    public int ChestCount { get; }
    public int ScatteredLootCount { get; }

    public int BaseMonsterLevel { get; }

    public FloorProfile(int width, int height, int minRoomSize, int maxRoomSize, int targetRoomCount, int monsterCount, int easyThreatWeight, int mediumThreatWeight, int hardThreatWeight, int chestCount, int scatteredLootCount, int baseMonsterLevel)
    {
        Width = width;
        Height = height;

        MinRoomSize = minRoomSize;
        MaxRoomSize = maxRoomSize;
        TargetRoomCount = targetRoomCount;

        MonsterCount = monsterCount;

        EasyThreatWeight = easyThreatWeight;
        MediumThreatWeight = mediumThreatWeight;
        HardThreatWeight = hardThreatWeight;

        ChestCount = chestCount;
        ScatteredLootCount = scatteredLootCount;

        BaseMonsterLevel = baseMonsterLevel;
    }

    public static FloorProfile Create(int floorNumber)
    {
        int depth = Math.Max(1, floorNumber);

        int sizeIncrease = (depth - 1) / 3;

        int width = Math.Min(60, 20 + sizeIncrease * 3);
        int height = Math.Min(32, 20 + sizeIncrease);

        int roomIncrease = (depth - 1) / 4;
        int targetRoomCount = Math.Min(12, 6 + roomIncrease);

        int maxRoomSize = Math.Min(10, 5 + (depth - 1) / 5);

        int monsterCount = 2 + (depth - 1) / 3;
        int chestCount = 1 + (depth - 1) / 8;
        int scatteredLootCount = 3 + (depth - 1) / 5;

        int baseMonsterLevel = 1 + (depth - 1) / 2;

        GetThreatWeights(depth, out int easyWeight, out int mediumWeight, out int hardWeight);

        return new FloorProfile(
            width,
            height,
            3,
            maxRoomSize,
            targetRoomCount,
            monsterCount,
            easyWeight,
            mediumWeight,
            hardWeight,
            chestCount,
            scatteredLootCount,
            baseMonsterLevel);
    }

    private static void GetThreatWeights(int floorNumber, out int easy, out int medium, out int hard)
    {
        if (floorNumber <= 10)
        {
            double progress = (floorNumber - 1) / 9.0;

            easy = (int)Math.Round(98 - 68 * progress);
            medium = (int)Math.Round(1 + 59 * progress);
            hard = 100 - easy - medium;

            return;
        }

        if (floorNumber <= 20)
        {
            double progress = (floorNumber - 10) / 10.0;

            easy = (int)Math.Round(30 - 29 * progress);
            medium = (int)Math.Round(60 - 59 * progress);
            hard = 100 - easy - medium;

            return;
        }

        easy = 1;
        medium = 1;
        hard = 98;
    }

    public MonsterThreatTier RollThreatTier(Random random)
    {
        int totalWeight = EasyThreatWeight + MediumThreatWeight + HardThreatWeight;
        int roll = random.Next(totalWeight);

        if (roll < EasyThreatWeight)
        {
            return MonsterThreatTier.Easy;
        }

        roll -= EasyThreatWeight;

        if (roll < MediumThreatWeight)
        {
            return MonsterThreatTier.Medium;
        }

        return MonsterThreatTier.Hard;
    }
}