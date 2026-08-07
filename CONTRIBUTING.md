# Contributing to BetterAccounting

Thank you for your interest in contributing to BetterAccounting! This document provides guidelines and instructions for contributing.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Environment Setup](#development-environment-setup)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Running Tests](#running-tests)
- [Submitting Changes](#submitting-changes)
- [Reporting Issues](#reporting-issues)

## Code of Conduct

We expect all contributors to follow these principles:
- Be respectful and constructive in discussions
- Welcome newcomers and be patient with questions
- Focus on what is best for the project and community
- Show empathy towards other contributors

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/BetterAccounting-WinForms.git
   cd BetterAccounting-WinForms
   ```
3. Set upstream remote:
   ```bash
   git remote add upstream https://github.com/AltafYafai/BetterAccounting-WinForms.git
   ```

## Development Environment Setup

### Prerequisites
- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or Visual Studio Code
- Git for Windows

### Building the Project
1. Open `src/BetterAccounting.sln` in Visual Studio 2022
2. Restore NuGet packages (should happen automatically)
3. Build the solution (`Ctrl+Shift+B`)
4. Run the application (`F5`)

## Project Structure

```
src/
├── Core/                      # Business logic layer
│   ├── Data/Models/           # Domain entities
│   └── Services/              # Service interfaces and implementations
│       ├── Data/              # Data access layer
│       └── Reports/           # Reporting services
└── UI/                        # WPF Presentation layer
    ├── ViewModels/            # MVVM ViewModels
    ├── Views/                 # XAML Views
    └── Themes/                # Resource dictionaries
```

## Coding Standards

We follow Microsoft's .NET coding conventions with these specifics:

### C# Conventions
- Use `var` when type is evident from right-hand side
- Use expression-bodied members where appropriate
- Prefer readonly fields over const where possible
- Use async/await for all I/O operations
- Follow MVVM pattern strictly in UI layer

### Naming Conventions
- Classes: PascalCase (`LedgerService.cs`)
- Methods: PascalCase (`CalculateBalance()`)
- Private fields: camelCase with underscore prefix (`_context`)
- Properties: PascalCase (`TotalAssets`)
- Events: PascalCase with EventHandler suffix (`SaveCompleted`)

### XAML Conventions
- Always use `x:Name` for controls that need code-behind references
- Bind commands using `Command="{Binding ...}"`
- Use `StringFormat=C` for currency bindings
- Avoid code-behind logic except for UI-specific operations

## Running Tests

1. Navigate to test directory:
   ```bash
   cd BetterAccounting-WinForms/src
   dotnet test src/Core.Tests/Core.Tests.csproj
   ```

2. View test results in terminal or use Test Explorer in Visual Studio

## Submitting Changes

1. Create a new branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes following our coding standards

3. Write/update tests for any new functionality

4. Ensure all existing tests pass

5. Commit your changes with a descriptive message:
   ```bash
   git commit -m "Add: Brief description of change"
   ```

6. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

7. Submit a pull request against the `main` branch

### Commit Message Convention
We follow conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Code formatting changes
- `refactor:` Refactoring code
- `test:` Adding/updating tests
- `chore:` Maintenance tasks

Example:
```bash
git commit -m "feat: add GST tax calculation service"
```

## Reporting Issues

When reporting issues, please include:
1. A clear title describing the problem
2. Detailed steps to reproduce
3. Expected behavior vs actual behavior
4. Environment details (Windows version, .NET version)
5. Screenshots or logs if applicable

Thank you for contributing!
