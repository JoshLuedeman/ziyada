using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Services;
using Ziyada.Views;

Application.Init();
Application.Force16Colors = false;

var wingetService = new WingetService();
var sourceService = new SourceService();

var mainWindow = new MainWindow();

var searchView = new SearchView(wingetService);
var installedView = new InstalledView(wingetService);
var upgradeView = new UpgradeView(wingetService);
var sourcesView = new SourcesView(sourceService);

mainWindow.AddTab("🔍 Search", searchView);
mainWindow.AddTab("📦 Installed", installedView);
mainWindow.AddTab("⬆️ Upgrade", upgradeView);
mainWindow.AddTab("🌐 Sources", sourcesView);

var statusBar = new StatusBar(
[
    new Shortcut(Key.F5, "Refresh", () =>
    {
        installedView.LoadPackagesAsync();
        upgradeView.LoadUpgradesAsync();
        sourcesView.LoadSourcesAsync();
    }),
    new Shortcut(Key.F10, "Quit", () =>
    {
        Application.RequestStop();
    }),
])
{
    ColorScheme = Theme.StatusBarScheme,
};

mainWindow.Add(statusBar);

Application.Run(mainWindow);
Application.Shutdown();
