using System.Text.Json;
using Ziyada.Models;

namespace Ziyada.Services;

public class ConfigurationService
{
    private static ConfigurationService? _instance;
    private static readonly object _lock = new();
    private readonly string _configDirectory;
    private readonly string _configFilePath;
    private AppSettings _settings;

    private ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appDataPath, "Ziyada");
        _configFilePath = Path.Combine(_configDirectory, "appsettings.json");
        
        Directory.CreateDirectory(_configDirectory);
        _settings = LoadSettings();
    }

    public static ConfigurationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ConfigurationService();
                }
            }
            return _instance;
        }
    }

    public AppSettings Settings => _settings;

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    LoggingService.Instance.LogInfo("Configuration loaded successfully");
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("Failed to load configuration, using defaults", exception: ex);
        }

        // Return default settings if file doesn't exist or loading failed
        return new AppSettings();
    }

    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_configFilePath, json);
            LoggingService.Instance.LogInfo("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.LogError("Failed to save configuration", exception: ex);
        }
    }
}
