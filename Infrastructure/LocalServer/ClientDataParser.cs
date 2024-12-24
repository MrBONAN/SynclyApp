using System.Diagnostics;
using System.Text.RegularExpressions;
using Domain;

namespace Infrastructure.LocalServer;

public class ClientDataParser
{
    private Dictionary<string, Action<object, EventArgs>> handlers;
    private string ActionParseFormat = @"\[(?<action>.+?)\](?<function>[^|]*)";
    private string FuncNameParseFormat = @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*?)(,\s*{.*})?\)$";
    private string FuncArgsParseFormat = @"^(\w+)\(([^)]*)\)$";
    private string ArgsParseFormat = @"(\w+):\s*(\{?.+?\}?)";

    public ClientDataParser()
    {
        handlers = new Dictionary<string, Action<object, EventArgs>>();
    }

    public void AddHandler(string name, Action<object, EventArgs> func) => handlers.Add(name, func);

    public void HandleClientData(string data)
    {
        var matches = Regex.Matches(data, ActionParseFormat);

        foreach (Match match in matches)
        {
            string action = match.Groups["action"].Value;
            string function = match.Groups["function"].Value;

            switch (action)
            {
                case "RUN":
                    handle_RUN(function);
                    break;
                default:
                    Debug.WriteLine($"Unknown action: {action}");
                    break;
            }
        }
    }

    private void handle_RUN(string function)
    {
        if (string.IsNullOrWhiteSpace(function))
            return;

        var match = Regex.Match(function, FuncNameParseFormat);

        if (match.Success)
        {
            var parsedFunc = ParseFunctionCall(function);
            var fName = parsedFunc.Item1;
            var args = parsedFunc.Item2;

            switch (fName)
            {
                case "OpenUserProfile":
                    var eventArg = new ProfileEventArgs(fName, args);
                    handlers[fName]?.Invoke("SERVER", eventArg);
                    break;
                case "NULL":
                    Debug.WriteLine("Error Parsing Name and Arguments");
                    return;
                default:
                    Debug.WriteLine($"Unknown function: {function}");
                    return;
            }
        }
    }

    private (string functionName, Dictionary<string, object> args) ParseFunctionCall(string input)
    {
        (string functionName, Dictionary<string, object> args) errorParsing = ("NULL", null);

        if (string.IsNullOrWhiteSpace(input))
            return errorParsing;

        var functionMatch = Regex.Match(input, FuncArgsParseFormat);
        if (!functionMatch.Success)
            return errorParsing;

        string functionName = functionMatch.Groups[1].Value;
        string arguments = functionMatch.Groups[2].Value;

        var args = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var argMatches = Regex.Matches(arguments, ArgsParseFormat);
            foreach (Match argMatch in argMatches)
            {
                string argKey = argMatch.Groups[1].Value;
                string argValue = argMatch.Groups[2].Value;
                args[argKey] = argValue;
            }
        }

        return (functionName, args);
    }
}