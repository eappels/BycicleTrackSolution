using BycicleTrackApp.ViewModels;

namespace BycicleTrackApp.Views;

public partial class HistoryView : ContentView
{
	public HistoryView(HistoryViewModel viewModel)
	{
		InitializeComponent();

        if (BindingContext is not HistoryViewModel historyViewModel)
            BindingContext = viewModel;
    }
}