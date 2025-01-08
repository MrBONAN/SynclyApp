using Infrastructure.API.ServerApi;
using Infrastructure.API.SpotifyAPI;

namespace App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}