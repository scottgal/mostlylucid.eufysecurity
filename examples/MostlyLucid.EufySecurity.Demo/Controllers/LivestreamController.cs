using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace MostlyLucid.EufySecurity.Demo.Controllers;

/// <summary>
/// API endpoints for managing device livestreams
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LivestreamController : ControllerBase
{
    private readonly ILogger<LivestreamController> _logger;
    private readonly EufySecurityHostedService _eufyService;

    public LivestreamController(
        ILogger<LivestreamController> logger,
        EufySecurityHostedService eufyService)
    {
        _logger = logger;
        _eufyService = eufyService;
    }

    /// <summary>
    /// Start livestream for a device
    /// </summary>
    /// <param name="deviceSerial">Device serial number</param>
    /// <returns>Success status</returns>
    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> StartLivestream([FromQuery] string deviceSerial)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        try
        {
            await _eufyService.Client.StartLivestreamAsync(deviceSerial);
            return Ok(new
            {
                success = true,
                message = $"Livestream started for device {deviceSerial}",
                deviceSerial
            });
        }
        catch (DeviceNotFoundException)
        {
            return NotFound(new { error = $"Device {deviceSerial} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start livestream for device {DeviceSerial}", deviceSerial);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to start livestream", details = ex.Message });
        }
    }

    /// <summary>
    /// Stop livestream for a device
    /// </summary>
    /// <param name="deviceSerial">Device serial number</param>
    /// <returns>Success status</returns>
    [HttpPost("stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> StopLivestream([FromQuery] string deviceSerial)
    {
        if (_eufyService.Client == null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "EufySecurity client not connected" });
        }

        try
        {
            await _eufyService.Client.StopLivestreamAsync(deviceSerial);
            return Ok(new
            {
                success = true,
                message = $"Livestream stopped for device {deviceSerial}",
                deviceSerial
            });
        }
        catch (DeviceNotFoundException)
        {
            return NotFound(new { error = $"Device {deviceSerial} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop livestream for device {DeviceSerial}", deviceSerial);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to stop livestream", details = ex.Message });
        }
    }
}
