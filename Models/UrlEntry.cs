using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PoELauncher.Models;

public class UrlEntry : INotifyPropertyChanged
{
    private bool _isEnabled;
    private string _name = string.Empty;
    private string _url = string.Empty;

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

    public string Url
    {
        get => _url;
        set
        {
            _url = value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
