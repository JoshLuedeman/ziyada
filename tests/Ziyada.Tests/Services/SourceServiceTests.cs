namespace Ziyada.Tests.Services;

using Ziyada.Services;
using Ziyada.Tests.Mocks;

/// <summary>
/// Tests for SourceService focusing on cancellation behavior.
/// Full integration tests would require the winget executable.
/// </summary>
public class SourceServiceTests
{
    [Fact]
    public void SourceService_CanBeInstantiated()
    {
        var service = new SourceService();
        Assert.NotNull(service);
    }

    [Fact]
    public void SourceService_WithProcessHelper_CanBeInstantiated()
    {
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListSourcesAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListSourcesAsync(cts.Token));
    }

    [Fact]
    public async Task AddSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AddSourceAsync("test-source", "https://example.com", cts.Token));
    }

    [Fact]
    public async Task RemoveSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.RemoveSourceAsync("test-source", cts.Token));
    }
}
