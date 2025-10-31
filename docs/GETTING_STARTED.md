# Getting Started with MostlyLucid.EufySecurity

This guide will help you get up and running with MostlyLucid.EufySecurity quickly.

## Prerequisites

- .NET 8.0 or later
- Active Eufy Security account
- At least one Eufy Security device registered in your account

## Installation

### Via NuGet Package Manager Console
```powershell
Install-Package EufySecurity
```

### Via .NET CLI
```bash
dotnet add package EufySecurity
```

### Via PackageReference
```xml
<PackageReference Include="EufySecurity" Version="1.0.0" />
```

## Basic Setup

### 1. Create a Configuration

```csharp
using EufySecurity;
using EufySecurity.Common;

var config = new EufySecurityConfig
{
    Username = "your-email@example.com",
    Password = "your-password",
    Country = "US", // Must match your Eufy app setting
    Language = "en"
};
```

### 2. Initialize the Client

```csharp
using var client = new EufySecurityClient(config);
await client.ConnectAsync();
```

### 3. Access Your Devices

```csharp
// Get all stations (hubs)
var stations = client.GetStations();
foreach (var station in stations.Values)
{
    Console.WriteLine($"Station: {station.Name} ({station.SerialNumber})");
}

// Get all devices
var devices = client.GetDevices();
foreach (var device in devices.Values)
{
    Console.WriteLine($"Device: {device.Name} ({device.SerialNumber})");
}
```

## Common Tasks

### Start a Livestream

```csharp
using EufySecurity.Events;

// Subscribe to livestream events
client.LivestreamStarted += (sender, e) =>
{
    Console.WriteLine($"Livestream started: {e.Device.Name}");
    Console.WriteLine($"Video: {e.Metadata.VideoCodec} {e.Metadata.VideoWidth}x{e.Metadata.VideoHeight}");
};

client.LivestreamDataReceived += (sender, e) =>
{
    // Process video/audio data
    if (e.IsVideo)
    {
        // Handle video frame
        Console.WriteLine($"Received video frame: {e.Data.Length} bytes");
    }
    else
    {
        // Handle audio frame
        Console.WriteLine($"Received audio frame: {e.Data.Length} bytes");
    }
};

// Start the stream
var camera = client.GetDevice("CAMERA_SERIAL");
if (camera != null)
{
    await client.StartLivestreamAsync(camera.SerialNumber);

    // Stream for 30 seconds
    await Task.Delay(TimeSpan.FromSeconds(30));

    // Stop the stream
    await client.StopLivestreamAsync(camera.SerialNumber);
}
```

### Change Guard Mode

```csharp
// Set station to Away mode
await client.SetGuardModeAsync("STATION_SERIAL", GuardMode.Away);

// Set station to Home mode
await client.SetGuardModeAsync("STATION_SERIAL", GuardMode.Home);

// Disarm the station
await client.SetGuardModeAsync("STATION_SERIAL", GuardMode.Disarmed);
```

### Listen for Push Notifications

```csharp
client.PushNotificationReceived += (sender, e) =>
{
    Console.WriteLine($"Push notification: {e.Message.Type}");
    Console.WriteLine($"Device: {e.Message.DeviceSerial}");
    Console.WriteLine($"Title: {e.Message.Title}");
    Console.WriteLine($"Message: {e.Message.Message}");

    // Check notification type
    if (e.Message.MotionDetected == true)
    {
        Console.WriteLine("Motion detected!");
    }

    if (e.Message.PersonDetected == true)
    {
        Console.WriteLine("Person detected!");
    }

    if (e.Message.DoorbellRing == true)
    {
        Console.WriteLine("Doorbell pressed!");
    }
};
```

### Monitor Device Changes

```csharp
client.DeviceAdded += (sender, e) =>
{
    Console.WriteLine($"New device discovered: {e.Device.Name}");
};

client.StationAdded += (sender, e) =>
{
    Console.WriteLine($"New station discovered: {e.Station.Name}");
};
```

## Advanced Configuration

### Custom Logging

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug);
});

var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    Logger = loggerFactory.CreateLogger<EufySecurityClient>(),
    Logging = new LoggingConfig
    {
        Level = LogLevel.Information
    }
};
```

### Persistent Data Storage

```csharp
var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    PersistentDataPath = "./eufy-data" // Store credentials and state
};
```

### Custom Polling Interval

```csharp
var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    PollingIntervalMinutes = 5, // Poll cloud every 5 minutes
};
```

### Disable Automatic Polling

```csharp
var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    DisableAutomaticCloudPolling = true
};

// Manually refresh when needed
await client.RefreshDevicesAsync();
```

## Error Handling

```csharp
try
{
    using var client = new EufySecurityClient(config);
    await client.ConnectAsync();
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (DeviceNotFoundException ex)
{
    Console.WriteLine($"Device not found: {ex.DeviceSerial}");
}
catch (P2PConnectionException ex)
{
    Console.WriteLine($"P2P connection failed: {ex.Message}");
}
catch (EufySecurityException ex)
{
    Console.WriteLine($"Eufy error: {ex.Message}");
}
```

## Best Practices

1. **Always dispose of the client** when done using `using` statement or call `Dispose()`
2. **Handle exceptions** gracefully, especially during authentication and P2P connections
3. **Use CancellationTokens** for long-running operations
4. **Store credentials securely** - never hard-code them in production
5. **Respect API rate limits** - don't poll too frequently
6. **Test with one device first** before scaling to multiple devices

## Troubleshooting

### Connection Issues
- Ensure your country setting matches the Eufy app
- Check your network connection
- Verify credentials are correct

### P2P Connection Fails
- Ensure devices are online
- Check if you're on the same network for local connections
- Try remote P2P connection type

### No Devices Found
- Wait a few seconds and call `RefreshDevicesAsync()`
- Check if devices are registered in the Eufy app
- Ensure API credentials are correct

## Next Steps

- Read the [API Documentation](API.md)
- Check out [Examples](../examples/)
- Learn about [Advanced Features](ADVANCED.md)
- Explore [Device Types](DEVICES.md)
