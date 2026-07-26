using CommunityToolkit.Mvvm.Messaging.Messages;

namespace BycicleTrackApp.Messages;

public class LocationUpdatedMessage : ValueChangedMessage<Location>
{
    public LocationUpdatedMessage(Location value)
        : base(value)
    {
    }
}