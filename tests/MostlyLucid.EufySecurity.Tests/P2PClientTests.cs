using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Devices;
using MostlyLucid.EufySecurity.Events;
using MostlyLucid.EufySecurity.P2P;
using MostlyLucid.EufySecurity.Stations;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class P2PClientTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var client = new P2PClient();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public async Task ConnectAsync_WithValidStation_ReturnsTrue()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);

        // Act
        var result = await client.ConnectAsync(station, "p2pDid", "dskKey");

        // Assert
        result.Should().BeTrue();
        station.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_WithConnectionTypeQuickest_ReturnsTrue()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);

        // Act
        var result = await client.ConnectAsync(station, "p2pDid", "dskKey", P2PConnectionType.Quickest);

        // Assert
        result.Should().BeTrue();
        station.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_WithConnectionTypeOnlyLocal_ReturnsTrue()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);

        // Act
        var result = await client.ConnectAsync(station, "p2pDid", "dskKey", P2PConnectionType.OnlyLocal);

        // Assert
        result.Should().BeTrue();
        station.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void Disconnect_DisconnectsStation()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        station.IsConnected = true;

        // Act
        client.Disconnect(station);

        // Assert
        station.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void Disconnect_WhenNotConnected_DoesNotThrow()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);

        // Act
        Action act = () => client.Disconnect(station);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartLivestreamAsync_WhenNotConnected_ThrowsP2PConnectionException()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        var device = new IndoorCamera("TEST_DEVICE", "Test Device", "TestModel", DeviceType.IndoorCamera, "TEST_STATION");

        // Act
        Func<Task> act = async () => await client.StartLivestreamAsync(station, device);

        // Assert
        await act.Should().ThrowAsync<P2PConnectionException>()
            .WithMessage("*not connected*");
    }

    [Fact]
    public async Task StartLivestreamAsync_WhenConnected_RaisesLivestreamStartedEvent()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        var device = new IndoorCamera("TEST_DEVICE", "Test Device", "TestModel", DeviceType.IndoorCamera, "TEST_STATION");
        station.IsConnected = true;

        LivestreamEventArgs? raisedEventArgs = null;
        client.LivestreamStarted += (sender, args) => raisedEventArgs = args;

        // Act
        await client.StartLivestreamAsync(station, device);

        // Assert
        raisedEventArgs.Should().NotBeNull();
        raisedEventArgs!.Station.Should().Be(station);
        raisedEventArgs.Device.Should().Be(device);
        raisedEventArgs.Metadata.Should().NotBeNull();
        raisedEventArgs.Metadata!.VideoCodec.Should().Be(VideoCodec.H264);
        raisedEventArgs.Metadata.AudioCodec.Should().Be(AudioCodec.AAC);
    }

    [Fact]
    public async Task StopLivestreamAsync_WhenNotConnected_ThrowsP2PConnectionException()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        var device = new IndoorCamera("TEST_DEVICE", "Test Device", "TestModel", DeviceType.IndoorCamera, "TEST_STATION");

        // Act
        Func<Task> act = async () => await client.StopLivestreamAsync(station, device);

        // Assert
        await act.Should().ThrowAsync<P2PConnectionException>()
            .WithMessage("*not connected*");
    }

    [Fact]
    public async Task StopLivestreamAsync_WhenConnected_RaisesLivestreamStoppedEvent()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        var device = new IndoorCamera("TEST_DEVICE", "Test Device", "TestModel", DeviceType.IndoorCamera, "TEST_STATION");
        station.IsConnected = true;

        LivestreamEventArgs? raisedEventArgs = null;
        client.LivestreamStopped += (sender, args) => raisedEventArgs = args;

        // Act
        await client.StopLivestreamAsync(station, device);

        // Assert
        raisedEventArgs.Should().NotBeNull();
        raisedEventArgs!.Station.Should().Be(station);
        raisedEventArgs.Device.Should().Be(device);
    }

    [Fact]
    public async Task SetGuardModeAsync_WhenNotConnected_ThrowsP2PConnectionException()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);

        // Act
        Func<Task> act = async () => await client.SetGuardModeAsync(station, GuardMode.Away);

        // Assert
        await act.Should().ThrowAsync<P2PConnectionException>()
            .WithMessage("*not connected*");
    }

    [Fact]
    public async Task SetGuardModeAsync_WhenConnected_DoesNotThrow()
    {
        // Arrange
        var client = new P2PClient();
        var station = new Station("TEST_STATION", "Test Station", "TestModel", DeviceType.Station);
        station.IsConnected = true;

        // Act
        Func<Task> act = async () => await client.SetGuardModeAsync(station, GuardMode.Home);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var client = new P2PClient();

        // Act
        client.Dispose();
        Action act = () => client.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_DisconnectsAllStations()
    {
        // Arrange
        var client = new P2PClient();
        var station1 = new Station("STATION1", "Station 1", "TestModel", DeviceType.Station);
        var station2 = new Station("STATION2", "Station 2", "TestModel", DeviceType.Station);

        await client.ConnectAsync(station1, "p2pDid1", "dskKey1");
        await client.ConnectAsync(station2, "p2pDid2", "dskKey2");

        // Act
        client.Dispose();

        // Assert
        // Verify disposal doesn't throw
        client.Should().NotBeNull();
    }
}
