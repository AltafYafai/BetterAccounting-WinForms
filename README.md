BetterAccounting
===============

A modern, fast, and intuitive Windows accounting software written in C#/.NET 8 + WPF.

## Architecture Overview

```
src/
├── Core/                          # Business logic layer
│   ├── Data/
│   │   └── Models/
│   │       ├── LedgerEntry.cs     # Core accounting entry model
│   │       ├── Ledger.cs          # Ledger aggregation model
│   │       └── Account.cs         # Chart of Accounts model
│   └── Services/
│       ├── Data/
│       │   ├── IDataContext.cs           # Data interface
│       │   ├── SQLiteContext.cs          # SQLite implementation
│       │   ├── IAccountRepository.cs     # Account interface
│       │   ├── AccountRepository.cs      # Account implementation
│       │   └── BackupService.cs          # Backup/Restore service
│       └── Reports/
│           ├── TrialBalanceService.cs    # Trial Balance generator
│           ├── FinancialStatementService.cs # Balance Sheet logic
│           ├── ProfitAndLossService.cs   # P&L statement generator
│           └── LedgerReportService.cs    # Detailed ledger report
└── UI/                            # Presentation layer
    ├── Models/
    │   └── ThemeManager.cs        # Theme switching logic
    ├── ViewModels/
    │   ├── ViewModelBase.cs       # Base MVVM class
    │   ├── RelayCommand.cs        # ICommand helper
    │   ├── DashboardViewModel.cs  # Main dashboard logic
    │   ├── VoucherEntryViewModel.cs # Voucher entry logic
    │   ├── ReportViewerViewModel.cs # Report viewer logic
    │   ├── SyncSettingsViewModel.cs # Sync configuration
    │   └── BackupRestoreViewModel.cs # Backup/restore logic
    ├── Views/
    │   ├── MainWindow.xaml       # Main dashboard view
    │   ├── VoucherEntryView.xaml # Voucher entry form
    │   ├── ReportViewerView.xaml # Report viewer
    │   ├── SyncSettingsView.xaml # Sync configuration
    │   ├── BackupRestoreView.xaml # Backup/restore UI
    │   └── ChartOfAccountsView.xaml # Account management
    ├── Themes/
    │   ├── LightTheme.xaml       # Light theme resources
    │   └── DarkTheme.xaml        # Dark theme resources
    └── App.xaml                  # Application entry point

tests/                           # Unit/integration tests
README.md                        # Project documentation
.gitignore                       # Git ignore rules
LICENSE                          # MIT License
```

## Features

### Core Accounting
- **Ledger Management** – Double-entry bookkeeping with full audit trail
- **SQLite Data Layer** – Encrypted local database storage
- **Chart of Accounts** – Group-based account management
- **Financial Reports** – Trial Balance, Balance Sheet, Profit & Loss, Ledger views
- **Voucher Entry** – Full-featured transaction recording with validation

### User Experience
- **Themes** – Light and Dark modes with persistent preferences
- **Responsive UI** – MVVM pattern with WPF best practices
- **Intuitive Navigation** – Toolbar-based interface

### Sync & Backup
- **Cloud Sync** – OneDrive/Google Drive integration via configurable sync folder
- **Backup/Restore** – Encrypted ZIP backups with timestamp retention

## Setup (Windows)

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download)
2. Clone and open `src/BetterAccounting.sln` in Visual Studio 2022
3. Restore packages, build solution (`F6`), and run (`F5`)

## License
MIT License - free for personal and commercial use.

## Contributing
Contributions are welcome! See our [documentation](README.md) for architecture details.

Contact: betteraccounting@yourdomain.com
