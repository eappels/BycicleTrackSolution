namespace BycicleTrackApp.Services.Interfaces;

public interface ILocationService
{
    Action<Location>? OnLocationUpdate { get; set; }
    void StartTracking();
    void StopTracking();
}