using System;
using System.Collections.Generic;
using FluentAssertions;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Stations;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class StationTests
{
    private class TestStation : Station
    {
        public TestStation(string sn) : base(sn, "HomeBase", "HB2", DeviceType.Station, null) { }
        public void SetGuard(GuardMode mode) => typeof(Station)
            .GetProperty("GuardMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(this, mode);
        public void SetPropertyInternal<T>(string name, T value)
        {
            var m = typeof(Station).GetMethod("SetPropertyValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(typeof(T));
            m.Invoke(this, new object?[] { name, value });
        }
    }

    [Fact]
    public void Defaults_AreReasonable()
    {
        var s = new TestStation("station-1");
        s.SerialNumber.Should().Be("station-1");
        s.Name.Should().Be("HomeBase");
        s.Model.Should().Be("HB2");
        s.HardwareVersion.Should().Be("");
        s.SoftwareVersion.Should().Be("");
        s.IsConnected.Should().BeFalse();
        s.Devices.Should().NotBeNull();
        s.Devices.Should().BeEmpty();
    }

    [Fact]
    public void Update_AppliesKnownFields()
    {
        var s = new TestStation("station-1");
        var data = new Dictionary<string, object>
        {
            ["station_name"] = "My Base",
            ["main_hw_version"] = "1.0",
            ["main_sw_version"] = "2.0",
            ["ip_addr"] = "1.2.3.4",
            ["mac_address"] = "AA:BB",
            ["lan_ip_addr"] = "192.168.0.2",
        };

        s.Update(data);

        s.Name.Should().Be("My Base");
        s.HardwareVersion.Should().Be("1.0");
        s.SoftwareVersion.Should().Be("2.0");
        s.IpAddress.Should().Be("1.2.3.4");
        s.MacAddress.Should().Be("AA:BB");
        s.LanIpAddress.Should().Be("192.168.0.2");
    }

    [Fact]
    public void PropertyChanged_Fires_On_Change()
    {
        var s = new TestStation("station-1");
        PropertyChangedEventArgs? received = null;
        s.PropertyChanged += (_, e) => received = e;

        s.SetPropertyInternal("custom", 123);

        received.Should().NotBeNull();
        received!.PropertyName.Should().Be("custom");
        received.OldValue.Should().BeNull();
        received.NewValue.Should().Be(123);
    }

    [Fact]
    public void GuardModeChanged_Fires_When_GuardMode_Changes()
    {
        var s = new TestStation("station-1");
        GuardMode? mode = null;
        s.GuardModeChanged += (_, m) => mode = m;

        s.SetPropertyInternal("guardMode", GuardMode.Away);

        mode.Should().Be(GuardMode.Away);
    }
}