using System.Reflection;

namespace Ziyada.Helpers;

public static class AppVersion
{
    public static string Version { get; } = GetVersion();

    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.1.0";
    }
}
