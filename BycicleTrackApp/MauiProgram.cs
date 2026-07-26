using BycicleTrackApp.Services;
using BycicleTrackApp.Services.Interfaces;
using BycicleTrackApp.Data.Models;
using BycicleTrackApp.Data.Repositories;
using BycicleTrackApp.Services;
using BycicleTrackApp.ViewModels;
using BycicleTrackApp.Views;
using Microsoft.Extensions.Logging;

namespace BycicleTrackApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IRepository<LocationOnMap>, LocationRepository>();

        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddSingleton<HistoryViewModel>();
        builder.Services.AddTransient<MapView>();
        builder.Services.AddTransient<HistoryView>();

        return builder.Build();
    }
}