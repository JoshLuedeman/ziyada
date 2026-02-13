using Ziyada.Helpers;

namespace Ziyada.Tests.Mocks;

/// <summary>
/// Mock implementation of IProcessHelper for testing without requiring winget.
/// Returns pre-configured ProcessResults based on command arguments.
/// </summary>
public class MockProcessHelper : IProcessHelper
{
    private readonly Dictionary<string, Func<ProcessResult>> _responses = new();
    private ProcessResult? _defaultResponse;
    private readonly List<string> _executedCommands = new();

    /// <summary>
    /// Configure a response for a specific command pattern.
    /// </summary>
    public void SetResponse(string commandPattern, ProcessResult response)
    {
        _responses[commandPattern] = () => response;
    }

    /// <summary>
    /// Configure a response with a function for dynamic behavior.
    /// </summary>
    public void SetResponse(string commandPattern, Func<ProcessResult> responseFunc)
    {
        _responses[commandPattern] = responseFunc;
    }

    /// <summary>
    /// Configure a default response for any unmatched commands.
    /// </summary>
    public void SetDefaultResponse(ProcessResult response)
    {
        _defaultResponse = response;
    }

    /// <summary>
    /// Get the list of executed commands for verification.
    /// </summary>
    public IReadOnlyList<string> ExecutedCommands => _executedCommands.AsReadOnly();

    /// <summary>
    /// Clear all recorded commands.
    /// </summary>
    public void ClearExecutedCommands()
    {
        _executedCommands.Clear();
    }

    public Task<ProcessResult> RunAsync(string arguments, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _executedCommands.Add(arguments);

        // Try to find exact match
        if (_responses.TryGetValue(arguments, out var responseFunc))
        {
            return Task.FromResult(responseFunc());
        }

        // Try to find partial match (command starts with pattern)
        foreach (var kvp in _responses)
        {
            if (arguments.StartsWith(kvp.Key))
            {
                return Task.FromResult(kvp.Value());
            }
        }

        // Return default response if configured
        if (_defaultResponse != null)
        {
            return Task.FromResult(_defaultResponse);
        }

        // Return empty successful response by default
        return Task.FromResult(new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = string.Empty,
            StandardError = string.Empty
        });
    }
}
