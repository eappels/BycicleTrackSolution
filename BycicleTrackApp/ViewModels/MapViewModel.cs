using BycicleTrackApp.Messages;
using BycicleTrackApp.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls.Maps;

namespace BycicleTrackApp.ViewModels;

public partial class MapViewModel : ObservableObject, IDisposable
{

    private readonly ILocationService locationService;

    public MapViewModel(ILocationService locationService)
    {
        Track = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };
        this.locationService = locationService;
        this.locationService.OnLocationUpdate += OnLocationUpdate;
    }

    private void OnLocationUpdate(Location location)
    {
        if (Track != null)
            Track.Geopath.Add(location);
        WeakReferenceMessenger.Default.Send(new LocationUpdatedMessage(location));
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
            locationService.StartTracking();
            StartStopButtonText = "Stop";
            StartStopButtonColor = Colors.Red;
        }
        else
        {
            locationService.StopTracking();
            StartStopButtonText = "Start";
            StartStopButtonColor = Colors.Green;
        }
    }

    [RelayCommand]
    private void ToggleHistory()
    {
        IsHistoryVisible = !IsHistoryVisible;
    }

    [RelayCommand]
    private void CloseHistory()
    {
        IsHistoryVisible = false;
    }

    [ObservableProperty]
    public Polyline track = new();

    [ObservableProperty]
    public string startStopButtonText = "Start";

    [ObservableProperty]
    public Color startStopButtonColor = Colors.Green;

    [ObservableProperty]
    public bool isHistoryVisible;

}