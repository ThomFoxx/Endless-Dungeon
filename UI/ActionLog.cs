namespace EndlessDungeon.UI;

public class ActionLog
{
    private const int MaxEntries = 4;

    private readonly Queue<string> _entries = new();

    public IReadOnlyList<string> Entries =>
        _entries.ToList();

    public void Add(string message)
    {
        _entries.Enqueue(message);

        while (_entries.Count > MaxEntries)
        {
            _entries.Dequeue();
        }
    }

    public void Clear()
    {
        _entries.Clear();
    }
}