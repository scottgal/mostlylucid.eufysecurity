using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Devices;

/// <summary>
/// Generic camera device for unknown or unsupported camera types
/// </summary>
public class GenericCamera : Camera
{
    public GenericCamera(
        string serialNumber,
        string name,
        string model,
        DeviceType deviceType,
        string stationSerialNumber,
        ILogger? logger = null)
        : base(serialNumber, name, model, deviceType, stationSerialNumber, logger)
    {
    }
}
