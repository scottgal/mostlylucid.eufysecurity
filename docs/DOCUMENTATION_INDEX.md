# MostlyLucid.EufySecurity - Documentation Index

**Complete documentation for the MostlyLucid.EufySecurity .NET library**

## 📚 Quick Links

| Document | Description | For |
|----------|-------------|-----|
| [Getting Started](./GETTING_STARTED.md) | Quick start guide | New users |
| [Security Guide](./SECURITY.md) | Security audit & best practices | All users |
| [2FA Authentication](./2FA-AUTHENTICATION.md) | Two-factor authentication guide | All users |
| [Code Review](./CODE_REVIEW_REPORT.md) | Comprehensive code review | Developers |
| [Main README](../README.md) | Project overview | Everyone |

## 📖 Documentation Categories

### Getting Started
- **[Getting Started Guide](./GETTING_STARTED.md)**
  - Installation instructions
  - Basic usage examples
  - Configuration options
  - Common tasks
  - Error handling
  - Best practices

### Security
- **[Security Guide](./SECURITY.md)** ⭐ IMPORTANT
  - Security audit results
  - Credential management
  - Password encryption details
  - Logging security
  - Network security
  - Best practices for production
  - Compliance considerations

- **[2FA Authentication](./2FA-AUTHENTICATION.md)**
  - Two-factor authentication setup
  - Handling verification codes
  - MFA flow implementation

### Development
- **[Code Review Report](./CODE_REVIEW_REPORT.md)**
  - Comprehensive static analysis
  - Architecture review
  - Security assessment
  - Test coverage analysis
  - Best practices validation
  - Recommendations

## 🏗️ Architecture Overview

### System Components

```
┌─────────────────────────────────────────┐
│      EufySecurityClient (Main)          │
│  - Orchestrates all subsystems          │
│  - Manages device/station lifecycle     │
│  - Event aggregation                    │
└───────────┬─────────────────────────────┘
            │
     ┌──────┴──────┬──────────────┬────────────┐
     │             │              │            │
┌────▼────┐  ┌────▼────┐   ┌─────▼─────┐  ┌──▼───┐
│  HTTP   │  │   P2P   │   │   Push    │  │ MQTT │
│   API   │  │ Client  │   │  Service  │  │(Plan)│
└─────────┘  └─────────┘   └───────────┘  └──────┘
     │             │              │
     │             │              │
┌────▼─────────────▼──────────────▼────────────┐
│           Devices & Stations                  │
│  - Property management                        │
│  - Change tracking                            │
│  - Event notifications                        │
└───────────────────────────────────────────────┘
```

### Key Features

#### HTTP Layer
- Eufy cloud API communication
- ECDH key exchange + AES-256 encryption
- 2FA/MFA support
- Token management
- Device/station metadata retrieval

#### P2P Layer (Partial Implementation)
- Direct UDP connection to stations
- Local and remote connectivity
- Livestream data transmission
- Command execution (guard mode, etc.)
- **Status:** Placeholder - requires protocol implementation

#### Push Notifications (Stub)
- Firebase Cloud Messaging integration
- Real-time event notifications
- **Status:** Stub - requires FCM setup

#### Core Features
- Device discovery and management
- Property change tracking
- Event-driven architecture
- Concurrent collections for thread safety
- Proper async/await patterns

## 🔒 Security Highlights

### ✅ Secure by Default
- HTTPS-only communication
- No hardcoded credentials
- Strong password encryption (ECDH + AES-256)
- Secure token management
- No sensitive data in logs (after fixes)

### ⚠️ User Responsibilities
- Store credentials securely (User Secrets, Key Vault)
- Use appropriate log levels in production
- Enable 2FA on Eufy accounts
- Keep dependencies updated
- Review security guide before deployment

## 🛠️ API Reference

### Core Classes

#### EufySecurityClient
Main entry point for the library.

```csharp
public class EufySecurityClient : IDisposable
{
    // Constructor
    public EufySecurityClient(EufySecurityConfig config);

    // Connection
    public Task<AuthenticationResult> ConnectAsync(string? verifyCode = null, CancellationToken ct = default);
    public Task DisconnectAsync(CancellationToken ct = default);

    // Device Management
    public IReadOnlyDictionary<string, Device> GetDevices();
    public Device? GetDevice(string serialNumber);
    public IReadOnlyDictionary<string, Station> GetStations();
    public Station? GetStation(string serialNumber);
    public Task RefreshDevicesAsync(CancellationToken ct = default);

    // P2P Operations
    public Task ConnectToStationAsync(string stationSerial, CancellationToken ct = default);
    public Task StartLivestreamAsync(string deviceSerial, CancellationToken ct = default);
    public Task StopLivestreamAsync(string deviceSerial, CancellationToken ct = default);
    public Task SetGuardModeAsync(string stationSerial, GuardMode mode, CancellationToken ct = default);

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
}
```

#### EufySecurityConfig
Configuration for the client.

```csharp
public class EufySecurityConfig
{
    // Required
    public required string Username { get; init; }
    public string? Password { get; init; }  // Or use AppPassword/Pin

    // Optional
    public string Country { get; init; } = "US";
    public string Language { get; init; } = "en";
    public string TrustedDeviceName { get; init; } = "EufySecurity.NET";
    public P2PConnectionType P2PConnectionSetup { get; init; } = Quickest;
    public int PollingIntervalMinutes { get; init; } = 10;
    public bool DisableAutomaticCloudPolling { get; init; } = false;
    public ILogger? Logger { get; init; }
    // ... more options
}
```

#### Device & Station
Base classes for all devices and stations.

```csharp
public class Device
{
    public string SerialNumber { get; }
    public string Name { get; }
    public string Model { get; }
    public DeviceType DeviceType { get; }
    public string StationSerialNumber { get; }
    public int BatteryLevel { get; }
    // ... more properties

    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;
}

public class Station
{
    public string SerialNumber { get; }
    public string Name { get; }
    public string Model { get; }
    public bool IsConnected { get; }
    public GuardMode GuardMode { get; }
    public IReadOnlyDictionary<string, Device> Devices { get; }
    // ... more properties
}
```

### Exception Types

All exceptions inherit from `EufySecurityException`:

- `AuthenticationException` - Authentication failures
- `DeviceNotFoundException` - Device not found
- `StationNotFoundException` - Station not found
- `P2PConnectionException` - P2P connection issues
- `LivestreamException` - Livestream failures
- `ApiException` - Eufy API errors
- `ReadOnlyPropertyException` - Property modification attempt
- `InvalidPropertyException` - Invalid property access
- `NotSupportedException` - Unsupported operation

## 💡 Usage Examples

### Basic Authentication
```csharp
var config = new EufySecurityConfig
{
    Username = "user@example.com",
    Password = "password",
    Country = "US"
};

using var client = new EufySecurityClient(config);
var result = await client.ConnectAsync();

if (result.Success)
{
    var devices = client.GetDevices();
    Console.WriteLine($"Found {devices.Count} devices");
}
```

### 2FA Flow
```csharp
var result = await client.ConnectAsync();

if (result.RequiresTwoFactor)
{
    Console.Write("Enter 2FA code: ");
    var code = Console.ReadLine();
    result = await client.ConnectAsync(verifyCode: code);
}
```

### Event Subscription
```csharp
client.DeviceAdded += (sender, e) =>
{
    Console.WriteLine($"Device added: {e.Device.Name}");
};

client.PushNotificationReceived += (sender, e) =>
{
    if (e.Message.MotionDetected)
    {
        Console.WriteLine($"Motion on {e.Message.DeviceSerial}!");
    }
};
```

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~EufySecurityClientTests"
```

### Test Coverage
11 test files covering:
- Client behavior
- Configuration validation
- Device/Station management
- HTTP API mocking
- P2P client operations
- Event handling
- Exception scenarios

## 🔧 Development

### Build
```bash
dotnet restore
dotnet build
```

### Create Package
```bash
dotnet pack -c Release
```

### Run Demo App
```bash
cd examples/MostlyLucid.EufySecurity.Demo
dotnet run
```

Demo app available at: `https://localhost:5001`

## 📦 Dependencies

### Production
- BouncyCastle.Cryptography (2.4.0) - Crypto operations
- Google.Protobuf (3.27.0) - P2P protocol
- MQTTnet (4.3.7) - MQTT support
- System.Reactive (6.0.1) - Event streams
- Microsoft.Extensions.Logging (8.0.1) - Logging

### Development
- xUnit (2.9.0) - Testing
- Moq (4.20.70) - Mocking
- FluentAssertions (6.12.0) - Assertions

## 🐛 Troubleshooting

### Common Issues

#### Authentication Fails
- Verify username/password are correct
- Check country matches Eufy app setting
- Enable 2FA if required
- Check network connectivity

#### No Devices Found
- Call `RefreshDevicesAsync()`
- Verify devices are registered in Eufy app
- Check account has devices

#### P2P Connection Fails
- Ensure station is online
- Check network connectivity
- Try different P2P connection type
- Note: P2P protocol is placeholder implementation

#### Logging Too Verbose
```csharp
// Set appropriate log level
config.Logger = loggerFactory.CreateLogger<EufySecurityClient>();
// In appsettings.json:
{
  "Logging": {
    "LogLevel": {
      "MostlyLucid.EufySecurity": "Warning"  // Not Debug!
    }
  }
}
```

## 🔐 Security Checklist

Before deploying to production:

- [ ] Credentials stored securely (not in code)
- [ ] Logging set to Warning or higher
- [ ] HTTPS enforced (default behavior)
- [ ] 2FA enabled on Eufy account
- [ ] Dependencies updated
- [ ] Error handling doesn't expose sensitive data
- [ ] Security guide reviewed

## 📝 License

Public Domain (Unlicense)

## 🤝 Contributing

See main README for contribution guidelines.

## 📧 Support

- Issues: [GitHub Issues](https://github.com/scottgal/mostlylucid.eufysecurity/issues)
- Discussions: [GitHub Discussions](https://github.com/scottgal/mostlylucid.eufysecurity/discussions)

---

**Documentation Last Updated:** 2025-11-06
**Library Version:** 1.0.0
