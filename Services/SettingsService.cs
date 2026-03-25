using PoELauncher.Models;
using System.IO;
using System.Text.Json;

namespace PoELauncher.Services;

public class SettingsService
{
    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SettingsService()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaults = new AppSettings();
            Save(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
