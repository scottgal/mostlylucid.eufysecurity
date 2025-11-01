using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using MostlyLucid.EufySecurity.Common;
using Microsoft.Extensions.Logging;

namespace MostlyLucid.EufySecurity.Http;

/// <summary>
/// Event args for 2FA request
/// </summary>
public class TwoFactorAuthRequestEventArgs : EventArgs
{
    public string Message { get; set; } = "Two-factor authentication code required";
}

/// <summary>
/// Event args for captcha request
/// </summary>
public class CaptchaRequestEventArgs : EventArgs
{
    public string CaptchaId { get; set; } = string.Empty;
    public string CaptchaUrl { get; set; } = string.Empty;
}

/// <summary>
/// HTTP API client for Eufy cloud services
/// </summary>
public class HttpApiClient : IDisposable
{
    private const string ApiDomainBase = "https://security-app.eufylife.com/api";
    private const int CodeNeedVerifyCode = 26052;
    private const int CodeWhateverError = 0;
    private const string DefaultServerPublicKey = "04c5c00c4f8d1197cc7c3167c52bf7acb054d722f0ef08dcd7e0883236e0d72a3868d9750cb47fa4619248f3d83f0f662671dadc6e2d31c2f41db0161651c7c076";

    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;
    private readonly string _username;
    private readonly string _password;
    private readonly string _country;
    private readonly string _language;
    private readonly string _trustedDeviceName;

    private ECPrivateKeyParameters? _privateKey;
    private ECPublicKeyParameters? _publicKey;
    private byte[]? _serverPublicKeyBytes;

    private string? _token;
    private DateTime? _tokenExpiration;
    private bool _connected;

    /// <summary>
    /// Event raised when connection state changes
    /// </summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// Event raised when 2FA code is required
    /// </summary>
    public event EventHandler<TwoFactorAuthRequestEventArgs>? TwoFactorAuthRequired;

    /// <summary>
    /// Event raised when captcha is required
    /// </summary>
    public event EventHandler<CaptchaRequestEventArgs>? CaptchaRequired;

    public HttpApiClient(
        string username,
        string password,
        string country = "US",
        string language = "en",
        string trustedDeviceName = "EufySecurity.NET",
        ILogger? logger = null)
    {
        _username = username;
        _password = password;
        _country = country.ToUpperInvariant();
        _language = language.ToLowerInvariant();
        _trustedDeviceName = trustedDeviceName;
        _logger = logger;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiDomainBase),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Initialize ECDH keys
        InitializeECDH();

        SetupHeaders();
    }

    private void InitializeECDH()
    {
        try
        {
            // Generate ECDH key pair using prime256v1 (secp256r1) curve
            var ecParams = ECNamedCurveTable.GetByName("secp256r1");
            var domainParams = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);

            var keyGenParams = new ECKeyGenerationParameters(domainParams, new SecureRandom());
            var keyGenerator = new ECKeyPairGenerator();
            keyGenerator.Init(keyGenParams);

            var keyPair = keyGenerator.GenerateKeyPair();
            _privateKey = (ECPrivateKeyParameters)keyPair.Private;
            _publicKey = (ECPublicKeyParameters)keyPair.Public;

            // Parse default server public key
            _serverPublicKeyBytes = HexStringToBytes(DefaultServerPublicKey);

            _logger?.LogDebug("ECDH key pair initialized");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize ECDH");
            throw;
        }
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
    /// <param name="verifyCode">Optional 2FA verification code</param>
    /// <param name="captchaInfo">Optional captcha response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<AuthenticationResult> AuthenticateAsync(
        string? verifyCode = null,
        CaptchaInfo? captchaInfo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Authenticating with Eufy cloud...");

            if (_publicKey == null || _privateKey == null)
            {
                throw new InvalidOperationException("ECDH keys not initialized");
            }

            // Get client public key as hex string
            var clientPublicKey = GetPublicKeyHex(_publicKey);

            // Encrypt password using ECDH
            var encryptedPassword = EncryptPasswordWithECDH(_password);

            // Build login request
            var loginData = new Dictionary<string, object>
            {
                ["ab"] = clientPublicKey,
                ["password"] = encryptedPassword,
                ["email"] = _username,
                ["time_zone"] = DateTimeOffset.UtcNow.Offset.TotalMilliseconds,
                ["transaction"] = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
            };

            // Add verification code if provided
            if (!string.IsNullOrEmpty(verifyCode))
            {
                loginData["verify_code"] = verifyCode;
            }

            // Add captcha if provided
            if (captchaInfo != null)
            {
                loginData["captcha"] = new Dictionary<string, string>
                {
                    ["captcha_id"] = captchaInfo.CaptchaId,
                    ["captcha_code"] = captchaInfo.CaptchaCode
                };
            }

            var response = await _httpClient.PostAsJsonAsync(
                "/v2/passport/login",
                loginData,
                cancellationToken);

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger?.LogDebug("Login response: {Response}", responseText);

            var result = JsonSerializer.Deserialize<LoginSecResponse>(responseText);

            if (result == null)
            {
                _logger?.LogError("Failed to parse login response");
                return new AuthenticationResult { Success = false, Message = "Invalid response from server" };
            }

            // Handle different response codes
            switch (result.Code)
            {
                case CodeWhateverError: // Success
                    if (result.Data?.AuthToken != null)
                    {
                        _token = result.Data.AuthToken;
                        _tokenExpiration = DateTime.UtcNow.AddDays(30);
                        _connected = true;

                        // Update server public key if provided
                        if (!string.IsNullOrEmpty(result.Data.ServerPublicKey))
                        {
                            _serverPublicKeyBytes = HexStringToBytes(result.Data.ServerPublicKey);
                        }

                        _logger?.LogInformation("Authentication successful");
                        ConnectionStateChanged?.Invoke(this, true);

                        return new AuthenticationResult { Success = true };
                    }
                    break;

                case CodeNeedVerifyCode: // 2FA required
                    _logger?.LogInformation("Two-factor authentication required");

                    // Send verification code via email
                    await SendVerificationCodeAsync(cancellationToken);

                    // Raise event to notify caller
                    TwoFactorAuthRequired?.Invoke(this, new TwoFactorAuthRequestEventArgs());

                    return new AuthenticationResult
                    {
                        Success = false,
                        RequiresTwoFactor = true,
                        Message = "Two-factor authentication code required. Check your email."
                    };

                default:
                    _logger?.LogError("Authentication failed with code {Code}: {Message}", result.Code, result.Msg);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = result.Msg ?? $"Authentication failed with code {result.Code}"
                    };
            }

            return new AuthenticationResult { Success = false, Message = "Unknown authentication error" };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Authentication error");
            return new AuthenticationResult { Success = false, Message = $"Exception: {ex.Message}" };
        }
    }

    /// <summary>
    /// Send verification code to user's email
    /// </summary>
    private async Task SendVerificationCodeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new
            {
                email = _username,
                message_type = 2 // Email verification
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/user/email/sendcode", data, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Verification code sent to email");
            }
            else
            {
                _logger?.LogWarning("Failed to send verification code");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error sending verification code");
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

    private string EncryptPasswordWithECDH(string password)
    {
        if (_privateKey == null || _serverPublicKeyBytes == null)
        {
            throw new InvalidOperationException("ECDH not properly initialized");
        }

        try
        {
            // Parse server public key
            var curve = _privateKey.Parameters.Curve;
            var serverPublicKey = curve.DecodePoint(_serverPublicKeyBytes);
            var serverPublicKeyParams = new ECPublicKeyParameters(serverPublicKey, _privateKey.Parameters);

            // Compute shared secret using ECDH
            var agreement = new ECDHBasicAgreement();
            agreement.Init(_privateKey);
            var sharedSecret = agreement.CalculateAgreement(serverPublicKeyParams);

            // Convert shared secret to bytes (32 bytes for AES-256)
            var secretBytes = sharedSecret.ToByteArrayUnsigned();
            if (secretBytes.Length > 32)
            {
                Array.Resize(ref secretBytes, 32);
            }
            else if (secretBytes.Length < 32)
            {
                var padded = new byte[32];
                Buffer.BlockCopy(secretBytes, 0, padded, 32 - secretBytes.Length, secretBytes.Length);
                secretBytes = padded;
            }

            // Encrypt password using AES-256-CBC
            var encrypted = EncryptAES(password, secretBytes);
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to encrypt password");
            throw;
        }
    }

    private static byte[] EncryptAES(string plaintext, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = new byte[16]; // Zero IV

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        return encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
    }

    private string GetPublicKeyHex(ECPublicKeyParameters publicKey)
    {
        var q = publicKey.Q;
        var encoded = q.GetEncoded(false); // Uncompressed format
        return BytesToHexString(encoded);
    }

    private static byte[] HexStringToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    private static string BytesToHexString(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
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
public class AuthenticationResult
{
    public bool Success { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool RequiresCaptcha { get; set; }
    public string? Message { get; set; }
    public string? CaptchaId { get; set; }
    public string? CaptchaUrl { get; set; }
}

public class CaptchaInfo
{
    public string CaptchaId { get; set; } = string.Empty;
    public string CaptchaCode { get; set; } = string.Empty;
}

internal class LoginSecResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

    [JsonPropertyName("data")]
    public LoginSecData? Data { get; set; }
}

internal class LoginSecData
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("nick_name")]
    public string? NickName { get; set; }

    [JsonPropertyName("auth_token")]
    public string? AuthToken { get; set; }

    [JsonPropertyName("token_expires_at")]
    public long TokenExpiresAt { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("invitation_code")]
    public string? InvitationCode { get; set; }

    [JsonPropertyName("inviter_code")]
    public string? InviterCode { get; set; }

    [JsonPropertyName("verify_code_url")]
    public string? VerifyCodeUrl { get; set; }

    [JsonPropertyName("mac_addr")]
    public string? MacAddr { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("ab")]
    public string? ServerPublicKey { get; set; }

    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
}

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
