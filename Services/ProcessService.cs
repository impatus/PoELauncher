using PoELauncher.Models;
using System.Diagnostics;
using System.IO;

namespace PoELauncher.Services;

public class ProcessService
{
    private readonly Dictionary<string, List<Process>> _launchedMods = new(StringComparer.OrdinalIgnoreCase);

    public string StartGame(GameConfig gameConfig)
    {
        if (string.IsNullOrWhiteSpace(gameConfig.GameExePath) || !File.Exists(gameConfig.GameExePath))
        {
            return $"Could not find the game executable for {gameConfig.DisplayName}.";
        }

        CleanupExitedProcesses(gameConfig.Key);

        var launchedCount = StartTrackedEntries(gameConfig.Key, gameConfig.Mods.Where(m => m.IsEnabled), out var modErrors);
        var openedUrlCount = OpenUrls(gameConfig.Urls.Where(u => u.IsEnabled), out var urlErrors);
        var errors = new List<string>();
        errors.AddRange(modErrors);
        errors.AddRange(urlErrors);

        try
        {
            LaunchProcess(gameConfig.GameExePath, string.Empty, track: false);
        }
        catch (Exception ex)
        {
            errors.Add($"Could not start the game: {ex.Message}");
        }

        if (errors.Count == 0)
        {
            return $"Started {gameConfig.DisplayName}, {launchedCount} mod(s), and opened {openedUrlCount} URL(s).";
        }

        return $"Started with errors. {string.Join(" | ", errors)}";
    }

    public string StartLabCompass(GameConfig gameConfig)
    {
        CleanupExitedProcesses(gameConfig.Key);

        var launchedCount = StartTrackedEntries(gameConfig.Key, gameConfig.LabTools.Where(m => m.IsEnabled), out var errors);

        if (errors.Count == 0)
        {
            return launchedCount == 0
                ? "No enabled Lab Compass tool is configured."
                : $"Started {launchedCount} Lab Compass tool(s).";
        }

        return $"Lab Compass started with errors. {string.Join(" | ", errors)}";
    }

    public string CloseMods(GameConfig gameConfig)
    {
        CleanupExitedProcesses(gameConfig.Key);

        if (!_launchedMods.TryGetValue(gameConfig.Key, out var processes) || processes.Count == 0)
        {
            return $"No tracked mod processes found for {gameConfig.DisplayName}. Start the tools from this launcher first.";
        }

        var closed = 0;
        foreach (var process in processes.ToList())
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                    process.WaitForExit(3000);
                    closed++;
                }
            }
            catch
            {
                // Ignore and continue.
            }
        }

        processes.Clear();
        return $"Closed {closed} tracked process(es) for {gameConfig.DisplayName}.";
    }

    private int StartTrackedEntries(string gameKey, IEnumerable<ModEntry> entries, out List<string> errors)
    {
        var launchedCount = 0;
        errors = new List<string>();

        foreach (var entry in entries)
        {
            try
            {
                var process = LaunchProcess(entry.FilePath, entry.Arguments, track: true);
                if (process != null)
                {
                    TrackMod(gameKey, process);
                    launchedCount++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{entry.Name}: {ex.Message}");
            }
        }

        return launchedCount;
    }

    private static int OpenUrls(IEnumerable<UrlEntry> urls, out List<string> errors)
    {
        var count = 0;
        errors = new List<string>();

        foreach (var url in urls)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url.Url))
                {
                    throw new InvalidOperationException("URL is empty.");
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url.Url,
                    UseShellExecute = true
                });

                count++;
            }
            catch (Exception ex)
            {
                errors.Add($"{url.Name}: {ex.Message}");
            }
        }

        return count;
    }

    private void TrackMod(string gameKey, Process process)
    {
        if (!_launchedMods.ContainsKey(gameKey))
        {
            _launchedMods[gameKey] = new List<Process>();
        }

        _launchedMods[gameKey].Add(process);
    }

    private void CleanupExitedProcesses(string gameKey)
    {
        if (!_launchedMods.TryGetValue(gameKey, out var processes))
        {
            return;
        }

        processes.RemoveAll(process =>
        {
            try
            {
                return process.HasExited;
            }
            catch
            {
                return true;
            }
        });
    }

    private static Process? LaunchProcess(string filePath, string arguments, bool track)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException("Could not find the file.", filePath);
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var workingDirectory = Path.GetDirectoryName(filePath) ?? AppDomain.CurrentDomain.BaseDirectory;
        var startInfo = BuildStartInfo(filePath, arguments, extension, workingDirectory, track);

        return Process.Start(startInfo);
    }

    private static ProcessStartInfo BuildStartInfo(string filePath, string arguments, string extension, string workingDirectory, bool track)
    {
        if (extension == ".jar")
        {
            return new ProcessStartInfo
            {
                FileName = "javaw.exe",
                Arguments = $"-jar \"{filePath}\" {arguments}".Trim(),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        if (extension == ".ahk")
        {
            var autoHotkeyExe = FindAutoHotkeyExecutable();
            if (string.IsNullOrWhiteSpace(autoHotkeyExe))
            {
                throw new FileNotFoundException("Could not find AutoHotkey. Install AutoHotkey v1 or v2 to run .ahk files.");
            }

            return new ProcessStartInfo
            {
                FileName = autoHotkeyExe,
                Arguments = $"\"{filePath}\" {arguments}".Trim(),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = !track,
                WindowStyle = track ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };
        }

        if (extension == ".ps1")
        {
            return new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{filePath}\" {arguments}".Trim(),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        if (extension is ".bat" or ".cmd")
        {
            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{filePath}\" {arguments}".Trim(),
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        return new ProcessStartInfo
        {
            FileName = filePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = !track,
            WindowStyle = ProcessWindowStyle.Normal
        };
    }

    private static string? FindAutoHotkeyExecutable()
    {
        var candidates = new List<string>();

        void AddIfValue(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                candidates.Add(value);
            }
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        AddIfValue(Path.Combine(programFiles, "AutoHotkey", "v2", "AutoHotkey64.exe"));
        AddIfValue(Path.Combine(programFiles, "AutoHotkey", "v2", "AutoHotkey.exe"));
        AddIfValue(Path.Combine(programFiles, "AutoHotkey", "AutoHotkey64.exe"));
        AddIfValue(Path.Combine(programFiles, "AutoHotkey", "AutoHotkey.exe"));
        AddIfValue(Path.Combine(programFilesX86, "AutoHotkey", "AutoHotkeyU64.exe"));
        AddIfValue(Path.Combine(programFilesX86, "AutoHotkey", "AutoHotkey.exe"));
        AddIfValue(Path.Combine(localAppData, "Programs", "AutoHotkey", "AutoHotkey64.exe"));
        AddIfValue(Path.Combine(localAppData, "Programs", "AutoHotkey", "AutoHotkey.exe"));

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var pathEntry in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            AddIfValue(Path.Combine(pathEntry, "AutoHotkey64.exe"));
            AddIfValue(Path.Combine(pathEntry, "AutoHotkey.exe"));
            AddIfValue(Path.Combine(pathEntry, "AutoHotkeyU64.exe"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }
}
