using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Services;
using Ziyada.Views;

Application.Init();
Application.Force16Colors = false;

// Initialize logging
var logger = LoggingService.Instance;
logger.LogInfo("Ziyada application started");

// Initialize configuration
var config = ConfigurationService.Instance;

var wingetService = new WingetService();
var sourceService = new SourceService();

var mainWindow = new MainWindow();

var searchView = new SearchView(wingetService);
var installedView = new InstalledView(wingetService);
var upgradeView = new UpgradeView(wingetService);
var sourcesView = new SourcesView(sourceService);
var logsView = new LogsView();

mainWindow.AddTab("🔍 Search", searchView);
mainWindow.AddTab("📦 Installed", installedView);
mainWindow.AddTab("⬆️ Upgrade", upgradeView);
mainWindow.AddTab("🌐 Sources", sourcesView);
mainWindow.AddTab("📋 Logs", logsView);

// Load initial data for logs
logsView.LoadLogsAsync();

// Check for updates in background if enabled
if (config.Settings.CheckForUpdates)
{
    _ = Task.Run(async () =>
    {
        try
        {
            var updateCheckService = new UpdateCheckService();
            var updateInfo = await updateCheckService.CheckForUpdatesAsync();
            
            if (updateInfo.IsUpdateAvailable)
            {
                Application.Invoke(() =>
                {
                    mainWindow.ShowUpdateNotification(updateInfo);
                    Application.Wakeup();
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error checking for updates", exception: ex);
        }
    });
}

var statusBar = new StatusBar(
[
    new Shortcut(Key.F5, "Refresh", () =>
    {
        installedView.LoadPackagesAsync();
        upgradeView.LoadUpgradesAsync();
        sourcesView.LoadSourcesAsync();
        logsView.LoadLogsAsync();
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

// Shutdown logging
logger.LogInfo("Ziyada application shutting down");
logger.Close();

Application.Shutdown();
