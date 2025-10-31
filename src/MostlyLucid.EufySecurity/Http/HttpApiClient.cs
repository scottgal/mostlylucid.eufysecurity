using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Http;

/// <summary>
/// HTTP API client for Eufy cloud services
/// </summary>
public class HttpApiClient : IDisposable
{
    private const string ApiDomainBase = "https://extend.eufylife.com";
    private const string ServerPublicKey = "04c5c00c4f8d1197cc7c3167c52bf7acb054d722f0ef08dcd7e0883236e0d72a3868d9750cb47fa4619248f3d83f0f662671dadc6e2d31c2f41db0161651c7c076";

    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;
    private readonly string _username;
    private readonly string _password;
    private readonly string _country;
    private readonly string _language;

    private string? _token;
    private DateTime? _tokenExpiration;
    private bool _connected;

    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    public HttpApiClient(
        string username,
        string password,
        string country = "US",
        string language = "en",
        ILogger? logger = null)
    {
        _username = username;
        _password = password;
        _country = country.ToUpperInvariant();
        _language = language.ToLowerInvariant();
        _logger = logger;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiDomainBase),
            Timeout = TimeSpan.FromSeconds(30)
        };

        SetupHeaders();
    }

    private void SetupHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "EufySecurity.NET/1.0");
        _httpClient.DefaultRequestHeaders.Add("App_version", "v4.6.0_1630");
        _httpClient.DefaultRequestHeaders.Add("Os_type", "android");
        _httpClient.DefaultRequestHeaders.Add("Os_version", "31");
        _httpClient.DefaultRequestHeaders.Add("Phone_model", "ONEPLUS A3003");
        _httpClient.DefaultRequestHeaders.Add("Country", _country);
        _httpClient.DefaultRequestHeaders.Add("Language", _language);
        _httpClient.DefaultRequestHeaders.Add("Openudid", GenerateOpenUdid());
        _httpClient.DefaultRequestHeaders.Add("Net_type", "wifi");
        _httpClient.DefaultRequestHeaders.Add("Mnc", "02");
        _httpClient.DefaultRequestHeaders.Add("Mcc", "262");
        _httpClient.DefaultRequestHeaders.Add("Sn", GenerateSerialNumber());
        _httpClient.DefaultRequestHeaders.Add("Model_type", "PHONE");
        _httpClient.DefaultRequestHeaders.Add("Timezone", GetTimezoneString());
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
    }

    /// <summary>
    /// Authenticate with Eufy cloud
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Authenticating with Eufy cloud...");

            var loginData = new
            {
                email = _username,
                password = EncryptPassword(_password)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/v1/user/email/login",
                loginData,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Authentication failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
            if (result?.Data?.AuthToken == null)
            {
                _logger?.LogError("Authentication failed: no token in response");
                return false;
            }

            _token = result.Data.AuthToken;
            _tokenExpiration = DateTime.UtcNow.AddDays(30);
            _connected = true;

            _logger?.LogInformation("Authentication successful");
            ConnectionStateChanged?.Invoke(this, true);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Authentication error");
            throw new AuthenticationException("Failed to authenticate", ex);
        }
    }

    /// <summary>
    /// Get list of stations
    /// </summary>
    public async Task<List<Dictionary<string, object>>> GetStationsAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        try
        {
            var response = await _httpClient.GetAsync("/v1/app/get_devs_list", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<StationListResponse>(cancellationToken);
            return result?.Data ?? new List<Dictionary<string, object>>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get stations");
            throw new ApiException("Failed to retrieve stations", ex);
        }
    }

    /// <summary>
    /// Get list of devices
    /// </summary>
    public async Task<List<Dictionary<string, object>>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        try
        {
            var response = await _httpClient.GetAsync("/v1/app/get_devs_list", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DeviceListResponse>(cancellationToken);
            return result?.Data ?? new List<Dictionary<string, object>>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get devices");
            throw new ApiException("Failed to retrieve devices", ex);
        }
    }

    private void EnsureAuthenticated()
    {
        if (!_connected || string.IsNullOrEmpty(_token))
            throw new AuthenticationException("Not authenticated. Call AuthenticateAsync first.");

        if (_tokenExpiration.HasValue && DateTime.UtcNow >= _tokenExpiration.Value)
            throw new AuthenticationException("Token expired. Re-authenticate required.");
    }

    private string EncryptPassword(string password)
    {
        // Simplified encryption - in production, use proper ECDH key exchange
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GenerateOpenUdid()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }

    private static string GenerateSerialNumber()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }

    private static string GetTimezoneString()
    {
        var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        return $"GMT{sign}{Math.Abs(offset.Hours):D2}:{Math.Abs(offset.Minutes):D2}";
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Response models
internal class LoginResponse
{
    public LoginData? Data { get; set; }
}

internal class LoginData
{
    public string? AuthToken { get; set; }
}

internal class StationListResponse
{
    public List<Dictionary<string, object>>? Data { get; set; }
}

internal class DeviceListResponse
{
    public List<Dictionary<string, object>>? Data { get; set; }
}
