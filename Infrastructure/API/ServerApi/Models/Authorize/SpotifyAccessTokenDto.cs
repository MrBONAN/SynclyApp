namespace Infrastructure.API.ServerApi.Models.Authorize;

public class SpotifyAccessTokenDto
{
    public string AccessToken { get; set; } = null!;
    public string TokenType { get; set; } = null!;
    public DateTime ExpiresDate { get; set; }
    public string RefreshToken { get; set; } = null!;
}