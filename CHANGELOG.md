# Changelog

All notable changes to Ziyada will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.1] - 2026-04-22

### Fixed

- **MSI installer** — Embedded the cabinet file directly into the MSI so installers work when run from any directory (e.g. Downloads) without requiring a companion `cab1.cab` file (#34)

## [0.4.0] - 2026-04-08

### Added

- **MSI Installer** — Official MSI installers for win-x64 and win-arm64 are now published with each release (#30)
  - Installs Ziyada to `Program Files\Ziyada`
  - Automatically adds the install directory to the system PATH so `ziyada` works from any terminal
  - Supports silent installation: `msiexec /i Ziyada-vX.Y.Z-win-x64.msi /quiet`
  - Handles upgrades and downgrades cleanly via `MajorUpgrade`

## [0.3.0] - 2026-03-12

### Added

- **Install Failure Summary** — When an installation fails, the failure reason is now displayed on screen and logged to the Logs tab with full details (#28)
  - Single install shows failure reason in both dialog and background-mode status bar
  - Bulk install shows per-package failure reasons in results display
  - All failures logged at Error level with stderr, stdout, and exit code

## [0.2.1] - 2026-02-14

### Fixed

- Fixed nullable reference warnings (CS8604) in `Application.RemoveTimeout()` calls (#25)
  - Added null checks in `SearchView.cs` and `PackageDetailsDialog.cs`

## [0.2.0] - 2026-02-14

### Added

- **Bulk Install/Upgrade** — Multi-select packages with checkboxes and install/upgrade them all at once with queue-based execution (#13)
  - `Space` to toggle individual package selection
  - `Ctrl+A` to select all packages
  - `Ctrl+D` to deselect all packages
  - `Ctrl+I` to install selected packages (Search tab)
  - `Ctrl+U` to upgrade selected packages (Upgrade tab)
- **Package Pinning** — Pin packages to exclude them from "Upgrade All" operations using `F6` or the pin button (#19)
- **Uninstall Packages** — Remove packages directly from the Installed tab with `F3` or `Delete` key (#17)
- **Package Details Dialog** — View detailed package information with `F4` key
- **Auto-Update Check** — Automatically checks for new Ziyada releases on startup via GitHub Releases API, configurable in settings (#24)
- **ARM64 Support** — Native builds for ARM64 devices (Surface Pro, Copilot+ PCs) (#23)
- **Structured Logging** — In-app log viewer and comprehensive logging service for debugging (#22)
- **Per-Package Progress Tracking** — Import workflow now shows individual package progress (#20)
- **Configuration Service** — Settings stored in `%APPDATA%\Ziyada\appsettings.json`
- **Process Helper Abstraction** — Improved testability with mockable process execution (#18)
- Comprehensive test suite with code coverage reporting (#11)
- Integration tests with mock data for WingetService and SourceService
- Issue templates for bugs, features, and documentation
- CODEOWNERS file for required reviews
- Contributing guidelines (CONTRIBUTING.md)

### Changed

- Enhanced release workflow with semantic versioning, branch safety checks, and artifact packaging (#21)
- CI workflow now runs for all PR base branches (#14)
- Improved README with ARM64 installation instructions and new keyboard shortcuts

### Fixed

- Code style: removed unnecessary parentheses from KeyCode.Space

## [0.1.0] - 2026-02-12

### Added

- Initial release
- Terminal UI for winget using Terminal.Gui v2
- **Search & Install** — Search the winget repository, browse results, install with Enter or F2
- **Installed Packages** — View all installed packages with "User-installed only" filter
- **Upgrades** — See available upgrades, upgrade individual packages or all at once
- **Source Management** — List, add, and remove winget sources
- **Export/Import** — Export installed packages to JSON, import on another machine
- **Dark Theme** — Cyberpunk-inspired dark UI with neon cyan/green accents
- **Progress Dialog** — Animated install progress with option to background long installs
