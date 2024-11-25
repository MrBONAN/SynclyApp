using Infrastructure;

namespace App;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnMapOpened(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapPage());
    }
}