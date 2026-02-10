using System.Data;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class UpgradeView : View
{
    private readonly WingetService _winget;
    private readonly TableView _table;
    private readonly Label _statusLabel;
    private List<InstalledPackage> _packages = [];

    public UpgradeView(WingetService winget)
    {
        _winget = winget;
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();

        _statusLabel = new Label { Text = "Press Refresh to check for upgrades", X = 0, Y = 0, Width = Dim.Fill(), ColorScheme = Theme.Status };

        _table = new TableView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(2),
            FullRowSelect = true,
            ColorScheme = Theme.Table,
        };
        _table.Table = new DataTableSource(CreateDataTable());

        var refreshBtn = new Button { Text = "Refresh", X = 0, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        refreshBtn.Accepting += (s, e) => LoadUpgradesAsync();

        var upgradeBtn = new Button { Text = "Upgrade Selected", X = Pos.Right(refreshBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        upgradeBtn.Accepting += OnUpgradeSelected;

        var upgradeAllBtn = new Button { Text = "Upgrade All", X = Pos.Right(upgradeBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        upgradeAllBtn.Accepting += OnUpgradeAll;

        Add(_statusLabel, _table, refreshBtn, upgradeBtn, upgradeAllBtn);
    }

    private DataTable CreateDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Id", typeof(string));
        dt.Columns.Add("Version", typeof(string));
        dt.Columns.Add("Available", typeof(string));
        dt.Columns.Add("Source", typeof(string));
        return dt;
    }

    private void RefreshTable()
    {
        var dt = CreateDataTable();
        foreach (var p in _packages)
            dt.Rows.Add(p.Name, p.Id, p.Version, p.AvailableVersion, p.Source);
        _table.Table = new DataTableSource(dt);
        _table.SetNeedsDraw();
    }

    public void LoadUpgradesAsync()
    {
        _statusLabel.Text = "Checking for upgrades...";

        Task.Run(async () =>
        {
            var packages = await _winget.ListUpgradesAsync();
            Application.Invoke(() =>
            {
                _packages = packages;
                _statusLabel.Text = $"{_packages.Count} upgrade(s) available";
                RefreshTable();
            });
            Application.Wakeup();
        });
    }

    private void OnUpgradeSelected(object? sender, EventArgs e)
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        int result = MessageBox.Query("Upgrade", $"Upgrade {pkg.Name} from {pkg.Version} to {pkg.AvailableVersion}?", "Yes", "No");
        if (result != 0) return;

        _statusLabel.Text = $"Upgrading {pkg.Id}...";

        Task.Run(async () =>
        {
            var upgradeResult = await _winget.UpgradeAsync(pkg.Id);
            Application.Invoke(() =>
            {
                if (upgradeResult.Success)
                {
                    _statusLabel.Text = $"Successfully upgraded {pkg.Id}";
                    LoadUpgradesAsync();
                }
                else
                {
                    _statusLabel.Text = $"Failed: {upgradeResult.StandardError.Split('\n').FirstOrDefault()}";
                }
            });
            Application.Wakeup();
        });
    }

    private void OnUpgradeAll(object? sender, EventArgs e)
    {
        int result = MessageBox.Query("Upgrade All", $"Upgrade all {_packages.Count} package(s)?", "Yes", "No");
        if (result != 0) return;

        _statusLabel.Text = "Upgrading all packages...";

        Task.Run(async () =>
        {
            var upgradeResult = await _winget.UpgradeAllAsync();
            Application.Invoke(() =>
            {
                _statusLabel.Text = upgradeResult.Success ? "All packages upgraded" : "Some upgrades failed";
                LoadUpgradesAsync();
            });
            Application.Wakeup();
        });
    }
}
