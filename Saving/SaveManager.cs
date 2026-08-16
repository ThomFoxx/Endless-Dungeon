using System.Text.Json;

namespace EndlessDungeon.Saving;

public class SaveManager
{
    private readonly string _saveDirectory;
    private readonly string _saveFilePath;

    public string SaveFilePath => _saveFilePath;
    public bool HasSaveFile => File.Exists(_saveFilePath);

    public SaveManager()
    {
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EndlessDungeon");

        _saveFilePath = Path.Combine(_saveDirectory, "save.json");
    }

    public void Save(SaveData saveData)
    {
        Directory.CreateDirectory(_saveDirectory);

        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(saveData, options);

        File.WriteAllText(_saveFilePath, json);
    }

    public SaveData? Load()
    {
        if (!File.Exists(_saveFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(_saveFilePath);

        return JsonSerializer.Deserialize<SaveData>(json);
    }
}