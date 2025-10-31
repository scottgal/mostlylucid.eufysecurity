using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Devices;

/// <summary>
/// Base class for camera devices
/// </summary>
public abstract class Camera : Device
{
    /// <summary>
    /// Battery level percentage (0-100)
    /// </summary>
    public int? BatteryLevel
    {
        get => GetPropertyValue<int?>("battery");
        protected set => SetPropertyValue("battery", value);
    }

    /// <summary>
    /// Battery temperature in Celsius
    /// </summary>
    public double? BatteryTemperature
    {
        get => GetPropertyValue<double?>("batteryTemperature");
        protected set => SetPropertyValue("batteryTemperature", value);
    }

    /// <summary>
    /// Battery charging status
    /// </summary>
    public bool? IsCharging
    {
        get => GetPropertyValue<bool?>("isCharging");
        protected set => SetPropertyValue("isCharging", value);
    }

    /// <summary>
    /// WiFi signal strength (RSSI)
    /// </summary>
    public int? WifiRssi
    {
        get => GetPropertyValue<int?>("wifiRssi");
        protected set => SetPropertyValue("wifiRssi", value);
    }

    /// <summary>
    /// Whether motion detection is enabled
    /// </summary>
    public bool MotionDetectionEnabled
    {
        get => GetPropertyValue<bool>("motionDetectionEnabled");
        protected set => SetPropertyValue("motionDetectionEnabled", value);
    }

    /// <summary>
    /// Motion detection sensitivity (1-5)
    /// </summary>
    public int MotionDetectionSensitivity
    {
        get => GetPropertyValue<int>("motionDetectionSensitivity");
        protected set => SetPropertyValue("motionDetectionSensitivity", value);
    }

    /// <summary>
    /// Whether LED is enabled
    /// </summary>
    public bool LedEnabled
    {
        get => GetPropertyValue<bool>("ledEnabled");
        protected set => SetPropertyValue("ledEnabled", value);
    }

    /// <summary>
    /// Whether night vision is in auto mode
    /// </summary>
    public bool AutoNightVision
    {
        get => GetPropertyValue<bool>("autoNightVision");
        protected set => SetPropertyValue("autoNightVision", value);
    }

    /// <summary>
    /// Whether audio recording is enabled
    /// </summary>
    public bool AudioRecordingEnabled
    {
        get => GetPropertyValue<bool>("audioRecordingEnabled");
        protected set => SetPropertyValue("audioRecordingEnabled", value);
    }

    /// <summary>
    /// RTSP stream URL
    /// </summary>
    public string? RtspStreamUrl
    {
        get => GetPropertyValue<string?>("rtspStreamUrl");
        protected set => SetPropertyValue("rtspStreamUrl", value);
    }

    /// <summary>
    /// Last event picture URL
    /// </summary>
    public string? LastEventPictureUrl
    {
        get => GetPropertyValue<string?>("lastEventPictureUrl");
        protected set => SetPropertyValue("lastEventPictureUrl", value);
    }

    /// <summary>
    /// Last event picture data
    /// </summary>
    public byte[]? LastEventPicture
    {
        get => GetPropertyValue<byte[]?>("lastEventPicture");
        protected set => SetPropertyValue("lastEventPicture", value);
    }

    protected Camera(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
        : base(serialNumber, name, model, deviceType, stationSerialNumber, logger)
    {
    }

    protected override void UpdateProperties(Dictionary<string, object> data)
    {
        base.UpdateProperties(data);

        // Update camera-specific properties from params
        if (data.TryGetValue("params", out var paramsObj) && paramsObj is List<Dictionary<string, object>> paramsList)
        {
            foreach (var param in paramsList)
            {
                if (!param.TryGetValue("param_type", out var typeObj) || typeObj is not int paramType)
                    continue;

                if (!param.TryGetValue("param_value", out var valueObj))
                    continue;

                UpdateParameter(paramType, valueObj);
            }
        }
    }

    protected virtual void UpdateParameter(int paramType, object value)
    {
        // Override in derived classes to handle device-specific parameters
    }
}

/// <summary>
/// Indoor camera device
/// </summary>
public class IndoorCamera : Camera
{
    public IndoorCamera(
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
/// Solo camera device (battery powered, standalone)
/// </summary>
public class SoloCamera : Camera
{
    public SoloCamera(
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
/// Floodlight camera device
/// </summary>
public class FloodlightCamera : Camera
{
    /// <summary>
    /// Floodlight switch state
    /// </summary>
    public bool FloodlightEnabled
    {
        get => GetPropertyValue<bool>("floodlightEnabled");
        protected set => SetPropertyValue("floodlightEnabled", value);
    }

    /// <summary>
    /// Floodlight brightness (22-100)
    /// </summary>
    public int FloodlightBrightness
    {
        get => GetPropertyValue<int>("floodlightBrightness");
        protected set => SetPropertyValue("floodlightBrightness", value);
    }

    public FloodlightCamera(
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
