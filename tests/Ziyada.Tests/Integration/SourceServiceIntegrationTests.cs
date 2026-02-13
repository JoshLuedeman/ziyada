using Ziyada.Helpers;
using Ziyada.Services;
using Ziyada.Tests.Mocks;

namespace Ziyada.Tests.Integration;

/// <summary>
/// Integration tests for SourceService using mocked ProcessHelper.
/// Tests the full service pipeline for source management operations.
/// </summary>
public class SourceServiceIntegrationTests
{
    #region List Sources Tests

    [Fact]
    public async Task ListSourcesAsync_WithSources_ReturnsSources()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.ListSources,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var results = await service.ListSourcesAsync();

        // Assert
        Assert.Equal(2, results.Count);
        
        var winget = results[0];
        Assert.Equal("winget", winget.Name);
        Assert.Equal("https://cdn.winget.microsoft.com/cache", winget.Argument);
        Assert.Equal("Microsoft.PreIndexed.Package", winget.Type);

        var msstore = results[1];
        Assert.Equal("msstore", msstore.Name);
        Assert.Equal("https://storeedgefd.dsx.mp.microsoft.com/v9.0", msstore.Argument);
        Assert.Equal("Microsoft.Rest", msstore.Type);
    }

    [Fact]
    public async Task ListSourcesAsync_NoSources_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var noSourcesOutput = string.Join("\n", new[]
        {
            "Name    Argument                                       Type",
            "---------------------------------------------------------------",
        });
        mockHelper.SetResponse("source list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = noSourcesOutput,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var results = await service.ListSourcesAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ListSourcesAsync_NonZeroExitCode_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source list", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Command failed"
        });
        var service = new SourceService(mockHelper);

        // Act
        var results = await service.ListSourcesAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ListSourcesAsync_MalformedOutput_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.MalformedOutput,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var results = await service.ListSourcesAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ListSourcesAsync_EmptyOutput_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.EmptyOutput,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var results = await service.ListSourcesAsync();

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region Add Source Tests

    [Fact]
    public async Task AddSourceAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source add", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.SourceAddSuccess,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var result = await service.AddSourceAsync("custom-source", "https://example.com/packages");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Source added successfully", result.StandardOutput);
    }

    [Fact]
    public async Task AddSourceAsync_Failure_ReturnsFailureResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source add", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Failed to add source: Source already exists"
        });
        var service = new SourceService(mockHelper);

        // Act
        var result = await service.AddSourceAsync("winget", "https://example.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("already exists", result.StandardError);
    }

    [Fact]
    public async Task AddSourceAsync_InvalidUrl_ReturnsFailureResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source add", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Invalid source URL"
        });
        var service = new SourceService(mockHelper);

        // Act
        var result = await service.AddSourceAsync("test", "invalid-url");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid", result.StandardError);
    }

    #endregion

    #region Remove Source Tests

    [Fact]
    public async Task RemoveSourceAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source remove", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.SourceRemoveSuccess,
            StandardError = string.Empty
        });
        var service = new SourceService(mockHelper);

        // Act
        var result = await service.RemoveSourceAsync("custom-source");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Source removed successfully", result.StandardOutput);
    }

    [Fact]
    public async Task RemoveSourceAsync_NonExistentSource_ReturnsFailureResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("source remove", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Source not found"
        });
        var service = new SourceService(mockHelper);

        // Act
        var result = await service.RemoveSourceAsync("nonexistent");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.StandardError);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task ListSourcesAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListSourcesAsync(cts.Token));
    }

    [Fact]
    public async Task AddSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.AddSourceAsync("test", "https://example.com", cts.Token));
    }

    [Fact]
    public async Task RemoveSourceAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var service = new SourceService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.RemoveSourceAsync("test", cts.Token));
    }

    #endregion

    #region Command Verification Tests

    [Fact]
    public async Task AddSourceAsync_GeneratesCorrectCommand()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetDefaultResponse(new ProcessResult { ExitCode = 0, StandardOutput = "OK" });
        var service = new SourceService(mockHelper);

        // Act
        await service.AddSourceAsync("my-source", "https://packages.example.com");

        // Assert
        Assert.Single(mockHelper.ExecutedCommands);
        var command = mockHelper.ExecutedCommands[0];
        Assert.Contains("source add", command);
        Assert.Contains("--name \"my-source\"", command);
        Assert.Contains("--arg \"https://packages.example.com\"", command);
    }

    [Fact]
    public async Task RemoveSourceAsync_GeneratesCorrectCommand()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetDefaultResponse(new ProcessResult { ExitCode = 0, StandardOutput = "OK" });
        var service = new SourceService(mockHelper);

        // Act
        await service.RemoveSourceAsync("my-source");

        // Assert
        Assert.Single(mockHelper.ExecutedCommands);
        var command = mockHelper.ExecutedCommands[0];
        Assert.Contains("source remove", command);
        Assert.Contains("--name \"my-source\"", command);
    }

    #endregion
}
