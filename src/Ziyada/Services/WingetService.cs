using System.Text.Json;
using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Services;

public class WingetService
{
    // --accept-package-agreements is only valid for install/upgrade/import actions
    private const string SourceFlags = "--accept-source-agreements";
    private const string InstallFlags = "--accept-package-agreements --accept-source-agreements";

    private readonly IProcessHelper _processHelper;

    public WingetService(IProcessHelper? processHelper = null)
    {
        _processHelper = processHelper ?? new ProcessHelper();
    }

    public async Task<List<Package>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync($"search \"{query}\" {SourceFlags}", ct);
            return result.Success ? WingetParser.ParseSearchResults(result.StandardOutput) : [];
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError($"SearchAsync failed for query: {query}", exception: ex);
            return [];
        }
    }

    public async Task<ProcessResult> InstallAsync(string packageId, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"install --id \"{packageId}\" --exact {InstallFlags}", ct);
    }

    public async Task<List<InstalledPackage>> ListInstalledAsync(bool userOnly = false, CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync($"list {SourceFlags}", ct);
            if (!result.Success) return [];

            var packages = WingetParser.ParseInstalledPackages(result.StandardOutput);
            if (userOnly)
                packages = packages.Where(p => !string.IsNullOrEmpty(p.Source) && !p.Id.StartsWith("ARP\\")).ToList();
            return packages;
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("ListInstalledAsync failed", exception: ex);
            return [];
        }
    }

    public async Task<List<InstalledPackage>> ListUpgradesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync($"upgrade {SourceFlags}", ct);
            return result.Success ? WingetParser.ParseUpgradeList(result.StandardOutput) : [];
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("ListUpgradesAsync failed", exception: ex);
            return [];
        }
    }

    public async Task<ProcessResult> UpgradeAsync(string packageId, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"upgrade --id \"{packageId}\" --exact {InstallFlags}", ct);
    }

    public async Task<ProcessResult> UpgradeAllAsync(CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"upgrade --all {InstallFlags}", ct);
    }

    public async Task<ProcessResult> UninstallAsync(string packageId, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"uninstall --id \"{packageId}\" --exact {SourceFlags}", ct);
    }

    public async Task<ProcessResult> ExportAsync(string filePath, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"export -o \"{filePath}\" {SourceFlags}", ct);
    }

    public async Task<ProcessResult> ImportAsync(string filePath, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"import -i \"{filePath}\" {InstallFlags}", ct);
    }

    public async Task<PackageDetails?> ShowAsync(string packageId, CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync($"show --id \"{packageId}\" --exact {SourceFlags}", ct);
            return result.Success ? WingetParser.ParsePackageDetails(result.StandardOutput) : null;
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError($"ShowAsync failed for package: {packageId}", exception: ex);
            return null;
        }
    }

    public async Task<ProcessResult> PinAsync(string packageId, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"pin add --id \"{packageId}\" {SourceFlags}", ct);
    }

    public async Task<ProcessResult> UnpinAsync(string packageId, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"pin remove --id \"{packageId}\" {SourceFlags}", ct);
    }

    public async Task<List<string>> ListPinnedAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync($"pin list {SourceFlags}", ct);
            return result.Success ? WingetParser.ParsePinnedPackages(result.StandardOutput) : [];
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("ListPinnedAsync failed", exception: ex);
            return [];
        }
    }

    public async Task<WingetExportFile?> ParseExportFileAsync(string filePath, CancellationToken ct = default)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<WingetExportFile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw cancellation exceptions
        }
        catch (JsonException ex)
        {
            LoggingService.Instance.LogError($"Failed to parse export file (invalid JSON): {filePath}", exception: ex);
            return null;
        }
        catch (IOException ex)
        {
            LoggingService.Instance.LogError($"Failed to read export file: {filePath}", exception: ex);
            return null;
        }
    }

    public async Task<(int succeeded, int failed, List<string> errors)> ImportWithProgressAsync(
        string filePath,
        Action<int, int, string>? onProgress = null,
        CancellationToken ct = default)
    {
        var exportFile = await ParseExportFileAsync(filePath, ct);
        if (exportFile == null || exportFile.Packages.Count == 0)
        {
            return (0, 0, ["Failed to parse export file or no packages found"]);
        }

        int succeeded = 0;
        int failed = 0;
        var errors = new List<string>();
        int total = exportFile.Packages.Count;

        for (int i = 0; i < exportFile.Packages.Count; i++)
        {
            var pkg = exportFile.Packages[i];
            onProgress?.Invoke(i + 1, total, pkg.PackageIdentifier);

            var result = await InstallAsync(pkg.PackageIdentifier, ct);
            if (result.Success)
            {
                succeeded++;
            }
            else
            {
                failed++;
                var errorMsg = result.StandardError.Split('\n').FirstOrDefault() ?? "Unknown error";
                errors.Add($"{pkg.PackageIdentifier}: {errorMsg}");
            }
        }

        return (succeeded, failed, errors);
    }
}
