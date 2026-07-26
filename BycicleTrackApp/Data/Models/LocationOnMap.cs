using SQLite;

namespace BycicleTrackApp.Data.Models;

public class LocationOnMap
{

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string RideId { get; set; } = string.Empty;
    public long RecordedAtUtcTicks { get; set; }

    public LocationOnMap()
    {        
    }

    public LocationOnMap(double latitude, double longitude, string rideId, long recordedAtUtcTicks)
    {
        Latitude = latitude;
        Longitude = longitude;
        RideId = rideId;
        RecordedAtUtcTicks = recordedAtUtcTicks;
    }
}