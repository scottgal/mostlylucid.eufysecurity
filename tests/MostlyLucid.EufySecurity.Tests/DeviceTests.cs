using System;
using System.Collections.Generic;
using FluentAssertions;
using MostlyLucid.EufySecurity.Common;
using MostlyLucid.EufySecurity.Devices;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class DeviceTests
{
    private class TestDevice : Device
    {
        public TestDevice(string sn) : base(sn, "Cam", "C1", DeviceType.Camera, "station-1", null) { }
        public void SetPropertyInternal<T>(string name, T value)
        {
            var m = typeof(Device).GetMethod("SetPropertyValue", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(typeof(T));
            m.Invoke(this, new object?[] { name, value });
        }
    }

    [Fact]
    public void Defaults_AreReasonable()
    {
        var d = new TestDevice("dev-1");
        d.SerialNumber.Should().Be("dev-1");
        d.Name.Should().Be("Cam");
        d.Model.Should().Be("C1");
        d.HardwareVersion.Should().Be("");
        d.SoftwareVersion.Should().Be("");
        d.StationSerialNumber.Should().Be("station-1");
        d.HasProperty("enabled").Should().BeFalse();
        d.ToString().Should().Contain("TestDevice").And.Contain("dev-1").And.Contain("Cam").And.Contain("C1");
    }

    [Fact]
    public void Update_AppliesCommonFields()
    {
        var d = new TestDevice("dev-1");
        var data = new Dictionary<string, object>
        {
            ["device_name"] = "NewCam",
            ["main_hw_version"] = "1.2",
            ["main_sw_version"] = "3.4",
        };

        d.Update(data);

        d.Name.Should().Be("NewCam");
        d.HardwareVersion.Should().Be("1.2");
        d.SoftwareVersion.Should().Be("3.4");
    }

    [Fact]
    public void PropertyChanged_Fires_On_Change()
    {
        var d = new TestDevice("dev-1");
        PropertyChangedEventArgs? received = null;
        d.PropertyChanged += (_, e) => received = e;

        d.SetPropertyInternal("enabled", true);

        received.Should().NotBeNull();
        received!.PropertyName.Should().Be("enabled");
        received.OldValue.Should().BeNull();
        received.NewValue.Should().Be(true);
        d.HasProperty("enabled").Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidSerialNumber_ThrowsArgumentException(string? serialNumber)
    {
        // Act
        Action act = () => new IndoorCamera(serialNumber!, "Test", "Model", DeviceType.IndoorCamera, "STATION1");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Serial number*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStationSerialNumber_ThrowsArgumentException(string? stationSerialNumber)
    {
        // Act
        Action act = () => new IndoorCamera("DEVICE1", "Test", "Model", DeviceType.IndoorCamera, stationSerialNumber!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Station serial number*");
    }

    [Fact]
    public void Constructor_WithNullName_UsesUnknown()
    {
        // Act
        var device = new IndoorCamera("DEVICE1", null!, "Model", DeviceType.IndoorCamera, "STATION1");

        // Assert
        device.Name.Should().Be("Unknown");
    }

    [Fact]
    public void Constructor_WithNullModel_UsesUnknown()
    {
        // Act
        var device = new IndoorCamera("DEVICE1", "Test", null!, DeviceType.IndoorCamera, "STATION1");

        // Assert
        device.Model.Should().Be("Unknown");
    }

    [Fact]
    public void Update_WithNullData_DoesNotThrow()
    {
        // Arrange
        var device = new IndoorCamera("DEVICE1", "Test", "Model", DeviceType.IndoorCamera, "STATION1");

        // Act
        Action act = () => device.Update(null!);

        // Assert
        act.Should().NotThrow();
    }
}