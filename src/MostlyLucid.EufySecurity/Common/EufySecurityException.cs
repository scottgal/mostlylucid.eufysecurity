namespace MostlyLucid.EufySecurity.Common;

/// <summary>
/// Base exception for all EufySecurity errors
/// </summary>
public class EufySecurityException : Exception
{
    public EufySecurityException() { }
    public EufySecurityException(string message) : base(message) { }
    public EufySecurityException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when a device is not found
/// </summary>
public class DeviceNotFoundException : EufySecurityException
{
    public string DeviceSerial { get; }

    public DeviceNotFoundException(string deviceSerial)
        : base($"Device with serial {deviceSerial} not found")
    {
        DeviceSerial = deviceSerial;
    }
}

/// <summary>
/// Thrown when a station is not found
/// </summary>
public class StationNotFoundException : EufySecurityException
{
    public string StationSerial { get; }

    public StationNotFoundException(string stationSerial)
        : base($"Station with serial {stationSerial} not found")
    {
        StationSerial = stationSerial;
    }
}

/// <summary>
/// Thrown when attempting to set a read-only property
/// </summary>
public class ReadOnlyPropertyException : EufySecurityException
{
    public string PropertyName { get; }

    public ReadOnlyPropertyException(string propertyName)
        : base($"Property {propertyName} is read-only")
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// Thrown when a feature is not supported
/// </summary>
public class NotSupportedException : EufySecurityException
{
    public NotSupportedException(string message) : base(message) { }
}

/// <summary>
/// Thrown when authentication fails
/// </summary>
public class AuthenticationException : EufySecurityException
{
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when an invalid property is accessed
/// </summary>
public class InvalidPropertyException : EufySecurityException
{
    public string PropertyName { get; }

    public InvalidPropertyException(string propertyName, string message)
        : base($"Invalid property {propertyName}: {message}")
    {
        PropertyName = propertyName;
    }
}

/// <summary>
/// Thrown when livestream operations fail
/// </summary>
public class LivestreamException : EufySecurityException
{
    public LivestreamException(string message) : base(message) { }
    public LivestreamException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when P2P connection fails
/// </summary>
public class P2PConnectionException : EufySecurityException
{
    public P2PConnectionException(string message) : base(message) { }
    public P2PConnectionException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Thrown when API requests fail
/// </summary>
public class ApiException : EufySecurityException
{
    public int? StatusCode { get; }
    public string? ErrorCode { get; }

    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception inner) : base(message, inner) { }

    public ApiException(string message, int statusCode, string? errorCode = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
