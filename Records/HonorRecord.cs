namespace EndlessDungeon.Records;

public class HonorRecord
{
    public string ExplorerName { get; }
    public int DeepestFloor { get; }
    public int Level { get; }
    public string CauseOfDeath { get; }

    public HonorRecord(
        string explorerName,
        int deepestFloor,
        int level,
        string causeOfDeath)
    {
        ExplorerName = explorerName;
        DeepestFloor = deepestFloor;
        Level = level;
        CauseOfDeath = causeOfDeath;
    }
}