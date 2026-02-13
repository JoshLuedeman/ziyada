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
    private HashSet<int> _selectedIndices = [];

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

        var detailsBtn = new Button { Text = "Details (F4)", X = 0, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        detailsBtn.Accepting += (s, e) => ShowPackageDetails();

        var pinBtn = new Button { Text = "Toggle Pin (F6)", X = Pos.Right(detailsBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        pinBtn.Accepting += (s, e) => TogglePin();

        var refreshBtn = new Button { Text = "Refresh", X = Pos.Right(pinBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        refreshBtn.Accepting += (s, e) => LoadUpgradesAsync();

        var upgradeBtn = new Button { Text = "Upgrade Selected (Ctrl+U)", X = Pos.Right(refreshBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        upgradeBtn.Accepting += OnUpgradeSelected;

        var upgradeAllBtn = new Button { Text = "Upgrade All", X = Pos.Right(upgradeBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        upgradeAllBtn.Accepting += OnUpgradeAll;

        var selectAllBtn = new Button { Text = "Select All (Ctrl+A)", X = Pos.Right(upgradeAllBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        selectAllBtn.Accepting += (s, e) => SelectAll();

        var deselectAllBtn = new Button { Text = "Deselect All (Ctrl+D)", X = Pos.Right(selectAllBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        deselectAllBtn.Accepting += (s, e) => DeselectAll();

        // Keyboard shortcuts
        KeyDown += (s, e) =>
        {
            if (e.KeyCode == KeyCode.F4)
            {
                ShowPackageDetails();
                e.Handled = true;
            }
            else if (e.KeyCode == KeyCode.F6)
            {
                TogglePin();
                e.Handled = true;
            }
            else if (e.KeyCode == KeyCode.Space)
            {
                ToggleSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.U | KeyCode.CtrlMask))
            {
                OnUpgradeSelected(null, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.A | KeyCode.CtrlMask))
            {
                SelectAll();
                e.Handled = true;
            }
            else if (e.KeyCode == (KeyCode.D | KeyCode.CtrlMask))
            {
                DeselectAll();
                e.Handled = true;
            }
        };

        Add(_statusLabel, _table, detailsBtn, pinBtn, refreshBtn, upgradeBtn, upgradeAllBtn, selectAllBtn, deselectAllBtn);
    }

    private DataTable CreateDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("☑", typeof(string)); // Checkbox column
        dt.Columns.Add("📌", typeof(string)); // Pin status column
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
        for (int i = 0; i < _packages.Count; i++)
        {
            var p = _packages[i];
            string checkbox = _selectedIndices.Contains(i) ? "☑" : "☐";
            string pinStatus = p.IsPinned ? "📌" : "";
            dt.Rows.Add(checkbox, pinStatus, p.Name, p.Id, p.Version, p.AvailableVersion, p.Source);
        }
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
                _selectedIndices.Clear(); // Clear selections on refresh
                int pinnedCount = _packages.Count(p => p.IsPinned);
                string pinnedText = pinnedCount > 0 ? $" ({pinnedCount} pinned)" : "";
                _statusLabel.Text = $"{_packages.Count} upgrade(s) available{pinnedText}";
                RefreshTable();
            });
            Application.Wakeup();
        });
    }

    private void ToggleSelection()
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        
        if (_selectedIndices.Contains(_table.SelectedRow))
            _selectedIndices.Remove(_table.SelectedRow);
        else
            _selectedIndices.Add(_table.SelectedRow);
        
        RefreshTable();
    }

    private void SelectAll()
    {
        _selectedIndices.Clear();
        for (int i = 0; i < _packages.Count; i++)
            _selectedIndices.Add(i);
        RefreshTable();
    }

    private void DeselectAll()
    {
        _selectedIndices.Clear();
        RefreshTable();
    }

    private void ShowPackageDetails()
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        var detailsDialog = new PackageDetailsDialog(_winget, pkg.Id, pkg.Name);
        Application.Run(detailsDialog);
    }

    private void TogglePin()
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _packages.Count) return;
        var pkg = _packages[_table.SelectedRow];

        _statusLabel.Text = pkg.IsPinned ? $"Unpinning {pkg.Id}..." : $"Pinning {pkg.Id}...";

        Task.Run(async () =>
        {
            ProcessResult result;
            if (pkg.IsPinned)
                result = await _winget.UnpinAsync(pkg.Id);
            else
                result = await _winget.PinAsync(pkg.Id);

            Application.Invoke(() =>
            {
                if (result.Success)
                {
                    pkg.IsPinned = !pkg.IsPinned;
                    _statusLabel.Text = pkg.IsPinned ? $"Pinned {pkg.Id}" : $"Unpinned {pkg.Id}";
                    RefreshTable();
                }
                else
                {
                    _statusLabel.Text = $"Failed: {result.StandardError.Split('\n').FirstOrDefault()}";
                }
            });
            Application.Wakeup();
        });
    }

    private void OnUpgradeSelected(object? sender, EventArgs e)
    {
        // Check if any packages are selected
        if (_selectedIndices.Count > 0)
        {
            // Bulk upgrade
            DoBulkUpgrade();
        }
        else if (_table.SelectedRow >= 0 && _table.SelectedRow < _packages.Count)
        {
            // Single upgrade (fallback to old behavior)
            DoSingleUpgrade();
        }
        else
        {
            MessageBox.ErrorQuery("No Selection", "Please select packages to upgrade using Space or Ctrl+A", "OK");
        }
    }

    private void DoSingleUpgrade()
    {
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

    private void DoBulkUpgrade()
    {
        var selectedPackages = _selectedIndices.OrderBy(i => i).Select(i => _packages[i]).ToList();

        // Confirmation dialog
        var confirmDialog = new Dialog
        {
            Title = "Confirm Bulk Upgrade",
            Width = 60,
            Height = 9,
            ColorScheme = Theme.Base,
        };
        confirmDialog.Add(new Label
        {
            Text = $"Upgrade {selectedPackages.Count} selected package(s)?",
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

        // Progress dialog for bulk upgrade
        var dialog = new Dialog
        {
            Title = "Upgrading Packages",
            Width = 70,
            Height = 11,
            ColorScheme = Theme.Base,
        };

        var overallLabel = new Label
        {
            Text = $"Upgrading 0/{selectedPackages.Count} packages...",
            X = Pos.Center(),
            Y = 1,
            ColorScheme = Theme.Accent,
        };

        var currentLabel = new Label
        {
            Text = "",
            X = Pos.Center(),
            Y = 2,
            ColorScheme = Theme.Status,
        };

        var progressBar = new ProgressBar
        {
            X = 2,
            Y = 4,
            Width = Dim.Fill(2),
            Height = 1,
            ProgressBarStyle = ProgressBarStyle.Continuous,
            ColorScheme = Theme.Accent,
            Fraction = 0f,
        };

        var resultsLabel = new Label
        {
            Text = "",
            X = 2,
            Y = 6,
            Width = Dim.Fill(2),
            Height = 2,
            ColorScheme = Theme.Status,
        };

        var bgBtn = new Button { Text = "Background", ColorScheme = Theme.Button };
        bool movedToBackground = false;

        bgBtn.Accepting += (s, e) =>
        {
            movedToBackground = true;
            Application.RequestStop();
        };
        dialog.AddButton(bgBtn);
        dialog.Add(overallLabel, currentLabel, progressBar, resultsLabel);

        // Run upgrades sequentially
        Task.Run(async () =>
        {
            int completed = 0;
            int succeeded = 0;
            int failed = 0;
            var failedPackages = new List<string>();

            foreach (var pkg in selectedPackages)
            {
                Application.Invoke(() =>
                {
                    currentLabel.Text = $"Upgrading {pkg.Id}...";
                    currentLabel.SetNeedsDraw();
                });
                Application.Wakeup();

                var upgradeResult = await _winget.UpgradeAsync(pkg.Id);
                completed++;

                if (upgradeResult.Success)
                    succeeded++;
                else
                {
                    failed++;
                    failedPackages.Add(pkg.Id);
                }

                Application.Invoke(() =>
                {
                    progressBar.Fraction = (float)completed / selectedPackages.Count;
                    overallLabel.Text = $"Upgrading {completed}/{selectedPackages.Count} packages...";
                    resultsLabel.Text = $"✓ Succeeded: {succeeded}  ✗ Failed: {failed}";
                    
                    overallLabel.SetNeedsDraw();
                    progressBar.SetNeedsDraw();
                    resultsLabel.SetNeedsDraw();
                });
                Application.Wakeup();
            }

            // All done
            Application.Invoke(() =>
            {
                if (!movedToBackground)
                {
                    currentLabel.Text = "Upgrade complete!";
                    overallLabel.Text = $"Upgraded {completed}/{selectedPackages.Count} packages";
                    
                    if (failedPackages.Count > 0)
                    {
                        string failedList = string.Join(", ", failedPackages.Take(3));
                        if (failedPackages.Count > 3)
                            failedList += $", and {failedPackages.Count - 3} more...";
                        resultsLabel.Text = $"✓ Succeeded: {succeeded}  ✗ Failed: {failed}\nFailed: {failedList}";
                    }

                    currentLabel.SetNeedsDraw();
                    overallLabel.SetNeedsDraw();
                    resultsLabel.SetNeedsDraw();

                    // Replace Background button with Close
                    bgBtn.Visible = false;
                    var closeBtn = new Button { Text = "Close", ColorScheme = Theme.Button };
                    closeBtn.Accepting += (s2, e2) => Application.RequestStop();
                    dialog.AddButton(closeBtn);
                }
                else
                {
                    // Was moved to background — update status label
                    _statusLabel.Text = $"✓ Upgraded {succeeded}/{selectedPackages.Count} packages (Failed: {failed})";
                    _statusLabel.SetNeedsDraw();
                }

                // Refresh upgrade list
                LoadUpgradesAsync();
            });
            Application.Wakeup();
        });

        Application.Run(dialog);

        if (movedToBackground)
        {
            _statusLabel.Text = $"⟳ Upgrading {selectedPackages.Count} packages in background...";
            _statusLabel.SetNeedsDraw();
        }
        else
        {
            // Clear selections after successful bulk upgrade
            _selectedIndices.Clear();
            RefreshTable();
        }
    }

    private void OnUpgradeAll(object? sender, EventArgs e)
    {
        var unpinnedCount = _packages.Count(p => !p.IsPinned);
        var pinnedCount = _packages.Count(p => p.IsPinned);
        
        if (unpinnedCount == 0)
        {
            MessageBox.ErrorQuery("No Packages", "All packages are pinned. Unpin some packages to upgrade them.", "OK");
            return;
        }

        string message = pinnedCount > 0 
            ? $"Upgrade {unpinnedCount} package(s)? ({pinnedCount} pinned will be skipped)"
            : $"Upgrade all {_packages.Count} package(s)?";
        
        int result = MessageBox.Query("Upgrade All", message, "Yes", "No");
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
