using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace MostlyLucid.EufySecurity.Demo.Controllers;

/// <summary>
/// API endpoints for managing Eufy Security stations (hubs)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StationsController : ControllerBase
{
    private readonly ILogger<StationsController> _logger;
    private readonly EufySecurityHostedService _eufyService;

    public StationsController(
        ILogger<StationsController> logger,
        EufySecurityHostedService eufyService)
    {
        _logger = logger;
        _eufyService = eufyService;
    }

    /// <summary>
    /// Get all stations
    /// </summary>
    /// <returns>List of all stations</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetAllStations()
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        var stations = _eufyService.Client.GetStations().Values.Select(s => new
        {
            serialNumber = s.SerialNumber,
            name = s.Name,
            model = s.Model,
            type = s.DeviceType.ToString(),
            hardwareVersion = s.HardwareVersion,
            softwareVersion = s.SoftwareVersion,
            ipAddress = s.IpAddress,
            lanIpAddress = s.LanIpAddress,
            macAddress = s.MacAddress,
            isConnected = s.IsConnected,
            guardMode = s.GuardMode.ToString(),
            currentMode = s.CurrentMode.ToString(),
            deviceCount = s.Devices.Count
        });

        return Ok(stations);
    }

    /// <summary>
    /// Get a specific station by serial number
    /// </summary>
    /// <param name="serialNumber">Station serial number</param>
    /// <returns>Station details</returns>
    [HttpGet("{serialNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetStation(string serialNumber)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        var station = _eufyService.Client.GetStation(serialNumber);
        if (station == null)
        {
            return NotFound(new { error = $"Station {serialNumber} not found" });
        }

        return Ok(new
        {
            serialNumber = station.SerialNumber,
            name = station.Name,
            model = station.Model,
            type = station.DeviceType.ToString(),
            hardwareVersion = station.HardwareVersion,
            softwareVersion = station.SoftwareVersion,
            ipAddress = station.IpAddress,
            lanIpAddress = station.LanIpAddress,
            macAddress = station.MacAddress,
            isConnected = station.IsConnected,
            guardMode = station.GuardMode.ToString(),
            currentMode = station.CurrentMode.ToString(),
            devices = station.Devices.Values.Select(d => new
            {
                serialNumber = d.SerialNumber,
                name = d.Name,
                type = d.DeviceType.ToString()
            })
        });
    }

    /// <summary>
    /// Connect to a station via P2P
    /// </summary>
    /// <param name="serialNumber">Station serial number</param>
    /// <returns>Connection status</returns>
    [HttpPost("{serialNumber}/connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConnectToStation(string serialNumber)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        try
        {
            await _eufyService.Client.ConnectToStationAsync(serialNumber);
            return Ok(new
            {
                success = true,
                message = $"Connected to station {serialNumber}"
            });
        }
        catch (StationNotFoundException)
        {
            return NotFound(new { error = $"Station {serialNumber} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to station {SerialNumber}", serialNumber);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to connect to station", details = ex.Message });
        }
    }

    /// <summary>
    /// Set guard mode for a station
    /// </summary>
    /// <param name="serialNumber">Station serial number</param>
    /// <param name="request">Guard mode request</param>
    /// <returns>Success status</returns>
    [HttpPost("{serialNumber}/guard-mode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SetGuardMode(string serialNumber, [FromBody] SetGuardModeRequest request)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        if (!Enum.TryParse<GuardMode>(request.Mode, true, out var guardMode))
        {
            return BadRequest(new
            {
                error = "Invalid guard mode",
                validModes = Enum.GetNames<GuardMode>()
            });
        }

        try
        {
            await _eufyService.Client.SetGuardModeAsync(serialNumber, guardMode);
            return Ok(new
            {
                success = true,
                message = $"Guard mode set to {guardMode}",
                stationSerial = serialNumber,
                guardMode = guardMode.ToString()
            });
        }
        catch (StationNotFoundException)
        {
            return NotFound(new { error = $"Station {serialNumber} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set guard mode for station {SerialNumber}", serialNumber);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to set guard mode", details = ex.Message });
        }
    }
}

/// <summary>
/// Request model for setting guard mode
/// </summary>
public class SetGuardModeRequest
{
    /// <summary>
    /// Guard mode (Away, Home, Disarmed, Schedule, etc.)
    /// </summary>
    public required string Mode { get; set; }
}
