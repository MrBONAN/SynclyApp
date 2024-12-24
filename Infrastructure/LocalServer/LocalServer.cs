using System.Net;
using System.Text.RegularExpressions;
using Domain;
using Infrastructure.LocalServer;

namespace Infrastructure;

public class SimpleServer
{
    private HttpListener _listener;
    private bool _isRunning;
    private PortChecker _portChecker;
    private ClientDataParser _clientDataParser;

    public SimpleServer(PortChecker portChecker)
    {
        _portChecker = portChecker;
        _listener = new HttpListener();
        var port = _portChecker.GetFreePort();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _clientDataParser = new ClientDataParser();
    }

    public void AddHandler(string name, Action<object, EventArgs> func)
    {
        _clientDataParser.AddHandler(name, func);
    }

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

    private void ConfigureCORS(HttpListenerContext context)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
    }


    private async void HandleClientResponse(HttpListenerContext context)
    {
        var response = context.Response;
        string responseString = "<html><body>Message received</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }

    private async void ProcessRequest(HttpListenerContext context)
    {
        ConfigureCORS(context);

        switch (context.Request.HttpMethod)
        {
            case "OPTIONS":
                context.Response.StatusCode = 200;
                context.Response.Close();
                return;
            case "POST":
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                _clientDataParser.HandleClientData(await reader.ReadToEndAsync());
                break;
            }
        }

        HandleClientResponse(context);
    }
}