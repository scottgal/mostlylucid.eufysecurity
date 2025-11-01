using MostlyLucid.EufySecurity;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Demo.Hubs;
using MostlyLucid.EufySecurity.Events;
using Microsoft.AspNetCore.SignalR;

namespace MostlyLucid.EufySecurity.Demo.Services;

/// <summary>
/// Background service that manages the EufySecurity client lifecycle
/// </summary>
public class EufySecurityHostedService : IHostedService, IDisposable
{
    private readonly ILogger<EufySecurityHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<EufyEventsHub> _hubContext;
    private EufySecurityClient? _client;

    public EufySecurityHostedService(
        ILogger<EufySecurityHostedService> logger,
        IConfiguration configuration,
        IHubContext<EufyEventsHub> hubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gets the current EufySecurity client instance
    /// </summary>
    public EufySecurityClient? Client => _client;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting EufySecurity hosted service...");

            // Load configuration
            var username = _configuration["Eufy:Username"];
            var password = _configuration["Eufy:Password"];
            var verifyCode = _configuration["Eufy:VerifyCode"]; // Optional 2FA code
            var country = _configuration["Eufy:Country"] ?? "US";
            var language = _configuration["Eufy:Language"] ?? "en";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Eufy credentials not configured. Service will not connect.");
                return;
            }

            var config = new EufySecurityConfig
            {
                Username = username,
                Password = password,
                Country = country,
                Language = language,
                PollingIntervalMinutes = _configuration.GetValue("Eufy:PollingIntervalMinutes", 10),
                Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EufySecurityClient>()
            };

            _client = new EufySecurityClient(config);

            // Subscribe to events
            SubscribeToEvents();

            // Connect to Eufy cloud
            var authResult = await _client.ConnectAsync(verifyCode, cancellationToken);

            if (!authResult.Success)
            {
                if (authResult.RequiresTwoFactor)
                {
                    _logger.LogWarning("Two-factor authentication required. Check your email for verification code.");
                    _logger.LogWarning("Add the verification code to appsettings.json as 'Eufy:VerifyCode' and restart the application.");
                }
                else
                {
                    throw new Exception($"Authentication failed: {authResult.Message}");
                }
            }

            _logger.LogInformation("EufySecurity hosted service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start EufySecurity hosted service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping EufySecurity hosted service...");

        if (_client != null)
        {
            await _client.DisconnectAsync(cancellationToken);
        }

        _logger.LogInformation("EufySecurity hosted service stopped");
    }

    private void SubscribeToEvents()
    {
        if (_client == null) return;

        _client.DeviceAdded += async (sender, e) =>
        {
            _logger.LogInformation("Device added: {DeviceName} ({DeviceSerial})",
                e.Device.Name, e.Device.SerialNumber);

            await _hubContext.Clients.All.SendAsync("DeviceAdded", new
            {
                serialNumber = e.Device.SerialNumber,
                name = e.Device.Name,
                type = e.Device.DeviceType.ToString(),
                model = e.Device.Model
            });
        };

        _client.StationAdded += async (sender, e) =>
        {
            _logger.LogInformation("Station added: {StationName} ({StationSerial})",
                e.Station.Name, e.Station.SerialNumber);

            await _hubContext.Clients.All.SendAsync("StationAdded", new
            {
                serialNumber = e.Station.SerialNumber,
                name = e.Station.Name,
                type = e.Station.DeviceType.ToString(),
                model = e.Station.Model
            });
        };

        _client.LivestreamStarted += async (sender, e) =>
        {
            _logger.LogInformation("Livestream started: {DeviceName}", e.Device.Name);

            await _hubContext.Clients.All.SendAsync("LivestreamStarted", new
            {
                deviceSerial = e.Device.SerialNumber,
                deviceName = e.Device.Name,
                metadata = e.Metadata != null ? new
                {
                    videoCodec = e.Metadata.VideoCodec.ToString(),
                    audioCodec = e.Metadata.AudioCodec.ToString(),
                    videoWidth = e.Metadata.VideoWidth,
                    videoHeight = e.Metadata.VideoHeight,
                    videoFps = e.Metadata.VideoFps
                } : null
            });
        };

        _client.LivestreamStopped += async (sender, e) =>
        {
            _logger.LogInformation("Livestream stopped: {DeviceName}", e.Device.Name);

            await _hubContext.Clients.All.SendAsync("LivestreamStopped", new
            {
                deviceSerial = e.Device.SerialNumber,
                deviceName = e.Device.Name
            });
        };

        _client.PushNotificationReceived += async (sender, e) =>
        {
            _logger.LogInformation("Push notification: {Type} from {DeviceSerial}",
                e.Message.Type, e.Message.DeviceSerial);

            await _hubContext.Clients.All.SendAsync("PushNotification", new
            {
                type = e.Message.Type,
                deviceSerial = e.Message.DeviceSerial,
                stationSerial = e.Message.StationSerial,
                timestamp = e.Message.Timestamp,
                title = e.Message.Title,
                message = e.Message.Message,
                personDetected = e.Message.PersonDetected,
                motionDetected = e.Message.MotionDetected,
                doorbellRing = e.Message.DoorbellRing
            });
        };
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
