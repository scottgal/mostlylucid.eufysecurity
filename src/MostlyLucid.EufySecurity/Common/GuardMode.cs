namespace MostlyLucid.EufySecurity.Common;

/// <summary>
/// Guard modes for stations
/// </summary>
public enum GuardMode
{
    /// <summary>
    /// Away mode - full security active
    /// </summary>
    Away = 0,

    /// <summary>
    /// Home mode - partial security
    /// </summary>
    Home = 1,

    /// <summary>
    /// Schedule mode - automatic switching based on schedule
    /// </summary>
    Schedule = 2,

    /// <summary>
    /// Disarmed - security disabled
    /// </summary>
    Disarmed = 63,

    /// <summary>
    /// Custom mode 1
    /// </summary>
    Custom1 = 3,

    /// <summary>
    /// Custom mode 2
    /// </summary>
    Custom2 = 4,

    /// <summary>
    /// Custom mode 3
    /// </summary>
    Custom3 = 5,

    /// <summary>
    /// Off mode (for some devices)
    /// </summary>
    Off = 47
}
