using System;
using FluentAssertions;
using MostlyLucid.EufySecurity.Devices;
using MostlyLucid.EufySecurity.Events;
using MostlyLucid.EufySecurity.P2P;
using MostlyLucid.EufySecurity.Push;
using MostlyLucid.EufySecurity.Stations;
using MostlyLucid.EufySecurity.Common;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class EventArgsTests
{
    private class DummyDevice : Device
    {
        public DummyDevice(string sn) : base(sn, "Name", "Model", DeviceType.Camera, "station-1", null) {}
    }

    private class DummyStation : Station
    {
        public DummyStation(string sn) : base(sn, "Station", "HB", DeviceType.Station, null) {}
    }

    [Fact]
    public void DeviceEventArgs_Holds_Device()
    {
        var d = new DummyDevice("d1");
        var args = new DeviceEventArgs(d);
        args.Device.Should().BeSameAs(d);
    }

    [Fact]
    public void StationEventArgs_Holds_Station()
    {
        var s = new DummyStation("s1");
        var args = new StationEventArgs(s);
        args.Station.Should().BeSameAs(s);
    }

    [Fact]
    public void PushNotificationEventArgs_Holds_Message()
    {
        var msg = new PushMessage { Type = PushEventTypes.Motion, Timestamp = DateTime.UtcNow };
        var args = new PushNotificationEventArgs(msg);
        args.Message.Should().BeSameAs(msg);
    }

    [Fact]
    public void LivestreamEventArgs_Hold_Refs()
    {
        var s = new DummyStation("s1");
        var d = new DummyDevice("d1");
        var meta = new StreamMetadata { VideoWidth = 1920, VideoHeight = 1080, VideoCodec = VideoCodec.H265, AudioCodec = AudioCodec.AAC };
        var args = new LivestreamEventArgs(s, d, meta);
        args.Station.Should().BeSameAs(s);
        args.Device.Should().BeSameAs(d);
        args.Metadata.Should().BeSameAs(meta);
    }

    [Fact]
    public void LivestreamDataEventArgs_Hold_Data()
    {
        var s = new DummyStation("s1");
        var d = new DummyDevice("d1");
        var data = new byte[] { 1, 2, 3 };
        var args = new LivestreamDataEventArgs(s, d, data, isVideo: true);
        args.Station.Should().BeSameAs(s);
        args.Device.Should().BeSameAs(d);
        args.Data.Should().BeSameAs(data);
        args.IsVideo.Should().BeTrue();
    }

    [Fact]
    public void GuardModeEventArgs_Hold_Values()
    {
        var s = new DummyStation("s1");
        var args = new GuardModeEventArgs(s, GuardMode.Home);
        args.Station.Should().BeSameAs(s);
        args.GuardMode.Should().Be(GuardMode.Home);
    }
}