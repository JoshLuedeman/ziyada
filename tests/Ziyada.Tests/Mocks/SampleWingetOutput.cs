namespace Ziyada.Tests.Mocks;

/// <summary>
/// Contains sample winget command output for use in integration tests.
/// </summary>
public static class SampleWingetOutput
{
    public static string SearchResults => string.Join("\n", new[]
    {
        "-",
        "\\",
        "|",
        "/",
        "Name                            Id                                   Version  Match            Source",
        "------------------------------------------------------------------------------------------------------",
        "Visual Studio Code              Microsoft.VisualStudioCode           1.85.1   Tag: vscode      winget",
        "VSCodium                        VSCodium.VSCodium                    1.85.1   Tag: vscode      winget",
        "Visual Studio Code Insiders     Microsoft.VisualStudioCode.Insiders  1.86.0   Tag: vscode      winget",
    });

    public static string SearchNoResults => string.Join("\n", new[]
    {
        "Name                            Id                          Version   Source",
        "------------------------------------------------------------------------------",
    });

    public static string ListInstalledPackages => string.Join("\n", new[]
    {
        "Name                                   Id                                    Version          Available        Source",
        "--------------------------------------------------------------------------------------------------------------------------",
        "Microsoft Visual C++ 2015-2022 Redu…   Microsoft.VCRedist.2015+.x64          14.38.33130.0    14.38.33135.0    winget",
        "Microsoft .NET SDK 8.0.404 (x64)       Microsoft.DotNet.SDK.8                8.0.404                           winget",
        "Windows Terminal                        Microsoft.WindowsTerminal             1.19.3172.0      1.20.3171.0      winget",
        "7-Zip                                   7zip.7zip                             23.01                             winget",
        "Git                                     Git.Git                               2.43.0           2.44.0           winget",
    });

    public static string ListUpgrades => string.Join("\n", new[]
    {
        "-",
        "\\",
        "|",
        "Name                            Id                              Version         Available       Source",
        "------------------------------------------------------------------------------------------------------",
        "Git                             Git.Git                         2.43.0          2.44.0          winget",
        "Visual Studio Code              Microsoft.VisualStudioCode      1.85.0          1.85.1          winget",
        "PowerShell                      Microsoft.PowerShell            7.4.0           7.4.1           winget",
    });

    public static string ListSources => string.Join("\n", new[]
    {
        "Name    Argument                                       Type",
        "---------------------------------------------------------------",
        "winget  https://cdn.winget.microsoft.com/cache          Microsoft.PreIndexed.Package",
        "msstore https://storeedgefd.dsx.mp.microsoft.com/v9.0   Microsoft.Rest",
    });

    public static string PackageDetails => string.Join("\n", new[]
    {
        "Found Visual Studio Code [Microsoft.VisualStudioCode]",
        "Version: 1.85.1",
        "Publisher: Microsoft Corporation",
        "Description: Visual Studio Code is a lightweight but powerful source code editor.",
        "Homepage: https://code.visualstudio.com/",
        "License: MIT",
        "License Url: https://code.visualstudio.com/license",
        "Release Notes: Bug fixes and improvements.",
        "Release Notes Url: https://code.visualstudio.com/updates/v1_85",
        "Dependencies:",
        "  Microsoft.VCRedist.2015+.x64",
        "  Microsoft.DotNet.Runtime.8",
        "Source: winget",
    });

    public static string InstallSuccess => "Successfully installed";

    public static string InstallError => "Failed to install package\nPackage not found in source";

    public static string UpgradeSuccess => "Successfully upgraded";

    public static string UninstallSuccess => "Successfully uninstalled";

    public static string ExportSuccess => "Successfully exported";

    public static string ImportSuccess => "Successfully imported";

    public static string SourceAddSuccess => "Source added successfully";

    public static string SourceRemoveSuccess => "Source removed successfully";

    public static string NotFoundError => "The system cannot find the file specified.";

    public static string MalformedOutput => "This is not valid winget output\nRandom text\n123";

    public static string EmptyOutput => "";
}
