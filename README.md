# Ziyada ⚡

A terminal UI for [winget](https://github.com/microsoft/winget-cli) — the Windows Package Manager.

![C#](https://img.shields.io/badge/C%23-.NET%209-blue) ![Terminal.Gui](https://img.shields.io/badge/TUI-Terminal.Gui%20v2-green) ![License](https://img.shields.io/badge/license-MIT-yellow)

## Features

- **🔍 Search & Install** — Search the winget repository, browse results in a table, install with Enter or F2
- **📦 Installed Packages** — View all installed packages with a "User-installed only" filter to hide system packages
- **⬆️ Upgrades** — See available upgrades, upgrade individual packages or all at once
- **🌐 Source Management** — List, add, and remove winget sources
- **📤 Export/Import** — Export your installed packages to JSON, import on another machine
- **🎨 Dark Theme** — Cyberpunk-inspired dark UI with neon cyan/green accents
- **⏳ Progress Dialog** — Animated install progress with option to background long installs

## Screenshots

*Coming soon*

## Requirements

- Windows 10/11
- [winget](https://github.com/microsoft/winget-cli) (pre-installed on Windows 11, available for Windows 10)
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)

## Installation

### From Source

```bash
git clone https://github.com/JoshLuedeman/ziyada.git
cd ziyada
dotnet build
dotnet run --project src/Ziyada
```

### Build a Single Executable

```bash
dotnet publish src/Ziyada -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Tab` | Switch between controls |
| `Ctrl+Tab` | Switch tabs |
| `Enter` | Activate button / Install selected package |
| `F2` | Install selected package (Search tab) |
| `F5` | Refresh all tabs |
| `F10` | Quit |

## Architecture

```
src/Ziyada/
├── Program.cs              # Entry point
├── Helpers/
│   ├── ProcessHelper.cs    # Async winget process runner
│   └── Theme.cs            # Dark color theme
├── Models/
│   ├── Package.cs          # Search result model
│   ├── InstalledPackage.cs # Installed package model
│   └── SourceInfo.cs       # Winget source model
├── Services/
│   ├── WingetService.cs    # Winget CLI wrapper
│   ├── WingetParser.cs     # Tabular output parser
│   └── SourceService.cs    # Source management
└── Views/
    ├── MainWindow.cs       # Tabbed main window
    ├── SearchView.cs       # Search & install
    ├── InstalledView.cs    # Installed packages
    ├── UpgradeView.cs      # Available upgrades
    └── SourcesView.cs      # Source management
```

## Contributing

Contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT
