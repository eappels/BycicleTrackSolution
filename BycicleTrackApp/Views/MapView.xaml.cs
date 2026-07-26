using BycicleTrackApp.Messages;
using BycicleTrackApp.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.ComponentModel;

namespace BycicleTrackApp.Views;

public partial class MapView : ContentPage
{

    private bool isZooming = false;
    private double zoomLevel = 250;
    private IDispatcherTimer timer;
    private PropertyChangedEventHandler mapPropertyChangedHandler;

    public MapView(MapViewModel viewModel, HistoryView historyView)
    {
        InitializeComponent();

        this.On<iOS>().SetUseSafeArea(false);
        Padding = 0;

        if (BindingContext is not MapViewModel mapViewModel)
            BindingContext = viewModel;

        HistoryHost.Content = historyView;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(3);
        timer.Tick += OnTimerTick;

        WeakReferenceMessenger.Default.Register<LocationUpdatedMessage>(this, (r, m) =>
        {
            if (MyMap != null && m.Value != null)
            {
                if (MyMap.MapElements.Count == 0)
                {
                    var track = ((MapViewModel)BindingContext).Track;
                    if (track != null)
                        MyMap.MapElements.Add(track);
                }
                if (!isZooming)
                    MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(m.Value.Latitude, m.Value.Longitude), Distance.FromMeters(zoomLevel)));
            }
        });
        if (MyMap != null)
        {
            mapPropertyChangedHandler = (s, e) =>
            {
                if (e.PropertyName == "VisibleRegion")
                {
                    if (MyMap.VisibleRegion != null)
                    {
                        timer.Start();
                        isZooming = true;
                        zoomLevel = MyMap.VisibleRegion.Radius.Meters;
                    }
                }
            };
            MyMap.PropertyChanged += mapPropertyChangedHandler;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        isZooming = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var location = await Geolocation.Default.GetLastKnownLocationAsync();
        if (MyMap != null && location != null)
        {
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(zoomLevel)));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Dispose();
    }

    public void Dispose()
    {
        if (timer != null)
        {
            timer.Tick -= OnTimerTick;
            timer.Stop();
            timer = null;
        }

        WeakReferenceMessenger.Default.Unregister<LocationUpdatedMessage>(this);

        if (MyMap != null && mapPropertyChangedHandler != null)
        {
            MyMap.PropertyChanged -= mapPropertyChangedHandler;
            mapPropertyChangedHandler = null;
        }
    }
}