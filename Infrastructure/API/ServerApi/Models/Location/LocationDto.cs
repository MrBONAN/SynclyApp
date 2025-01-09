namespace Infrastructure.API.ServerApi.Models.Location;

public class LocationDto
{
    public int UserId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.Now;
}