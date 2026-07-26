using CoreLocation;

namespace BycicleTrackApp.Services;

public partial class LocationService
{
    public readonly CLLocationManager locationManager;
    private bool isSubscribedToLocationUpdates;

    public LocationService()
    {
        OnLocationUpdate = _ => { };
        locationManager = new CLLocationManager();
        locationManager.PausesLocationUpdatesAutomatically = false;
        locationManager.DesiredAccuracy = CLLocation.AccuracyBestForNavigation;
        locationManager.AllowsBackgroundLocationUpdates = true;
        locationManager.ActivityType = CLActivityType.AutomotiveNavigation;
    }

    partial void StartTrackingInternal()
    {
        if (!CLLocationManager.LocationServicesEnabled)
            return;

        if (!isSubscribedToLocationUpdates)
        {
            locationManager.LocationsUpdated += OnLocationsUpdated;
            isSubscribedToLocationUpdates = true;
        }

        locationManager.RequestAlwaysAuthorization();
        locationManager.StartUpdatingLocation();
    }

    partial void StopTrackingInternal()
    {
        locationManager.StopUpdatingLocation();
    }

    private void OnLocationsUpdated(object? sender, CLLocationsUpdatedEventArgs e)
    {
        var lastLocation = e.Locations?.LastOrDefault();
        if (lastLocation != null)
        {
            var newlocation = new Location(lastLocation.Coordinate.Latitude, lastLocation.Coordinate.Longitude);
            OnLocationUpdate(newlocation);
        }
    }
}
