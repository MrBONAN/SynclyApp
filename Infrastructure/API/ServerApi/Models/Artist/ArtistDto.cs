using Infrastructure.API.ServerApi.Models.Links;

namespace Infrastructure.API.ServerApi.Models;

public class ArtistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();
    public LinksDto Links { get; set; } = null!;
}