using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static class CertificateHandler
{
    private const string ServerAddress = "https://94.228.164.4:7199";

    public static async Task<RestClient> CreateCustomHttpClientHandler()
    {
        var handler = await CreateHttpClientHandler()!;
        var httpClient = new HttpClient(handler);
        var options = new RestClientOptions
        {
            BaseUrl = new Uri(ServerAddress),
            ConfigureMessageHandler = _ => handler
        };

        return new RestClient(httpClient, options);
    }

    private static async Task<HttpClientHandler> CreateHttpClientHandler()
    {
        var handler = new HttpClientHandler();

        var certificateContent = await ReadTextFile("server.crt");

        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            var trustedCertificate = new X509Certificate2(Encoding.ASCII.GetBytes(certificateContent));

            return cert != null && cert.GetCertHashString() == trustedCertificate.GetCertHashString();
        };

        return handler;
    }

    private static async Task<string> ReadTextFile(string filePath)
    {
        using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
        using StreamReader reader = new StreamReader(fileStream);

        return await reader.ReadToEndAsync();
    }
}