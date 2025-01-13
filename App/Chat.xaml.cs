using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using App.UserAuthorization;
using Domain;
using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.User;
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
        SendMessageButton.Clicked += SendMessage;
        if (BindingContext is not ViewModel viewModel) return;
        var loadingTasks = new List<Task>
        {
            viewModel.LoadDataAsync(),
            viewModel.LoadMessagesAsync()
        };
        await Task.WhenAny(loadingTasks);
    }

    private void SendMessage(object? sender, EventArgs e)
    {
        if (BindingContext is not ViewModel viewModel) return;
        viewModel.SendMessage(MessageEditor.Text, DateTime.UtcNow);
        MessageEditor.Text = string.Empty;
    }
}

public class ViewModel : INotifyPropertyChanged
{
    private User _chatPartner;
    private string _partnerProfileImage = "profile_icon.png";
    private string _partnerUsername = "Загрузка...";
    public ObservableCollection<Message> Messages { get; set; } = new ();
    public User ChatPartner
    {
        get => _chatPartner;
        set
        {
            if (_chatPartner == value || value == null) return;
            _chatPartner = value;
            OnPropertyChanged();
        }
    }

    public ViewModel(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
    
    public string PartnerProfileImage {
        get => _partnerProfileImage;
        set
        {
            if (_partnerProfileImage == value) return;
            _partnerProfileImage = value;
            OnPropertyChanged();
        }
    }
    
    public string PartnerUsername {
        get => _partnerUsername;
        set
        {
            if (_partnerUsername == value || value == null) return;
            _partnerUsername = value;
            OnPropertyChanged();
        }
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
        CurrentUser = await App.App.Services.GetRequiredService<IUserDataHandler>().GetUserDataAsync();
        var resultData = (await ServerApi.GetUserAsync(Id)).Data;
        if (resultData != null)
        {
            ChatPartner = new User();
            await ChatPartner.Initialize(resultData);
        }

        if (ChatPartner.ProfileImageURL != null)
            PartnerProfileImage = ChatPartner.ProfileImageURL;
        
        if (ChatPartner.Name != null)
            PartnerUsername = ChatPartner.Name;
    }

    public UserDto? CurrentUser { get; set; }

    public async Task LoadMessagesAsync()
    {
        
    }

    public void SendMessage(string messageText, DateTime date)
    {
        if (CurrentUser != null)
            Messages.Add(new Message(CurrentUser.Id, ChatPartner.Id, messageText, date, LayoutOptions.End));
    }
}