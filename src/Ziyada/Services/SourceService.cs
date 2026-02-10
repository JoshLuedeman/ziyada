using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Services;

public class SourceService
{
    private const string Flags = "";

    public async Task<List<SourceInfo>> ListSourcesAsync(CancellationToken ct = default)
    {
        var result = await ProcessHelper.RunAsync("source list", ct);
        return result.Success ? WingetParser.ParseSources(result.StandardOutput) : [];
    }

    public async Task<ProcessResult> AddSourceAsync(string name, string url, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"source add --name \"{name}\" --arg \"{url}\"", ct);
    }

    public async Task<ProcessResult> RemoveSourceAsync(string name, CancellationToken ct = default)
    {
        return await ProcessHelper.RunAsync($"source remove --name \"{name}\"", ct);
    }
}
