using FluentAssertions;
using MostlyLucid.EufySecurity.Common;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class ExceptionsTests
{
    [Fact]
    public void DeviceNotFoundException_Sets_Message_And_Serial()
    {
        var ex = new DeviceNotFoundException("dev-1");
        ex.Message.Should().Contain("dev-1");
        ex.DeviceSerial.Should().Be("dev-1");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void StationNotFoundException_Sets_Message_And_Serial()
    {
        var ex = new StationNotFoundException("stn-1");
        ex.Message.Should().Contain("stn-1");
        ex.StationSerial.Should().Be("stn-1");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void ReadOnlyPropertyException_Sets_Message_And_Name()
    {
        var ex = new ReadOnlyPropertyException("propX");
        ex.Message.Should().Contain("propX");
        ex.PropertyName.Should().Be("propX");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void InvalidPropertyException_Sets_Message_And_Name()
    {
        var ex = new InvalidPropertyException("propY", "why");
        ex.Message.Should().Contain("propY");
        ex.Message.Should().Contain("why");
        ex.PropertyName.Should().Be("propY");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void LivestreamException_Can_Carry_Message()
    {
        var ex = new LivestreamException("oops");
        ex.Message.Should().Contain("oops");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void P2PConnectionException_Can_Carry_Message()
    {
        var ex = new P2PConnectionException("p2p");
        ex.Message.Should().Contain("p2p");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }

    [Fact]
    public void ApiException_Can_Carry_Status_And_Code()
    {
        var ex = new ApiException("bad", 500, "X42");
        ex.Message.Should().Contain("bad");
        ex.StatusCode.Should().Be(500);
        ex.ErrorCode.Should().Be("X42");
        ex.Should().BeAssignableTo<EufySecurityException>();
    }
}