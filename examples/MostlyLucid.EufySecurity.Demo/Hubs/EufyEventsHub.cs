using Microsoft.AspNetCore.SignalR;

namespace MostlyLucid.EufySecurity.Demo.Hubs;

/// <summary>
/// SignalR hub for real-time Eufy Security events
/// </summary>
public class EufyEventsHub : Hub
{
    private readonly ILogger<EufyEventsHub> _logger;

    public EufyEventsHub(ILogger<EufyEventsHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to specific device events
    /// </summary>
    public async Task SubscribeToDevice(string deviceSerial)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceSerial}");
        _logger.LogInformation("Client {ConnectionId} subscribed to device {DeviceSerial}",
            Context.ConnectionId, deviceSerial);
    }

    /// <summary>
    /// Unsubscribe from specific device events
    /// </summary>
    public async Task UnsubscribeFromDevice(string deviceSerial)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceSerial}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from device {DeviceSerial}",
            Context.ConnectionId, deviceSerial);
    }

    /// <summary>
    /// Subscribe to specific station events
    /// </summary>
    public async Task SubscribeToStation(string stationSerial)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"station_{stationSerial}");
        _logger.LogInformation("Client {ConnectionId} subscribed to station {StationSerial}",
            Context.ConnectionId, stationSerial);
    }

    /// <summary>
    /// Unsubscribe from specific station events
    /// </summary>
    public async Task UnsubscribeFromStation(string stationSerial)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"station_{stationSerial}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from station {StationSerial}",
            Context.ConnectionId, stationSerial);
    }
}
