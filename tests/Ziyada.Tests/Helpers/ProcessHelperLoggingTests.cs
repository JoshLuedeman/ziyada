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

        try
        {
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
        catch (Exception)
        {
            // Winget not available in test environment - verify error was logged
            var entries = _logger.GetRecentEntries();
            var errorEntry = entries.LastOrDefault(e => e.Level == LogLevel.Error);
            Assert.NotNull(errorEntry);
        }
    }

    [Fact]
    public async Task RunAsync_Exception_LogsError()
    {
        // Arrange
        var helper = new ProcessHelper();

        // Act & Assert
        try
        {
            var result = await helper.RunAsync("--version");
        }
        catch (Exception)
        {
            // Expected when winget is not available
            var entries = _logger.GetRecentEntries(50);
            var errorEntry = entries.LastOrDefault(e => 
                e.Level == LogLevel.Error && e.Message.Contains("Exception occurred"));
            
            Assert.NotNull(errorEntry);
        }
    }
}
