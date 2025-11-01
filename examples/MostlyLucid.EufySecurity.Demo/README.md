# MostlyLucid.EufySecurity Demo Application

A modern ASP.NET Core web application showcasing the MostlyLucid.EufySecurity library with a beautiful UI and complete 2FA flow.

## Features

- 🎨 **Beautiful UI** - Tailwind CSS + DaisyUI themed interface
- 🔐 **2FA Authentication** - Full two-factor authentication flow with email verification
- 🚀 **Interactive Login** - Alpine.js powered reactive forms
- 📡 **REST API** - Full CRUD operations for devices and stations
- 🔄 **SignalR Hub** - Real-time event streaming
- 📖 **Swagger UI** - Interactive API documentation
- ❤️ **Health Checks** - Monitor service status
- 🏗️ **Modern C#** - Primary constructors, nullable reference types
- 🎯 **CORS Enabled** - Ready for frontend integration

## Quick Start

### 1. Run the Application

```bash
cd examples/MostlyLucid.EufySecurity.Demo
dotnet run
```

### 2. Open Your Browser

Navigate to `https://localhost:5001` (or `http://localhost:5000`)

You'll be automatically redirected to the beautiful login page!

### 3. Login with 2FA

**Step 1: Enter Your Credentials**
- Email address (your Eufy account email)
- Password (your Eufy account password)
- Country (must match your Eufy app - e.g., UK, US, DE)
- Language (e.g., en, de, fr)

**Step 2: Verify Your Email (if 2FA is enabled)**
- Check your email for a 6-digit verification code
- Enter the code on the verification page
- The code expires in 5 minutes
- You can request a new code if needed

**Step 3: You're In!**
- After successful authentication, you'll be redirected to the Swagger API documentation
- Your Eufy devices are now accessible via the API

### 4. Access Points

- **Login Page**: https://localhost:5001/Auth/Login
- **Swagger UI**: https://localhost:5001/swagger
- **SignalR Hub**: https://localhost:5001/hubs/events
- **Health Check**: https://localhost:5001/health

## Configuration (Optional)

You can optionally pre-configure credentials in `appsettings.json`, but the UI login is the recommended approach:

```json
{
  "Eufy": {
    "PollingIntervalMinutes": 10
  }
}
```

**Note:** Never commit credentials to source control. The interactive login is more secure.

## API Endpoints

### Devices

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/devices` | Get all devices |
| GET | `/api/devices/{serialNumber}` | Get specific device |
| POST | `/api/devices/refresh` | Refresh device list from cloud |

### Stations

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/stations` | Get all stations |
| GET | `/api/stations/{serialNumber}` | Get specific station |
| POST | `/api/stations/{serialNumber}/connect` | Connect to station via P2P |
| POST | `/api/stations/{serialNumber}/guard-mode` | Set guard mode |

### Livestream

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/livestream/start?deviceSerial={serial}` | Start livestream |
| POST | `/api/livestream/stop?deviceSerial={serial}` | Stop livestream |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Get service health status |

## SignalR Events

Connect to the SignalR hub at `/hubs/events` to receive real-time events:

### Client-to-Server Methods

- `SubscribeToDevice(string deviceSerial)` - Subscribe to device-specific events
- `UnsubscribeFromDevice(string deviceSerial)` - Unsubscribe from device events
- `SubscribeToStation(string stationSerial)` - Subscribe to station-specific events
- `UnsubscribeFromStation(string stationSerial)` - Unsubscribe from station events

### Server-to-Client Events

- `DeviceAdded` - New device discovered
- `StationAdded` - New station discovered
- `LivestreamStarted` - Livestream began
- `LivestreamStopped` - Livestream ended
- `PushNotification` - Push notification received

## Example Usage

### REST API (cURL)

```bash
# Get all devices
curl https://localhost:5001/api/devices

# Get specific device
curl https://localhost:5001/api/devices/DEVICE_SERIAL

# Set guard mode to Away
curl -X POST https://localhost:5001/api/stations/STATION_SERIAL/guard-mode \
  -H "Content-Type: application/json" \
  -d '{"mode":"Away"}'

# Start livestream
curl -X POST "https://localhost:5001/api/livestream/start?deviceSerial=DEVICE_SERIAL"

# Check health
curl https://localhost:5001/health
```

### SignalR Client (JavaScript)

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js"></script>
</head>
<body>
    <h1>EufySecurity Events</h1>
    <div id="events"></div>

    <script>
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:5001/hubs/events")
            .build();

        // Subscribe to events
        connection.on("DeviceAdded", (device) => {
            console.log("Device added:", device);
            document.getElementById("events").innerHTML +=
                `<p>Device Added: ${device.name} (${device.serialNumber})</p>`;
        });

        connection.on("PushNotification", (notification) => {
            console.log("Push notification:", notification);
            document.getElementById("events").innerHTML +=
                `<p>Notification: ${notification.type} - ${notification.message}</p>`;
        });

        connection.on("LivestreamStarted", (data) => {
            console.log("Livestream started:", data);
            document.getElementById("events").innerHTML +=
                `<p>Livestream started: ${data.deviceName}</p>`;
        });

        // Start connection
        connection.start()
            .then(() => {
                console.log("Connected to SignalR hub");
                // Subscribe to specific device
                connection.invoke("SubscribeToDevice", "DEVICE_SERIAL");
            })
            .catch(err => console.error("Error connecting:", err));
    </script>
</body>
</html>
```

### C# Client

```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/hubs/events")
    .Build();

connection.On<object>("DeviceAdded", (device) =>
{
    Console.WriteLine($"Device added: {device}");
});

connection.On<object>("PushNotification", (notification) =>
{
    Console.WriteLine($"Push notification: {notification}");
});

await connection.StartAsync();
Console.WriteLine("Connected to SignalR hub");

// Subscribe to device
await connection.InvokeAsync("SubscribeToDevice", "DEVICE_SERIAL");
```

## Configuration Options

### appsettings.json

```json
{
  "Eufy": {
    "Username": "your-email@example.com",
    "Password": "your-password",
    "Country": "US",              // Must match Eufy app setting
    "Language": "en",
    "PollingIntervalMinutes": 10  // How often to poll cloud for updates
  }
}
```

### Environment Variables

For production, use environment variables:

```bash
export Eufy__Username="your-email@example.com"
export Eufy__Password="your-password"
export Eufy__Country="US"
```

### User Secrets (Development)

```bash
dotnet user-secrets set "Eufy:Username" "your-email@example.com"
dotnet user-secrets set "Eufy:Password" "your-password"
```

## Architecture

```
MostlyLucid.EufySecurity.Demo/
├── Controllers/
│   ├── DevicesController.cs      # Device management endpoints
│   ├── StationsController.cs     # Station management endpoints
│   └── LivestreamController.cs   # Livestream control endpoints
├── Hubs/
│   └── EufyEventsHub.cs          # SignalR hub for real-time events
├── Services/
│   └── EufySecurityHostedService.cs  # Background service managing EufySecurity client
├── Program.cs                    # Application startup and configuration
└── appsettings.json             # Configuration

```

### Key Components

1. **EufySecurityHostedService** - Background service that:
   - Initializes and manages the EufySecurity client
   - Subscribes to all events
   - Forwards events to SignalR hub
   - Runs for the lifetime of the application

2. **Controllers** - REST API endpoints for:
   - Querying devices and stations
   - Controlling guard modes
   - Starting/stopping livestreams
   - Refreshing data

3. **EufyEventsHub** - SignalR hub that:
   - Broadcasts events to connected clients
   - Supports device/station-specific subscriptions
   - Provides real-time updates

4. **Health Checks** - Monitor:
   - EufySecurity client connection status
   - API availability

## Deployment

### Docker

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MostlyLucid.EufySecurity.Demo.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MostlyLucid.EufySecurity.Demo.dll"]
```

Build and run:

```bash
docker build -t eufysecurity-demo .
docker run -p 5000:80 \
  -e Eufy__Username="your-email" \
  -e Eufy__Password="your-password" \
  eufysecurity-demo
```

### Azure App Service

```bash
az webapp up --name eufysecurity-demo --runtime "DOTNET:8.0"
```

Configure app settings in Azure Portal:
- `Eufy__Username`
- `Eufy__Password`
- `Eufy__Country`

## Observer.AI Integration

This demo is ready for Observer.AI integration! The API provides:

- REST endpoints for synchronous operations
- SignalR for real-time event streaming
- Health checks for monitoring
- Swagger for API discovery

## Troubleshooting

### Connection Issues

**Problem**: "EufySecurity client not connected"
**Solution**: Check your credentials in `appsettings.json` and ensure the Eufy service can connect to the internet.

### No Devices Found

**Problem**: API returns empty device list
**Solution**:
1. Verify credentials are correct
2. Ensure country setting matches Eufy app
3. Call `/api/devices/refresh` to force refresh

### SignalR Connection Failed

**Problem**: Cannot connect to SignalR hub
**Solution**:
1. Ensure CORS is configured properly
2. Check firewall settings
3. Verify URL is correct (https://localhost:5001/hubs/events)

## Development

```bash
# Build
dotnet build

# Run
dotnet run

# Watch mode (auto-reload)
dotnet watch run

# Publish
dotnet publish -c Release
```

## License

This project is released into the public domain under the Unlicense license.

## Credits

- Based on [eufy-security-client](https://github.com/bropat/eufy-security-client) by bropat
- C# port: MostlyLucid.EufySecurity
