using MostlyLucid.EufySecurity.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace MostlyLucid.EufySecurity.Demo.Controllers;

/// <summary>
/// API endpoints for managing Eufy Security devices
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly ILogger<DevicesController> _logger;
    private readonly EufySecurityHostedService _eufyService;

    public DevicesController(
        ILogger<DevicesController> logger,
        EufySecurityHostedService eufyService)
    {
        _logger = logger;
        _eufyService = eufyService;
    }

    /// <summary>
    /// Get all devices
    /// </summary>
    /// <returns>List of all devices</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetAllDevices()
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        var devices = _eufyService.Client.GetDevices().Values.Select(d => new
        {
            serialNumber = d.SerialNumber,
            name = d.Name,
            model = d.Model,
            type = d.DeviceType.ToString(),
            hardwareVersion = d.HardwareVersion,
            softwareVersion = d.SoftwareVersion,
            stationSerial = d.StationSerialNumber,
            enabled = d.IsEnabled
        });

        return Ok(devices);
    }

    /// <summary>
    /// Get a specific device by serial number
    /// </summary>
    /// <param name="serialNumber">Device serial number</param>
    /// <returns>Device details</returns>
    [HttpGet("{serialNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetDevice(string serialNumber)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        var device = _eufyService.Client.GetDevice(serialNumber);
        if (device == null)
        {
            return NotFound(new { error = $"Device {serialNumber} not found" });
        }

        return Ok(new
        {
            serialNumber = device.SerialNumber,
            name = device.Name,
            model = device.Model,
            type = device.DeviceType.ToString(),
            hardwareVersion = device.HardwareVersion,
            softwareVersion = device.SoftwareVersion,
            stationSerial = device.StationSerialNumber,
            enabled = device.IsEnabled
        });
    }

    /// <summary>
    /// Refresh device list from cloud
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RefreshDevices()
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        try
        {
            await _eufyService.Client.RefreshDevicesAsync();
            var deviceCount = _eufyService.Client.GetDevices().Count;
            var stationCount = _eufyService.Client.GetStations().Count;

            return Ok(new
            {
                success = true,
                message = "Device list refreshed",
                deviceCount,
                stationCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh devices");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to refresh devices", details = ex.Message });
        }
    }
}
