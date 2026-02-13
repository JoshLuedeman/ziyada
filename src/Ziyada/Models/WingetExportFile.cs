namespace Ziyada.Models;

public class WingetExportFile
{
    public string? WinGetVersion { get; set; }
    public List<WingetSource>? Sources { get; set; }
    public List<WingetPackageEntry> Packages { get; set; } = [];
}

public class WingetSource
{
    public string? Name { get; set; }
    public string? Identifier { get; set; }
    public string? Argument { get; set; }
    public string? Type { get; set; }
}

public class WingetPackageEntry
{
    public string PackageIdentifier { get; set; } = string.Empty;
    public string? Version { get; set; }
}
