namespace Infrastructure.API.ServerApi.Models.Location;

public class LocationDto
{
    public int UserId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    
    public double Haversine(double otherLatitude, double otherLongitude)
    {
        const double R = 6371.0;

        var lat1 = DegreesToRadians((double)Latitude);
        var lon1 = DegreesToRadians((double)Longitude);
        var lat2 = DegreesToRadians(otherLatitude);
        var lon2 = DegreesToRadians(otherLongitude);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;
        
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}