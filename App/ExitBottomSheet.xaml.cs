using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App;
using App.UserAuthorization.SpotifyAuthorization;
using CommunityToolkit.Maui.Views;
using The49.Maui.BottomSheet;

namespace Exit;

public partial class Sheet : BottomSheet
{
    private readonly ISpotifyAuthManager
        SpotifyAuthManager = App.App.Services.GetRequiredService<ISpotifyAuthManager>();

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
        SpotifyAuthManager.LogOut();
        await BottomSheet.DismissAsync();
        Application.Current.MainPage = App.App.Services.GetRequiredService<SignIn>();
    }
}