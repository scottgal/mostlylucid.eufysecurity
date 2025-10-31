using System;
using System.Threading.Tasks;
using FluentAssertions;
using MostlyLucid.EufySecurity;
using MostlyLucid.EufySecurity.Common;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class EufySecurityClientBehaviorTests
{
    private static EufySecurityConfig Cfg(bool disablePolling = true) => new()
    {
        Username = "user@example.com",
        Password = "password",
        DisableAutomaticCloudPolling = disablePolling,
        PollingIntervalMinutes = 1,
    };

    [Fact]
    public void Getters_WhenNotConnected_ReturnEmptyCollections()
    {
        using var client = new EufySecurityClient(Cfg());

        var stations = client.GetStations();
        var devices = client.GetDevices();

        stations.Should().NotBeNull();
        stations.Should().BeEmpty();
        devices.Should().NotBeNull();
        devices.Should().BeEmpty();
    }

    [Fact]
    public void GetNonExisting_ReturnsNull()
    {
        using var client = new EufySecurityClient(Cfg());

        client.GetStation("unknown").Should().BeNull();
        client.GetDevice("unknown").Should().BeNull();
    }

    [Fact]
    public async Task ConnectToStationAsync_WhenStationMissing_ShouldThrow()
    {
        using var client = new EufySecurityClient(Cfg());
        Func<Task> act = async () => await client.ConnectToStationAsync("station-123");
        await act.Should().ThrowAsync<StationNotFoundException>()
            .WithMessage("*station-123*");
    }

    [Fact]
    public async Task StartLivestreamAsync_WhenDeviceMissing_ShouldThrow()
    {
        using var client = new EufySecurityClient(Cfg());
        Func<Task> act = async () => await client.StartLivestreamAsync("device-123");
        await act.Should().ThrowAsync<DeviceNotFoundException>()
            .WithMessage("*device-123*");
    }

    [Fact]
    public async Task StopLivestreamAsync_WhenDeviceMissing_ShouldThrow()
    {
        using var client = new EufySecurityClient(Cfg());
        Func<Task> act = async () => await client.StopLivestreamAsync("device-123");
        await act.Should().ThrowAsync<DeviceNotFoundException>()
            .WithMessage("*device-123*");
    }

    [Fact]
    public async Task SetGuardModeAsync_WhenStationMissing_ShouldThrow()
    {
        using var client = new EufySecurityClient(Cfg());
        Func<Task> act = async () => await client.SetGuardModeAsync("station-xyz", GuardMode.Away);
        await act.Should().ThrowAsync<StationNotFoundException>()
            .WithMessage("*station-xyz*");
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var client = new EufySecurityClient(Cfg());
        client.Dispose();
        client.Invoking(c => c.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Version_Format_ShouldBeSemVerLike()
    {
        EufySecurityClient.Version.Should().MatchRegex(@"\d+\.\d+\.\d+");
    }
}