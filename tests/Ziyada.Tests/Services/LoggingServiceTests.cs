using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Tests.Services;

public class LoggingServiceTests : IDisposable
{
    private readonly LoggingService _logger;
    private readonly string _logDirectory;

    public LoggingServiceTests()
    {
        _logger = LoggingService.Instance;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logDirectory = Path.Combine(appDataPath, "Ziyada", "logs");
    }

    [Fact]
    public void LoggingService_Instance_ReturnsSameInstance()
    {
        // Act
        var instance1 = LoggingService.Instance;
        var instance2 = LoggingService.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void LogInfo_AddsEntryToRecentEntries()
    {
        // Arrange
        var initialCount = _logger.GetRecentEntries().Count;

        // Act
        _logger.LogInfo("Test info message");

        // Assert
        var entries = _logger.GetRecentEntries();
        Assert.True(entries.Count > initialCount);
        var lastEntry = entries.Last();
        Assert.Equal(LogLevel.Info, lastEntry.Level);
        Assert.Contains("Test info message", lastEntry.Message);
    }

    [Fact]
    public void LogInfo_WithCommand_IncludesCommandInEntry()
    {
        // Arrange
        var command = "search \"vscode\"";

        // Act
        _logger.LogInfo("Executing winget command", command: command);

        // Assert
        var entries = _logger.GetRecentEntries();
        var lastEntry = entries.Last();
        Assert.Equal(LogLevel.Info, lastEntry.Level);
        Assert.Equal(command, lastEntry.Command);
    }

    [Fact]
    public void LogWarning_AddsWarningEntry()
    {
        // Act
        _logger.LogWarning("Test warning message", exitCode: 1);

        // Assert
        var entries = _logger.GetRecentEntries();
        var lastEntry = entries.Last();
        Assert.Equal(LogLevel.Warning, lastEntry.Level);
        Assert.Contains("Test warning message", lastEntry.Message);
        Assert.Equal(1, lastEntry.ExitCode);
    }

    [Fact]
    public void LogError_WithException_AddsErrorEntry()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        _logger.LogError("Test error message", exception: exception);

        // Assert
        var entries = _logger.GetRecentEntries();
        var lastEntry = entries.Last();
        Assert.Equal(LogLevel.Error, lastEntry.Level);
        Assert.Contains("Test error message", lastEntry.Message);
    }

    [Fact]
    public void LogInfo_WithOutputs_StoresOutputs()
    {
        // Arrange
        var stdout = "Command output";
        var stderr = "Command error";
        var exitCode = 0;

        // Act
        _logger.LogInfo("Test with outputs", 
            command: "test command", 
            stdout: stdout, 
            stderr: stderr, 
            exitCode: exitCode);

        // Assert
        var entries = _logger.GetRecentEntries();
        var lastEntry = entries.Last();
        Assert.Equal(stdout, lastEntry.StandardOutput);
        Assert.Equal(stderr, lastEntry.StandardError);
        Assert.Equal(exitCode, lastEntry.ExitCode);
    }

    [Fact]
    public void GetRecentEntries_ReturnsRequestedCount()
    {
        // Arrange - Add multiple entries
        for (int i = 0; i < 10; i++)
        {
            _logger.LogInfo($"Test message {i}");
        }

        // Act
        var entries = _logger.GetRecentEntries(5);

        // Assert
        Assert.True(entries.Count <= 5);
    }

    [Fact]
    public async Task GetRecentEntries_ReturnsInChronologicalOrder()
    {
        // Arrange - Clear and add entries with distinct timestamps
        var startTime = DateTime.Now;
        _logger.LogInfo("First message");
        await Task.Delay(10);
        _logger.LogInfo("Second message");
        await Task.Delay(10);
        _logger.LogInfo("Third message");

        // Act
        var entries = _logger.GetRecentEntries(10);
        var recentEntries = entries.Where(e => e.Timestamp >= startTime).ToList();

        // Assert
        Assert.True(recentEntries.Count >= 3);
        // Verify chronological order (oldest to newest)
        for (int i = 1; i < recentEntries.Count; i++)
        {
            Assert.True(recentEntries[i].Timestamp >= recentEntries[i - 1].Timestamp);
        }
    }

    [Fact]
    public void LogDirectory_CreatesDirectory()
    {
        // Assert
        Assert.True(Directory.Exists(_logDirectory));
    }

    [Fact]
    public async Task LogFile_CreatesLogFile()
    {
        // Arrange - Log something to ensure file is created
        _logger.LogInfo("Test log file creation");
        
        // Wait a bit for file to be flushed
        await Task.Delay(100);

        // Act
        var logFiles = Directory.GetFiles(_logDirectory, "ziyada-*.log");

        // Assert
        Assert.NotEmpty(logFiles);
    }

    public void Dispose()
    {
        // Cleanup is handled by singleton, no need to dispose
    }
}
