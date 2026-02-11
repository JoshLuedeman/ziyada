namespace Ziyada.Tests.Helpers;

using Ziyada.Helpers;

public class ProcessResultTests
{
    [Fact]
    public void ProcessResult_DefaultValues_AreCorrect()
    {
        var result = new ProcessResult();
        
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    [Fact]
    public void ProcessResult_Success_TrueWhenExitCodeZero()
    {
        var result = new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = "Some output",
            StandardError = ""
        };

        Assert.True(result.Success);
    }

    [Fact]
    public void ProcessResult_Success_FalseWhenExitCodeNonZero()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardOutput = "",
            StandardError = "Error occurred"
        };

        Assert.False(result.Success);
    }

    [Fact]
    public void ProcessResult_Success_FalseWhenExitCodeNegative()
    {
        var result = new ProcessResult { ExitCode = -1 };
        Assert.False(result.Success);
    }

    [Fact]
    public void ProcessResult_CanHaveOutputAndError()
    {
        var result = new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = "Standard output text",
            StandardError = "Warning: something happened"
        };

        Assert.True(result.Success);
        Assert.Equal("Standard output text", result.StandardOutput);
        Assert.Equal("Warning: something happened", result.StandardError);
    }

    [Fact]
    public void ProcessResult_LargeOutput_HandledCorrectly()
    {
        var largeOutput = new string('X', 100000);
        var result = new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = largeOutput
        };

        Assert.Equal(100000, result.StandardOutput.Length);
    }

    [Fact]
    public void ProcessResult_MultilineOutput_PreservedCorrectly()
    {
        var multiline = "Line 1\nLine 2\nLine 3";
        var result = new ProcessResult
        {
            StandardOutput = multiline
        };

        Assert.Equal(multiline, result.StandardOutput);
        Assert.Contains("\n", result.StandardOutput);
    }

    [Fact]
    public void ProcessResult_WindowsLineEndings_PreservedCorrectly()
    {
        var windowsLines = "Line 1\r\nLine 2\r\nLine 3";
        var result = new ProcessResult
        {
            StandardOutput = windowsLines
        };

        Assert.Equal(windowsLines, result.StandardOutput);
        Assert.Contains("\r\n", result.StandardOutput);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(-1, false)]
    [InlineData(255, false)]
    [InlineData(int.MaxValue, false)]
    [InlineData(int.MinValue, false)]
    public void ProcessResult_Success_CorrectForVariousExitCodes(int exitCode, bool expectedSuccess)
    {
        var result = new ProcessResult { ExitCode = exitCode };
        Assert.Equal(expectedSuccess, result.Success);
    }
}
