using BycicleTrackApp.Messages;
using BycicleTrackApp.Services.Interfaces;
using BycicleTrackApp.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Maps;

namespace BycicleTrackApp.ViewModels;

public partial class MapViewModel : ObservableObject, IDisposable
{

    private readonly ILocationService locationService;
    private readonly IRepository<LocationOnMap> repository;
    private string? currentRideId;

    public MapViewModel(ILocationService locationService, IRepository<LocationOnMap> repository)
    {
        Track = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };
        this.locationService = locationService;
        this.repository = repository;
        this.locationService.OnLocationUpdate += OnLocationUpdate;
    }

    private void OnLocationUpdate(Location location)
    {
        if (Track != null)
            Track.Geopath.Add(location);
        WeakReferenceMessenger.Default.Send(new LocationUpdatedMessage(location));

        if (!string.IsNullOrWhiteSpace(currentRideId))
        {
            _ = repository.AddAsync(new LocationOnMap(location.Latitude, location.Longitude, currentRideId, DateTime.UtcNow.Ticks));
        }
    }

    public void Dispose()
    {
        if (locationService != null)
        {
            locationService.OnLocationUpdate -= OnLocationUpdate;
            locationService.StopTracking();
        }
    }

    [RelayCommand]
    private async Task StartStop()
    {
        if (StartStopButtonText == "Start")
        {
            if (Track?.Geopath?.Count > 0)
            {
                Track.Geopath.Clear();
            }

            currentRideId = Guid.NewGuid().ToString("N");
            locationService.StartTracking();
            StartStopButtonText = "Stop";
            StartStopButtonColor = Colors.Red;
        }
        else
        {
            locationService.StopTracking();
            currentRideId = null;
            StartStopButtonText = "Start";
            StartStopButtonColor = Colors.Green;
        }
    }

    [ObservableProperty]
    public Polyline track = new();

    [ObservableProperty]
    public string startStopButtonText = "Start";

    [ObservableProperty]
    public Color startStopButtonColor = Colors.Green;

}