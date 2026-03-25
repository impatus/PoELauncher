using Microsoft.Win32;

namespace PoELauncher.Services;

public class ThemeService
{
    public bool IsWindowsDarkMode()
    {
        try
        {
            using var personalize = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = personalize?.GetValue("AppsUseLightTheme");
            return value is int intValue && intValue == 0;
        }
        catch
        {
            return true;
        }
    }
}
