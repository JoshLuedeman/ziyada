namespace Ziyada.Models;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Command { get; set; }
    public string? StandardOutput { get; set; }
    public string? StandardError { get; set; }
    public int? ExitCode { get; set; }
}
