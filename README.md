# MostlyLucid.EufySecurity

[![NuGet](https://img.shields.io/nuget/v/MostlyLucid.EufySecurity.svg)](https://www.nuget.org/packages/MostlyLucid.EufySecurity/)
[![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)](http://unlicense.org/)

A .NET client library for controlling Eufy Security devices by connecting to Eufy cloud servers and local/remote stations over P2P.

> **🙏 Credits:** This project is a C# port of the excellent [eufy-security-client](https://github.com/bropat/eufy-security-client) TypeScript library by [bropat](https://github.com/bropat). All credit for the original design, protocol reverse-engineering, and implementation goes to bropat and the contributors of the original project. This .NET port was created with assistance from Claude AI to bring this functionality to the .NET ecosystem.

## Features

- 🔐 Secure authentication with Eufy cloud
- 📡 P2P communication with stations (local and remote)
- 📹 Livestream support for cameras and doorbells
- 🔔 Push notification support via FCM
- 🏠 Support for multiple device types:
  - Cameras (Indoor, Outdoor, Solo, Floodlight)
  - Doorbells (Battery, Wired)
  - Locks
  - Sensors
  - Stations/Hubs (HomeBase)
- ⚡ Event-driven architecture
- 🔄 Automatic device discovery and updates
- 🎯 Strongly-typed API

## Installation

```bash
dotnet add package MostlyLucid.EufySecurity
```

Or via NuGet Package Manager:

```powershell
Install-Package MostlyLucid.EufySecurity
```

## Quick Start

```csharp
using MostlyLucid.EufySecurity;
using MostlyLucid.EufySecurity.Common;

// Configure the client
var config = new EufySecurityConfig
{
    Username = "your-email@example.com",
    Password = "your-password",
    Country = "US",
    Language = "en"
};

// Create and connect the client
using var client = new EufySecurityClient(config);
var authResult = await client.ConnectAsync();

if (authResult.RequiresTwoFactor)
{
    // Check email for verification code
    Console.Write("Enter 2FA code: ");
    var code = Console.ReadLine();
    authResult = await client.ConnectAsync(code);
}

// Get all devices
var devices = client.GetDevices();
foreach (var device in devices.Values)
{
    Console.WriteLine($"Device: {device.Name} ({device.SerialNumber})");
}

// Start livestream from a camera
await client.StartLivestreamAsync(cameraSerial);

// Set guard mode on a station
await client.SetGuardModeAsync(stationSerial, GuardMode.Away);

// Listen for events
client.DeviceAdded += (sender, e) =>
{
    Console.WriteLine($"Device added: {e.Device.Name}");
};

client.PushNotificationReceived += (sender, e) =>
{
    Console.WriteLine($"Push notification: {e.Message.Type}");
};
```

## Configuration

### EufySecurityConfig Options

```csharp
var config = new EufySecurityConfig
{
    // Required
    Username = "your-email",
    Password = "your-password",

    // Optional
    Country = "US",                          // Must match Eufy app setting
    Language = "en",
    TrustedDeviceName = "MyApp",
    PollingIntervalMinutes = 10,             // Cloud polling interval
    P2PConnectionSetup = P2PConnectionType.Quickest,
    AcceptInvitations = true,
    DisableAutomaticCloudPolling = false,
    PersistentDataPath = "./eufy-data",     // For storing credentials

    // Logging
    Logger = loggerInstance,
    Logging = new LoggingConfig
    {
        Level = LogLevel.Information
    }
};
```

## Supported Devices

### Cameras
- EufyCam (1/E, 2/2C, 2 Pro, 3/3C)
- Indoor Cam (Pan & Tilt, 1080p, 2K, S350)
- SoloCam (E20, E40, L20, L40, S40)
- Floodlight Cam (8422, 8423, 8424, 8425, E340)
- Outdoor Cam (Pan & Tilt)

### Doorbells
- Battery Doorbell (1/2, Plus, E340)
- Wired Doorbell
- Doorbell Dual

### Locks
- Smart Lock (Touch & WiFi, R10, R20)
- Video Smart Lock
- Retrofit Smart Lock

### Sensors & Others
- Entry Sensor
- Motion Sensor
- Keypad
- Smart Safe
- Smart Drop

## Events

The library provides comprehensive events for monitoring device state changes:

```csharp
// Device/Station management
client.DeviceAdded += OnDeviceAdded;
client.DeviceRemoved += OnDeviceRemoved;
client.StationAdded += OnStationAdded;
client.StationRemoved += OnStationRemoved;

// Livestream events
client.LivestreamStarted += OnLivestreamStarted;
client.LivestreamStopped += OnLivestreamStopped;
client.LivestreamDataReceived += OnLivestreamData;

// Push notifications
client.PushNotificationReceived += OnPushNotification;

// Guard mode changes
client.GuardModeChanged += OnGuardModeChanged;
```

## Advanced Usage

### Custom Device Configuration

```csharp
var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    Devices = new Dictionary<string, DeviceConfig>
    {
        ["DEVICE_SERIAL"] = new DeviceConfig
        {
            SimultaneousDetections = true
        }
    },
    Stations = new Dictionary<string, StationConfig>
    {
        ["STATION_SERIAL"] = new StationConfig
        {
            IpAddress = "192.168.1.100",
            P2PPort = 32108
        }
    }
};
```

### Manual Device Refresh

```csharp
// Disable automatic polling
var config = new EufySecurityConfig
{
    Username = "email",
    Password = "password",
    DisableAutomaticCloudPolling = true
};

// Manually refresh when needed
await client.RefreshDevicesAsync();
```

### P2P Connection Management

```csharp
// Connect to a specific station
await client.ConnectToStationAsync(stationSerial);

// Check connection status
var station = client.GetStation(stationSerial);
if (station?.IsConnected == true)
{
    // Station is connected via P2P
}
```

## Architecture

The library is organized into four main subsystems:

1. **HTTP Layer** - Eufy Cloud API communication
2. **P2P Layer** - Direct peer-to-peer device communication
3. **Push Service** - Firebase Cloud Messaging for notifications
4. **MQTT** - Protocol support for specific devices

All subsystems are coordinated by the main `EufySecurityClient` class.

## Requirements

- .NET 8.0 or higher
- Active Eufy Security account
- Network access to Eufy cloud servers

## Important Notes

- **Country Setting**: Must match the country set in your Eufy Security mobile app
- **2FA**: Full two-factor authentication support with email verification codes - see [2FA Documentation](docs/2FA-AUTHENTICATION.md)
- **P2P**: Local P2P connections work only when on the same network as your devices
- **Rate Limiting**: The library implements automatic request throttling to avoid API limits

## Building from Source

```bash
git clone https://github.com/eufy-security/MostlyLucid.EufySecurity.git
cd MostlyLucid.EufySecurity
dotnet build
dotnet test
```

## Credits & Acknowledgments

### Original Library
This project is a C# port of the excellent [eufy-security-client](https://github.com/bropat/eufy-security-client) TypeScript library created and maintained by [bropat](https://github.com/bropat).

**All credit for the original protocol reverse-engineering, design, and implementation belongs to bropat and the contributors of the original TypeScript project.** Without their incredible work, this .NET port would not exist.

Special thanks to:
- **bropat** - Original author and maintainer of eufy-security-client
- The eufy-security-client **contributors** - For continuous improvements
- **Claude (Anthropic AI)** - For assistance in creating this C# port

### Disclaimer
**This project is not affiliated with, endorsed by, or connected to Anker Innovations or Eufy Security in any way.** This is an independent, community-driven project maintained in spare time.

## License

This is free and unencumbered software released into the public domain. See [UNLICENSE](UNLICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

- 📝 [Documentation](https://github.com/eufy-security/MostlyLucid.EufySecurity/wiki)
- 🐛 [Issue Tracker](https://github.com/eufy-security/MostlyLucid.EufySecurity/issues)
- 💬 [Discussions](https://github.com/eufy-security/MostlyLucid.EufySecurity/discussions)

## Changelog

### 1.0.0 (Initial Release)

- Initial port from TypeScript library
- Core HTTP API implementation
- P2P protocol support
- Push notification service
- Support for major device types
- Event-driven architecture
- Comprehensive configuration options
