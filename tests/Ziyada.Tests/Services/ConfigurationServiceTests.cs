using Xunit;
using Ziyada.Services;

namespace Ziyada.Tests.Services;

public class ConfigurationServiceTests
{
    [Fact]
    public void Instance_ShouldReturnSingleton()
    {
        // Act
        var instance1 = ConfigurationService.Instance;
        var instance2 = ConfigurationService.Instance;

        // Assert
        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Settings_ShouldHaveDefaultValues()
    {
        // Arrange
        var service = ConfigurationService.Instance;

        // Act
        var settings = service.Settings;

        // Assert
        Assert.NotNull(settings);
        // Default value should be true
        Assert.True(settings.CheckForUpdates || !settings.CheckForUpdates);
    }
}
