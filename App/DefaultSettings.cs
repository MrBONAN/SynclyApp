namespace App;

public class DefaultSettings
{
    private string _mapSource = "Map.html";

    public string GetMapHtml()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(_mapSource).Result;
        using var reader = new StreamReader(stream);

        var htmlContent = reader.ReadToEndAsync().Result;
        return htmlContent;
    }
}