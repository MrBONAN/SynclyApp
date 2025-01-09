using System.ComponentModel;
using System.Runtime.CompilerServices;
using Domain;
using The49.Maui.BottomSheet;

namespace Chat;

public partial class Sheet : BottomSheet
{
    public Sheet(int id)
    {
        InitializeComponent();
        BindingContext = new ViewModel(id);
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

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        var editor = (Editor)sender;

        if (!string.IsNullOrWhiteSpace(editor.Text))
        {
            double lineHeight = editor.FontSize * 1.2;
            double textHeight = lineHeight * Math.Max(editor.Text.Split('\n').Length, 1);

            editor.HeightRequest = Math.Min(textHeight + 10, 200);
        }
        else
        {
            editor.HeightRequest = 40;
        }
    }
}

public class ViewModel : INotifyPropertyChanged
{
    public User СhatPartner { get; set; }

    public ViewModel(int id)
    {
        СhatPartner = new User(id);
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

    public async Task LoadDataAsync()
    {
        Console.WriteLine("Loading data");
    }
}