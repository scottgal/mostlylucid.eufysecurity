using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Devices;

/// <summary>
/// Base class for all Eufy security devices
/// </summary>
public abstract class Device
{
    private readonly ILogger? _logger;
    private readonly Dictionary<string, object?> _properties = new();

    /// <summary>
    /// Device serial number (unique identifier)
    /// </summary>
    public string SerialNumber { get; }

    /// <summary>
    /// Device name
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Device model
    /// </summary>
    public string Model { get; protected set; }

    /// <summary>
    /// Device type
    /// </summary>
    public DeviceType DeviceType { get; }

    /// <summary>
    /// Hardware version
    /// </summary>
    public string HardwareVersion { get; protected set; }

    /// <summary>
    /// Software version
    /// </summary>
    public string SoftwareVersion { get; protected set; }

    /// <summary>
    /// Station serial number this device belongs to
    /// </summary>
    public string StationSerialNumber { get; }

    /// <summary>
    /// Whether device is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => GetPropertyValue<bool>("enabled");
        protected set => SetPropertyValue("enabled", value);
    }

    /// <summary>
    /// Device event raised when property changes
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    protected Device(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
    {
        SerialNumber = serialNumber;
        Name = name;
        Model = model;
        DeviceType = deviceType;
        StationSerialNumber = stationSerialNumber;
        HardwareVersion = string.Empty;
        SoftwareVersion = string.Empty;
        _logger = logger;
    }

    /// <summary>
    /// Get property value by name
    /// </summary>
    protected T? GetPropertyValue<T>(string propertyName)
    {
        if (_properties.TryGetValue(propertyName, out var value))
        {
            if (value is T typedValue)
                return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Set property value and raise change event
    /// </summary>
    protected void SetPropertyValue<T>(string propertyName, T value)
    {
        var oldValue = _properties.TryGetValue(propertyName, out var old) ? old : default;
        _properties[propertyName] = value;

        if (!Equals(oldValue, value))
        {
            OnPropertyChanged(propertyName, oldValue, value);
        }
    }

    /// <summary>
    /// Raise property changed event
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName, oldValue, newValue));
    }

    /// <summary>
    /// Check if device has a specific property
    /// </summary>
    public bool HasProperty(string propertyName)
    {
        return _properties.ContainsKey(propertyName);
    }

    /// <summary>
    /// Update device from raw API data
    /// </summary>
    public virtual void Update(Dictionary<string, object> data)
    {
        if (data.TryGetValue("device_name", out var name) && name is string deviceName)
            Name = deviceName;

        if (data.TryGetValue("main_hw_version", out var hwVersion) && hwVersion is string hw)
            HardwareVersion = hw;

        if (data.TryGetValue("main_sw_version", out var swVersion) && swVersion is string sw)
            SoftwareVersion = sw;

        // Update other common properties
        UpdateProperties(data);
    }

    /// <summary>
    /// Update device properties from raw data
    /// </summary>
    protected virtual void UpdateProperties(Dictionary<string, object> data)
    {
        // Override in derived classes to handle device-specific properties
    }

    public override string ToString()
    {
        return $"{GetType().Name} [{SerialNumber}] {Name} ({Model})";
    }
}

/// <summary>
/// Event args for property changes
/// </summary>
public class PropertyChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public PropertyChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}
