using System.Text.Json;
using CryptoLab.Core.Models;

namespace CryptoLab.Core.Storage;

public sealed class JsonFileStore
{
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonFileStore(string? directory = null)
    {
        _directory = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CryptoLab");
        Directory.CreateDirectory(_directory);
    }

    public string DirectoryPath => _directory;

    public AppSettings LoadSettings()
    {
        return Load(Path.Combine(_directory, "settings.json"), new AppSettings());
    }

    public void SaveSettings(AppSettings settings)
    {
        Save(Path.Combine(_directory, "settings.json"), settings);
    }

    public IReadOnlyList<OperationHistoryEntry> LoadHistory()
    {
        return Load(Path.Combine(_directory, "history.json"), new List<OperationHistoryEntry>());
    }

    public void SaveHistory(IEnumerable<OperationHistoryEntry> history)
    {
        Save(Path.Combine(_directory, "history.json"), history.Take(250).ToList());
    }

    public void ClearSessionData()
    {
        foreach (var name in new[] { "history.json" })
        {
            var path = Path.Combine(_directory, name);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private T Load<T>(string path, T fallback)
    {
        if (!File.Exists(path)) return fallback;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private void Save<T>(string path, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        File.WriteAllText(path, json);
    }
}
