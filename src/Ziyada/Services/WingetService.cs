using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Services;

public class WingetService
{
    // --accept-package-agreements is only valid for install/upgrade/import actions
    private const string SourceFlags = "--accept-source-agreements";
    private const string InstallFlags = "--accept-package-agreements --accept-source-agreements";

    public async Task<List<Package>> SearchAsync(string query, CancellationToken ct = default)
    {
        var result = await ProcessHelper.RunAsync($"search \"{query}\" {SourceFlags}", ct);
        return result.Success ? WingetParser.ParseSearchResults(result.StandardOutput) : [];
    }

    public async Task<ProcessResult> InstallAsync(string packageId, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"install --id \"{packageId}\" --exact {InstallFlags}", ct);
    }

    public async Task<List<InstalledPackage>> ListInstalledAsync(bool userOnly = false, CancellationToken ct = default)
    {
        var result = await ProcessHelper.RunAsync($"list {SourceFlags}", ct);
        if (!result.Success) return [];

        var packages = WingetParser.ParseInstalledPackages(result.StandardOutput);
        if (userOnly)
            packages = packages.Where(p => !string.IsNullOrEmpty(p.Source) && !p.Id.StartsWith("ARP\\")).ToList();
        return packages;
    }

    public async Task<List<InstalledPackage>> ListUpgradesAsync(CancellationToken ct = default)
    {
        var result = await ProcessHelper.RunAsync($"upgrade {SourceFlags}", ct);
        return result.Success ? WingetParser.ParseUpgradeList(result.StandardOutput) : [];
    }

    public async Task<ProcessResult> UpgradeAsync(string packageId, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"upgrade --id \"{packageId}\" --exact {InstallFlags}", ct);
    }

    public async Task<ProcessResult> UpgradeAllAsync(CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"upgrade --all {InstallFlags}", ct);
    }

    public async Task<ProcessResult> UninstallAsync(string packageId, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"uninstall --id \"{packageId}\" --exact {SourceFlags}", ct);
    }

    public async Task<ProcessResult> ExportAsync(string filePath, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"export -o \"{filePath}\" {SourceFlags}", ct);
    }

    public async Task<ProcessResult> ImportAsync(string filePath, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"import -i \"{filePath}\" {InstallFlags}", ct);
    }

    public async Task<PackageDetails?> ShowAsync(string packageId, CancellationToken ct = default)
    {
        var result = await ProcessHelper.RunAsync($"show --id \"{packageId}\" --exact {SourceFlags}", ct);
        return result.Success ? WingetParser.ParsePackageDetails(result.StandardOutput) : null;
    }
}
