using Microsoft.Win32;
using System.Windows;

namespace PoELauncher;

public partial class App : Application
{
    private readonly Services.ThemeService _themeService = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApplyTheme();
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        base.OnExit(e);
    }

    private void OnUserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
        Dispatcher.Invoke(ApplyTheme);
    }

    public void ApplyTheme()
    {
        var themePath = _themeService.IsWindowsDarkMode() ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        });
    }
}
