using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Services;

public class SourceService
{
    private const string Flags = "";
    private readonly IProcessHelper _processHelper;

    public SourceService(IProcessHelper? processHelper = null)
    {
        _processHelper = processHelper ?? new ProcessHelper();
    }

    public async Task<List<SourceInfo>> ListSourcesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _processHelper.RunAsync("source list", ct);
            return result.Success ? WingetParser.ParseSources(result.StandardOutput) : [];
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("ListSourcesAsync failed", exception: ex);
            return [];
        }
    }

    public async Task<ProcessResult> AddSourceAsync(string name, string url, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"source add --name \"{name}\" --arg \"{url}\"", ct);
    }

    public async Task<ProcessResult> RemoveSourceAsync(string name, CancellationToken ct = default)
    {
        return await _processHelper.RunAsync($"source remove --name \"{name}\"", ct);
    }
}
