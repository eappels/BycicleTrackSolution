using BycicleTrackApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace BycicleTrackApp;

public partial class App : Application
{
    private readonly IServiceProvider serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        this.serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(serviceProvider.GetRequiredService<MapView>());
    }
}