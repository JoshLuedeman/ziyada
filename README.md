# Ziyada ⚡

A terminal UI for [winget](https://github.com/microsoft/winget-cli) — the Windows Package Manager.

![C#](https://img.shields.io/badge/C%23-.NET%209-blue) ![Terminal.Gui](https://img.shields.io/badge/TUI-Terminal.Gui%20v2-green) ![License](https://img.shields.io/badge/license-MIT-yellow)

## Features

- **🔍 Search & Install** — Search the winget repository, browse results in a table, install with Enter or F2
- **📦 Bulk Install/Upgrade** — Multi-select packages with checkboxes and install/upgrade them all at once with queue-based execution
- **📦 Installed Packages** — View all installed packages with a "User-installed only" filter to hide system packages
- **⬆️ Upgrades** — See available upgrades, upgrade individual packages or all at once
- **📌 Package Pinning** — Pin packages to exclude them from "Upgrade All" operations using F6 or the pin button
- **🌐 Source Management** — List, add, and remove winget sources
- **📤 Export/Import** — Export your installed packages to JSON, import on another machine
- **🔔 Auto-Update Check** — Automatically checks for new Ziyada releases on startup (configurable)
- **🎨 Dark Theme** — Cyberpunk-inspired dark UI with neon cyan/green accents
- **⏳ Progress Dialog** — Animated install progress with option to background long installs

## Screenshots

*Coming soon*

## Requirements

- Windows 10/11
- [winget](https://github.com/microsoft/winget-cli) (pre-installed on Windows 11, available for Windows 10)
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

## Installation

### From Releases

Download the latest release from the [Releases page](https://github.com/JoshLuedeman/ziyada/releases):

**MSI Installer (recommended):**
- **Ziyada-vX.Y.Z-win-x64.msi** — Installer for Intel/AMD systems
- **Ziyada-vX.Y.Z-win-arm64.msi** — Installer for ARM64 systems (Surface Pro, Copilot+ PCs)

The MSI installer adds Ziyada to your system PATH so you can run `ziyada` from any terminal. To install silently from the command line:

```cmd
msiexec /i Ziyada-vX.Y.Z-win-x64.msi /quiet
```

**Portable (no install):**

*x64 (Intel/AMD):*
- **Ziyada-vX.Y.Z-win-x64.zip** — Zipped executable (extract and run `Ziyada.exe`)
- **Ziyada-win-x64.exe** — Raw executable (run directly)

*ARM64 (Surface Pro, Copilot+ PCs):*
- **Ziyada-vX.Y.Z-win-arm64.zip** — Zipped executable (extract and run `Ziyada.exe`)
- **Ziyada-win-arm64.exe** — Raw executable (run directly)

### From Source

```bash
git clone https://github.com/JoshLuedeman/ziyada.git
cd ziyada
dotnet build
dotnet run --project src/Ziyada
```

### Build a Single Executable

For x64 (Intel/AMD):
```bash
dotnet publish src/Ziyada -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

For ARM64 (Surface Pro, Copilot+ PCs):
```bash
dotnet publish src/Ziyada -c Release -r win-arm64 --self-contained -p:PublishSingleFile=true -o publish
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Tab` | Switch between controls |
| `Ctrl+Tab` | Switch tabs |
| `Enter` | Activate button / Install selected package |
| `F2` | Install selected package (Search tab) |
| `F3` / `Delete` | Uninstall selected package (Installed tab) |
| `F4` | Show package details |
| `F5` | Refresh all tabs |
| `F6` | Toggle pin/unpin package (Installed & Upgrade tabs) |
| `Space` | Toggle package selection (Search & Upgrade tabs) |
| `Ctrl+A` | Select all packages (Search & Upgrade tabs) |
| `Ctrl+D` | Deselect all packages (Search & Upgrade tabs) |
| `Ctrl+I` | Install selected packages (Search tab) |
| `Ctrl+U` | Upgrade selected packages (Upgrade tab) |
| `F10` | Quit |

## Architecture

```
src/Ziyada/
├── Program.cs              # Entry point
├── Helpers/
│   ├── ProcessHelper.cs    # Async winget process runner
│   ├── AppVersion.cs       # Version info from assembly
│   └── Theme.cs            # Dark color theme
├── Models/
│   ├── Package.cs          # Search result model
│   ├── InstalledPackage.cs # Installed package model
│   ├── SourceInfo.cs       # Winget source model
│   ├── AppSettings.cs      # Application configuration
│   └── UpdateInfo.cs       # Update check result
├── Services/
│   ├── WingetService.cs    # Winget CLI wrapper
│   ├── WingetParser.cs     # Tabular output parser
│   ├── SourceService.cs    # Source management
│   ├── ConfigurationService.cs # Settings management
│   ├── UpdateCheckService.cs   # GitHub release checker
│   └── LoggingService.cs   # Logging service
└── Views/
    ├── MainWindow.cs       # Tabbed main window
    ├── SearchView.cs       # Search & install
    ├── InstalledView.cs    # Installed packages
    ├── UpgradeView.cs      # Available upgrades
    └── SourcesView.cs      # Source management
```

## Configuration

Ziyada stores configuration in `%APPDATA%\Ziyada\appsettings.json`:

```json
{
  "CheckForUpdates": true
}
```

To disable automatic update checks, set `CheckForUpdates` to `false`.

## Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT
