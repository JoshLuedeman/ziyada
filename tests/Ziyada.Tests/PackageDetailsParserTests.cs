using Ziyada.Services;

namespace Ziyada.Tests;

public class PackageDetailsParserTests
{
    [Fact]
    public void ParsePackageDetails_TypicalOutput_ParsesCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "Found Visual Studio Code [Microsoft.VisualStudioCode]",
            "Version: 1.85.1",
            "Publisher: Microsoft Corporation",
            "Description: Visual Studio Code is a lightweight but powerful source code editor.",
            "Homepage: https://code.visualstudio.com",
            "License: MIT",
            "License Url: https://code.visualstudio.com/license",
            "Source: winget",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("Visual Studio Code", result.Name);
        Assert.Equal("Microsoft.VisualStudioCode", result.Id);
        Assert.Equal("1.85.1", result.Version);
        Assert.Equal("Microsoft Corporation", result.Publisher);
        Assert.Equal("Visual Studio Code is a lightweight but powerful source code editor.", result.Description);
        Assert.Equal("https://code.visualstudio.com", result.Homepage);
        Assert.Equal("MIT", result.License);
        Assert.Equal("https://code.visualstudio.com/license", result.LicenseUrl);
        Assert.Equal("winget", result.Source);
    }

    [Fact]
    public void ParsePackageDetails_DependenciesDebug_ParsesCorrectly()
    {
        var output = "Dependencies: \n  Dep1\n  Dep2";
        var result = WingetParser.ParsePackageDetails(output);
        Assert.Equal(2, result.Dependencies.Count);
    }

    [Fact]
    public void ParsePackageDetails_WithDependencies_ParsesCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "Found Test App [Test.App]",
            "Version: 1.0.0",
            "Publisher: Test Publisher",
            "Dependencies:",
            "  Microsoft.VCRedist.2015+.x64 [>= 14.0.0.0]",
            "  Microsoft.DotNet.Runtime.8 [>= 8.0.0]",
            "Source: winget",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("Test App", result.Name);
        Assert.Equal("Test.App", result.Id);
        Assert.Equal(2, result.Dependencies.Count);
        Assert.Contains("Microsoft.VCRedist.2015+.x64 [>= 14.0.0.0]", result.Dependencies);
        Assert.Contains("Microsoft.DotNet.Runtime.8 [>= 8.0.0]", result.Dependencies);
    }

    [Fact]
    public void ParsePackageDetails_WithReleaseNotes_ParsesCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "Found Git [Git.Git]",
            "Version: 2.44.0",
            "Publisher: The Git Development Community",
            "Release Notes: Bug fixes and performance improvements",
            "Release Notes Url: https://github.com/git/git/releases/tag/v2.44.0",
            "Source: winget",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("Git", result.Name);
        Assert.Equal("Git.Git", result.Id);
        Assert.Equal("2.44.0", result.Version);
        Assert.Equal("Bug fixes and performance improvements", result.ReleaseNotes);
        Assert.Equal("https://github.com/git/git/releases/tag/v2.44.0", result.ReleaseNotesUrl);
    }

    [Fact]
    public void ParsePackageDetails_EmptyInput_ReturnsEmptyDetails()
    {
        var result = WingetParser.ParsePackageDetails("");

        Assert.Empty(result.Name);
        Assert.Empty(result.Id);
        Assert.Empty(result.Version);
        Assert.Empty(result.Dependencies);
    }

    [Fact]
    public void ParsePackageDetails_MinimalOutput_ParsesBasicFields()
    {
        var output = string.Join("\n", new[]
        {
            "Found App [App.Id]",
            "Version: 1.0.0",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("App", result.Name);
        Assert.Equal("App.Id", result.Id);
        Assert.Equal("1.0.0", result.Version);
        Assert.Empty(result.Publisher);
        Assert.Empty(result.Description);
    }

    [Fact]
    public void ParsePackageDetails_WindowsLineEndings_ParsesCorrectly()
    {
        var output = "Found Notepad++ [Notepad++.Notepad++]\r\n" +
                     "Version: 8.6.2\r\n" +
                     "Publisher: Don Ho\r\n";

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("Notepad++", result.Name);
        Assert.Equal("Notepad++.Notepad++", result.Id);
        Assert.Equal("8.6.2", result.Version);
        Assert.Equal("Don Ho", result.Publisher);
    }

    [Fact]
    public void ParsePackageDetails_PublisherUrl_SetsHomepage()
    {
        var output = string.Join("\n", new[]
        {
            "Found Test [Test.Id]",
            "Publisher Url: https://example.com",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("https://example.com", result.Homepage);
    }

    [Fact]
    public void ParsePackageDetails_DependenciesOnSameLine_ParsesCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "Found Test [Test.Id]",
            "Dependencies: SomeDep [>= 1.0.0]",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Single(result.Dependencies);
        Assert.Contains("SomeDep [>= 1.0.0]", result.Dependencies);
    }

    [Fact]
    public void ParsePackageDetails_MultilineDescription_ParsesFirstLine()
    {
        // winget show typically outputs only one line per field
        var output = string.Join("\n", new[]
        {
            "Found Test [Test.Id]",
            "Description: This is a test description",
            "Homepage: https://test.com",
        });

        var result = WingetParser.ParsePackageDetails(output);

        Assert.Equal("This is a test description", result.Description);
        Assert.Equal("https://test.com", result.Homepage);
    }
}
