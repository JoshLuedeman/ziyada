# Contributing to Ziyada ⚡

Thanks for your interest in contributing to Ziyada! Whether it's a bug fix, new feature, or documentation improvement, your help is welcome.

## Prerequisites

- **Windows 10 or 11**
- **[.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)**
- **[winget](https://github.com/microsoft/winget-cli)** (pre-installed on Windows 11, available for Windows 10)
- **[Git](https://git-scm.com/)**

## Getting Started

```bash
# Clone the repository
git clone https://github.com/JoshLuedeman/ziyada.git
cd ziyada

# Build the project
dotnet build

# Run the app
dotnet run --project src/Ziyada
```

## Running Tests

```bash
dotnet test
```

Make sure all tests pass before submitting a pull request.

## Project Structure

```
src/Ziyada/
├── Program.cs              # Entry point
├── Helpers/                # Process runner, theme
├── Models/                 # Data models (Package, InstalledPackage, SourceInfo)
├── Services/               # Winget CLI wrapper, output parser, source management
└── Views/                  # Terminal.Gui views (Search, Installed, Upgrade, Sources)
```

## How to Contribute

1. **Fork** the repository
2. **Create a branch** for your change (`git checkout -b my-feature`)
3. **Make your changes** and test locally
4. **Run tests** with `dotnet test`
5. **Commit** with a clear message (`git commit -m "Add cool feature"`)
6. **Push** to your fork and open a **Pull Request**

## Pull Request Guidelines

- Use a **descriptive title** that summarizes the change
- **Link related issues** (e.g., "Closes #42")
- Ensure **CI passes** before requesting review
- Keep PRs focused — **one feature or fix per PR**
- Include screenshots for UI changes

## Code Style

- Follow the **existing code conventions** in the project
- Use **nullable reference types** (`string?`, `Package?`, etc.)
- Prefer **async/await** patterns for I/O operations
- Keep methods focused and reasonably sized

## Reporting Issues

Use [GitHub Issues](https://github.com/JoshLuedeman/ziyada/issues) to report bugs or request features. When reporting a bug, please include:

- **Steps to reproduce** the issue
- **Expected vs. actual behavior**
- **winget version** (`winget --version`)
- **OS version** (Windows 10/11 build number)
- **Ziyada version or commit hash**

## License

This project is licensed under the [MIT License](LICENSE). By contributing, you agree that your contributions will be licensed under the same license.
