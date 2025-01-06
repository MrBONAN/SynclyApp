using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using App;
using The49.Maui.BottomSheet;

namespace AppearanceBottomSheet;

public partial class Sheet : BottomSheet
{
    public Sheet()
    {
        InitializeComponent();
        BindingContext = new ViewModel();
        InitializeData();
    }

    private async void InitializeData()
    {
        if (BindingContext is not ViewModel viewModel) return;
        var loadingTasks = new List<Task>
        {
            viewModel.LoadDataAsync()
        };

        await Task.WhenAny(loadingTasks);
    }
}

public class ViewModel : INotifyPropertyChanged
{
    public ViewModel()
    {
        Appearance.OnMapSelected += UpdateSelection;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<Appearance> AppearanceList { get; set; }

    public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task LoadDataAsync()
    {
        AppearanceList =
        [
            new Appearance("Обычная", "default_preview.png", "default"),
            new Appearance("Спутник", "satelite_preview.png", "satellite")
        ];
        OnPropertyChanged(nameof(AppearanceList));
    }

    private void UpdateSelection(Appearance selectedAppearance)
    {
        foreach (var appearance in AppearanceList)
        {
            appearance.IsNotSelected = appearance != selectedAppearance;
            appearance.BackgroundColor = appearance == selectedAppearance
                ? Colors.Gray
                : Colors.Transparent;
        }
    }
}

public class Appearance : INotifyPropertyChanged
{
    public Appearance(string title, string previewPath, string code)
    {
        _isNotSelected = Preferences.Get("MapStyle", "default") == code;
        _backgroundColor = Preferences.Get("MapStyle", "default") == code ? Colors.Gray : Colors.Transparent;
        Title = title;
        PreviewPath = previewPath;
        Code = code;
        ChangeMapCommand = new Command(ChangeMap);
    }

    public static event Action<Appearance>? OnMapSelected;

    public bool IsNotSelected
    {
        get => _isNotSelected;
        set
        {
            if (_isNotSelected == value) return;
            _isNotSelected = value;
            OnPropertyChanged(nameof(IsNotSelected));
        }
    }

    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (Equals(_backgroundColor, value)) return;
            _backgroundColor = value;
            OnPropertyChanged(nameof(BackgroundColor));
        }
    }

    private MapCommands mapControl = App.App.Services.GetRequiredService<MapCommands>();
    private bool _isNotSelected;
    private Color _backgroundColor;
    public string Title { get; set; }
    public string PreviewPath { get; set; }
    public string Code { get; set; }
    public ICommand ChangeMapCommand { get; set; }

    private void ChangeMap()
    {
        Preferences.Set("MapStyle", Code);
        mapControl.LoadMap();
        OnMapSelected?.Invoke(this);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}