using EndlessDungeon.Characters;

namespace EndlessDungeon.Records;

public class HonorBoard
{
    private readonly List<HonorRecord> _records = new();

    public IReadOnlyList<HonorRecord> Records => _records;

    public void AddExplorer(
        Explorer explorer)
    {
        string causeOfDeath =
            $"Slain by {explorer.LastDamageSource}";

        HonorRecord record = new(
            explorer.Name,
            explorer.DeepestFloorReached,
            explorer.Level,
            causeOfDeath);

        _records.Add(record);
    }

    public void AddDebugExplorer(
        Explorer explorer)
    {
        HonorRecord record = new(
            explorer.Name,
            explorer.DeepestFloorReached,
            explorer.Level,
            "Lost to the Debug Void");

        _records.Add(record);
    }
}