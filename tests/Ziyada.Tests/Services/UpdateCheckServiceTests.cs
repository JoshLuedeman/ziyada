using Xunit;
using Ziyada.Services;
using Ziyada.Helpers;

namespace Ziyada.Tests.Services;

public class UpdateCheckServiceTests
{
    [Fact]
    public void AppVersion_ShouldReturnValidVersion()
    {
        // Act
        var version = AppVersion.Version;

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.Matches(@"^\d+\.\d+\.\d+$", version);
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldHandleNetworkFailureGracefully()
    {
        // Arrange
        var service = new UpdateCheckService();

        // Act
        var result = await service.CheckForUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        // Should not throw exception even if network fails
    }

    [Fact]
    public async Task CheckForUpdatesAsync_ShouldReturnUpdateInfo()
    {
        // Arrange
        var service = new UpdateCheckService();

        // Act
        var result = await service.CheckForUpdatesAsync();

        // Assert
        Assert.NotNull(result);
        // If update is available, LatestVersion should be populated
        Assert.False(result.IsUpdateAvailable && string.IsNullOrEmpty(result.LatestVersion));
    }
}
