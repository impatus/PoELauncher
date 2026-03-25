using System.Windows;

namespace PoELauncher.Dialogs;

public partial class UrlEntryDialog : Window
{
    public string EntryName { get; private set; } = string.Empty;
    public string EntryUrl { get; private set; } = string.Empty;

    public UrlEntryDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => UrlTextBox.Focus();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var url = UrlTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show(this, "Enter a URL first.", "Missing URL", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = $"https://{url}";
        }

        EntryName = CreateDisplayName(url);
        EntryUrl = url;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }


    private static string CreateDisplayName(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
        {
            return uri.Host;
        }

        return url;
    }
}
