using Ziyada.Models;

namespace Ziyada.Services;

public static class WingetParser
{
    /// <summary>
    /// Parses winget tabular output by detecting column positions from the header line.
    /// Column boundaries are found using the separator line (all dashes) combined with
    /// the header text positions (columns are separated by 2+ spaces in the header).
    /// </summary>
    public static List<Dictionary<string, string>> ParseTable(string output)
    {
        var lines = output.Split('\n', StringSplitOptions.None)
                          .Select(l => l.TrimEnd('\r'))
                          .ToList();

        // Find the separator line (20+ dashes, ignoring short spinner chars like - \ | /)
        int sepIndex = lines.FindIndex(l => l.Length >= 20 && l.Trim().All(c => c == '-' || c == ' ') && l.Contains('-'));
        if (sepIndex < 1) return [];

        string headerLine = lines[sepIndex - 1];

        // Detect column starts from the header by finding word boundaries after 2+ spaces
        var columnStarts = new List<int> { 0 };
        for (int i = 1; i < headerLine.Length; i++)
        {
            if (headerLine[i] != ' ' && i >= 2 && headerLine[i - 1] == ' ' && headerLine[i - 2] == ' ')
            {
                columnStarts.Add(i);
            }
        }

        var columns = new List<(string Name, int Start)>();
        for (int c = 0; c < columnStarts.Count; c++)
        {
            int start = columnStarts[c];
            int end = c + 1 < columnStarts.Count ? columnStarts[c + 1] : headerLine.Length;
            string colName = headerLine[start..end].Trim();
            columns.Add((colName, start));
        }

        if (columns.Count == 0) return [];

        var results = new List<Dictionary<string, string>>();
        for (int row = sepIndex + 1; row < lines.Count; row++)
        {
            string line = lines[row];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var dict = new Dictionary<string, string>();
            for (int c = 0; c < columns.Count; c++)
            {
                var (name, start) = columns[c];
                string value;
                if (c == columns.Count - 1)
                    value = start < line.Length ? line[start..].Trim() : string.Empty;
                else
                {
                    int nextStart = columns[c + 1].Start;
                    value = SafeSubstring(line, start, nextStart - start).Trim();
                }
                dict[name] = value;
            }
            results.Add(dict);
        }

        return results;
    }

    public static List<Package> ParseSearchResults(string output)
    {
        return ParseTable(output).Select(d => new Package
        {
            Name = d.GetValueOrDefault("Name", ""),
            Id = d.GetValueOrDefault("Id", ""),
            Version = d.GetValueOrDefault("Version", ""),
            Source = d.GetValueOrDefault("Source", ""),
            Match = d.GetValueOrDefault("Match", ""),
        }).Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
    }

    public static List<InstalledPackage> ParseInstalledPackages(string output)
    {
        return ParseTable(output).Select(d => new InstalledPackage
        {
            Name = d.GetValueOrDefault("Name", ""),
            Id = d.GetValueOrDefault("Id", ""),
            Version = d.GetValueOrDefault("Version", ""),
            AvailableVersion = d.GetValueOrDefault("Available", ""),
            Source = d.GetValueOrDefault("Source", ""),
            IsPinned = d.GetValueOrDefault("Pinned", "").Trim() != "",
        }).Where(p => !string.IsNullOrEmpty(p.Id)).ToList();
    }

    public static List<InstalledPackage> ParseUpgradeList(string output)
    {
        return ParseInstalledPackages(output);
    }

    public static List<SourceInfo> ParseSources(string output)
    {
        return ParseTable(output).Select(d => new SourceInfo
        {
            Name = d.GetValueOrDefault("Name", ""),
            Argument = d.GetValueOrDefault("Argument", d.GetValueOrDefault("URL", "")),
            Type = d.GetValueOrDefault("Type", ""),
        }).Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
    }

    public static List<string> ParsePinnedPackages(string output)
    {
        return ParseTable(output)
            .Select(d => d.GetValueOrDefault("Package", d.GetValueOrDefault("Id", "")))
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    public static PackageDetails ParsePackageDetails(string output)
    {
        var details = new PackageDetails();
        var lines = output.Split('\n', StringSplitOptions.None)
                          .Select(l => l.TrimEnd('\r'))
                          .ToList();

        var dependencies = new List<string>();
        bool inDependenciesSection = false;

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                inDependenciesSection = false;
                continue;
            }

            // Check for "Found" line (special case: doesn't have ": ")
            if (line.StartsWith("Found ") && line.Contains('[') && line.Contains(']'))
            {
                int idStart = line.LastIndexOf('[');
                int idEnd = line.LastIndexOf(']');
                if (idStart > 0 && idStart < idEnd)
                {
                    details.Name = line[6..idStart].Trim(); // Skip "Found "
                    details.Id = line[(idStart + 1)..idEnd].Trim();
                }
                continue;
            }

            // Check if line is a key-value pair (contains ": " or ends with ":")
            int colonIndex = line.IndexOf(": ");
            bool isKeyValuePair = colonIndex > 0 || (line.Contains(':') && !line.StartsWith(" "));
            
            if (isKeyValuePair && !line.StartsWith(" "))
            {
                inDependenciesSection = false;
                string key, value;
                
                if (colonIndex > 0)
                {
                    key = line[..colonIndex].Trim();
                    value = line[(colonIndex + 2)..].Trim();
                }
                else
                {
                    // Handle "Key:" format (colon at end, no space after)
                    int colonPos = line.IndexOf(':');
                    key = line[..colonPos].Trim();
                    value = colonPos < line.Length - 1 ? line[(colonPos + 1)..].Trim() : string.Empty;
                }

                switch (key)
                {
                    case "Version":
                        details.Version = value;
                        break;
                    case "Publisher":
                        details.Publisher = value;
                        break;
                    case "Description":
                        details.Description = value;
                        break;
                    case "Homepage":
                    case "Publisher Url":
                    case "Author":
                        if (string.IsNullOrEmpty(details.Homepage))
                            details.Homepage = value;
                        break;
                    case "License":
                        details.License = value;
                        break;
                    case "License Url":
                        details.LicenseUrl = value;
                        break;
                    case "Release Notes":
                        details.ReleaseNotes = value;
                        break;
                    case "Release Notes Url":
                        details.ReleaseNotesUrl = value;
                        break;
                    case "Dependencies":
                        inDependenciesSection = true;
                        // The value after "Dependencies:" might be empty or contain first dependency
                        if (!string.IsNullOrEmpty(value))
                            dependencies.Add(value);
                        break;
                    case "Source":
                        details.Source = value;
                        break;
                }
            }
            else if (inDependenciesSection && line.StartsWith(" "))
            {
                // Continuation of dependencies section
                string dep = line.Trim();
                if (!string.IsNullOrEmpty(dep))
                    dependencies.Add(dep);
            }
        }

        details.Dependencies = dependencies;
        return details;
    }

    private static string SafeSubstring(string s, int start, int length)
    {
        if (start >= s.Length) return string.Empty;
        return s.Substring(start, Math.Min(length, s.Length - start));
    }
}
