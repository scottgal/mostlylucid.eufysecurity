using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MostlyLucid.EufySecurity.Telemetry;

/// <summary>
/// OpenTelemetry instrumentation for EufySecurity.NET
/// Provides distributed tracing and metrics for monitoring with Grafana, Prometheus, etc.
/// </summary>
public static class EufySecurityInstrumentation
{
    /// <summary>
    /// Service name for OpenTelemetry resource attributes
    /// </summary>
    public const string ServiceName = "EufySecurity.NET";

    /// <summary>
    /// Service version
    /// </summary>
    public const string ServiceVersion = "1.0.0";

    /// <summary>
    /// ActivitySource for distributed tracing
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Meter for metrics collection
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    // ==================== COUNTERS ====================

    /// <summary>
    /// Counter for authentication attempts
    /// Tags: result (success/failure), requires_2fa (true/false)
    /// </summary>
    public static readonly Counter<long> AuthenticationAttempts = Meter.CreateCounter<long>(
        name: "eufy.authentication.attempts",
        unit: "{attempt}",
        description: "Number of authentication attempts");

    /// <summary>
    /// Counter for authentication failures
    /// Tags: reason (invalid_credentials/network_error/2fa_failed/etc)
    /// </summary>
    public static readonly Counter<long> AuthenticationFailures = Meter.CreateCounter<long>(
        name: "eufy.authentication.failures",
        unit: "{failure}",
        description: "Number of authentication failures");

    /// <summary>
    /// Counter for HTTP API calls
    /// Tags: endpoint, method, status_code
    /// </summary>
    public static readonly Counter<long> HttpApiCalls = Meter.CreateCounter<long>(
        name: "eufy.http.api.calls",
        unit: "{call}",
        description: "Number of HTTP API calls to Eufy cloud");

    /// <summary>
    /// Counter for HTTP API errors
    /// Tags: endpoint, error_type, status_code
    /// </summary>
    public static readonly Counter<long> HttpApiErrors = Meter.CreateCounter<long>(
        name: "eufy.http.api.errors",
        unit: "{error}",
        description: "Number of HTTP API errors");

    /// <summary>
    /// Counter for P2P connection attempts
    /// Tags: connection_type (local/remote/quickest), result (success/failure)
    /// </summary>
    public static readonly Counter<long> P2PConnectionAttempts = Meter.CreateCounter<long>(
        name: "eufy.p2p.connection.attempts",
        unit: "{attempt}",
        description: "Number of P2P connection attempts");

    /// <summary>
    /// Counter for livestream sessions
    /// Tags: device_type, result (started/stopped/error)
    /// </summary>
    public static readonly Counter<long> LivestreamSessions = Meter.CreateCounter<long>(
        name: "eufy.livestream.sessions",
        unit: "{session}",
        description: "Number of livestream sessions");

    /// <summary>
    /// Counter for livestream data received
    /// Tags: data_type (video/audio), device_serial
    /// </summary>
    public static readonly Counter<long> LivestreamBytesReceived = Meter.CreateCounter<long>(
        name: "eufy.livestream.bytes_received",
        unit: "By",
        description: "Total bytes received from livestreams");

    /// <summary>
    /// Counter for push notifications received
    /// Tags: notification_type, device_type
    /// </summary>
    public static readonly Counter<long> PushNotificationsReceived = Meter.CreateCounter<long>(
        name: "eufy.push.notifications_received",
        unit: "{notification}",
        description: "Number of push notifications received");

    /// <summary>
    /// Counter for device refreshes
    /// Tags: trigger (automatic/manual)
    /// </summary>
    public static readonly Counter<long> DeviceRefreshes = Meter.CreateCounter<long>(
        name: "eufy.devices.refreshes",
        unit: "{refresh}",
        description: "Number of device list refreshes");

    /// <summary>
    /// Counter for errors by type
    /// Tags: error_type, operation
    /// </summary>
    public static readonly Counter<long> Errors = Meter.CreateCounter<long>(
        name: "eufy.errors",
        unit: "{error}",
        description: "Total errors by type");

    // ==================== HISTOGRAMS ====================

    /// <summary>
    /// Histogram for HTTP API call duration
    /// Tags: endpoint, method, status_code
    /// </summary>
    public static readonly Histogram<double> HttpApiDuration = Meter.CreateHistogram<double>(
        name: "eufy.http.api.duration",
        unit: "ms",
        description: "Duration of HTTP API calls");

    /// <summary>
    /// Histogram for authentication duration
    /// Tags: requires_2fa
    /// </summary>
    public static readonly Histogram<double> AuthenticationDuration = Meter.CreateHistogram<double>(
        name: "eufy.authentication.duration",
        unit: "ms",
        description: "Duration of authentication flow");

    /// <summary>
    /// Histogram for P2P connection establishment duration
    /// Tags: connection_type, result
    /// </summary>
    public static readonly Histogram<double> P2PConnectionDuration = Meter.CreateHistogram<double>(
        name: "eufy.p2p.connection.duration",
        unit: "ms",
        description: "Duration to establish P2P connections");

    /// <summary>
    /// Histogram for device refresh duration
    /// Tags: device_count
    /// </summary>
    public static readonly Histogram<double> DeviceRefreshDuration = Meter.CreateHistogram<double>(
        name: "eufy.devices.refresh.duration",
        unit: "ms",
        description: "Duration of device list refresh operations");

    /// <summary>
    /// Histogram for livestream frame processing time
    /// Tags: data_type (video/audio)
    /// </summary>
    public static readonly Histogram<double> LivestreamFrameProcessingTime = Meter.CreateHistogram<double>(
        name: "eufy.livestream.frame_processing_time",
        unit: "ms",
        description: "Time to process livestream frames");

    // ==================== GAUGES (Observable) ====================

    /// <summary>
    /// Observable gauge for number of connected devices
    /// </summary>
    private static int _connectedDevicesCount;
    public static readonly ObservableGauge<int> ConnectedDevices = Meter.CreateObservableGauge<int>(
        name: "eufy.devices.connected",
        observeValue: () => _connectedDevicesCount,
        unit: "{device}",
        description: "Number of currently connected devices");

    /// <summary>
    /// Observable gauge for number of stations
    /// </summary>
    private static int _stationsCount;
    public static readonly ObservableGauge<int> Stations = Meter.CreateObservableGauge<int>(
        name: "eufy.stations.count",
        observeValue: () => _stationsCount,
        unit: "{station}",
        description: "Number of stations (hubs)");

    /// <summary>
    /// Observable gauge for number of active livestreams
    /// </summary>
    private static int _activeLivestreamsCount;
    public static readonly ObservableGauge<int> ActiveLivestreams = Meter.CreateObservableGauge<int>(
        name: "eufy.livestream.active",
        observeValue: () => _activeLivestreamsCount,
        unit: "{stream}",
        description: "Number of active livestream sessions");

    /// <summary>
    /// Observable gauge for number of P2P connections
    /// </summary>
    private static int _activeP2PConnectionsCount;
    public static readonly ObservableGauge<int> ActiveP2PConnections = Meter.CreateObservableGauge<int>(
        name: "eufy.p2p.connections.active",
        observeValue: () => _activeP2PConnectionsCount,
        unit: "{connection}",
        description: "Number of active P2P connections");

    /// <summary>
    /// Observable gauge for authentication status
    /// Values: 1 = authenticated, 0 = not authenticated
    /// </summary>
    private static int _isAuthenticated;
    public static readonly ObservableGauge<int> AuthenticationStatus = Meter.CreateObservableGauge<int>(
        name: "eufy.authentication.status",
        observeValue: () => _isAuthenticated,
        unit: "{status}",
        description: "Current authentication status (1=authenticated, 0=not authenticated)");

    /// <summary>
    /// Observable gauge for token expiration time (seconds until expiry)
    /// </summary>
    private static long _tokenExpirationSeconds;
    public static readonly ObservableGauge<long> TokenExpirationSeconds = Meter.CreateObservableGauge<long>(
        name: "eufy.authentication.token_expiration_seconds",
        observeValue: () => _tokenExpirationSeconds,
        unit: "s",
        description: "Seconds until authentication token expires");

    // ==================== GAUGE UPDATE METHODS ====================

    /// <summary>
    /// Update the connected devices count gauge
    /// </summary>
    public static void SetConnectedDevicesCount(int count) => _connectedDevicesCount = count;

    /// <summary>
    /// Update the stations count gauge
    /// </summary>
    public static void SetStationsCount(int count) => _stationsCount = count;

    /// <summary>
    /// Update the active livestreams count gauge
    /// </summary>
    public static void SetActiveLivestreamsCount(int count) => _activeLivestreamsCount = count;

    /// <summary>
    /// Update the active P2P connections count gauge
    /// </summary>
    public static void SetActiveP2PConnectionsCount(int count) => _activeP2PConnectionsCount = count;

    /// <summary>
    /// Update the authentication status
    /// </summary>
    public static void SetAuthenticationStatus(bool isAuthenticated) => _isAuthenticated = isAuthenticated ? 1 : 0;

    /// <summary>
    /// Update the token expiration time
    /// </summary>
    public static void SetTokenExpirationSeconds(long seconds) => _tokenExpirationSeconds = seconds;

    // ==================== ACTIVITY TAGS ====================

    /// <summary>
    /// Common tag names for activities
    /// </summary>
    public static class Tags
    {
        public const string Result = "result";
        public const string ErrorType = "error.type";
        public const string Endpoint = "http.endpoint";
        public const string Method = "http.method";
        public const string StatusCode = "http.status_code";
        public const string DeviceSerial = "eufy.device.serial";
        public const string DeviceType = "eufy.device.type";
        public const string StationSerial = "eufy.station.serial";
        public const string ConnectionType = "eufy.connection.type";
        public const string Requires2FA = "eufy.auth.requires_2fa";
        public const string NotificationType = "eufy.notification.type";
        public const string DataType = "eufy.data.type";
        public const string Trigger = "eufy.trigger";
        public const string Operation = "operation";
    }

    /// <summary>
    /// Common tag values
    /// </summary>
    public static class Values
    {
        public const string Success = "success";
        public const string Failure = "failure";
        public const string Started = "started";
        public const string Stopped = "stopped";
        public const string Error = "error";
        public const string Video = "video";
        public const string Audio = "audio";
        public const string Automatic = "automatic";
        public const string Manual = "manual";
        public const string Local = "local";
        public const string Remote = "remote";
        public const string Quickest = "quickest";
    }
}
