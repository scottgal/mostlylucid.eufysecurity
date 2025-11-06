using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MostlyLucid.EufySecurity.Push;
using Xunit;

namespace MostlyLucid.EufySecurity.Tests;

public class PushNotificationServiceTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var service = new PushNotificationService();

        // Assert
        service.Should().NotBeNull();
        service.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task StartAsync_SetsConnectedState()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act
        await service.StartAsync();

        // Assert
        service.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_RaisesConnectionStateChangedEvent()
    {
        // Arrange
        var service = new PushNotificationService();
        bool? connectionState = null;
        service.ConnectionStateChanged += (sender, state) => connectionState = state;

        // Act
        await service.StartAsync();

        // Assert
        connectionState.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_SetsDisconnectedState()
    {
        // Arrange
        var service = new PushNotificationService();
        await service.StartAsync();

        // Act
        await service.StopAsync();

        // Assert
        service.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_RaisesConnectionStateChangedEvent()
    {
        // Arrange
        var service = new PushNotificationService();
        await service.StartAsync();

        bool? connectionState = null;
        service.ConnectionStateChanged += (sender, state) => connectionState = state;

        // Act
        await service.StopAsync();

        // Assert
        connectionState.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotStarted_DoesNotThrow()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act
        Func<Task> act = async () => await service.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StartAsync_WithCancellation_RespectsCancellationToken()
    {
        // Arrange
        var service = new PushNotificationService();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await service.StartAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StopAsync_WithCancellation_RespectsCancellationToken()
    {
        // Arrange
        var service = new PushNotificationService();
        await service.StartAsync();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await service.StopAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Dispose_SetsDisconnectedState()
    {
        // Arrange
        var service = new PushNotificationService();
        service.StartAsync().Wait();

        // Act
        service.Dispose();

        // Assert
        service.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act
        service.Dispose();
        Action act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_WhenNotStarted_DoesNotThrow()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act
        Action act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IsConnected_InitiallyFalse()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act & Assert
        service.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task IsConnected_TrueAfterStart()
    {
        // Arrange
        var service = new PushNotificationService();

        // Act
        await service.StartAsync();

        // Assert
        service.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnected_FalseAfterStop()
    {
        // Arrange
        var service = new PushNotificationService();
        await service.StartAsync();

        // Act
        await service.StopAsync();

        // Assert
        service.IsConnected.Should().BeFalse();
    }
}
