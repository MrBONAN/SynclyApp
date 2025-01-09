using App;
using App.UserAuthorization;
using The49.Maui.BottomSheet;

namespace Exit;

public partial class Sheet : BottomSheet
{
    private readonly IUserDataHandler
        UserDataHandler = App.App.Services.GetRequiredService<IUserDataHandler>();

    public Sheet()
    {
        InitializeComponent();
        InitializeButtonsAction();
    }

    private void InitializeButtonsAction()
    {
        LeaveButton.Clicked += LeaveApp;
        StayButton.Clicked += CloseSheet;
    }

    private async void CloseSheet(object? sender, EventArgs e) =>
        await BottomSheet.DismissAsync();

    private async void LeaveApp(object? sender, EventArgs e)
    {
        UserDataHandler.RemoveUserData();
        await BottomSheet.DismissAsync();
        Application.Current.MainPage = App.App.Services.GetRequiredService<SignIn>();
    }
}