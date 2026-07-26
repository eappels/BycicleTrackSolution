using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BycicleTrackApp.Data.Models;
using BycicleTrackApp.Services.Interfaces;
using System.Collections.ObjectModel;

namespace BycicleTrackApp.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IRepository<LocationOnMap> repository;
    private List<List<LocationOnMap>> rides = [];
    private int selectedRideIndex = -1;
    private string currentRideTitle = "No rides yet";
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

    private void ShowSelectedRide()
    {
        HistoryLocations.Clear();

        if (selectedRideIndex < 0 || selectedRideIndex >= rides.Count)
        {
            CurrentRideTitle = "No rides yet";
            CanShowOlderRide = false;
            CanShowNewerRide = false;
            return;
        }

        foreach (var location in rides[selectedRideIndex])
        {
            HistoryLocations.Add(location);
        }

        CurrentRideTitle = BuildRideTitle(rides[selectedRideIndex]);
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
}