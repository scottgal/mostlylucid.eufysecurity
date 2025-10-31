using System;
using FluentAssertions;
using MostlyLucid.EufySecurity.Push;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class PushMessageTests
{
    [Fact]
    public void Can_Construct_With_Required_Type_And_Defaults()
    {
        var ts = DateTime.UtcNow;
        var msg = new PushMessage
        {
            Type = PushEventTypes.Motion,
            Timestamp = ts,
        };

        msg.Type.Should().Be(PushEventTypes.Motion);
        msg.Timestamp.Should().Be(ts);
        msg.DeviceSerial.Should().BeNull();
        msg.StationSerial.Should().BeNull();
        msg.Title.Should().BeNull();
        msg.Message.Should().BeNull();
        msg.PersonDetected.Should().BeNull();
        msg.MotionDetected.Should().BeNull();
        msg.DoorbellRing.Should().BeNull();
        msg.PictureUrl.Should().BeNull();
        msg.PictureData.Should().BeNull();
        msg.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void Constants_Should_Be_Stable()
    {
        PushEventTypes.Motion.Should().Be("motion");
        PushEventTypes.PersonDetected.Should().Be("person");
        PushEventTypes.DoorbellRing.Should().Be("doorbell_ring");
        PushEventTypes.PetDetected.Should().Be("pet");
        PushEventTypes.SoundDetected.Should().Be("sound");
        PushEventTypes.CryingDetected.Should().Be("crying");
        PushEventTypes.VehicleDetected.Should().Be("vehicle");
        PushEventTypes.PackageDetected.Should().Be("package");
        PushEventTypes.LockLocked.Should().Be("lock_locked");
        PushEventTypes.LockUnlocked.Should().Be("lock_unlocked");
        PushEventTypes.AlarmTriggered.Should().Be("alarm");
    }
}