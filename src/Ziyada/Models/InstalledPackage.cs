namespace Ziyada.Models;

public class InstalledPackage
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string AvailableVersion { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public bool HasUpgrade => !string.IsNullOrEmpty(AvailableVersion);
}
