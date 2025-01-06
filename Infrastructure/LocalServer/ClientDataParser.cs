using System.Diagnostics;
using System.Text.RegularExpressions;
using Domain;

namespace Infrastructure.LocalServer;

public class ClientDataParser
{
    private Dictionary<string, Action<object, EventArgs>> handlers;
    private string ActionParseFormat = @"\[(?<action>.+?)\](?<function>.*)";
    private string FuncArgsParseFormat = @"^(?<name>\w+)\((?<args>.*)\)$";
    private string ArgsParseFormat = @"(?<key>\w+)\s*:\s*(?<value>[^:,]+)(?:,|$)";

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

        var match = Regex.Match(function, FuncArgsParseFormat);

        if (!match.Success) return;
        var (fName, args) = ParseFunctionCall(function);

        switch (fName)
        {
            case "OpenUserProfile":
            case "ShowMap":
                var eventArg = new ProfileEventArgs(fName, args);
                handlers[fName]?.Invoke("SERVER", eventArg);
                break;
            case "NULL":
                Debug.WriteLine("Error Parsing Name and Arguments");
                return;
            default:
                if (!handlers.Keys.Contains(fName))
                    return;
                var otherEventArgs = new OtherEventArgs(fName, args);
                handlers[fName]?.Invoke("SERVER", otherEventArgs);
                break;
        }
    }

    private (string functionName, Dictionary<string, object> args) ParseFunctionCall(string input)
    {
        (string functionName, Dictionary<string, object> args) errorParsing = ("NULL", null);

        if (string.IsNullOrWhiteSpace(input))
            return errorParsing;

        var functionMatch = Regex.Match(input.Trim(), FuncArgsParseFormat);
        if (!functionMatch.Success)
            return errorParsing;

        string functionName = functionMatch.Groups["name"].Value;
        string arguments = functionMatch.Groups["args"].Value.Trim();

        var args = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            var argMatches = Regex.Matches(arguments, ArgsParseFormat);
            if (argMatches.Count == 0)
                return errorParsing;

            string remainingText = arguments;
            foreach (Match argMatch in argMatches)
            {
                remainingText = remainingText.Replace(argMatch.Value, "").Trim();
            }

            if (!string.IsNullOrWhiteSpace(remainingText))
                return errorParsing;

            foreach (Match argMatch in argMatches)
            {
                string argKey = argMatch.Groups["key"].Value;
                string argValue = argMatch.Groups["value"].Value.Trim();
                args[argKey] = argValue;
            }
        }

        return (functionName, args);
    }
}