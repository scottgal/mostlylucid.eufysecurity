using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Devices;

/// <summary>
/// Base class for doorbell devices
/// </summary>
public abstract class Doorbell : Camera
{
    /// <summary>
    /// Ringtone volume
    /// </summary>
    public int RingtoneVolume
    {
        get => GetPropertyValue<int>("ringtoneVolume");
        protected set => SetPropertyValue("ringtoneVolume", value);
    }

    /// <summary>
    /// Whether ring notifications are enabled
    /// </summary>
    public bool RingNotificationEnabled
    {
        get => GetPropertyValue<bool>("ringNotificationEnabled");
        protected set => SetPropertyValue("ringNotificationEnabled", value);
    }

    /// <summary>
    /// Whether motion notifications are enabled
    /// </summary>
    public bool MotionNotificationEnabled
    {
        get => GetPropertyValue<bool>("motionNotificationEnabled");
        protected set => SetPropertyValue("motionNotificationEnabled", value);
    }

    /// <summary>
    /// HDR enabled
    /// </summary>
    public bool HdrEnabled
    {
        get => GetPropertyValue<bool>("hdrEnabled");
        protected set => SetPropertyValue("hdrEnabled", value);
    }

    /// <summary>
    /// Distortion correction enabled
    /// </summary>
    public bool DistortionCorrectionEnabled
    {
        get => GetPropertyValue<bool>("distortionCorrectionEnabled");
        protected set => SetPropertyValue("distortionCorrectionEnabled", value);
    }

    /// <summary>
    /// Video recording quality
    /// </summary>
    public int VideoRecordingQuality
    {
        get => GetPropertyValue<int>("videoRecordingQuality");
        protected set => SetPropertyValue("videoRecordingQuality", value);
    }

    protected Doorbell(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
        : base(serialNumber, name, model, deviceType, stationSerialNumber, logger)
    {
    }
}

/// <summary>
/// Battery-powered doorbell
/// </summary>
public class BatteryDoorbell : Doorbell
{
    /// <summary>
    /// Whether indoor chime is enabled
    /// </summary>
    public bool IndoorChimeEnabled
    {
        get => GetPropertyValue<bool>("indoorChimeEnabled");
        protected set => SetPropertyValue("indoorChimeEnabled", value);
    }

    public BatteryDoorbell(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
        : base(serialNumber, name, model, deviceType, stationSerialNumber, logger)
    {
    }
}

/// <summary>
/// Wired doorbell
/// </summary>
public class WiredDoorbell : Doorbell
{
    public WiredDoorbell(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
        : base(serialNumber, name, model, deviceType, stationSerialNumber, logger)
    {
    }
}
