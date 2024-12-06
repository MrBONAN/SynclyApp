using Infrastructure;

namespace App;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Fuck it {count} time";
        else
            CounterBtn.Text = $"Fuck it {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
        
    }
    
    private async void OnMapOpened(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Map());
        //await Navigation.PushAsync(new MapPage());
    }
}