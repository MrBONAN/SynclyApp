using Infrastructure.API.ServerApi.Models.Links;

namespace Infrastructure.API.ServerApi.Models.User;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CurrentTrackId { get; set; }
    public LinksDto Links { get; set; } = null!;
    public DateTime RecentlyTracksUpdateDate { get; set; } = DateTime.MinValue;
    public DateTime TopTracksUpdateDate { get; set; } = DateTime.MinValue;
    public DateTime TopArtistsUpdateDate { get; set; } = DateTime.MinValue;
}