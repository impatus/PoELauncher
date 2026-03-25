using System.IO;

namespace PoELauncher.Services;

public class GameDetectionService
{
    public string? TryDetectPoe1()
    {
        var candidates = new[]
        {
            @"C:\Program Files (x86)\Grinding Gear Games\Path of Exile\PathOfExile_x64.exe",
            @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile\PathOfExileSteam.exe",
            @"C:\Program Files\Steam\steamapps\common\Path of Exile\PathOfExileSteam.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    public string? TryDetectPoe2()
    {
        var candidates = new[]
        {
            @"C:\Program Files (x86)\Grinding Gear Games\Path of Exile 2\PathOfExile2.exe",
            @"C:\Program Files (x86)\Steam\steamapps\common\Path of Exile 2\PathOfExile2.exe",
            @"C:\Program Files\Steam\steamapps\common\Path of Exile 2\PathOfExile2.exe"
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
