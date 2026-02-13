using System.Data;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class LogsView : View
{
    private readonly TableView _tableView;
    private readonly DataTable _dataTable;
    private readonly Button _refreshButton;
    private readonly Button _clearButton;
    private readonly Label _statusLabel;
    private object? _refreshTimeoutToken;

    public LogsView()
    {
        ColorScheme = Theme.Base;

        var titleLabel = new Label
        {
            Text = "📋 Recent Activity Logs",
            X = 0,
            Y = 0,
            ColorScheme = Theme.Accent,
        };

        _dataTable = new DataTable();
        _dataTable.Columns.Add("Timestamp", typeof(string));
        _dataTable.Columns.Add("Level", typeof(string));
        _dataTable.Columns.Add("Message", typeof(string));

        _tableView = new TableView
        {
            X = 0,
            Y = 2,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
            ColorScheme = Theme.Table,
            FullRowSelect = true,
        };

        _tableView.Table = new DataTableSource(_dataTable);
        _tableView.Style.AlwaysShowHeaders = true;

        _refreshButton = new Button
        {
            Text = "🔄 Refresh",
            X = 0,
            Y = Pos.Bottom(_tableView),
            ColorScheme = Theme.Button,
        };

        _refreshButton.Accepting += (s, e) => LoadLogsAsync();

        _clearButton = new Button
        {
            Text = "🗑️ Clear Display",
            X = Pos.Right(_refreshButton) + 2,
            Y = Pos.Bottom(_tableView),
            ColorScheme = Theme.Button,
        };

        _clearButton.Accepting += (s, e) => ClearDisplay();

        _statusLabel = new Label
        {
            Text = "Press F5 to refresh",
            X = Pos.Right(_clearButton) + 2,
            Y = Pos.Bottom(_tableView),
            ColorScheme = Theme.Status,
        };

        Add(titleLabel, _tableView, _refreshButton, _clearButton, _statusLabel);

        // Set up key bindings
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == KeyCode.F5)
            {
                LoadLogsAsync();
                e.Handled = true;
            }
        };

        // Start auto-refresh
        StartAutoRefresh();
    }

    public void LoadLogsAsync()
    {
        Task.Run(() =>
        {
            try
            {
                var logs = LoggingService.Instance.GetRecentEntries(1000);

                Application.Invoke(() =>
                {
                    _dataTable.Rows.Clear();

                    foreach (var log in logs.OrderByDescending(l => l.Timestamp))
                    {
                        var levelIcon = log.Level switch
                        {
                            Models.LogLevel.Info => "ℹ️",
                            Models.LogLevel.Warning => "⚠️",
                            Models.LogLevel.Error => "❌",
                            _ => " "
                        };

                        var message = log.Message;
                        if (!string.IsNullOrEmpty(log.Command))
                        {
                            message = $"{message} | winget {log.Command}";
                        }

                        _dataTable.Rows.Add(
                            log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            $"{levelIcon} {log.Level}",
                            message
                        );
                    }

                    _statusLabel.Text = $"Loaded {logs.Count} log entries - {DateTime.Now:HH:mm:ss}";
                    Application.Wakeup();
                });
            }
            catch (Exception ex)
            {
                Application.Invoke(() =>
                {
                    _statusLabel.Text = $"Error loading logs: {ex.Message}";
                    Application.Wakeup();
                });
            }
        });
    }

    private void ClearDisplay()
    {
        _dataTable.Rows.Clear();
        _statusLabel.Text = "Display cleared (logs still saved to file)";
    }

    private void StartAutoRefresh()
    {
        // Refresh every 5 seconds
        _refreshTimeoutToken = Application.AddTimeout(TimeSpan.FromSeconds(5), () =>
        {
            LoadLogsAsync();
            StartAutoRefresh(); // Schedule next refresh
            return false;
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _refreshTimeoutToken != null)
        {
            Application.RemoveTimeout(_refreshTimeoutToken);
            _refreshTimeoutToken = null;
        }
        base.Dispose(disposing);
    }
}
