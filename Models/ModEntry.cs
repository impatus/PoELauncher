using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PoELauncher.Models;

public class ModEntry : INotifyPropertyChanged
{
    private bool _isEnabled;
    private string _name = string.Empty;
    private string _filePath = string.Empty;
    private string _arguments = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
        }
    }

    public string Arguments
    {
        get => _arguments;
        set
        {
            _arguments = value;
            OnPropertyChanged();
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool CloseWithLauncher { get; set; } = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
