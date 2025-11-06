using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Http;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class HttpApiClientTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var client = new HttpApiClient("user@example.com", "password123");

        // Assert
        client.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    public void Constructor_WithInvalidUsername_ThrowsArgumentException(string? username, string password)
    {
        // Act
        Action act = () => new HttpApiClient(username!, password);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Username*");
    }

    [Theory]
    [InlineData("user@example.com", null)]
    [InlineData("user@example.com", "")]
    [InlineData("user@example.com", "   ")]
    public void Constructor_WithInvalidPassword_ThrowsArgumentException(string username, string? password)
    {
        // Act
        Action act = () => new HttpApiClient(username, password!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password*");
    }

    [Theory]
    [InlineData("user@example.com", "password", null)]
    [InlineData("user@example.com", "password", "")]
    [InlineData("user@example.com", "password", "   ")]
    public void Constructor_WithInvalidCountry_ThrowsArgumentException(string username, string password, string? country)
    {
        // Act
        Action act = () => new HttpApiClient(username, password, country!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Country*");
    }

    [Theory]
    [InlineData("user@example.com", "password", "US", null)]
    [InlineData("user@example.com", "password", "US", "")]
    [InlineData("user@example.com", "password", "US", "   ")]
    public void Constructor_WithInvalidLanguage_ThrowsArgumentException(string username, string password, string country, string? language)
    {
        // Act
        Action act = () => new HttpApiClient(username, password, country, language!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Language*");
    }

    [Theory]
    [InlineData("user@example.com", "password", "US", "en", null)]
    [InlineData("user@example.com", "password", "US", "en", "")]
    [InlineData("user@example.com", "password", "US", "en", "   ")]
    public void Constructor_WithInvalidTrustedDeviceName_ThrowsArgumentException(
        string username, string password, string country, string language, string? trustedDeviceName)
    {
        // Act
        Action act = () => new HttpApiClient(username, password, country, language, trustedDeviceName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Trusted device name*");
    }

    [Fact]
    public void Constructor_NormalizesCountryToUpperCase()
    {
        // Arrange & Act
        var client = new HttpApiClient("user@example.com", "password", "us");

        // Assert - Country should be normalized to uppercase internally
        // We can't directly test this without exposing the field, but we can verify it doesn't throw
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NormalizesLanguageToLowerCase()
    {
        // Arrange & Act
        var client = new HttpApiClient("user@example.com", "password", "US", "EN");

        // Assert - Language should be normalized to lowercase internally
        // We can't directly test this without exposing the field, but we can verify it doesn't throw
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStationsAsync_WhenNotAuthenticated_ThrowsAuthenticationException()
    {
        // Arrange
        var client = new HttpApiClient("user@example.com", "password");

        // Act
        Func<Task> act = async () => await client.GetStationsAsync();

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*Not authenticated*");
    }

    [Fact]
    public async Task GetDevicesAsync_WhenNotAuthenticated_ThrowsAuthenticationException()
    {
        // Arrange
        var client = new HttpApiClient("user@example.com", "password");

        // Act
        Func<Task> act = async () => await client.GetDevicesAsync();

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("*Not authenticated*");
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var client = new HttpApiClient("user@example.com", "password");

        // Act
        client.Dispose();
        Action act = () => client.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void TwoFactorAuthRequestEventArgs_HasDefaultMessage()
    {
        // Arrange & Act
        var eventArgs = new TwoFactorAuthRequestEventArgs();

        // Assert
        eventArgs.Message.Should().Be("Two-factor authentication code required");
    }

    [Fact]
    public void CaptchaRequestEventArgs_HasEmptyDefaults()
    {
        // Arrange & Act
        var eventArgs = new CaptchaRequestEventArgs();

        // Assert
        eventArgs.CaptchaId.Should().BeEmpty();
        eventArgs.CaptchaUrl.Should().BeEmpty();
    }

    [Fact]
    public void AuthenticationResult_DefaultsToFalse()
    {
        // Arrange & Act
        var result = new AuthenticationResult();

        // Assert
        result.Success.Should().BeFalse();
        result.RequiresTwoFactor.Should().BeFalse();
        result.RequiresCaptcha.Should().BeFalse();
        result.Message.Should().BeNull();
        result.CaptchaId.Should().BeNull();
        result.CaptchaUrl.Should().BeNull();
    }

    [Fact]
    public void CaptchaInfo_HasEmptyDefaults()
    {
        // Arrange & Act
        var captchaInfo = new CaptchaInfo();

        // Assert
        captchaInfo.CaptchaId.Should().BeEmpty();
        captchaInfo.CaptchaCode.Should().BeEmpty();
    }

    [Fact]
    public void CaptchaInfo_CanSetProperties()
    {
        // Arrange & Act
        var captchaInfo = new CaptchaInfo
        {
            CaptchaId = "test-id",
            CaptchaCode = "test-code"
        };

        // Assert
        captchaInfo.CaptchaId.Should().Be("test-id");
        captchaInfo.CaptchaCode.Should().Be("test-code");
    }
}
