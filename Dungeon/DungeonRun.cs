namespace EndlessDungeon.Dungeon;

public class DungeonRun
{
    private const int TestFloorWidth = 20;
    private const int TestFloorHeight = 20;

    private readonly DungeonGenerator _generator;
    private readonly Dictionary<int, DungeonFloor> _floors;

    public int ExplorerSeed { get; }

    public int CurrentFloorNumber { get; private set; } = 1;

    public DungeonRun(int explorerSeed)
    {
        ExplorerSeed = explorerSeed;

        _generator = new DungeonGenerator();
        _floors = new Dictionary<int, DungeonFloor>();
    }

    public DungeonFloor GetCurrentFloor()
    {
        return GetFloor(CurrentFloorNumber);
    }

    public DungeonFloor Descend()
    {
        CurrentFloorNumber++;

        return GetFloor(CurrentFloorNumber);
    }

    public DungeonFloor Ascend()
    {
        if (CurrentFloorNumber > 1)
        {
            CurrentFloorNumber--;
        }

        return GetFloor(CurrentFloorNumber);
    }

    private DungeonFloor GetFloor(int floorNumber)
    {
        // Return the existing floor if we've already visited it.
        if (_floors.TryGetValue(
            floorNumber,
            out DungeonFloor? existingFloor))
        {
            return existingFloor;
        }

        int floorSeed = unchecked(
            ExplorerSeed + floorNumber - 1);

        DungeonFloor newFloor = _generator.GenerateFloor(
            floorNumber,
            TestFloorWidth,
            TestFloorHeight,
            floorSeed);

        _floors.Add(
            floorNumber,
            newFloor);

        return newFloor;
    }
}