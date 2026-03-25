namespace PoELauncher.Models;

public class AppSettings
{
    public string BackgroundImagePath { get; set; } = string.Empty;

    public GameConfig Poe1 { get; set; } = new()
    {
        Key = "poe1",
        DisplayName = "Path of Exile 1"
    };

    public GameConfig Poe2 { get; set; } = new()
    {
        Key = "poe2",
        DisplayName = "Path of Exile 2"
    };
}
