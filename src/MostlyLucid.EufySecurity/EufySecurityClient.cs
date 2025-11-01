using System.Collections.Concurrent;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Devices;
using MostlyLucid.EufySecurity.Events;
using MostlyLucid.EufySecurity.Http;
using MostlyLucid.EufySecurity.P2P;
using MostlyLucid.EufySecurity.Push;
using MostlyLucid.EufySecurity.Stations;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity;

/// <summary>
/// Main client for controlling Eufy Security devices
/// </summary>
public class EufySecurityClient : IDisposable
{
    private readonly EufySecurityConfig _config;
    private readonly ILogger? _logger;
    private readonly HttpApiClient _httpApi;
    private readonly P2PClient _p2pClient;
    private readonly PushNotificationService _pushService;

    private readonly ConcurrentDictionary<string, Station> _stations = new();
    private readonly ConcurrentDictionary<string, Device> _devices = new();

    private Timer? _refreshTimer;
    private bool _disposed;

    // Events
    public event DeviceAddedEventHandler? DeviceAdded;
    public event DeviceRemovedEventHandler? DeviceRemoved;
    public event StationAddedEventHandler? StationAdded;
    public event StationRemovedEventHandler? StationRemoved;
    public event PushNotificationEventHandler? PushNotificationReceived;
    public event LivestreamStartEventHandler? LivestreamStarted;
    public event LivestreamStopEventHandler? LivestreamStopped;
    public event LivestreamDataEventHandler? LivestreamDataReceived;
    public event GuardModeChangedEventHandler? GuardModeChanged;

    /// <summary>
    /// Library version
    /// </summary>
    public static string Version => "1.0.0";

    /// <summary>
    /// Create a new instance of the Eufy Security client
    /// </summary>
    public EufySecurityClient(EufySecurityConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = config.Logger;

        // Initialize subsystems
        _httpApi = new HttpApiClient(
            config.Username,
            config.EffectivePassword,
            config.Country,
            config.Language,
            config.TrustedDeviceName,
            _logger);

        _p2pClient = new P2PClient(_logger);
        _pushService = new PushNotificationService(_logger);

        // Wire up events
        _p2pClient.LivestreamStarted += OnP2PLivestreamStarted;
        _p2pClient.LivestreamStopped += OnP2PLivestreamStopped;
        _p2pClient.LivestreamDataReceived += OnP2PLivestreamDataReceived;
        _pushService.NotificationReceived += OnPushNotificationReceived;

        _logger?.LogInformation("EufySecurity.NET v{Version} initialized", Version);
    }

    /// <summary>
    /// Connect to Eufy cloud and initialize devices
    /// </summary>
    /// <param name="verifyCode">Optional 2FA verification code if previously required</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result indicating success or if 2FA is required</returns>
    public async Task<AuthenticationResult> ConnectAsync(string? verifyCode = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Connecting to Eufy Security cloud...");

            // Authenticate with cloud API
            var result = await _httpApi.AuthenticateAsync(verifyCode, null, cancellationToken);
            if (!result.Success)
            {
                if (result.RequiresTwoFactor)
                {
                    _logger?.LogInformation("Two-factor authentication required. Call ConnectAsync again with the verification code from your email.");
                    return result;
                }

                throw new AuthenticationException(result.Message ?? "Failed to authenticate with Eufy cloud");
            }

            // Load devices and stations
            await RefreshDevicesAsync(cancellationToken);

            // Start push notifications
            if (!_config.DisableAutomaticCloudPolling)
            {
                await _pushService.StartAsync(cancellationToken);
            }

            // Set up periodic refresh
            if (!_config.DisableAutomaticCloudPolling && _config.PollingIntervalMinutes > 0)
            {
                var interval = TimeSpan.FromMinutes(_config.PollingIntervalMinutes);
                _refreshTimer = new Timer(
                    _ => RefreshDevicesAsync(CancellationToken.None).GetAwaiter().GetResult(),
                    null,
                    interval,
                    interval);
            }

            _logger?.LogInformation("Connected to Eufy Security cloud with {StationCount} stations and {DeviceCount} devices",
                _stations.Count, _devices.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to connect to Eufy Security cloud");
            throw;
        }
    }

    /// <summary>
    /// Disconnect from Eufy cloud
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Disconnecting from Eufy Security cloud...");

        _refreshTimer?.Dispose();
        _refreshTimer = null;

        await _pushService.StopAsync(cancellationToken);

        foreach (var station in _stations.Values)
        {
            _p2pClient.Disconnect(station);
        }

        _logger?.LogInformation("Disconnected from Eufy Security cloud");
    }

    /// <summary>
    /// Refresh device list from cloud
    /// </summary>
    public async Task RefreshDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogDebug("Refreshing device list...");

            // Get stations
            var stationData = await _httpApi.GetStationsAsync(cancellationToken);
            foreach (var data in stationData)
            {
                if (!data.TryGetValue("station_sn", out var snObj) || snObj is not string sn)
                    continue;

                if (_stations.TryGetValue(sn, out var station))
                {
                    station.Update(data);
                }
                else
                {
                    station = CreateStation(data);
                    if (station != null && _stations.TryAdd(sn, station))
                    {
                        StationAdded?.Invoke(this, new StationEventArgs(station));
                        _logger?.LogInformation("Station added: {StationName} ({StationSerial})",
                            station.Name, station.SerialNumber);
                    }
                }
            }

            // Get devices
            var deviceData = await _httpApi.GetDevicesAsync(cancellationToken);
            foreach (var data in deviceData)
            {
                if (!data.TryGetValue("device_sn", out var snObj) || snObj is not string sn)
                    continue;

                if (_devices.TryGetValue(sn, out var device))
                {
                    device.Update(data);
                }
                else
                {
                    device = CreateDevice(data);
                    if (device != null && _devices.TryAdd(sn, device))
                    {
                        // Add device to its station
                        if (_stations.TryGetValue(device.StationSerialNumber, out var station))
                        {
                            station.AddDevice(device);
                        }

                        DeviceAdded?.Invoke(this, new DeviceEventArgs(device));
                        _logger?.LogInformation("Device added: {DeviceName} ({DeviceSerial})",
                            device.Name, device.SerialNumber);
                    }
                }
            }

            _logger?.LogDebug("Device list refreshed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh device list");
            throw;
        }
    }

    /// <summary>
    /// Get all stations
    /// </summary>
    public IReadOnlyDictionary<string, Station> GetStations()
    {
        return _stations;
    }

    /// <summary>
    /// Get station by serial number
    /// </summary>
    public Station? GetStation(string serialNumber)
    {
        _stations.TryGetValue(serialNumber, out var station);
        return station;
    }

    /// <summary>
    /// Get all devices
    /// </summary>
    public IReadOnlyDictionary<string, Device> GetDevices()
    {
        return _devices;
    }

    /// <summary>
    /// Get device by serial number
    /// </summary>
    public Device? GetDevice(string serialNumber)
    {
        _devices.TryGetValue(serialNumber, out var device);
        return device;
    }

    /// <summary>
    /// Connect to a station via P2P
    /// </summary>
    public async Task ConnectToStationAsync(
        string stationSerial,
        CancellationToken cancellationToken = default)
    {
        var station = GetStation(stationSerial);
        if (station == null)
            throw new StationNotFoundException(stationSerial);

        // In a real implementation, we'd get P2P credentials from the API
        await _p2pClient.ConnectAsync(station, "", "", _config.P2PConnectionSetup, cancellationToken);
    }

    /// <summary>
    /// Start livestream from a device
    /// </summary>
    public async Task StartLivestreamAsync(
        string deviceSerial,
        CancellationToken cancellationToken = default)
    {
        var device = GetDevice(deviceSerial);
        if (device == null)
            throw new DeviceNotFoundException(deviceSerial);

        var station = GetStation(device.StationSerialNumber);
        if (station == null)
            throw new StationNotFoundException(device.StationSerialNumber);

        if (!station.IsConnected)
        {
            await ConnectToStationAsync(station.SerialNumber, cancellationToken);
        }

        await _p2pClient.StartLivestreamAsync(station, device, cancellationToken);
    }

    /// <summary>
    /// Stop livestream from a device
    /// </summary>
    public async Task StopLivestreamAsync(
        string deviceSerial,
        CancellationToken cancellationToken = default)
    {
        var device = GetDevice(deviceSerial);
        if (device == null)
            throw new DeviceNotFoundException(deviceSerial);

        var station = GetStation(device.StationSerialNumber);
        if (station == null)
            throw new StationNotFoundException(device.StationSerialNumber);

        await _p2pClient.StopLivestreamAsync(station, device, cancellationToken);
    }

    /// <summary>
    /// Set guard mode for a station
    /// </summary>
    public async Task SetGuardModeAsync(
        string stationSerial,
        GuardMode mode,
        CancellationToken cancellationToken = default)
    {
        var station = GetStation(stationSerial);
        if (station == null)
            throw new StationNotFoundException(stationSerial);

        if (!station.IsConnected)
        {
            await ConnectToStationAsync(station.SerialNumber, cancellationToken);
        }

        await _p2pClient.SetGuardModeAsync(station, mode, cancellationToken);
    }

    private Station? CreateStation(Dictionary<string, object> data)
    {
        if (!data.TryGetValue("station_sn", out var snObj) || snObj is not string sn)
            return null;

        var name = data.TryGetValue("station_name", out var nameObj) && nameObj is string n ? n : "Unknown";
        var model = data.TryGetValue("station_model", out var modelObj) && modelObj is string m ? m : "Unknown";
        var deviceType = data.TryGetValue("device_type", out var typeObj) && typeObj is int type
            ? (DeviceType)type
            : DeviceType.Station;

        var station = new Station(sn, name, model, deviceType, _logger);
        station.Update(data);
        return station;
    }

    private Device? CreateDevice(Dictionary<string, object> data)
    {
        if (!data.TryGetValue("device_sn", out var snObj) || snObj is not string sn)
            return null;

        if (!data.TryGetValue("station_sn", out var stationSnObj) || stationSnObj is not string stationSn)
            return null;

        var name = data.TryGetValue("device_name", out var nameObj) && nameObj is string n ? n : "Unknown";
        var model = data.TryGetValue("device_model", out var modelObj) && modelObj is string m ? m : "Unknown";
        var deviceType = data.TryGetValue("device_type", out var typeObj) && typeObj is int type
            ? (DeviceType)type
            : DeviceType.Camera;

        // Create appropriate device subclass based on type
        Device? device = deviceType switch
        {
            DeviceType.IndoorCamera or DeviceType.IndoorPTCamera => new IndoorCamera(sn, name, model, deviceType, stationSn, _logger),
            DeviceType.SoloCamera or DeviceType.SoloCameraPro => new SoloCamera(sn, name, model, deviceType, stationSn, _logger),
            DeviceType.Floodlight => new FloodlightCamera(sn, name, model, deviceType, stationSn, _logger),
            DeviceType.BatteryDoorbell or DeviceType.BatteryDoorbell2 => new BatteryDoorbell(sn, name, model, deviceType, stationSn, _logger),
            DeviceType.Doorbell => new WiredDoorbell(sn, name, model, deviceType, stationSn, _logger),
            _ => new GenericCamera(sn, name, model, deviceType, stationSn, _logger)
        };

        device?.Update(data);
        return device;
    }

    // Event handlers
    private void OnP2PLivestreamStarted(object? sender, LivestreamEventArgs e)
    {
        LivestreamStarted?.Invoke(this, e);
    }

    private void OnP2PLivestreamStopped(object? sender, LivestreamEventArgs e)
    {
        LivestreamStopped?.Invoke(this, e);
    }

    private void OnP2PLivestreamDataReceived(object? sender, LivestreamDataEventArgs e)
    {
        LivestreamDataReceived?.Invoke(this, new LivestreamDataEventArgs(e.Station, e.Device, e.Data, e.IsVideo));
    }

    private void OnPushNotificationReceived(object? sender, PushMessage e)
    {
        PushNotificationReceived?.Invoke(this, new PushNotificationEventArgs(e));
    }

    public void Dispose()
    {
        if (_disposed) return;

        _refreshTimer?.Dispose();
        _httpApi?.Dispose();
        _p2pClient?.Dispose();
        _pushService?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
