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
    public string Footer => $"С <3 от Лалки\nВерсия приложения: v{AppInfo.BuildString}";

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task LoadDataAsync()
    {
        SettingsList =
        [
            //new Setting("Подключения", "connections_icon.png", new ConnectionsBottomSheet.Sheet()),
            //new Setting("Приватность", "privacy_icon.png", new ConnectionsBottomSheet.Sheet()),
            new Setting("Вид карты", "appearance_icon.png", new AppearanceBottomSheet.Sheet()),
            new Setting("Выход из аккаунта", "exit_icon.png", new Exit.Sheet())
        ];
        OnPropertyChanged(nameof(SettingsList));
    }
}

public class Setting
{
    public Setting(string name, string icon, BottomSheet contentView)
    {
        Icon = icon;
        Name = name;
        Page = contentView;
        OpenPageCommand = new Command(OpenPage);
    }

    private async void OpenPage()
    {
        await Page.ShowAsync();
    }

    public ICommand OpenPageCommand { get; set; }

    public string Icon { get; set; }
    public string Name { get; set; }
    public BottomSheet Page { get; set; }
}