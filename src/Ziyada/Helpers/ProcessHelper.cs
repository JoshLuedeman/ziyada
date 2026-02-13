using System.Diagnostics;
using System.Text;
using Ziyada.Services;

namespace Ziyada.Helpers;

public class ProcessResult
{
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public bool Success => ExitCode == 0;
}

public interface IProcessHelper
{
    Task<ProcessResult> RunAsync(string arguments, CancellationToken ct = default);
}

public class ProcessHelper : IProcessHelper
{
    public async Task<ProcessResult> RunAsync(string arguments, CancellationToken ct = default)
    {
        var logger = LoggingService.Instance;
        logger.LogInfo("Executing winget command", command: arguments);

        var psi = new ProcessStartInfo
        {
            FileName = "winget",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(ct);

            var result = new ProcessResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = stdout.ToString(),
                StandardError = stderr.ToString(),
            };

            if (result.Success)
            {
                logger.LogInfo("Winget command completed successfully", 
                    command: arguments, 
                    stdout: result.StandardOutput, 
                    exitCode: result.ExitCode);
            }
            else
            {
                logger.LogWarning("Winget command completed with non-zero exit code", 
                    command: arguments, 
                    stdout: result.StandardOutput, 
                    stderr: result.StandardError, 
                    exitCode: result.ExitCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Winget command was cancelled", command: arguments);
            throw; // Re-throw cancellation exceptions
        }
        catch (Exception ex)
        {
            logger.LogError("Exception occurred while executing winget command", 
                exception: ex, 
                command: arguments);
            throw;
        }
    }
}
