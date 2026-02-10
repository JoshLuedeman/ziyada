using Terminal.Gui;
using Ziyada.Helpers;

namespace Ziyada.Views;

public class MainWindow : Window
{
    private readonly TabView _tabView;

    public MainWindow()
    {
        Title = "⚡ Ziyada — Winget Terminal UI ⚡";
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
}
