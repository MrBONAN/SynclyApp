using App.UserAuthorization;
using App.UserAuthorization.SpotifyAuthorization;
using Infrastructure.API.ServerApi;

namespace App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }
    public static ISpotifyAuthManager? SpotifyAuthManager { get; private set; }
    public static IUserDataHandler? UserDataHandler { get; private set; }

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Services = serviceProvider;
        SpotifyAuthManager = Services.GetService<ISpotifyAuthManager>();
        UserDataHandler = Services.GetService<IUserDataHandler>();

        var userDto = Task.Run(() => UserDataHandler?.GetUserDataAsync()).Result;
        if (userDto == null)
            MainPage = Services.GetRequiredService<SignIn>();
        else
            MainPage = Services.GetRequiredService<Map>();
    }

    protected override async void OnStart()
    {
        var restClient = await CertificateHandler.CreateCustomHttpClientHandler();
        ServerApi.ServerClient = restClient;
    }
}