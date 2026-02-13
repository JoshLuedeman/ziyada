using System.Data;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class InstalledView : View
{
    private readonly WingetService _winget;
    private readonly TableView _table;
    private readonly Label _statusLabel;
    private readonly CheckBox _userOnlyFilter;
    private List<InstalledPackage> _packages = [];

    public InstalledView(WingetService winget)
    {
        _winget = winget;
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();

        _statusLabel = new Label { Text = "Press F5 or Refresh to load installed packages", X = 0, Y = 0, Width = Dim.Fill(), ColorScheme = Theme.Status };

        _userOnlyFilter = new CheckBox { Text = "User-installed only", X = 0, Y = 1, CheckedState = CheckState.Checked, ColorScheme = Theme.Accent };
        _userOnlyFilter.CheckedStateChanged += (s, e) => LoadPackagesAsync();

        _table = new TableView
        {
            X = 0,
            Y = 2,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
            FullRowSelect = true,
            ColorScheme = Theme.Table,
        };
        _table.Table = new DataTableSource(CreateDataTable());

        var detailsBtn = new Button { Text = "Details (F4)", X = 0, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        detailsBtn.Accepting += (s, e) => ShowPackageDetails();

        var refreshBtn = new Button { Text = "Refresh", X = Pos.Right(detailsBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        refreshBtn.Accepting += (s, e) => LoadPackagesAsync();

        var uninstallBtn = new Button { Text = "Uninstall (F3/Del)", X = Pos.Right(refreshBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        uninstallBtn.Accepting += OnUninstall;

        var exportBtn = new Button { Text = "Export", X = Pos.Right(uninstallBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        exportBtn.Accepting += OnExport;

        var importBtn = new Button { Text = "Import", X = Pos.Right(exportBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        importBtn.Accepting += OnImport;

        // Keyboard shortcuts
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == KeyCode.F4)
            {
                ShowPackageDetails();
                e.Handled = true;
            }
            else if (e.KeyCode == KeyCode.F3 || e.KeyCode == KeyCode.Delete)
            {
                OnUninstall(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == KeyCode.F5)
            {
                LoadPackagesAsync();
                e.Handled = true;
            }
        };

        Add(_statusLabel, _userOnlyFilter, _table, detailsBtn, refreshBtn, uninstallBtn, exportBtn, importBtn);
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

    public void LoadPackagesAsync()
    {
        bool userOnly = _userOnlyFilter.CheckedState == CheckState.Checked;
        string filterText = userOnly ? " (user-installed)" : " (all)";
        _statusLabel.Text = $"Loading installed packages{filterText}...";
        _statusLabel.SetNeedsDraw();

        Task.Run(async () =>
        {
            var packages = await _winget.ListInstalledAsync(userOnly);
            Application.Invoke(() =>
            {
                _packages = packages;
                _statusLabel.Text = $"{_packages.Count} installed package(s){filterText}";
                RefreshTable();
            });
            Application.Wakeup();
        });
    }

    private void ShowPackageDetails()
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        var detailsDialog = new PackageDetailsDialog(_winget, pkg.Id, pkg.Name);
        Application.Run(detailsDialog);
    }

    private void OnUninstall(object? sender, EventArgs e)
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        // Custom confirmation dialog with themed buttons
        var confirmDialog = new Dialog
        {
            Title = "Confirm Uninstall",
            Width = 55,
            Height = 8,
            ColorScheme = Theme.Base,
        };
        confirmDialog.Add(new Label
        {
            Text = $"Uninstall {pkg.Name} ({pkg.Id})?",
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
            Title = $"Uninstalling {pkg.Name}",
            Width = 60,
            Height = 9,
            ColorScheme = Theme.Base,
        };

        var msgLabel = new Label
        {
            Text = $"Uninstalling {pkg.Id}...",
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

        // Run uninstall in background
        Task.Run(async () =>
        {
            var uninstallResult = await _winget.UninstallAsync(pkg.Id);
            Application.Invoke(() =>
            {
                Application.RemoveTimeout(pulseTimer);
                if (!movedToBackground)
                {
                    // Still showing dialog — update it and close
                    progressBar.ProgressBarStyle = ProgressBarStyle.Continuous;
                    progressBar.Fraction = 1f;
                    msgLabel.Text = uninstallResult.Success
                        ? $"✓ Successfully uninstalled {pkg.Id}"
                        : $"✗ Failed: {uninstallResult.StandardError.Split('\n').FirstOrDefault()}";
                    msgLabel.ColorScheme = uninstallResult.Success ? Theme.Accent : Theme.Status;
                    msgLabel.SetNeedsDraw();
                    progressBar.SetNeedsDraw();

                    // Replace Background button with Close
                    bgBtn.Visible = false;
                    var closeBtn = new Button { Text = "Close", ColorScheme = Theme.Button };
                    closeBtn.Accepting += (s2, e2) => Application.RequestStop();
                    dialog.AddButton(closeBtn);

                    // Refresh list if successful
                    if (uninstallResult.Success)
                    {
                        LoadPackagesAsync();
                    }
                }
                else
                {
                    // Was moved to background — update status label
                    _statusLabel.Text = uninstallResult.Success
                        ? $"✓ {pkg.Id} uninstalled successfully"
                        : $"✗ Failed to uninstall {pkg.Id}";
                    _statusLabel.SetNeedsDraw();

                    // Refresh list if successful
                    if (uninstallResult.Success)
                    {
                        LoadPackagesAsync();
                    }
                }
            });
            Application.Wakeup();
        });

        Application.Run(dialog);
    }

    private void OnExport(object? sender, EventArgs e)
    {
        var dialog = new SaveDialog { Title = "Export packages" };
        Application.Run(dialog);
        if (dialog.Canceled || string.IsNullOrEmpty(dialog.Path)) return;

        _statusLabel.Text = "Exporting...";
        string path = dialog.Path;

        Task.Run(async () =>
        {
            var result = await _winget.ExportAsync(path);
            Application.Invoke(() =>
            {
                _statusLabel.Text = result.Success ? $"Exported to {path}" : $"Export failed: {result.StandardError.Split('\n').FirstOrDefault()}";
            });
            Application.Wakeup();
        });
    }

    private void OnImport(object? sender, EventArgs e)
    {
        var dialog = new OpenDialog { Title = "Import packages" };
        Application.Run(dialog);
        if (dialog.Canceled || dialog.Path == null) return;

        _statusLabel.Text = "Importing...";
        string path = dialog.Path;

        Task.Run(async () =>
        {
            var result = await _winget.ImportAsync(path);
            Application.Invoke(() =>
            {
                _statusLabel.Text = result.Success ? "Import complete" : $"Import failed: {result.StandardError.Split('\n').FirstOrDefault()}";
                if (result.Success) LoadPackagesAsync();
            });
            Application.Wakeup();
        });
    }
}
