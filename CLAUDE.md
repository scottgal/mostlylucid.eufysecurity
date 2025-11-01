# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MostlyLucid.EufySecurity is a .NET client library for controlling Eufy Security devices. It's a C# port of the TypeScript [eufy-security-client](https://github.com/bropat/eufy-security-client) library, providing communication with Eufy cloud servers and local/remote stations over P2P.

**Target Framework**: .NET 8.0 (requires SDK 9.0.0 with rollForward)

## Building and Testing

```bash
# Restore and build entire solution
dotnet restore
dotnet build

# Build specific project
dotnet build src/MostlyLucid.EufySecurity/MostlyLucid.EufySecurity.csproj

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/MostlyLucid.EufySecurity.Tests/MostlyLucid.EufySecurity.Tests.csproj

# Run single test class
dotnet test --filter FullyQualifiedName~EufySecurityClientTests

# Run single test method
dotnet test --filter FullyQualifiedName~EufySecurityClientTests.ConnectAsync_ShouldAuthenticateAndLoadDevices

# Run demo application
dotnet run --project examples/MostlyLucid.EufySecurity.Demo/MostlyLucid.EufySecurity.Demo.csproj

# Create NuGet package
dotnet pack src/MostlyLucid.EufySecurity/MostlyLucid.EufySecurity.csproj -c Release
```

## Architecture

The library is organized into four main subsystems coordinated by `EufySecurityClient`:

### 1. HTTP Layer (`Http/`)
- **HttpApiClient**: Handles all Eufy cloud API communication
- Authenticates with Eufy servers using username/password
- Fetches device metadata, stations, and account info
- Uses custom HTTP headers to mimic official Eufy mobile app
- Implements token-based authentication with expiration handling

### 2. P2P Layer (`P2P/`)
- **P2PClient**: Manages direct peer-to-peer UDP connections to stations
- Enables local and remote station communication
- Handles livestream data transmission
- Implements connection types: Quickest, Local Only, Prefer Local
- Note: Current implementation contains placeholder logic for UDP/P2P protocol (requires protocol buffer implementation)

### 3. Push Service (`Push/`)
- **PushNotificationService**: Firebase Cloud Messaging integration
- Receives real-time notifications for device events
- Note: Current implementation is a stub requiring FCM integration

### 4. MQTT (Future)
- Protocol support planned for specific device types

### Core Components

**Devices** (`Devices/`):
- Base `Device` class with property change tracking
- Specialized classes: `Camera`, `Doorbell`, `GenericCamera`
- Each device belongs to a station via `StationSerialNumber`
- Property-based architecture using dictionary storage

**Stations** (`Stations/`):
- `Station` class represents HomeBase/Hub devices
- Manages connected devices
- Tracks guard mode states
- Handles P2P connection state

**Common** (`Common/`):
- `EufySecurityConfig`: Configuration with flexible password options (Password/AppPassword/Pin)
- `DeviceType`: Enum for all supported device types
- `GuardMode`: Station security modes (Away, Home, Off, Schedule, Geofence, Disarmed)
- Exception types for different failure scenarios

**Events** (`Events/`):
- Event-driven architecture throughout
- Event args for device/station changes, livestream, push notifications

### Client Flow

1. **Initialization**: Create `EufySecurityClient` with `EufySecurityConfig`
2. **Connection**: `ConnectAsync()` authenticates with HTTP API and loads devices
3. **Discovery**: Devices and stations are cached in concurrent dictionaries
4. **P2P Setup**: Optional automatic connection to stations based on config
5. **Polling**: Automatic cloud polling at configured intervals (unless disabled)
6. **Events**: Subscribe to events for real-time updates

## Configuration Notes

**Password Options**: `EufySecurityConfig` supports three password fields for backward compatibility:
- `Password`: Traditional account password
- `AppPassword`: Eufy-generated PIN/app password
- `Pin`: Alias for AppPassword
- Resolution order: Password → AppPassword → Pin (first non-null wins)

**Country Setting**: Must match the country configured in the Eufy mobile app, or authentication will fail.

**P2P Connection Types**:
- `Quickest`: Tries local first, falls back to remote
- `LocalOnly`: Only local network connections
- `PreferLocal`: Prefers local but allows remote

## Testing

Test framework: **xUnit** with **Moq** and **FluentAssertions**

Test organization:
- `DeviceTests.cs`: Device class behavior
- `EufySecurityClientTests.cs`: Client initialization and operations
- `EufySecurityClientBehaviorTests.cs`: Integration-style behavior tests
- `EufySecurityConfigTests.cs`: Configuration validation
- `EventArgsTests.cs`: Event argument classes

## Important Implementation Details

1. **Concurrent Collections**: `_stations` and `_devices` use `ConcurrentDictionary<string, T>` for thread safety
2. **Disposal Pattern**: Client implements `IDisposable` - always use `using` statements
3. **Async Throughout**: All I/O operations are async with `CancellationToken` support
4. **Property Storage**: Devices and Stations use dictionary-based property storage with change notifications
5. **Logging**: Optional `ILogger` injection throughout for diagnostics
6. **Timer Management**: Polling timer in `EufySecurityClient` must be properly disposed

## Protocol Implementation Status

This is a port-in-progress. Key areas marked as placeholders:
- P2P protocol handshake and encryption (see `P2PClient.ConnectAsync`)
- Firebase Cloud Messaging integration (see `PushNotificationService.StartAsync`)
- Protocol buffer definitions for P2P messages
- UDP lookup and NAT traversal logic

Refer to original TypeScript library for complete protocol implementations.

## Dependencies

Key packages:
- **BouncyCastle.Cryptography**: ECDH key exchange and encryption
- **Google.Protobuf**: Protocol buffer serialization for P2P
- **MQTTnet**: MQTT protocol support
- **System.Reactive**: Event stream handling
- **Nito.AsyncEx**: Async synchronization primitives