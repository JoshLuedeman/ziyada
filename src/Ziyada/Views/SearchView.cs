using System.Data;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class SearchView : View
{
    private readonly WingetService _winget;
    private readonly TextField _searchField;
    private readonly TableView _table;
    private readonly Label _statusLabel;
    private List<Package> _packages = [];

    public SearchView(WingetService winget)
    {
        _winget = winget;
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();

        var searchLabel = new Label { Text = "Search: ", X = 0, Y = 0, ColorScheme = Theme.Accent };
        _searchField = new TextField { X = 8, Y = 0, Width = Dim.Fill(10), Text = "" };
        var searchBtn = new Button { Text = "Go", X = Pos.AnchorEnd(6), Y = 0, ColorScheme = Theme.Button };
        searchBtn.Accepting += OnSearch;
        _searchField.Accepting += OnSearch;

        _statusLabel = new Label { Text = "Enter a search term and press Enter", X = 0, Y = 1, Width = Dim.Fill(), ColorScheme = Theme.Status };

        _table = new TableView
        {
            X = 0,
            Y = 2,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
            FullRowSelect = true,
            ColorScheme = Theme.Table,
        };        _table.Table = new DataTableSource(CreateDataTable());

        var installBtn = new Button { Text = "Install (F2/Enter)", X = 0, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        installBtn.Accepting += (s, e) => DoInstall();

        // Enter on a table row triggers install
        _table.CellActivated += (s, e) => DoInstall();

        // F2 keybinding for install
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == KeyCode.F2)
            {
                DoInstall();
                e.Handled = true;
            }
        };

        Add(searchLabel, _searchField, searchBtn, _statusLabel, _table, installBtn);
    }

    private DataTable CreateDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("Version", typeof(string));
        dt.Columns.Add("Source", typeof(string));
        return dt;
    }

    private void RefreshTable()
    {
        var dt = CreateDataTable();
        foreach (var p in _packages)
            dt.Rows.Add(p.Name, p.Id, p.Version, p.Source);
        _table.Table = new DataTableSource(dt);
        _table.SetNeedsDraw();
    }

    private void OnSearch(object? sender, EventArgs e)
    {
        string query = _searchField.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(query)) return;

        _statusLabel.Text = $"Searching for '{query}'...";
        _statusLabel.SetNeedsDraw();

        Task.Run(async () =>
        {
            try
            {
                var packages = await _winget.SearchAsync(query);
                Application.Invoke(() =>
                {
                    _packages = packages;
                    _statusLabel.Text = $"Found {_packages.Count} package(s)";
                    _statusLabel.SetNeedsDraw();
                    RefreshTable();
                });
                Application.Wakeup();
            }
            catch (Exception ex)
            {
                Application.Invoke(() =>
                {
                    _statusLabel.Text = $"Error: {ex.Message}";
                    _statusLabel.SetNeedsDraw();
                });
                Application.Wakeup();
            }
        });
    }

    private void DoInstall()
    {
        if (_packages.Count == 0 || _table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        // Custom confirmation dialog with themed buttons
        var confirmDialog = new Dialog
        {
            Title = "Confirm Install",
            Width = 55,
            Height = 8,
            ColorScheme = Theme.Base,
        };
        confirmDialog.Add(new Label
        {
            Text = $"Install {pkg.Name} ({pkg.Id})?",
            X = Pos.Center(),
            Y = 1,
            ColorScheme = Theme.Accent,
        });

        bool confirmed = false;
        var yesBtn = new Button { Text = "Yes", ColorScheme = Theme.Button };
        var noBtn = new Button { Text = "No", ColorScheme = Theme.Button };
        yesBtn.Accepting += (s, e) => { confirmed = true; Application.RequestStop(); };
        noBtn.Accepting += (s, e) => { Application.RequestStop(); };
        confirmDialog.AddButton(yesBtn);
        confirmDialog.AddButton(noBtn);

        Application.Run(confirmDialog);
        if (!confirmed) return;

        // Build progress dialog
        var dialog = new Dialog
        {
            Title = $"Installing {pkg.Name}",
            Width = 60,
            Height = 9,
            ColorScheme = Theme.Base,
        };

        var msgLabel = new Label
        {
            Text = $"Installing {pkg.Id}...",
            X = Pos.Center(),
            Y = 1,
            ColorScheme = Theme.Accent,
        };

        var progressBar = new ProgressBar
        {
            X = 2,
            Y = 3,
            Width = Dim.Fill(2),
            Height = 1,
            ProgressBarStyle = ProgressBarStyle.MarqueeContinuous,
            ColorScheme = Theme.Accent,
        };

        var bgBtn = new Button { Text = "Background", ColorScheme = Theme.Button };
        bool movedToBackground = false;

        bgBtn.Accepting += (s, e) =>
        {
            movedToBackground = true;
            Application.RequestStop();
        };
        dialog.AddButton(bgBtn);
        dialog.Add(msgLabel, progressBar);

        // Pulse the marquee animation
        var pulseTimer = Application.AddTimeout(TimeSpan.FromMilliseconds(100), () =>
        {
            progressBar.Pulse();
            return true;
        });

        // Run install in background
        Task.Run(async () =>
        {
            var installResult = await _winget.InstallAsync(pkg.Id);
            Application.Invoke(() =>
            {
                Application.RemoveTimeout(pulseTimer);
                if (!movedToBackground)
                {
                    // Still showing dialog — update it and close
                    progressBar.ProgressBarStyle = ProgressBarStyle.Continuous;
                    progressBar.Fraction = 1f;
                    msgLabel.Text = installResult.Success
                        ? $"✓ Successfully installed {pkg.Id}"
                        : $"✗ Failed: {installResult.StandardError.Split('\n').FirstOrDefault()}";
                    msgLabel.ColorScheme = installResult.Success ? Theme.Accent : Theme.Status;
                    msgLabel.SetNeedsDraw();
                    progressBar.SetNeedsDraw();

                    // Replace Background button with Close
                    bgBtn.Visible = false;
                    var closeBtn = new Button { Text = "Close", ColorScheme = Theme.Button };
                    closeBtn.Accepting += (s2, e2) => Application.RequestStop();
                    dialog.AddButton(closeBtn);
                }
                else
                {
                    // Was moved to background — update status label
                    _statusLabel.Text = installResult.Success
                        ? $"✓ {pkg.Id} installed successfully"
                        : $"✗ Failed to install {pkg.Id}";
                    _statusLabel.SetNeedsDraw();
                }
            });
            Application.Wakeup();
        });

        Application.Run(dialog);

        // If user closed after completion (not backgrounded), clean up
        if (!movedToBackground)
        {
            Application.RemoveTimeout(pulseTimer);
        }
        else
        {
            _statusLabel.Text = $"⟳ Installing {pkg.Id} in background...";
            _statusLabel.SetNeedsDraw();
        }
    }
}
