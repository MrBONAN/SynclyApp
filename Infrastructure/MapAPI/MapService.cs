using System.Text.RegularExpressions;

namespace App.Infrastructure;

public class MapService
{
    public static string FormatJsCodeWithInvariantCulture(string jsCode)
    {
        return Regex.Replace(jsCode, @"\b\d+,\d+\b", match =>
        {
            return match.Value.Replace(",", ".");
        });
    }
}