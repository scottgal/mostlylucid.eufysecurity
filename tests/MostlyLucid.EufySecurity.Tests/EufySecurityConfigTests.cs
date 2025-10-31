using FluentAssertions;
using Microsoft.Extensions.Logging;
using MostlyLucid.EufySecurity.Common;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class EufySecurityConfigTests
{
    private static EufySecurityConfig Create(string? user = "user", string? pass = "pass") => new()
    {
        Username = user!,
        Password = pass!,
    };

    [Fact]
    public void Defaults_ShouldBeAsExpected()
    {
        var cfg = Create();

        cfg.Country.Should().Be("US");
        cfg.Language.Should().Be("en");
        cfg.TrustedDeviceName.Should().Be("EufySecurity.NET");
        cfg.PersistentDataPath.Should().BeNull();
        cfg.P2PConnectionSetup.Should().Be(P2PConnectionType.Quickest);
        cfg.PollingIntervalMinutes.Should().Be(10);
        cfg.AcceptInvitations.Should().BeTrue();
        cfg.EventDurationSeconds.Should().Be(10);
        cfg.Logger.Should().BeNull();
        cfg.Logging.Should().BeNull();
        cfg.EnableEmbeddedPKCS1Support.Should().BeFalse();
        cfg.Stations.Should().BeNull();
        cfg.Devices.Should().BeNull();
        cfg.DisableAutomaticCloudPolling.Should().BeFalse();
    }

    [Fact]
    public void CanOverride_Properties()
    {
        ILogger? logger = null;
        var cfg = new EufySecurityConfig
        {
            Username = "u",
            Password = "p",
            Country = "GB",
            Language = "fr",
            TrustedDeviceName = "TDN",
            PersistentDataPath = "C:/tmp",
            P2PConnectionSetup = P2PConnectionType.OnlyLocal,
            PollingIntervalMinutes = 42,
            AcceptInvitations = false,
            EventDurationSeconds = 20,
            Logger = logger,
            Logging = new LoggingConfig
            {
                Level = LogLevel.Debug,
                Categories = new() { new CategoryLogLevel { Category = "HTTP", Level = LogLevel.Trace } }
            },
            EnableEmbeddedPKCS1Support = true,
            Stations = new(),
            Devices = new(),
            DisableAutomaticCloudPolling = true,
        };

        cfg.Country.Should().Be("GB");
        cfg.Language.Should().Be("fr");
        cfg.TrustedDeviceName.Should().Be("TDN");
        cfg.PersistentDataPath.Should().Be("C:/tmp");
        cfg.P2PConnectionSetup.Should().Be(P2PConnectionType.OnlyLocal);
        cfg.PollingIntervalMinutes.Should().Be(42);
        cfg.AcceptInvitations.Should().BeFalse();
        cfg.EventDurationSeconds.Should().Be(20);
        cfg.Logger.Should().Be(logger);
        cfg.Logging.Should().NotBeNull();
        cfg.EnableEmbeddedPKCS1Support.Should().BeTrue();
        cfg.Stations.Should().NotBeNull();
        cfg.Devices.Should().NotBeNull();
        cfg.DisableAutomaticCloudPolling.Should().BeTrue();
    }

    [Fact]
    public void P2PConnectionType_Values_ShouldBeStable()
    {
        ((int)P2PConnectionType.OnlyLocal).Should().Be(0);
        ((int)P2PConnectionType.Quickest).Should().Be(1);
    }
}