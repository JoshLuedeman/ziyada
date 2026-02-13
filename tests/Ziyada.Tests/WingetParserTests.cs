using Ziyada.Services;

namespace Ziyada.Tests;

public class WingetParserTests
{
    #region ParseTable Tests

    [Fact]
    public void ParseTable_SearchOutput_ReturnsCorrectColumns()
    {
        var output = string.Join("\n", new[]
        {
            "-",
            "\\",
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
            "Visual Studio Code              Microsoft.VisualStudioCode  1.85.1    winget",
            "Visual Studio Code Insiders     Microsoft.VisualStudioCode.Insiders  1.86.0  winget",
        });

        var result = WingetParser.ParseTable(output);

        Assert.Equal(2, result.Count);
        Assert.Equal("Visual Studio Code", result[0]["Name"]);
        Assert.Equal("Microsoft.VisualStudioCode", result[0]["Id"]);
        Assert.Equal("1.85.1", result[0]["Version"]);
        Assert.Equal("winget", result[0]["Source"]);
    }

    [Fact]
    public void ParseTable_EmptyInput_ReturnsEmptyList()
    {
        var result = WingetParser.ParseTable("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_OnlySpinnerChars_ReturnsEmptyList()
    {
        var output = "-\n\\\n|\n/\n-\n";
        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_NoSeparatorLine_ReturnsEmptyList()
    {
        var output = "Name  Id  Version\nSome data here\n";
        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_HeaderOnlyNoDataRows_ReturnsEmptyList()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_HeaderAndSeparatorWithBlankDataRows_ReturnsEmptyList()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
            "",
            "   ",
            "",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_SeparatorTooShort_ReturnsEmptyList()
    {
        var output = string.Join("\n", new[]
        {
            "Name  Id  Version",
            "---",
            "Foo   Bar 1.0",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseTable_SeparatorExactly20Dashes_IsDetected()
    {
        var output = string.Join("\n", new[]
        {
            "Name        Id        ",
            "--------------------",
            "TestApp     test.id   ",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Single(result);
        Assert.Equal("TestApp", result[0]["Name"]);
        Assert.Equal("test.id", result[0]["Id"]);
    }

    [Fact]
    public void ParseTable_SpinnerLinesBeforeHeader_AreIgnored()
    {
        var output = string.Join("\n", new[]
        {
            "-",
            "\\",
            "|",
            "/",
            "-",
            "Name                            Id                          Version",
            "------------------------------------------------------------------------",
            "Git                             Git.Git                     2.43.0",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Single(result);
        Assert.Equal("Git", result[0]["Name"]);
        Assert.Equal("Git.Git", result[0]["Id"]);
        Assert.Equal("2.43.0", result[0]["Version"]);
    }

    [Fact]
    public void ParseTable_WindowsLineEndings_ParsedCorrectly()
    {
        var output = "Name                  Id                  Version\r\n" +
                     "----------------------------------------------------\r\n" +
                     "Notepad++             Notepad++.Notepad++ 8.6.2\r\n";

        var result = WingetParser.ParseTable(output);
        Assert.Single(result);
        Assert.Equal("Notepad++", result[0]["Name"]);
        Assert.Equal("8.6.2", result[0]["Version"]);
    }

    [Fact]
    public void ParseTable_ShortDataLine_ReturnsEmptyForMissingColumns()
    {
        var output = string.Join("\n", new[]
        {
            "Name                  Id                  Version             Source",
            "----------------------------------------------------------------------",
            "Short",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Single(result);
        Assert.Equal("Short", result[0]["Name"]);
        Assert.Equal("", result[0]["Id"]);
        Assert.Equal("", result[0]["Version"]);
        Assert.Equal("", result[0]["Source"]);
    }

    #endregion

    #region ParseSearchResults Tests

    [Fact]
    public void ParseSearchResults_TypicalOutput_ReturnsPackages()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                                   Version  Match            Source",
            "------------------------------------------------------------------------------------------------------",
            "Visual Studio Code              Microsoft.VisualStudioCode           1.85.1   Tag: vscode      winget",
            "VSCodium                        VSCodium.VSCodium                    1.85.1   Tag: vscode      winget",
        });

        var result = WingetParser.ParseSearchResults(output);

        Assert.Equal(2, result.Count);
        Assert.Equal("Visual Studio Code", result[0].Name);
        Assert.Equal("Microsoft.VisualStudioCode", result[0].Id);
        Assert.Equal("1.85.1", result[0].Version);
        Assert.Equal("winget", result[0].Source);
        Assert.Equal("Tag: vscode", result[0].Match);

        Assert.Equal("VSCodium", result[1].Name);
        Assert.Equal("VSCodium.VSCodium", result[1].Id);
    }

    [Fact]
    public void ParseSearchResults_EmptyInput_ReturnsEmptyList()
    {
        var result = WingetParser.ParseSearchResults("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSearchResults_FiltersRowsWithEmptyId()
    {
        var output = string.Join("\n", new[]
        {
            "Name                  Id                  Version   Source",
            "------------------------------------------------------------",
            "Valid App             valid.id            1.0.0     winget",
            "                                          2.0.0     winget",
        });

        var result = WingetParser.ParseSearchResults(output);
        Assert.Single(result);
        Assert.Equal("valid.id", result[0].Id);
    }

    [Fact]
    public void ParseSearchResults_WithSpinnerPrefix_ParsesCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "-",
            "\\",
            "|",
            "/",
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
            "7-Zip                           7zip.7zip                   23.01     winget",
        });

        var result = WingetParser.ParseSearchResults(output);
        Assert.Single(result);
        Assert.Equal("7-Zip", result[0].Name);
        Assert.Equal("7zip.7zip", result[0].Id);
        Assert.Equal("23.01", result[0].Version);
        Assert.Equal("winget", result[0].Source);
    }

    [Fact]
    public void ParseSearchResults_MultipleResults_PreservesOrder()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                               Version   Source",
            "------------------------------------------------------------------------------------",
            "Python 3.12                     Python.Python.3.12               3.12.1    winget",
            "Python 3.11                     Python.Python.3.11               3.11.7    winget",
            "Python 3.10                     Python.Python.3.10               3.10.11   winget",
        });

        var result = WingetParser.ParseSearchResults(output);
        Assert.Equal(3, result.Count);
        Assert.Equal("Python.Python.3.12", result[0].Id);
        Assert.Equal("Python.Python.3.11", result[1].Id);
        Assert.Equal("Python.Python.3.10", result[2].Id);
    }

    #endregion

    #region ParseInstalledPackages Tests

    [Fact]
    public void ParseInstalledPackages_TypicalOutput_ReturnsInstalledPackages()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                              Version         Available       Source",
            "------------------------------------------------------------------------------------------------------",
            "Git                             Git.Git                         2.43.0          2.44.0          winget",
            "Node.js                         OpenJS.NodeJS.LTS               20.10.0                         winget",
        });

        var result = WingetParser.ParseInstalledPackages(output);

        Assert.Equal(2, result.Count);
        Assert.Equal("Git", result[0].Name);
        Assert.Equal("Git.Git", result[0].Id);
        Assert.Equal("2.43.0", result[0].Version);
        Assert.Equal("2.44.0", result[0].AvailableVersion);
        Assert.Equal("winget", result[0].Source);
        Assert.True(result[0].HasUpgrade);

        Assert.Equal("Node.js", result[1].Name);
        Assert.Equal("OpenJS.NodeJS.LTS", result[1].Id);
        Assert.Equal("20.10.0", result[1].Version);
        Assert.False(result[1].HasUpgrade);
    }

    [Fact]
    public void ParseInstalledPackages_EmptyInput_ReturnsEmptyList()
    {
        var result = WingetParser.ParseInstalledPackages("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseInstalledPackages_FiltersRowsWithEmptyId()
    {
        var output = string.Join("\n", new[]
        {
            "Name                  Id                  Version         Available       Source",
            "----------------------------------------------------------------------------------",
            "Valid App             valid.id            1.0.0                           winget",
            "                                          2.0.0                           winget",
        });

        var result = WingetParser.ParseInstalledPackages(output);
        Assert.Single(result);
        Assert.Equal("valid.id", result[0].Id);
    }

    [Fact]
    public void ParseInstalledPackages_NoAvailableVersion_HasUpgradeIsFalse()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                              Version         Available       Source",
            "------------------------------------------------------------------------------------------------------",
            "Some Tool                       Some.Tool                       1.0.0                           winget",
        });

        var result = WingetParser.ParseInstalledPackages(output);
        Assert.Single(result);
        Assert.Equal("", result[0].AvailableVersion);
        Assert.False(result[0].HasUpgrade);
    }

    #endregion

    #region ParseUpgradeList Tests

    [Fact]
    public void ParseUpgradeList_TypicalOutput_ReturnsPackagesWithUpgrades()
    {
        var output = string.Join("\n", new[]
        {
            "-",
            "\\",
            "|",
            "Name                            Id                              Version         Available       Source",
            "------------------------------------------------------------------------------------------------------",
            "Git                             Git.Git                         2.43.0          2.44.0          winget",
            "Visual Studio Code              Microsoft.VisualStudioCode      1.85.0          1.85.1          winget",
            "PowerShell                      Microsoft.PowerShell            7.4.0           7.4.1           winget",
        });

        var result = WingetParser.ParseUpgradeList(output);

        Assert.Equal(3, result.Count);

        Assert.Equal("Git.Git", result[0].Id);
        Assert.Equal("2.43.0", result[0].Version);
        Assert.Equal("2.44.0", result[0].AvailableVersion);
        Assert.True(result[0].HasUpgrade);

        Assert.Equal("Microsoft.VisualStudioCode", result[1].Id);
        Assert.Equal("Microsoft.PowerShell", result[2].Id);
    }

    [Fact]
    public void ParseUpgradeList_EmptyInput_ReturnsEmptyList()
    {
        var result = WingetParser.ParseUpgradeList("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseUpgradeList_NoUpgrades_ReturnsEmptyList()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                              Version         Available       Source",
            "------------------------------------------------------------------------------------------------------",
        });

        var result = WingetParser.ParseUpgradeList(output);
        Assert.Empty(result);
    }

    #endregion

    #region ParseSources Tests

    [Fact]
    public void ParseSources_TypicalOutput_ReturnsSources()
    {
        var output = string.Join("\n", new[]
        {
            "Name    Argument                                       Type",
            "---------------------------------------------------------------",
            "winget  https://cdn.winget.microsoft.com/cache          Microsoft.PreIndexed.Package",
            "msstore https://storeedgefd.dsx.mp.microsoft.com/v9.0   Microsoft.Rest",
        });

        var result = WingetParser.ParseSources(output);

        Assert.Equal(2, result.Count);
        Assert.Equal("winget", result[0].Name);
        Assert.Equal("https://cdn.winget.microsoft.com/cache", result[0].Argument);
        Assert.Equal("Microsoft.PreIndexed.Package", result[0].Type);

        Assert.Equal("msstore", result[1].Name);
        Assert.Equal("https://storeedgefd.dsx.mp.microsoft.com/v9.0", result[1].Argument);
        Assert.Equal("Microsoft.Rest", result[1].Type);
    }

    [Fact]
    public void ParseSources_WithUrlColumn_FallsBackToUrl()
    {
        var output = string.Join("\n", new[]
        {
            "Name    URL                                            Type",
            "---------------------------------------------------------------",
            "winget  https://cdn.winget.microsoft.com/cache          Microsoft.PreIndexed.Package",
        });

        var result = WingetParser.ParseSources(output);
        Assert.Single(result);
        Assert.Equal("winget", result[0].Name);
        Assert.Equal("https://cdn.winget.microsoft.com/cache", result[0].Argument);
    }

    [Fact]
    public void ParseSources_EmptyInput_ReturnsEmptyList()
    {
        var result = WingetParser.ParseSources("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParseSources_FiltersRowsWithEmptyName()
    {
        var output = string.Join("\n", new[]
        {
            "Name    Argument                                       Type",
            "---------------------------------------------------------------",
            "winget  https://cdn.winget.microsoft.com/cache          Microsoft.PreIndexed.Package",
            "        https://example.com                              SomeType",
        });

        var result = WingetParser.ParseSources(output);
        Assert.Single(result);
        Assert.Equal("winget", result[0].Name);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ParseTable_SeparatorWithSpacesAndDashes_IsDetected()
    {
        var output = string.Join("\n", new[]
        {
            "Name          Id            Version",
            "---- -------- ------------- -------",
            "TestApp       test.app      1.0.0",
        });

        var result = WingetParser.ParseTable(output);
        Assert.Single(result);
    }

    [Fact]
    public void ParseTable_MultipleSeparatorLikeLinesPicksFirst()
    {
        var output = string.Join("\n", new[]
        {
            "Name                  Id                  Version",
            "----------------------------------------------------",
            "App1                  app.one             1.0.0",
            "----------------------------------------------------",
            "App2                  app.two             2.0.0",
        });

        var result = WingetParser.ParseTable(output);
        // First separator is picked; rows after it (including second separator-like line) are data
        Assert.True(result.Count >= 1);
        Assert.Equal("App1", result[0]["Name"]);
    }

    [Fact]
    public void ParseSearchResults_TrailingNewlines_HandledGracefully()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
            "Firefox                         Mozilla.Firefox             121.0     winget",
            "",
            "",
            "",
        });

        var result = WingetParser.ParseSearchResults(output);
        Assert.Single(result);
        Assert.Equal("Mozilla.Firefox", result[0].Id);
    }

    [Fact]
    public void ParseTable_OnlySeparatorNoHeader_ReturnsEmptyList()
    {
        // sepIndex would be 0, which is < 1, so returns empty
        var output = "--------------------------------------\nSome data line\n";
        var result = WingetParser.ParseTable(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseInstalledPackages_RealisticWingetListOutput()
    {
        var output = string.Join("\n", new[]
        {
            "Name                                   Id                                    Version          Available        Source",
            "--------------------------------------------------------------------------------------------------------------------------",
            "Microsoft Visual C++ 2015-2022 Redu…   Microsoft.VCRedist.2015+.x64          14.38.33130.0    14.38.33135.0    winget",
            "Microsoft .NET SDK 8.0.404 (x64)       Microsoft.DotNet.SDK.8                8.0.404                           winget",
            "Windows Terminal                        Microsoft.WindowsTerminal             1.19.3172.0      1.20.3171.0      winget",
        });

        var result = WingetParser.ParseInstalledPackages(output);

        Assert.Equal(3, result.Count);

        Assert.Contains(result, p => p.Id == "Microsoft.VCRedist.2015+.x64" && p.HasUpgrade);
        Assert.Contains(result, p => p.Id == "Microsoft.DotNet.SDK.8" && !p.HasUpgrade);
        Assert.Contains(result, p => p.Id == "Microsoft.WindowsTerminal" && p.HasUpgrade);
    }

    [Fact]
    public void ParseSearchResults_SingleResult_ParsedCorrectly()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                          Version   Source",
            "------------------------------------------------------------------------------",
            "Notepad++                       Notepad++.Notepad++         8.6.2     winget",
        });

        var result = WingetParser.ParseSearchResults(output);
        Assert.Single(result);
        Assert.Equal("Notepad++", result[0].Name);
        Assert.Equal("Notepad++.Notepad++", result[0].Id);
        Assert.Equal("8.6.2", result[0].Version);
        Assert.Equal("winget", result[0].Source);
    }

    #endregion

    #region ParsePinnedPackages Tests

    [Fact]
    public void ParsePinnedPackages_TypicalOutput_ReturnsPinnedIds()
    {
        var output = string.Join("\n", new[]
        {
            "Package                         Version",
            "---------------------------------------",
            "Git.Git                         2.43.0",
            "Microsoft.VisualStudioCode      1.85.0",
        });

        var result = WingetParser.ParsePinnedPackages(output);

        Assert.Equal(2, result.Count);
        Assert.Contains("Git.Git", result);
        Assert.Contains("Microsoft.VisualStudioCode", result);
    }

    [Fact]
    public void ParsePinnedPackages_EmptyOutput_ReturnsEmptyList()
    {
        var result = WingetParser.ParsePinnedPackages("");
        Assert.Empty(result);
    }

    [Fact]
    public void ParsePinnedPackages_NoData_ReturnsEmptyList()
    {
        var output = string.Join("\n", new[]
        {
            "Package                         Version",
            "---------------------------------------",
        });

        var result = WingetParser.ParsePinnedPackages(output);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseInstalledPackages_WithPinnedColumn_DetectsPinStatus()
    {
        var output = string.Join("\n", new[]
        {
            "Name                            Id                              Version         Available       Source  Pinned",
            "--------------------------------------------------------------------------------------------------------------",
            "Git                             Git.Git                         2.43.0          2.44.0          winget  < 2.44",
            "Node.js                         OpenJS.NodeJS.LTS               20.10.0         20.11.0         winget",
        });

        var result = WingetParser.ParseInstalledPackages(output);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsPinned);
        Assert.False(result[1].IsPinned);
    }

    #endregion
}
