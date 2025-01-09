using Infrastructure.API.ServerApi.Models.Links;

namespace Infrastructure.API.ServerApi.Models.Track;

public class TrackDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    //public List<ArtistDto>? Artists { get; set; } = new();
    public LinksDto Links { get; set; } = null!;
}