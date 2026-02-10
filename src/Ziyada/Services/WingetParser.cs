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

    private static string SafeSubstring(string s, int start, int length)
    {
        if (start >= s.Length) return string.Empty;
        return s.Substring(start, Math.Min(length, s.Length - start));
    }
}
