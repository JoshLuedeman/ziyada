namespace Ziyada.Tests.Models;

using Ziyada.Models;

public class PackageTests
{
    [Fact]
    public void Package_DefaultValues_AreEmptyStrings()
    {
        var package = new Package();
        
        Assert.Equal(string.Empty, package.Name);
        Assert.Equal(string.Empty, package.Id);
        Assert.Equal(string.Empty, package.Version);
        Assert.Equal(string.Empty, package.Source);
        Assert.Equal(string.Empty, package.Match);
    }

    [Fact]
    public void Package_SetProperties_ReturnsCorrectValues()
    {
        var package = new Package
        {
            Name = "Visual Studio Code",
            Id = "Microsoft.VisualStudioCode",
            Version = "1.85.1",
            Source = "winget",
            Match = "Tag: vscode"
        };

        Assert.Equal("Visual Studio Code", package.Name);
        Assert.Equal("Microsoft.VisualStudioCode", package.Id);
        Assert.Equal("1.85.1", package.Version);
        Assert.Equal("winget", package.Source);
        Assert.Equal("Tag: vscode", package.Match);
    }

    [Fact]
    public void Package_CanSetNullProperties_ToEmptyString()
    {
        var package = new Package
        {
            Name = "Test",
            Id = "test.id"
        };
        
        // Version, Source, Match should still be empty strings
        Assert.Equal(string.Empty, package.Version);
        Assert.Equal(string.Empty, package.Source);
        Assert.Equal(string.Empty, package.Match);
    }
}
