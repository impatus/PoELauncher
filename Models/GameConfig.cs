using System.Collections.ObjectModel;

namespace PoELauncher.Models;

public class GameConfig
{
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GameExePath { get; set; } = string.Empty;
    public ObservableCollection<ModEntry> Mods { get; set; } = new();
    public ObservableCollection<UrlEntry> Urls { get; set; } = new();
    public ObservableCollection<ModEntry> LabTools { get; set; } = new();
}
