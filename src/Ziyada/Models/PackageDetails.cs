namespace Ziyada.Models;

public class PackageDetails
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Homepage { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string LicenseUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public string ReleaseNotesUrl { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = [];
    public string Source { get; set; } = string.Empty;
}
