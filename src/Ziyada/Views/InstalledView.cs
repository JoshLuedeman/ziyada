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

        var refreshBtn = new Button { Text = "Refresh", X = 0, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        refreshBtn.Accepting += (s, e) => LoadPackagesAsync();

        var uninstallBtn = new Button { Text = "Uninstall (Del)", X = Pos.Right(refreshBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        uninstallBtn.Accepting += OnUninstall;

        var exportBtn = new Button { Text = "Export", X = Pos.Right(uninstallBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        exportBtn.Accepting += OnExport;

        var importBtn = new Button { Text = "Import", X = Pos.Right(exportBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        importBtn.Accepting += OnImport;

        Add(_statusLabel, _userOnlyFilter, _table, refreshBtn, uninstallBtn, exportBtn, importBtn);
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

    private void OnUninstall(object? sender, EventArgs e)
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        int result = MessageBox.Query("Uninstall", $"Uninstall {pkg.Name} ({pkg.Id})?", "Yes", "No");
        if (result != 0) return;

        _statusLabel.Text = $"Uninstalling {pkg.Id}...";

        Task.Run(async () =>
        {
            var uninstallResult = await _winget.UninstallAsync(pkg.Id);
            Application.Invoke(() =>
            {
                if (uninstallResult.Success)
                {
                    _statusLabel.Text = $"Successfully uninstalled {pkg.Id}";
                    LoadPackagesAsync();
                }
                else
                {
                    _statusLabel.Text = $"Failed: {uninstallResult.StandardError.Split('\n').FirstOrDefault()}";
                }
            });
            Application.Wakeup();
        });
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
