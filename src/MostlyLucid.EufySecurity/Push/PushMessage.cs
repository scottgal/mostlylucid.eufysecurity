namespace MostlyLucid.EufySecurity.Push;

/// <summary>
/// Push notification message from Eufy cloud
/// </summary>
public class PushMessage
{
    /// <summary>
    /// Event type
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Device serial number
    /// </summary>
    public string? DeviceSerial { get; init; }

    /// <summary>
    /// Station serial number
    /// </summary>
    public string? StationSerial { get; init; }

    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Event title
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Event message
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Person detected
    /// </summary>
    public bool? PersonDetected { get; init; }

    /// <summary>
    /// Motion detected
    /// </summary>
    public bool? MotionDetected { get; init; }

    /// <summary>
    /// Doorbell ring
    /// </summary>
    public bool? DoorbellRing { get; init; }

    /// <summary>
    /// Picture URL
    /// </summary>
    public string? PictureUrl { get; init; }

    /// <summary>
    /// Picture data (base64 or binary)
    /// </summary>
    public byte[]? PictureData { get; init; }

    /// <summary>
    /// Additional data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; init; }
}

/// <summary>
/// Push notification event types
/// </summary>
public static class PushEventTypes
{
    public const string Motion = "motion";
    public const string PersonDetected = "person";
    public const string DoorbellRing = "doorbell_ring";
    public const string PetDetected = "pet";
    public const string SoundDetected = "sound";
    public const string CryingDetected = "crying";
    public const string VehicleDetected = "vehicle";
    public const string PackageDetected = "package";
    public const string LockLocked = "lock_locked";
    public const string LockUnlocked = "lock_unlocked";
    public const string AlarmTriggered = "alarm";
}
