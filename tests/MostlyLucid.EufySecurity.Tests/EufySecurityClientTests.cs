using MostlyLucid.EufySecurity.Common;
using FluentAssertions;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class EufySecurityClientTests
{
    [Fact]
    public void Constructor_WithValidConfig_ShouldCreateInstance()
    {
        // Arrange
        var config = new EufySecurityConfig
        {
            Username = "test@example.com",
            Password = "testpassword"
        };

        // Act
        using var client = new EufySecurityClient(config);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new EufySecurityClient(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDevices_WhenNotConnected_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var config = new EufySecurityConfig
        {
            Username = "test@example.com",
            Password = "testpassword"
        };
        using var client = new EufySecurityClient(config);

        // Act
        var devices = client.GetDevices();

        // Assert
        devices.Should().NotBeNull();
        devices.Should().BeEmpty();
    }

    [Fact]
    public void GetStations_WhenNotConnected_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var config = new EufySecurityConfig
        {
            Username = "test@example.com",
            Password = "testpassword"
        };
        using var client = new EufySecurityClient(config);

        // Act
        var stations = client.GetStations();

        // Assert
        stations.Should().NotBeNull();
        stations.Should().BeEmpty();
    }

    [Fact]
    public void Version_ShouldReturnValidVersionString()
    {
        // Act
        var version = EufySecurityClient.Version;

        // Assert
        version.Should().NotBeNullOrEmpty();
        version.Should().MatchRegex(@"\d+\.\d+\.\d+");
    }
}
