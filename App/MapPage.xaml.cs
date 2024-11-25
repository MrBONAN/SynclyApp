using Infrastructure;

namespace App;

public partial class MapPage : ContentPage
{
    int count = 0;

    public MapPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        var assembly1 = typeof(MapPage).Assembly;
        string[] resourceNames = assembly1.GetManifestResourceNames();

        foreach (var resourceName in resourceNames)
        {
            Console.WriteLine(resourceName);
        }

        var assembly = typeof(MapPage).Assembly;
        string resourcePath = "App.Resources.Raw.Map.html";

        using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
        using (StreamReader reader = new StreamReader(stream))
        {
            string htmlContent = reader.ReadToEnd();
            LeafletWebView.Source = new HtmlWebViewSource
            {
                Html = htmlContent
            };
        }
    }
}