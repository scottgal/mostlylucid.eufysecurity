using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Common;

/// <summary>
/// Configuration for EufySecurity client
/// </summary>
public class EufySecurityConfig
{
    /// <summary>
    /// Eufy account username/email
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Eufy account password or app password (PIN)
    /// Backward-compatible: if you already use this for the Eufy-generated PIN, you can keep using it.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Optional: Eufy app-specific password (generated PIN). Alias for clarity.
    /// </summary>
    public string? AppPassword { get; init; }

    /// <summary>
    /// Optional: Eufy generated PIN (alias). Same as AppPassword.
    /// </summary>
    public string? Pin { get; init; }

    /// <summary>
    /// Returns the password value that will be used for authentication.
    /// Resolution order: Password -> AppPassword -> Pin. Throws if none are provided.
    /// </summary>
    public string EffectivePassword
    {
        get
        {
            var value = Password ?? AppPassword ?? Pin;
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("You must provide either Password, AppPassword, or Pin in EufySecurityConfig.");
            return value;
        }
    }

    /// <summary>
    /// Country code (must match Eufy app setting)
    /// </summary>
    public string Country { get; init; } = "US";

    /// <summary>
    /// Language code
    /// </summary>
    public string Language { get; init; } = "en";

    /// <summary>
    /// Trusted device name for 2FA
    /// </summary>
    public string TrustedDeviceName { get; init; } = "EufySecurity.NET";

    /// <summary>
    /// Directory path for storing persistent data
    /// </summary>
    public string? PersistentDataPath { get; init; }

    /// <summary>
    /// P2P connection type preference
    /// </summary>
    public P2PConnectionType P2PConnectionSetup { get; init; } = P2PConnectionType.Quickest;

    /// <summary>
    /// Polling interval for cloud data refresh (minutes)
    /// </summary>
    public int PollingIntervalMinutes { get; init; } = 10;

    /// <summary>
    /// Whether to accept station invitations automatically
    /// </summary>
    public bool AcceptInvitations { get; init; } = true;

    /// <summary>
    /// Custom event duration in seconds
    /// </summary>
    public int EventDurationSeconds { get; init; } = 10;

    /// <summary>
    /// Logger instance
    /// </summary>
    public ILogger? Logger { get; init; }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public LoggingConfig? Logging { get; init; }

    /// <summary>
    /// Enable PKCS1 support
    /// </summary>
    public bool EnableEmbeddedPKCS1Support { get; init; } = false;

    /// <summary>
    /// Station configuration by serial number
    /// </summary>
    public Dictionary<string, StationConfig>? Stations { get; init; }

    /// <summary>
    /// Device configuration by serial number
    /// </summary>
    public Dictionary<string, DeviceConfig>? Devices { get; init; }

    /// <summary>
    /// Disable automatic cloud polling
    /// </summary>
    public bool DisableAutomaticCloudPolling { get; init; } = false;
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// Global log level
    /// </summary>
    public LogLevel? Level { get; init; }

    /// <summary>
    /// Per-category log levels
    /// </summary>
    public List<CategoryLogLevel>? Categories { get; init; }
}

/// <summary>
/// Log level for a specific category
/// </summary>
public class CategoryLogLevel
{
    /// <summary>
    /// Category name (HTTP, P2P, Push, MQTT)
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Log level for this category
    /// </summary>
    public required LogLevel Level { get; init; }
}

/// <summary>
/// Station-specific configuration
/// </summary>
public class StationConfig
{
    /// <summary>
    /// Suggested IP address for P2P connection
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Custom P2P port
    /// </summary>
    public int? P2PPort { get; init; }
}

/// <summary>
/// Device-specific configuration
/// </summary>
public class DeviceConfig
{
    /// <summary>
    /// Enable simultaneous detections
    /// </summary>
    public bool SimultaneousDetections { get; init; } = false;
}

/// <summary>
/// P2P connection type preference
/// </summary>
public enum P2PConnectionType
{
    /// <summary>
    /// Only connect locally (LAN)
    /// </summary>
    OnlyLocal = 0,

    /// <summary>
    /// Connect using quickest method (local or remote)
    /// </summary>
    Quickest = 1
}
