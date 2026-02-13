using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Tests.Helpers;

public class ProcessHelperLoggingTests
{
    private readonly LoggingService _logger;

    public ProcessHelperLoggingTests()
    {
        _logger = LoggingService.Instance;
    }

    [Fact]
    public async Task RunAsync_LogsCommandExecution()
    {
        // Arrange
        var helper = new ProcessHelper();
        var initialCount = _logger.GetRecentEntries().Count;

        // Act
        // Use a simple winget command that should work
        var result = await helper.RunAsync("--version");

        // Assert
        var entries = _logger.GetRecentEntries();
        Assert.True(entries.Count > initialCount);
        
        // Find the log entries for this command
        var commandEntries = entries.Where(e => 
            e.Command != null && e.Command.Contains("--version")).ToList();
        
        Assert.NotEmpty(commandEntries);
        
        // Should have at least one entry for command start
        var startEntry = commandEntries.FirstOrDefault(e => 
            e.Message.Contains("Executing winget command"));
        Assert.NotNull(startEntry);
        Assert.Equal(LogLevel.Info, startEntry.Level);
    }

    [Fact]
    public async Task RunAsync_SuccessfulCommand_LogsSuccess()
    {
        // Arrange
        var helper = new ProcessHelper();

        // Act
        var result = await helper.RunAsync("--version");

        // Assert
        var entries = _logger.GetRecentEntries(50);
        var successEntry = entries.LastOrDefault(e => 
            e.Message.Contains("completed successfully"));
        
        // If the command succeeded, we should have a success log
        if (result.Success)
        {
            Assert.NotNull(successEntry);
            Assert.Equal(LogLevel.Info, successEntry.Level);
        }
    }

    [Fact]
    public async Task RunAsync_FailedCommand_LogsWarning()
    {
        // Arrange
        var helper = new ProcessHelper();

        // Act
        // Use an invalid command that should fail
        var result = await helper.RunAsync("invalid-command-xyz");

        // Assert
        var entries = _logger.GetRecentEntries(50);
        
        // Should log either a warning for non-zero exit or an error for exception
        var warningOrErrorEntry = entries.LastOrDefault(e => 
            e.Level == LogLevel.Warning || e.Level == LogLevel.Error);
        
        Assert.NotNull(warningOrErrorEntry);
    }

    [Fact]
    public async Task RunAsync_LogsExitCode()
    {
        // Arrange
        var helper = new ProcessHelper();

        // Act
        var result = await helper.RunAsync("--version");

        // Assert
        var entries = _logger.GetRecentEntries(50);
        var entryWithExitCode = entries.LastOrDefault(e => 
            e.ExitCode.HasValue);
        
        Assert.NotNull(entryWithExitCode);
        Assert.Equal(result.ExitCode, entryWithExitCode.ExitCode);
    }

    [Fact]
    public async Task RunAsync_LogsStdoutAndStderr()
    {
        // Arrange
        var helper = new ProcessHelper();

        // Act
        var result = await helper.RunAsync("--version");

        // Assert
        var entries = _logger.GetRecentEntries(50);
        var entryWithOutput = entries.LastOrDefault(e => 
            !string.IsNullOrEmpty(e.StandardOutput) || 
            !string.IsNullOrEmpty(e.StandardError));
        
        // If there was output, it should be logged
        if (!string.IsNullOrEmpty(result.StandardOutput) || !string.IsNullOrEmpty(result.StandardError))
        {
            Assert.NotNull(entryWithOutput);
        }
    }
}
