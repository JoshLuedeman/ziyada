namespace Ziyada.Tests.Views;

using Ziyada.Helpers;
using Ziyada.Views;

public class InstallFailureSummaryTests
{
    [Fact]
    public void GetInstallFailureSummary_WithStandardError_ReturnsFirstErrorLine()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "Package not found\nAdditional details here",
            StandardOutput = "Some output"
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Package not found", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_WithEmptyStderr_FallsBackToStdout()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "",
            StandardOutput = "Searching for package...\nNo package found matching input criteria."
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("No package found matching input criteria.", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_WithBothEmpty_ReturnsUnknownError()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "",
            StandardOutput = ""
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Unknown error", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_WithNullOutputs_ReturnsUnknownError()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = null!,
            StandardOutput = null!
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Unknown error", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_WithWhitespaceOnlyStderr_FallsBackToStdout()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "  \n  \n  ",
            StandardOutput = "Installation failed due to conflict"
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Installation failed due to conflict", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_TrimsWhitespace()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "  Error: access denied  \n",
            StandardOutput = ""
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Error: access denied", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_StdoutUsesLastNonEmptyLine()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "",
            StandardOutput = "Progress: downloading...\nProgress: verifying...\nInstaller hash does not match\n\n"
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("Installer hash does not match", summary);
    }

    [Fact]
    public void GetInstallFailureSummary_StderrPrefersFirstNonEmptyLine()
    {
        var result = new ProcessResult
        {
            ExitCode = 1,
            StandardError = "\n\nThe installer failed.\nDetails: exit code 1603",
            StandardOutput = "Some stdout"
        };

        var summary = SearchView.GetInstallFailureSummary(result);

        Assert.Equal("The installer failed.", summary);
    }
}
