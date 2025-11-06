using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Devices;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Stations;

/// <summary>
/// Represents a Eufy station/hub (HomeBase)
/// </summary>
public class Station
{
    private readonly ILogger? _logger;
    private readonly Dictionary<string, object?> _properties = new();
    private readonly Dictionary<string, Device> _devices = new();

    /// <summary>
    /// Station serial number (unique identifier)
    /// </summary>
    public string SerialNumber { get; }

    /// <summary>
    /// Station name
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Station model
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
    /// IP address
    /// </summary>
    public string? IpAddress { get; protected set; }

    /// <summary>
    /// MAC address
    /// </summary>
    public string? MacAddress { get; protected set; }

    /// <summary>
    /// LAN IP address
    /// </summary>
    public string? LanIpAddress { get; protected set; }

    /// <summary>
    /// Current guard mode
    /// </summary>
    public GuardMode GuardMode
    {
        get => GetPropertyValue<GuardMode>("guardMode");
        protected set => SetPropertyValue("guardMode", value);
    }

    /// <summary>
    /// Current mode
    /// </summary>
    public GuardMode CurrentMode
    {
        get => GetPropertyValue<GuardMode>("currentMode");
        protected set => SetPropertyValue("currentMode", value);
    }

    /// <summary>
    /// Whether station is connected via P2P
    /// </summary>
    public bool IsConnected { get; internal set; }

    /// <summary>
    /// Devices attached to this station
    /// </summary>
    public IReadOnlyDictionary<string, Device> Devices => _devices;

    /// <summary>
    /// Station event raised when property changes
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    /// <summary>
    /// Event raised when guard mode changes
    /// </summary>
    public event EventHandler<GuardMode>? GuardModeChanged;

    public Station(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be null or empty", nameof(serialNumber));

        SerialNumber = serialNumber;
        Name = name ?? "Unknown";
        Model = model ?? "Unknown";
        DeviceType = deviceType;
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

            // Raise specific events
            if (propertyName == "guardMode" && value is GuardMode guardMode)
            {
                GuardModeChanged?.Invoke(this, guardMode);
            }
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
    /// Add device to station
    /// </summary>
    internal void AddDevice(Device device)
    {
        _devices[device.SerialNumber] = device;
    }

    /// <summary>
    /// Remove device from station
    /// </summary>
    internal void RemoveDevice(string serialNumber)
    {
        _devices.Remove(serialNumber);
    }

    /// <summary>
    /// Update station from raw API data
    /// </summary>
    public virtual void Update(Dictionary<string, object> data)
    {
        if (data == null)
        {
            _logger?.LogWarning("Attempted to update station {SerialNumber} with null data", SerialNumber);
            return;
        }

        if (data.TryGetValue("station_name", out var name) && name is string stationName)
            Name = stationName;

        if (data.TryGetValue("main_hw_version", out var hwVersion) && hwVersion is string hw)
            HardwareVersion = hw;

        if (data.TryGetValue("main_sw_version", out var swVersion) && swVersion is string sw)
            SoftwareVersion = sw;

        if (data.TryGetValue("ip_addr", out var ip) && ip is string ipAddr)
            IpAddress = ipAddr;

        if (data.TryGetValue("mac_address", out var mac) && mac is string macAddr)
            MacAddress = macAddr;

        if (data.TryGetValue("lan_ip_addr", out var lanIp) && lanIp is string lanIpAddr)
            LanIpAddress = lanIpAddr;

        // Update other properties
        UpdateProperties(data);
    }

    /// <summary>
    /// Update station properties from raw data
    /// </summary>
    protected virtual void UpdateProperties(Dictionary<string, object> data)
    {
        // Override in derived classes to handle station-specific properties
    }

    public override string ToString()
    {
        return $"Station [{SerialNumber}] {Name} ({Model})";
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
