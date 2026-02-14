using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Views;

public class MainWindow : Window
{
    private readonly TabView _tabView;
    private Label? _updateNotificationLabel;

    public MainWindow()
    {
        Title = $"⚡ Ziyada v{AppVersion.Version} — Winget Terminal UI ⚡";
        ColorScheme = Theme.Base;
        BorderStyle = LineStyle.Rounded;

        _tabView = new TabView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(1),
            ColorScheme = Theme.TabView,
        };

        Add(_tabView);
    }

    public void AddTab(string title, View content)
    {
        var tab = new Tab { DisplayText = title, View = content };
        _tabView.AddTab(tab, _tabView.Tabs.Count == 0);
    }

    public void ShowUpdateNotification(UpdateInfo updateInfo)
    {
        if (_updateNotificationLabel != null)
        {
            // Notification already shown
            return;
        }

        _updateNotificationLabel = new Label
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            Text = $"🎉 Update available! Version {updateInfo.LatestVersion} is now available. Download: {updateInfo.DownloadUrl}",
            ColorScheme = new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
                Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightYellow),
            }
        };

        // Shift the tab view down
        _tabView.Y = 1;
        _tabView.Height = Dim.Fill(1);

        Add(_updateNotificationLabel);
    }
}
