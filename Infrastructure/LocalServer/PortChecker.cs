namespace Infrastructure;

using System;
using System.Net.Sockets;

public class PortChecker
{
    public int PORT = -1;

    public int GetFreePort()
    {
        if (PORT != -1)
            return PORT;
        for (var port = 6000; port < 13000; port += 1)
            if (IsPortAvailable(port))
            {
                PORT = port;
                return port;
            }

        throw new Exception("No ports available");
    }

    private bool IsPortAvailable(int port)
    {
        bool isAvailable = true;

        TcpListener tcpListener = null;

        try
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Any, port);
            tcpListener.Start();
            PORT = port;
        }
        catch (SocketException)
        {
            isAvailable = false;
        }
        finally
        {
            tcpListener?.Stop();
        }

        return isAvailable;
    }
}