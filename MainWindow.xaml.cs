using Microsoft.Win32;
using PoELauncher.Dialogs;
using PoELauncher.Models;
using PoELauncher.Services;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PoELauncher;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly GameDetectionService _gameDetectionService = new();
    private readonly ProcessService _processService = new();

    private AppSettings _settings = new();
    private GameConfig? _selectedGame;

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        ShowHome();
        SetStatus("Ready.");
    }

    private void LoadSettings()
    {
        _settings = _settingsService.Load();

        ObserveGame(_settings.Poe1);
        ObserveGame(_settings.Poe2);

        Poe1PathTextBox.Text = _settings.Poe1.GameExePath;
        Poe2PathTextBox.Text = _settings.Poe2.GameExePath;
        BackgroundImagePathTextBox.Text = _settings.BackgroundImagePath;
        ApplyBackgroundImage();

        Poe1ModsListView.ItemsSource = _settings.Poe1.Mods;
        Poe2ModsListView.ItemsSource = _settings.Poe2.Mods;
        Poe1UrlsListView.ItemsSource = _settings.Poe1.Urls;
        Poe2UrlsListView.ItemsSource = _settings.Poe2.Urls;
        Poe1LabListView.ItemsSource = _settings.Poe1.LabTools;
    }

    private void ObserveGame(GameConfig game)
    {
        game.Mods.CollectionChanged += (_, e) => ModCollectionChanged(game, e);
        game.Urls.CollectionChanged += (_, e) => UrlCollectionChanged(game, e);
        game.LabTools.CollectionChanged += (_, e) => LabCollectionChanged(game, e);

        foreach (var mod in game.Mods)
        {
            mod.PropertyChanged += Mod_PropertyChanged;
        }

        foreach (var url in game.Urls)
        {
            url.PropertyChanged += Url_PropertyChanged;
        }

        foreach (var lab in game.LabTools)
        {
            lab.PropertyChanged += Mod_PropertyChanged;
        }
    }

    private void ModCollectionChanged(GameConfig game, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ModEntry mod in e.NewItems)
            {
                mod.PropertyChanged += Mod_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ModEntry mod in e.OldItems)
            {
                mod.PropertyChanged -= Mod_PropertyChanged;
            }
        }

        SaveSettings();
        RefreshSelectedGameView();
    }

    private void UrlCollectionChanged(GameConfig game, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (UrlEntry url in e.NewItems)
            {
                url.PropertyChanged += Url_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (UrlEntry url in e.OldItems)
            {
                url.PropertyChanged -= Url_PropertyChanged;
            }
        }

        SaveSettings();
        RefreshSelectedGameView();
    }

    private void LabCollectionChanged(GameConfig game, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ModEntry mod in e.NewItems)
            {
                mod.PropertyChanged += Mod_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ModEntry mod in e.OldItems)
            {
                mod.PropertyChanged -= Mod_PropertyChanged;
            }
        }

        SaveSettings();
        RefreshSelectedGameView();
    }

    private void Mod_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
        RefreshSelectedGameView();
    }

    private void Url_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
        RefreshSelectedGameView();
    }

    private void SaveSettings()
    {
        _settingsService.Save(_settings);
    }

    private void UpdateNavigation(bool showBack, bool showSettings)
    {
        BackButton.Visibility = showBack ? Visibility.Visible : Visibility.Collapsed;
        SettingsButton.Visibility = showSettings ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowHome()
    {
        _selectedGame = null;
        HomeView.Visibility = Visibility.Visible;
        GameView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Collapsed;
        UpdateNavigation(showBack: false, showSettings: true);
    }

    private void ShowGame(GameConfig game)
    {
        _selectedGame = game;
        HomeView.Visibility = Visibility.Collapsed;
        GameView.Visibility = Visibility.Visible;
        SettingsView.Visibility = Visibility.Collapsed;
        UpdateNavigation(showBack: true, showSettings: true);
        RefreshSelectedGameView();
    }

    private void ShowSettings()
    {
        HomeView.Visibility = Visibility.Collapsed;
        GameView.Visibility = Visibility.Collapsed;
        SettingsView.Visibility = Visibility.Visible;
        UpdateNavigation(showBack: true, showSettings: false);

        Poe1PathTextBox.Text = _settings.Poe1.GameExePath;
        Poe2PathTextBox.Text = _settings.Poe2.GameExePath;
        BackgroundImagePathTextBox.Text = _settings.BackgroundImagePath;
    }

    private void RefreshSelectedGameView()
    {
        if (_selectedGame == null)
        {
            return;
        }

        SelectedGameTitle.Text = _selectedGame.DisplayName;
        SelectedGamePath.Text = string.IsNullOrWhiteSpace(_selectedGame.GameExePath)
            ? "Game path not set yet. Open Settings and choose the game executable, or try Auto Detect."
            : _selectedGame.GameExePath;

        GameModsListView.ItemsSource = _selectedGame.Mods;
        GameUrlsListView.ItemsSource = _selectedGame.Urls;
        LabCompassButton.Visibility = _selectedGame.Key.Equals("poe1", StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private GameConfig GetGameByKey(string key)
    {
        return key.Equals("poe1", StringComparison.OrdinalIgnoreCase)
            ? _settings.Poe1
            : _settings.Poe2;
    }

    private ListView GetSettingsModListViewByKey(string key)
    {
        return key.Equals("poe1", StringComparison.OrdinalIgnoreCase)
            ? Poe1ModsListView
            : Poe2ModsListView;
    }

    private ListView GetSettingsUrlListViewByKey(string key)
    {
        return key.Equals("poe1", StringComparison.OrdinalIgnoreCase)
            ? Poe1UrlsListView
            : Poe2UrlsListView;
    }

    private void ApplyBackgroundImage()
    {
        if (!string.IsNullOrWhiteSpace(_settings.BackgroundImagePath) && File.Exists(_settings.BackgroundImagePath))
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(_settings.BackgroundImagePath, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                RootGrid.Background = new ImageBrush(image)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.17
                };

                return;
            }
            catch
            {
            }
        }

        RootGrid.SetResourceReference(Panel.BackgroundProperty, "BackgroundTextureBrush");
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private static ModEntry CreateModEntryFromFile(string filePath)
    {
        return new ModEntry
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            FilePath = filePath,
            Arguments = string.Empty,
            IsEnabled = true
        };
    }


    private static bool IsSupportedToolFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jar", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ahk", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase);
    }

    private void AddModFilesToGame(GameConfig game, IEnumerable<string> filePaths)
    {
        var addedCount = 0;
        var skippedCount = 0;

        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || !IsSupportedToolFile(filePath))
            {
                skippedCount++;
                continue;
            }

            game.Mods.Add(CreateModEntryFromFile(filePath));
            addedCount++;
        }

        if (addedCount == 0)
        {
            SetStatus("No supported mod files were dropped.");
            return;
        }

        SaveSettings();
        RefreshSelectedGameView();

        if (skippedCount > 0)
        {
            SetStatus($"Added {addedCount} mod(s) to {game.DisplayName}. Skipped {skippedCount} unsupported file(s).");
        }
        else
        {
            SetStatus($"Added {addedCount} mod(s) to {game.DisplayName}.");
        }
    }

    private void ModsListView_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void ModsListView_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListView listView || listView.Tag is not string key)
        {
            return;
        }

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] droppedFiles || droppedFiles.Length == 0)
        {
            return;
        }

        var game = GetGameByKey(key);
        AddModFilesToGame(game, droppedFiles);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        ShowHome();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowSettings();
    }

    private void OpenPoe1Button_Click(object sender, RoutedEventArgs e)
    {
        ShowGame(_settings.Poe1);
    }

    private void OpenPoe2Button_Click(object sender, RoutedEventArgs e)
    {
        ShowGame(_settings.Poe2);
    }

    private void StartGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null)
        {
            SetStatus("Select a game first.");
            return;
        }

        var message = _processService.StartGame(_selectedGame);
        SetStatus(message);
    }

    private void StartLabCompassButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null || !_selectedGame.Key.Equals("poe1", StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Lab Compass is only available for Path of Exile 1.");
            return;
        }

        var message = _processService.StartLabCompass(_selectedGame);
        SetStatus(message);
    }

    private void CloseModsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedGame == null)
        {
            SetStatus("Select a game first.");
            return;
        }

        var message = _processService.CloseMods(_selectedGame);
        SetStatus(message);
    }

    private void BrowseGamePathButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var dialog = new OpenFileDialog
        {
            Title = $"Choose the executable for {game.DisplayName}",
            Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            game.GameExePath = dialog.FileName;
            if (key == "poe1")
            {
                Poe1PathTextBox.Text = dialog.FileName;
            }
            else
            {
                Poe2PathTextBox.Text = dialog.FileName;
            }

            SaveSettings();
            RefreshSelectedGameView();
            SetStatus($"Updated game path for {game.DisplayName}.");
        }
    }

    private void AutoDetectButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var detectedPath = key == "poe1"
            ? _gameDetectionService.TryDetectPoe1()
            : _gameDetectionService.TryDetectPoe2();

        if (string.IsNullOrWhiteSpace(detectedPath))
        {
            SetStatus($"Could not auto-detect {game.DisplayName}. Add the path manually.");
            return;
        }

        game.GameExePath = detectedPath;
        if (key == "poe1")
        {
            Poe1PathTextBox.Text = detectedPath;
        }
        else
        {
            Poe2PathTextBox.Text = detectedPath;
        }

        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Auto-detected {game.DisplayName}.");
    }

    private void AddModButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var dialog = CreateToolDialog(game.DisplayName, "tool or mod");

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AddModFilesToGame(game, new[] { dialog.FileName });
    }

    private void AddLabButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var dialog = CreateToolDialog(game.DisplayName, "Lab Compass tool");

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var entry = CreateModEntryFromFile(dialog.FileName);
        game.LabTools.Add(entry);

        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Added {entry.Name} to Lab Compass for {game.DisplayName}.");
    }

    private static OpenFileDialog CreateToolDialog(string displayName, string category)
    {
        return new OpenFileDialog
        {
            Title = $"Choose a {category} for {displayName}",
            Filter = "Supported files (*.exe;*.jar;*.ahk;*.bat;*.cmd;*.ps1)|*.exe;*.jar;*.ahk;*.bat;*.cmd;*.ps1|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
    }

    private void AddUrlButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var dialog = new UrlEntryDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        game.Urls.Add(new UrlEntry
        {
            Name = dialog.EntryName,
            Url = dialog.EntryUrl,
            IsEnabled = true
        });

        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Added URL to {game.DisplayName}.");
    }

    private void RemoveModButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var listView = GetSettingsModListViewByKey(key);

        if (listView.SelectedItem is not ModEntry selectedMod)
        {
            SetStatus("Select a mod from the list first.");
            return;
        }

        game.Mods.Remove(selectedMod);
        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Removed {selectedMod.Name} from {game.DisplayName} mods.");
    }

    private void RemoveLabButton_Click(object sender, RoutedEventArgs e)
    {
        if (Poe1LabListView.SelectedItem is not ModEntry selectedMod)
        {
            SetStatus("Select a Lab Compass entry from the list first.");
            return;
        }

        _settings.Poe1.LabTools.Remove(selectedMod);
        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Removed {selectedMod.Name} from Lab Compass.");
    }

    private void RemoveUrlButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string key)
        {
            return;
        }

        var game = GetGameByKey(key);
        var listView = GetSettingsUrlListViewByKey(key);

        if (listView.SelectedItem is not UrlEntry selectedUrl)
        {
            SetStatus("Select a URL from the list first.");
            return;
        }

        game.Urls.Remove(selectedUrl);
        SaveSettings();
        RefreshSelectedGameView();
        SetStatus($"Removed URL from {game.DisplayName}.");
    }

    private void BrowseBackgroundImageButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose a background image",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.webp;*.bmp)|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All files (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _settings.BackgroundImagePath = dialog.FileName;
        BackgroundImagePathTextBox.Text = dialog.FileName;
        ApplyBackgroundImage();
        SaveSettings();
        SetStatus("Updated background image.");
    }

    private void ResetBackgroundImageButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.BackgroundImagePath = string.Empty;
        BackgroundImagePathTextBox.Text = string.Empty;
        ApplyBackgroundImage();
        SaveSettings();
        SetStatus("Using the default background image.");
    }

    private void ItemToggle_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        RefreshSelectedGameView();
    }

    private void Poe1PathTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _settings.Poe1.GameExePath = Poe1PathTextBox.Text.Trim();
        SaveSettings();
        RefreshSelectedGameView();
    }

    private void Poe2PathTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        _settings.Poe2.GameExePath = Poe2PathTextBox.Text.Trim();
        SaveSettings();
        RefreshSelectedGameView();
    }
}
