using MostlyLucid.EufySecurity.Devices;
using MostlyLucid.EufySecurity.Stations;
using MostlyLucid.EufySecurity.P2P;
using MostlyLucid.EufySecurity.Push;

namespace MostlyLucid.EufySecurity.Events;

/// <summary>
/// Event handler for device added
/// </summary>
public delegate void DeviceAddedEventHandler(object sender, DeviceEventArgs e);

/// <summary>
/// Event handler for device removed
/// </summary>
public delegate void DeviceRemovedEventHandler(object sender, DeviceEventArgs e);

/// <summary>
/// Event handler for station added
/// </summary>
public delegate void StationAddedEventHandler(object sender, StationEventArgs e);

/// <summary>
/// Event handler for station removed
/// </summary>
public delegate void StationRemovedEventHandler(object sender, StationEventArgs e);

/// <summary>
/// Event handler for push notifications
/// </summary>
public delegate void PushNotificationEventHandler(object sender, PushNotificationEventArgs e);

/// <summary>
/// Event handler for livestream start
/// </summary>
public delegate void LivestreamStartEventHandler(object sender, LivestreamEventArgs e);

/// <summary>
/// Event handler for livestream stop
/// </summary>
public delegate void LivestreamStopEventHandler(object sender, LivestreamEventArgs e);

/// <summary>
/// Event handler for livestream data
/// </summary>
public delegate void LivestreamDataEventHandler(object sender, LivestreamDataEventArgs e);

/// <summary>
/// Event handler for guard mode change
/// </summary>
public delegate void GuardModeChangedEventHandler(object sender, GuardModeEventArgs e);

/// <summary>
/// Event args for device events
/// </summary>
public class DeviceEventArgs : EventArgs
{
    public Device Device { get; }

    public DeviceEventArgs(Device device)
    {
        Device = device;
    }
}

/// <summary>
/// Event args for station events
/// </summary>
public class StationEventArgs : EventArgs
{
    public Station Station { get; }

    public StationEventArgs(Station station)
    {
        Station = station;
    }
}

/// <summary>
/// Event args for push notifications
/// </summary>
public class PushNotificationEventArgs : EventArgs
{
    public PushMessage Message { get; }

    public PushNotificationEventArgs(PushMessage message)
    {
        Message = message;
    }
}

/// <summary>
/// Event args for livestream events
/// </summary>
public class LivestreamEventArgs : EventArgs
{
    public Station Station { get; }
    public Device Device { get; }
    public StreamMetadata? Metadata { get; }

    public LivestreamEventArgs(Station station, Device device, StreamMetadata? metadata = null)
    {
        Station = station;
        Device = device;
        Metadata = metadata;
    }
}

/// <summary>
/// Event args for livestream data
/// </summary>
public class LivestreamDataEventArgs : EventArgs
{
    public Station Station { get; }
    public Device Device { get; }
    public byte[] Data { get; }
    public bool IsVideo { get; }

    public LivestreamDataEventArgs(Station station, Device device, byte[] data, bool isVideo)
    {
        Station = station;
        Device = device;
        Data = data;
        IsVideo = isVideo;
    }
}

/// <summary>
/// Event args for guard mode changes
/// </summary>
public class GuardModeEventArgs : EventArgs
{
    public Station Station { get; }
    public Common.GuardMode GuardMode { get; }

    public GuardModeEventArgs(Station station, Common.GuardMode guardMode)
    {
        Station = station;
        GuardMode = guardMode;
    }
}
