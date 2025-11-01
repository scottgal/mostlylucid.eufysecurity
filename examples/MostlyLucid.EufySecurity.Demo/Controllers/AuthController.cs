using Microsoft.AspNetCore.Mvc;
using MostlyLucid.EufySecurity.Demo.Services;
using MostlyLucid.EufySecurity.Http;

namespace MostlyLucid.EufySecurity.Demo.Controllers;

/// <summary>
/// Controller for Eufy authentication with 2FA support
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(EufySecurityHostedService eufyService, ILogger<AuthController> logger) : ControllerBase
{
    private readonly EufySecurityHostedService _eufyService = eufyService;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _eufyService.ConnectAsync(
                request.Username,
                request.Password,
                null, // No verify code yet
                request.Country ?? "US",
                request.Language ?? "en"
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(500, new AuthenticationResult
            {
                Success = false,
                Message = $"Server error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Verify 2FA code
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
    {
        try
        {
            var result = await _eufyService.ConnectAsync(
                request.Username,
                request.Password,
                request.VerifyCode,
                request.Country ?? "US",
                request.Language ?? "en"
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Verification error");
            return StatusCode(500, new AuthenticationResult
            {
                Success = false,
                Message = $"Server error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Get current connection status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            connected = _eufyService.IsConnected,
            hasClient = _eufyService.Client != null
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Language { get; set; }
}

public class VerifyRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VerifyCode { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Language { get; set; }
}
