namespace Ziyada.Tests.Models;

using Ziyada.Models;

public class SourceInfoTests
{
    [Fact]
    public void SourceInfo_DefaultValues_AreEmptyStrings()
    {
        var source = new SourceInfo();
        
        Assert.Equal(string.Empty, source.Name);
        Assert.Equal(string.Empty, source.Argument);
        Assert.Equal(string.Empty, source.Type);
    }

    [Fact]
    public void SourceInfo_SetProperties_ReturnsCorrectValues()
    {
        var source = new SourceInfo
        {
            Name = "winget",
            Argument = "https://cdn.winget.microsoft.com/cache",
            Type = "Microsoft.PreIndexed.Package"
        };

        Assert.Equal("winget", source.Name);
        Assert.Equal("https://cdn.winget.microsoft.com/cache", source.Argument);
        Assert.Equal("Microsoft.PreIndexed.Package", source.Type);
    }

    [Fact]
    public void SourceInfo_MSStore_ParsedCorrectly()
    {
        var source = new SourceInfo
        {
            Name = "msstore",
            Argument = "https://storeedgefd.dsx.mp.microsoft.com/v9.0",
            Type = "Microsoft.Rest"
        };

        Assert.Equal("msstore", source.Name);
        Assert.Contains("storeedgefd", source.Argument);
        Assert.Equal("Microsoft.Rest", source.Type);
    }

    [Fact]
    public void SourceInfo_CustomSource_CanBeCreated()
    {
        var source = new SourceInfo
        {
            Name = "my-custom-source",
            Argument = "https://example.com/winget/index.json",
            Type = "Microsoft.PreIndexed.Package"
        };

        Assert.Equal("my-custom-source", source.Name);
        Assert.Equal("https://example.com/winget/index.json", source.Argument);
    }
}
