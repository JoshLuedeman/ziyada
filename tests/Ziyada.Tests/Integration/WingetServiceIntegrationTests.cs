using Ziyada.Helpers;
using Ziyada.Services;
using Ziyada.Tests.Mocks;

namespace Ziyada.Tests.Integration;

/// <summary>
/// Integration tests for WingetService using mocked ProcessHelper.
/// Tests the full service pipeline: search → parse → return models.
/// </summary>
public class WingetServiceIntegrationTests
{
    #region Search Tests

    [Fact]
    public async Task SearchAsync_WithResults_ReturnsPackages()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("search", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.SearchResults,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.SearchAsync("vscode");

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("Visual Studio Code", results[0].Name);
        Assert.Equal("Microsoft.VisualStudioCode", results[0].Id);
        Assert.Equal("1.85.1", results[0].Version);
        Assert.Equal("winget", results[0].Source);
        Assert.Equal("Tag: vscode", results[0].Match);
    }

    [Fact]
    public async Task SearchAsync_NoResults_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("search", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.SearchNoResults,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.SearchAsync("nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_NonZeroExitCode_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("search", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Command failed"
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.SearchAsync("test");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_MalformedOutput_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("search", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.MalformedOutput,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.SearchAsync("test");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_EmptyOutput_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("search", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.EmptyOutput,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.SearchAsync("test");

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region Install Tests

    [Fact]
    public async Task InstallAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("install", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.InstallSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.InstallAsync("Microsoft.VisualStudioCode");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Successfully installed", result.StandardOutput);
    }

    [Fact]
    public async Task InstallAsync_Failure_ReturnsFailureResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("install", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = SampleWingetOutput.InstallError
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.InstallAsync("NonExistent.Package");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Failed to install", result.StandardError);
    }

    #endregion

    #region List Installed Tests

    [Fact]
    public async Task ListInstalledAsync_WithPackages_ReturnsInstalledPackages()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.ListInstalledPackages,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListInstalledAsync();

        // Assert
        Assert.Equal(5, results.Count);
        
        var vcRedist = results[0];
        Assert.Equal("Microsoft.VCRedist.2015+.x64", vcRedist.Id);
        Assert.Equal("14.38.33130.0", vcRedist.Version);
        Assert.Equal("14.38.33135.0", vcRedist.AvailableVersion);
        Assert.True(vcRedist.HasUpgrade);

        var dotnetSdk = results[1];
        Assert.Equal("Microsoft.DotNet.SDK.8", dotnetSdk.Id);
        Assert.False(dotnetSdk.HasUpgrade);
    }

    [Fact]
    public async Task ListInstalledAsync_UserOnlyFilter_FiltersCorrectly()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var outputWithArp = string.Join("\n", new[]
        {
            "Name                                   Id                                    Version          Available        Source",
            "--------------------------------------------------------------------------------------------------------------------------",
            "Microsoft Visual C++ 2015-2022 Redu…   Microsoft.VCRedist.2015+.x64          14.38.33130.0    14.38.33135.0    winget",
            "Some ARP Package                        ARP\\SomePackage                       1.0.0                             ",
            "Git                                     Git.Git                               2.43.0           2.44.0           winget",
        });
        mockHelper.SetResponse("list", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = outputWithArp,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListInstalledAsync(userOnly: true);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, p => Assert.False(p.Id.StartsWith("ARP\\")));
        Assert.All(results, p => Assert.NotEmpty(p.Source));
    }

    [Fact]
    public async Task ListInstalledAsync_NonZeroExitCode_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("list", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Command failed"
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListInstalledAsync();

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region List Upgrades Tests

    [Fact]
    public async Task ListUpgradesAsync_WithUpgrades_ReturnsPackagesWithUpgrades()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("upgrade", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.ListUpgrades,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListUpgradesAsync();

        // Assert
        Assert.Equal(3, results.Count);
        
        var git = results[0];
        Assert.Equal("Git.Git", git.Id);
        Assert.Equal("2.43.0", git.Version);
        Assert.Equal("2.44.0", git.AvailableVersion);
        Assert.True(git.HasUpgrade);
    }

    [Fact]
    public async Task ListUpgradesAsync_NoUpgrades_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var noUpgradesOutput = string.Join("\n", new[]
        {
            "Name                            Id                              Version         Available       Source",
            "------------------------------------------------------------------------------------------------------",
        });
        mockHelper.SetResponse("upgrade", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = noUpgradesOutput,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListUpgradesAsync();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task ListUpgradesAsync_NonZeroExitCode_ReturnsEmptyList()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("upgrade", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Command failed"
        });
        var service = new WingetService(mockHelper);

        // Act
        var results = await service.ListUpgradesAsync();

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region Upgrade Tests

    [Fact]
    public async Task UpgradeAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("upgrade", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.UpgradeSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.UpgradeAsync("Git.Git");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully upgraded", result.StandardOutput);
    }

    [Fact]
    public async Task UpgradeAllAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("upgrade --all", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = "All packages upgraded successfully",
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.UpgradeAllAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Contains("upgraded successfully", result.StandardOutput);
    }

    #endregion

    #region Uninstall Tests

    [Fact]
    public async Task UninstallAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("uninstall", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.UninstallSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.UninstallAsync("Git.Git");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully uninstalled", result.StandardOutput);
    }

    #endregion

    #region Export/Import Tests

    [Fact]
    public async Task ExportAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("export", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.ExportSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.ExportAsync("/tmp/packages.json");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully exported", result.StandardOutput);
    }

    [Fact]
    public async Task ImportAsync_Success_ReturnsSuccessResult()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("import", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.ImportSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.ImportAsync("/tmp/packages.json");

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully imported", result.StandardOutput);
    }

    [Fact]
    public async Task ParseExportFileAsync_ValidJson_ReturnsExportFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var json = @"{
            ""WinGetVersion"": ""1.7.1.0"",
            ""Sources"": [
                {
                    ""Name"": ""winget"",
                    ""Identifier"": ""Microsoft.Winget.Source_8wekyb3d8bbwe"",
                    ""Argument"": ""https://cdn.winget.microsoft.com/cache"",
                    ""Type"": ""Microsoft.PreIndexed.Package""
                }
            ],
            ""Packages"": [
                {
                    ""PackageIdentifier"": ""Microsoft.VisualStudioCode"",
                    ""Version"": ""1.85.1""
                },
                {
                    ""PackageIdentifier"": ""Git.Git""
                }
            ]
        }";
        await File.WriteAllTextAsync(tempFile, json);
        var service = new WingetService();

        try
        {
            // Act
            var result = await service.ParseExportFileAsync(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1.7.1.0", result.WinGetVersion);
            Assert.Single(result.Sources!);
            Assert.Equal(2, result.Packages.Count);
            Assert.Equal("Microsoft.VisualStudioCode", result.Packages[0].PackageIdentifier);
            Assert.Equal("1.85.1", result.Packages[0].Version);
            Assert.Equal("Git.Git", result.Packages[1].PackageIdentifier);
            Assert.Null(result.Packages[1].Version);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseExportFileAsync_InvalidJson_ReturnsNull()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "invalid json content");
        var service = new WingetService();

        try
        {
            // Act
            var result = await service.ParseExportFileAsync(tempFile);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportWithProgressAsync_ValidFile_InstallsPackagesAndReportsProgress()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var json = @"{
            ""Packages"": [
                {
                    ""PackageIdentifier"": ""Microsoft.VisualStudioCode""
                },
                {
                    ""PackageIdentifier"": ""Git.Git""
                }
            ]
        }";
        await File.WriteAllTextAsync(tempFile, json);

        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("install", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.InstallSuccess,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        var progressReports = new List<(int current, int total, string packageId)>();

        try
        {
            // Act
            var (succeeded, failed, errors) = await service.ImportWithProgressAsync(
                tempFile,
                (current, total, packageId) => progressReports.Add((current, total, packageId)));

            // Assert
            Assert.Equal(2, succeeded);
            Assert.Equal(0, failed);
            Assert.Empty(errors);
            Assert.Equal(2, progressReports.Count);
            Assert.Equal((1, 2, "Microsoft.VisualStudioCode"), progressReports[0]);
            Assert.Equal((2, 2, "Git.Git"), progressReports[1]);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportWithProgressAsync_SomeFailures_ReportsCorrectCounts()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var json = @"{
            ""Packages"": [
                {
                    ""PackageIdentifier"": ""Success.Package""
                },
                {
                    ""PackageIdentifier"": ""Failed.Package""
                }
            ]
        }";
        await File.WriteAllTextAsync(tempFile, json);

        var mockHelper = new MockProcessHelper();
        // First install succeeds
        mockHelper.SetResponse("install --id \"Success.Package\" --exact", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.InstallSuccess,
            StandardError = string.Empty
        });
        // Second install fails
        mockHelper.SetResponse("install --id \"Failed.Package\" --exact", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Package not found"
        });
        var service = new WingetService(mockHelper);

        try
        {
            // Act
            var (succeeded, failed, errors) = await service.ImportWithProgressAsync(tempFile);

            // Assert
            Assert.Equal(1, succeeded);
            Assert.Equal(1, failed);
            Assert.Single(errors);
            Assert.Contains("Failed.Package", errors[0]);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportWithProgressAsync_InvalidFile_ReturnsFailure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "invalid json");
        var service = new WingetService();

        try
        {
            // Act
            var (succeeded, failed, errors) = await service.ImportWithProgressAsync(tempFile);

            // Assert
            Assert.Equal(0, succeeded);
            Assert.Equal(0, failed);
            Assert.Single(errors);
            Assert.Contains("Failed to parse", errors[0]);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region Show Package Details Tests

    [Fact]
    public async Task ShowAsync_Success_ReturnsPackageDetails()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("show", new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = SampleWingetOutput.PackageDetails,
            StandardError = string.Empty
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.ShowAsync("Microsoft.VisualStudioCode");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Visual Studio Code", result.Name);
        Assert.Equal("Microsoft.VisualStudioCode", result.Id);
        Assert.Equal("1.85.1", result.Version);
        Assert.Equal("Microsoft Corporation", result.Publisher);
        Assert.Contains("lightweight but powerful", result.Description);
        Assert.Equal("https://code.visualstudio.com/", result.Homepage);
        Assert.Equal(2, result.Dependencies.Count);
    }

    [Fact]
    public async Task ShowAsync_NonZeroExitCode_ReturnsNull()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        mockHelper.SetResponse("show", new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = string.Empty,
            StandardError = "Package not found"
        });
        var service = new WingetService(mockHelper);

        // Act
        var result = await service.ShowAsync("NonExistent.Package");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SearchAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.SearchAsync("test", cts.Token));
    }

    [Fact]
    public async Task ListInstalledAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockHelper = new MockProcessHelper();
        var service = new WingetService(mockHelper);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ListInstalledAsync(false, cts.Token));
    }

    #endregion
}
