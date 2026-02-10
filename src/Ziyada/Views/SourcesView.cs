using System.Data;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class SourcesView : View
{
    private readonly SourceService _sourceService;
    private readonly TableView _table;
    private readonly Label _statusLabel;
    private List<SourceInfo> _sources = [];

    public SourcesView(SourceService sourceService)
    {
        _sourceService = sourceService;
        CanFocus = true;
        Width = Dim.Fill();
        Height = Dim.Fill();

        _statusLabel = new Label { Text = "Press Refresh to load sources", X = 0, Y = 0, Width = Dim.Fill(), ColorScheme = Theme.Status };

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
        refreshBtn.Accepting += (s, e) => LoadSourcesAsync();

        var addBtn = new Button { Text = "Add Source", X = Pos.Right(refreshBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        addBtn.Accepting += OnAddSource;

        var removeBtn = new Button { Text = "Remove Source", X = Pos.Right(addBtn) + 2, Y = Pos.Bottom(_table), ColorScheme = Theme.Button };
        removeBtn.Accepting += OnRemoveSource;

        Add(_statusLabel, _table, refreshBtn, addBtn, removeBtn);
    }

    private DataTable CreateDataTable()
    {
        var dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("URL", typeof(string));
        dt.Columns.Add("Type", typeof(string));
        return dt;
    }

    private void RefreshTable()
    {
        var dt = CreateDataTable();
        foreach (var s in _sources)
            dt.Rows.Add(s.Name, s.Argument, s.Type);
        _table.Table = new DataTableSource(dt);
        _table.SetNeedsDraw();
    }

    public void LoadSourcesAsync()
    {
        _statusLabel.Text = "Loading sources...";

        Task.Run(async () =>
        {
            var sources = await _sourceService.ListSourcesAsync();
            Application.Invoke(() =>
            {
                _sources = sources;
                _statusLabel.Text = $"{_sources.Count} source(s)";
                RefreshTable();
            });
            Application.Wakeup();
        });
    }

    private void OnAddSource(object? sender, EventArgs e)
    {
        var nameField = new TextField { X = 15, Y = 0, Width = 40 };
        var urlField = new TextField { X = 15, Y = 2, Width = 40 };

        var dialog = new Dialog
        {
            Title = "Add Source",
            Width = 60,
            Height = 10,
        };

        dialog.Add(
            new Label { Text = "Source Name:", X = 1, Y = 0 },
            nameField,
            new Label { Text = "Source URL:", X = 1, Y = 2 },
            urlField
        );

        var okBtn = new Button { Text = "OK" };
        var cancelBtn = new Button { Text = "Cancel" };
        bool ok = false;
        okBtn.Accepting += (s, e) => { ok = true; Application.RequestStop(); };
        cancelBtn.Accepting += (s, e) => { Application.RequestStop(); };
        dialog.AddButton(okBtn);
        dialog.AddButton(cancelBtn);

        Application.Run(dialog);

        if (!ok) return;

        string name = nameField.Text?.Trim() ?? "";
        string url = urlField.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
        {
            MessageBox.ErrorQuery("Error", "Name and URL are required", "OK");
            return;
        }

        _statusLabel.Text = $"Adding source '{name}'...";

        Task.Run(async () =>
        {
            var result = await _sourceService.AddSourceAsync(name, url);
            Application.Invoke(() =>
            {
                _statusLabel.Text = result.Success ? $"Added source '{name}'" : $"Failed: {result.StandardError.Split('\n').FirstOrDefault()}";
                if (result.Success) LoadSourcesAsync();
            });
            Application.Wakeup();
        });
    }

    private void OnRemoveSource(object? sender, EventArgs e)
    {
        if (_table.SelectedRow < 0 || _table.SelectedRow >= _sources.Count) return;
        var src = _sources[_table.SelectedRow];

        int result = MessageBox.Query("Remove Source", $"Remove source '{src.Name}'?", "Yes", "No");
        if (result != 0) return;

        _statusLabel.Text = $"Removing source '{src.Name}'...";

        Task.Run(async () =>
        {
            var removeResult = await _sourceService.RemoveSourceAsync(src.Name);
            Application.Invoke(() =>
            {
                if (removeResult.Success)
                {
                    _statusLabel.Text = $"Removed source '{src.Name}'";
                    LoadSourcesAsync();
                }
                else
                {
                    _statusLabel.Text = $"Failed: {removeResult.StandardError.Split('\n').FirstOrDefault()}";
                }
            });
            Application.Wakeup();
        });
    }
}
