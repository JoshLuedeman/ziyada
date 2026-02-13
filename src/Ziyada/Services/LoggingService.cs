using Serilog;
using Serilog.Events;
using Ziyada.Models;

namespace Ziyada.Services;

public class LoggingService
{
    private static LoggingService? _instance;
    private static readonly object _lock = new();
    private readonly ILogger _logger;
    private readonly List<LogEntry> _recentEntries = new();
    private readonly int _maxRecentEntries = 1000;

    private LoggingService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDirectory = Path.Combine(appDataPath, "Ziyada", "logs");
        
        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "ziyada-.log");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _logger.Information("Ziyada logging initialized");
    }

    public static LoggingService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new LoggingService();
                }
            }
            return _instance;
        }
    }

    public void LogInfo(string message, string? command = null, string? stdout = null, string? stderr = null, int? exitCode = null)
    {
        _logger.Information(FormatMessage(message, command, stdout, stderr, exitCode));
        AddToRecentEntries(Models.LogLevel.Info, message, command, stdout, stderr, exitCode);
    }

    public void LogWarning(string message, string? command = null, string? stdout = null, string? stderr = null, int? exitCode = null)
    {
        _logger.Warning(FormatMessage(message, command, stdout, stderr, exitCode));
        AddToRecentEntries(Models.LogLevel.Warning, message, command, stdout, stderr, exitCode);
    }

    public void LogError(string message, Exception? exception = null, string? command = null, string? stdout = null, string? stderr = null, int? exitCode = null)
    {
        if (exception != null)
        {
            _logger.Error(exception, FormatMessage(message, command, stdout, stderr, exitCode));
        }
        else
        {
            _logger.Error(FormatMessage(message, command, stdout, stderr, exitCode));
        }
        AddToRecentEntries(Models.LogLevel.Error, message, command, stdout, stderr, exitCode);
    }

    private string FormatMessage(string message, string? command, string? stdout, string? stderr, int? exitCode)
    {
        var parts = new List<string> { message };
        
        if (!string.IsNullOrEmpty(command))
            parts.Add($"Command: winget {command}");
        
        if (exitCode.HasValue)
            parts.Add($"ExitCode: {exitCode.Value}");
        
        if (!string.IsNullOrEmpty(stderr))
            parts.Add($"StdErr: {TruncateOutput(stderr, 500)}");
        
        if (!string.IsNullOrEmpty(stdout))
            parts.Add($"StdOut: {TruncateOutput(stdout, 500)}");
        
        return string.Join(" | ", parts);
    }

    private string TruncateOutput(string output, int maxLength)
    {
        if (string.IsNullOrEmpty(output))
            return string.Empty;
        
        var trimmed = output.Trim();
        if (trimmed.Length <= maxLength)
            return trimmed;
        
        return trimmed.Substring(0, maxLength) + "... (truncated)";
    }

    private void AddToRecentEntries(Models.LogLevel level, string message, string? command, string? stdout, string? stderr, int? exitCode)
    {
        lock (_recentEntries)
        {
            _recentEntries.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Command = command,
                StandardOutput = stdout,
                StandardError = stderr,
                ExitCode = exitCode
            });

            // Keep only recent entries
            while (_recentEntries.Count > _maxRecentEntries)
            {
                _recentEntries.RemoveAt(0);
            }
        }
    }

    public List<LogEntry> GetRecentEntries(int count = 100)
    {
        lock (_recentEntries)
        {
            return _recentEntries
                .TakeLast(count)
                .ToList();
        }
    }

    public void Close()
    {
        _logger.Information("Ziyada logging shutdown");
        (_logger as IDisposable)?.Dispose();
    }
}
