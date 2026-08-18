namespace EndlessDungeon.Saving;

public class SaveData
{
    public int Version { get; set; } = 3;

    public ExplorerSaveData Explorer { get; set; } = new();
    public List<StorageStackSaveData> Storage { get; set; } = new();
    public List<HonorRecordSaveData> HonorBoard { get; set; } = new();
    public List<DungeonFloorSaveData> DungeonFloors { get; set; } = new();
}

public class ExplorerSaveData
{
    public string Name { get; set; } = string.Empty;
    public int DungeonSeed { get; set; }

    public int Level { get; set; }
    public int Experience { get; set; }
    public int CurrentHealth { get; set; }
    public int DeepestFloorReached { get; set; }

    public string? EquippedWeaponId { get; set; }
    public string? EquippedArmorId { get; set; }

    public List<string> InventoryItemIds { get; set; } = new();
}

public class StorageStackSaveData
{
    public string ItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class HonorRecordSaveData
{
    public string ExplorerName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int DeepestFloor { get; set; }
    public string CauseOfDeath { get; set; } = string.Empty;
}

public class DungeonFloorSaveData
{
    public int FloorNumber { get; set; }

    public List<TilePositionSaveData> ExploredTiles { get; set; } = new();
    public List<MonsterSaveData> Monsters { get; set; } = new();
    public List<GroundItemSaveData> GroundItems { get; set; } = new();
    public List<ChestSaveData> Chests { get; set; } = new();
}

public class TilePositionSaveData
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class MonsterSaveData
{
    public string MonsterId { get; set; } = string.Empty;

    public int X { get; set; }
    public int Y { get; set; }
    public int CurrentHealth { get; set; }

    public int? HomeRegionId { get; set; }

    public double? InactivityChance { get; set; }

    public int? LastSeenX { get; set; }
    public int? LastSeenY { get; set; }
    public List<string> LootItemIds { get; set; } = new();
}

public class GroundItemSaveData
{
    public string ItemId { get; set; } = string.Empty;

    public int X { get; set; }
    public int Y { get; set; }
}

public class ChestSaveData
{
    public int X { get; set; }
    public int Y { get; set; }

    public bool IsOpened { get; set; }

    public List<string> ItemIds { get; set; } = new();
}