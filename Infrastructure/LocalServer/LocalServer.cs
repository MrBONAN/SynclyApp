using System.Net;
using System.Text.RegularExpressions;

namespace Infrastructure;

public class SimpleServer
{
    private HttpListener _listener;
    private bool _isRunning;
    private PortChecker _portChecker;
    private Dictionary<string, Action<object, EventArgs>> handlers;

    public SimpleServer(PortChecker portChecker)
    {
        _portChecker = portChecker;
        handlers = new Dictionary<string, Action<object, EventArgs>>();
        _listener = new HttpListener();
        var port = _portChecker.GetFreePort();
        _listener.Prefixes.Add($"http://localhost:6005/");
        Console.WriteLine($"Server started on port: {port}");
    }

    public void AddHandler(string name, Action<object, EventArgs> func) => handlers[name] = func;

    public void Start()
    {
        if (!_isRunning)
        {
            _isRunning = true;
            _listener.Start();
            Listen();
        }
    }

    public void Stop()
    {
        if (_isRunning)
        {
            _listener.Stop();
            _isRunning = false;
        }
    }

    private async void Listen()
    {
        while (_isRunning)
        {
            HttpListenerContext context = await _listener.GetContextAsync();
            ProcessRequest(context);
        }
    }

    private async void ProcessRequest(HttpListenerContext context)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");

        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            context.Response.Close();
            return;
        }

        if (context.Request.HttpMethod == "POST")
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            handle_data(await reader.ReadToEndAsync());
        }

        var response = context.Response;
        string responseString = "<html><body>Message received</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    private void handle_data(string data)
    {
        var matches = Regex.Matches(data, @"\[(?<action>.+?)\](?<function>[^|]*)");

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
                    Console.WriteLine($"Unknown action: {action}");
                    break;
            }
        }
    }

    private void handle_RUN(string function)
    {
        if (string.IsNullOrWhiteSpace(function))
            return;

        var match = Regex.Match(function, @"^([a-zA-Z_][a-zA-Z0-9_]*)\s*\((.*)\)$");

        if (match.Success)
        {
            string functionName = match.Groups[1].Value;
            var argument = Convert.ToInt16(match.Groups[2].Value);
            handlers[functionName](argument, EventArgs.Empty);
        }
    }
}