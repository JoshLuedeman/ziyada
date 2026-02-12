namespace Ziyada.Tests.Models;

using Ziyada.Models;

public class InstalledPackageTests
{
    [Fact]
    public void InstalledPackage_DefaultValues_AreEmptyStrings()
    {
        var package = new InstalledPackage();
        
        Assert.Equal(string.Empty, package.Name);
        Assert.Equal(string.Empty, package.Id);
        Assert.Equal(string.Empty, package.Version);
        Assert.Equal(string.Empty, package.AvailableVersion);
        Assert.Equal(string.Empty, package.Source);
    }

    [Fact]
    public void InstalledPackage_HasUpgrade_TrueWhenAvailableVersionSet()
    {
        var package = new InstalledPackage
        {
            Name = "Git",
            Id = "Git.Git",
            Version = "2.43.0",
            AvailableVersion = "2.44.0",
            Source = "winget"
        };

        Assert.True(package.HasUpgrade);
    }

    [Fact]
    public void InstalledPackage_HasUpgrade_FalseWhenAvailableVersionEmpty()
    {
        var package = new InstalledPackage
        {
            Name = "Node.js",
            Id = "OpenJS.NodeJS.LTS",
            Version = "20.10.0",
            AvailableVersion = "",
            Source = "winget"
        };

        Assert.False(package.HasUpgrade);
    }

    [Fact]
    public void InstalledPackage_HasUpgrade_FalseWhenAvailableVersionNull()
    {
        var package = new InstalledPackage
        {
            Name = "Test App",
            Id = "test.app",
            Version = "1.0.0"
        };

        // AvailableVersion defaults to empty string, not null
        Assert.False(package.HasUpgrade);
    }

    [Fact]
    public void InstalledPackage_HasUpgrade_TrueWhenAvailableVersionWhitespace()
    {
        // Whitespace-only string is not null or empty, so HasUpgrade returns true
        // This documents the actual behavior - whitespace isn't trimmed
        var package = new InstalledPackage
        {
            AvailableVersion = "   "
        };

        Assert.True(package.HasUpgrade);
    }

    [Fact]
    public void InstalledPackage_SetProperties_ReturnsCorrectValues()
    {
        var package = new InstalledPackage
        {
            Name = "PowerShell",
            Id = "Microsoft.PowerShell",
            Version = "7.4.0",
            AvailableVersion = "7.4.1",
            Source = "winget"
        };

        Assert.Equal("PowerShell", package.Name);
        Assert.Equal("Microsoft.PowerShell", package.Id);
        Assert.Equal("7.4.0", package.Version);
        Assert.Equal("7.4.1", package.AvailableVersion);
        Assert.Equal("winget", package.Source);
    }
}
