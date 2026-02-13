namespace Ziyada.Tests.Services;

using Ziyada.Helpers;
using Ziyada.Services;
using Ziyada.Tests.Mocks;

/// <summary>
/// Tests for WingetService focusing on input validation and cancellation behavior.
/// These tests verify behavior without requiring the actual winget executable.
/// </summary>
public class WingetServiceTests
{
    [Fact]
    public void WingetService_CanBeInstantiated()
    {
        var service = new WingetService();
        Assert.NotNull(service);
    }

    [Fact]
    public void WingetService_WithProcessHelper_CanBeInstantiated()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task SearchAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Searching with an already-cancelled token should throw
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.SearchAsync("test", cts.Token));
    }

    [Fact]
    public async Task InstallAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.InstallAsync("test.package", cts.Token));
    }

    [Fact]
    public async Task ListInstalledAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListInstalledAsync(false, cts.Token));
    }

    [Fact]
    public async Task ListUpgradesAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListUpgradesAsync(cts.Token));
    }

    [Fact]
    public async Task UpgradeAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.UpgradeAsync("test.package", cts.Token));
    }

    [Fact]
    public async Task UpgradeAllAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.UpgradeAllAsync(cts.Token));
    }

    [Fact]
    public async Task UninstallAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.UninstallAsync("test.package", cts.Token));
    }

    [Fact]
    public async Task ExportAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ExportAsync("/tmp/export.json", cts.Token));
    }

    [Fact]
    public async Task ImportAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync("/tmp/import.json", cts.Token));
    }

    [Fact]
    public async Task PinAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.PinAsync("test.package", cts.Token));
    }

    [Fact]
    public async Task UnpinAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.UnpinAsync("test.package", cts.Token));
    }

    [Fact]
    public async Task ListPinnedAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListPinnedAsync(cts.Token));
    }
}
