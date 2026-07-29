using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BycicleTrackApp.Data.Models;
using BycicleTrackApp.Services.Interfaces;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace BycicleTrackApp.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IRepository<LocationOnMap> repository;
    private List<List<LocationOnMap>> rides = [];
    private int selectedRideIndex = -1;
    private string currentRideTitle = "No rides yet";
    private string currentRideDuration = "--";
    private string currentRideDistance = "--";
    private bool canDeleteCurrentRide;
    private bool canShowOlderRide;
    private bool canShowNewerRide;

    public ObservableCollection<LocationOnMap> HistoryLocations { get; } = [];

    public string CurrentRideTitle
    {
        get => currentRideTitle;
        private set => SetProperty(ref currentRideTitle, value);
    }

    public bool CanShowOlderRide
    {
        get => canShowOlderRide;
        private set => SetProperty(ref canShowOlderRide, value);
    }

    public bool CanShowNewerRide
    {
        get => canShowNewerRide;
        private set => SetProperty(ref canShowNewerRide, value);
    }

    public string CurrentRideDuration
    {
        get => currentRideDuration;
        private set => SetProperty(ref currentRideDuration, value);
    }

    public string CurrentRideDistance
    {
        get => currentRideDistance;
        private set => SetProperty(ref currentRideDistance, value);
    }

    public bool CanDeleteCurrentRide
    {
        get => canDeleteCurrentRide;
        private set => SetProperty(ref canDeleteCurrentRide, value);
    }

    public HistoryViewModel(IRepository<LocationOnMap> repository)
    {
        this.repository = repository;
    }

    public async Task LoadHistoryAsync()
    {
        var locations = await repository.GetAllAsync();

        rides = locations
            .OrderBy(GetSortValue)
            .GroupBy(location => string.IsNullOrWhiteSpace(location.RideId) ? "legacy" : location.RideId)
            .Select(group => group.OrderBy(GetSortValue).ToList())
            .OrderByDescending(group => group.Max(GetSortValue))
            .ToList();

        selectedRideIndex = rides.Count > 0 ? 0 : -1;
        ShowSelectedRide();
    }

    [RelayCommand]
    private void ShowOlderRide()
    {
        if (selectedRideIndex >= 0 && selectedRideIndex < rides.Count - 1)
        {
            selectedRideIndex++;
            ShowSelectedRide();
        }
    }

    [RelayCommand]
    private void ShowNewerRide()
    {
        if (selectedRideIndex > 0)
        {
            selectedRideIndex--;
            ShowSelectedRide();
        }
    }

    public async Task DeleteCurrentRideAsync()
    {
        if (selectedRideIndex < 0 || selectedRideIndex >= rides.Count)
            return;

        var rideToDelete = rides[selectedRideIndex].ToList();

        foreach (var location in rideToDelete)
        {
            await repository.DeleteAsync(location);
        }

        rides.RemoveAt(selectedRideIndex);

        if (selectedRideIndex >= rides.Count)
        {
            selectedRideIndex = rides.Count - 1;
        }

        ShowSelectedRide();
    }

    private void ShowSelectedRide()
    {
        HistoryLocations.Clear();

        if (selectedRideIndex < 0 || selectedRideIndex >= rides.Count)
        {
            CurrentRideTitle = "No rides yet";
            CurrentRideDuration = "--";
            CurrentRideDistance = "--";
            CanDeleteCurrentRide = false;
            CanShowOlderRide = false;
            CanShowNewerRide = false;
            return;
        }

        foreach (var location in rides[selectedRideIndex])
        {
            HistoryLocations.Add(location);
        }

        CurrentRideTitle = BuildRideTitle(rides[selectedRideIndex]);
        CurrentRideDuration = BuildRideDuration(rides[selectedRideIndex]);
        CurrentRideDistance = BuildRideDistance(rides[selectedRideIndex]);
        CanDeleteCurrentRide = true;
        CanShowOlderRide = selectedRideIndex < rides.Count - 1;
        CanShowNewerRide = selectedRideIndex > 0;
    }

    private static long GetSortValue(LocationOnMap location)
    {
        return location.RecordedAtUtcTicks > 0 ? location.RecordedAtUtcTicks : location.Id;
    }

    private static string BuildRideTitle(IReadOnlyCollection<LocationOnMap> ride)
    {
        var firstPoint = ride.OrderBy(GetSortValue).First();

        if (firstPoint.RecordedAtUtcTicks > 0)
        {
            var startedAt = new DateTime(firstPoint.RecordedAtUtcTicks, DateTimeKind.Utc).ToLocalTime();
            return $"{startedAt:g} · {ride.Count} points";
        }

        return $"Ride with {ride.Count} points";
    }

    private static string BuildRideDuration(IReadOnlyList<LocationOnMap> ride)
    {
        if (ride.Count < 2 || ride[0].RecordedAtUtcTicks <= 0 || ride[^1].RecordedAtUtcTicks <= 0)
            return "--";

        var orderedRide = ride.OrderBy(GetSortValue).ToList();
        var duration = new TimeSpan(orderedRide[^1].RecordedAtUtcTicks - orderedRide[0].RecordedAtUtcTicks);

        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";

        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds}s";

        return $"{Math.Max(0, duration.Seconds)}s";
    }

    private static string BuildRideDistance(IReadOnlyList<LocationOnMap> ride)
    {
        if (ride.Count < 2)
            return "0.00 km";

        var orderedRide = ride.OrderBy(GetSortValue).ToList();
        double totalDistanceInKilometers = 0;

        for (var i = 1; i < orderedRide.Count; i++)
        {
            var previous = orderedRide[i - 1];
            var current = orderedRide[i];

            totalDistanceInKilometers += Location.CalculateDistance(
                new Location(previous.Latitude, previous.Longitude),
                new Location(current.Latitude, current.Longitude),
                DistanceUnits.Kilometers);
        }

        return $"{totalDistanceInKilometers:F2} km";
    }
}