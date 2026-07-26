using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BycicleTrackApp.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    public ObservableCollection<string> HistoryItems { get; } = [];

    public HistoryViewModel()
    {
    }
}