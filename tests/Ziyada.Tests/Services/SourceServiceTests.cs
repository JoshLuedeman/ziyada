namespace Ziyada.Tests.Services;

using Ziyada.Services;

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
    public async Task ListSourcesAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var service = new SourceService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListSourcesAsync(cts.Token));
    }

    [Fact]
    public async Task AddSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var service = new SourceService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AddSourceAsync("test-source", "https://example.com", cts.Token));
    }

    [Fact]
    public async Task RemoveSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        var service = new SourceService();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.RemoveSourceAsync("test-source", cts.Token));
    }
}
