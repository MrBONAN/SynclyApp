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
    
    protected override async void OnStart()
    {
        var restClient = await CertificateHandler.CreateCustomHttpClientHandler();
        ServerApi.ServerClient = restClient;
    }
}