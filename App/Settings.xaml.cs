using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using The49.Maui.BottomSheet;

namespace App;

public partial class SettingsBottomSheet : BottomSheet
{
    public SettingsBottomSheet()
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel();
        InitializeData();
    }

    private async void InitializeData()
    {
        if (BindingContext is not SettingsViewModel viewModel) return;
        var loadingTasks = new List<Task>
        {
            viewModel.LoadDataAsync()
        };

        await Task.WhenAny(loadingTasks);
    }
}

public class SettingsViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Setting> SettingsList { get; set; }
    public string Footer { get; private set; } = $"С <3 от Лалки\nВерсия приложения: v{AppInfo.BuildString}";
    public ICommand SettingTappedCommand { get; set; }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task LoadDataAsync()
    {
        SettingsList =
        [
            new Setting("Мой профиль", "profile_icon.png", new ProfileBottomSheet.Sheet()),
            new Setting("Подключения", "connections_icon.png", new ConnectionsBottomSheet.Sheet()),
            new Setting("Приватность", "privacy_icon.png", new ConnectionsBottomSheet.Sheet()),
            new Setting("Вид карты", "appearance_icon.png", new ConnectionsBottomSheet.Sheet()),
            new Setting("Приватность", "privacy_icon.png", new ConnectionsBottomSheet.Sheet())
        ];
        OnPropertyChanged(nameof(SettingsList));
        //SettingTappedCommand = new Command<ContentView>(async (item) => await OnSettingTapped(item));
    }

    private async void OnTapped(object sender, TappedEventArgs e)
    {
        var parameter = e.Parameter; // Здесь находится переданный аргумент
        if (parameter is not BottomSheet sheet)
            return;
        await sheet.ShowAsync();
    }
}

public class Setting
{
    public Setting(string name, string icon, ContentView contentView)
    {
        Icon = icon;
        Name = name;
        Page = contentView;
    }

    public string Icon { get; set; }
    public string Name { get; set; }
    public ContentView Page { get; set; }
}