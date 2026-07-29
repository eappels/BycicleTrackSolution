using BycicleTrackApp.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Collections.Specialized;

namespace BycicleTrackApp.Views;

public partial class HistoryView : ContentPage
{
    private readonly HistoryViewModel viewModel;

	public HistoryView(HistoryViewModel viewModel)
	{
		InitializeComponent();

        this.viewModel = viewModel;
        this.viewModel.HistoryLocations.CollectionChanged += OnHistoryLocationsChanged;

        if (BindingContext is not HistoryViewModel historyViewModel)
            BindingContext = viewModel;

        RefreshMap();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadHistoryAsync();
    }

    private void OnHistoryLocationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RefreshMap);
    }

    private void RefreshMap()
    {
        var historyMap = this.FindByName("HistoryMap") as Microsoft.Maui.Controls.Maps.Map;
        var emptyState = this.FindByName<VerticalStackLayout>("EmptyState");

        if (historyMap == null || emptyState == null)
            return;

        historyMap.MapElements.Clear();
        historyMap.Pins.Clear();

        var hasHistory = viewModel.HistoryLocations.Count > 0;

        historyMap.IsVisible = hasHistory;
        emptyState.IsVisible = !hasHistory;

        if (!hasHistory)
            return;

        var polyline = new Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 4
        };

        foreach (var item in viewModel.HistoryLocations)
        {
            polyline.Geopath.Add(new Location(item.Latitude, item.Longitude));
        }

        historyMap.MapElements.Add(polyline);

        var first = viewModel.HistoryLocations.First();
        var last = viewModel.HistoryLocations.Last();

        historyMap.Pins.Add(new Pin
        {
            Label = "Start",
            Location = new Location(first.Latitude, first.Longitude)
        });

        if (viewModel.HistoryLocations.Count > 1)
        {
            historyMap.Pins.Add(new Pin
            {
                Label = "End",
                Location = new Location(last.Latitude, last.Longitude)
            });
        }

        var center = new Location(
            viewModel.HistoryLocations.Average(x => x.Latitude),
            viewModel.HistoryLocations.Average(x => x.Longitude));

        var radiusInMeters = Math.Max(
            250,
            viewModel.HistoryLocations.Max(x => Location.CalculateDistance(center, new Location(x.Latitude, x.Longitude), DistanceUnits.Kilometers)) * 1000);

        historyMap.MoveToRegion(Microsoft.Maui.Maps.MapSpan.FromCenterAndRadius(center, Microsoft.Maui.Maps.Distance.FromMeters(radiusInMeters)));
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!viewModel.CanDeleteCurrentRide)
            return;

        var shouldDelete = await DisplayAlert(
            "Delete ride",
            "Delete the currently selected ride?",
            "Delete",
            "Cancel");

        if (!shouldDelete)
            return;

        await viewModel.DeleteCurrentRideAsync();
    }
}