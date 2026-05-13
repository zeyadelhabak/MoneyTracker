# 💰 MoneyTracker

> **Personal Finance Manager** — A modern desktop application for tracking income, expenses, and financial analytics.

![Platform](https://img.shields.io/badge/Platform-Windows-blue?logo=windows)
![Framework](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)
![UI](https://img.shields.io/badge/UI-Windows%20Forms-orange)
![Language](https://img.shields.io/badge/Language-C%23%2012-green?logo=csharp)
![Storage](https://img.shields.io/badge/Storage-CSV%20(Local)-yellow)
![Dependencies](https://img.shields.io/badge/Dependencies-Zero-brightgreen)

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Screenshots & UI](#-screenshots--ui)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [How to Use](#-how-to-use)
- [Keyboard Shortcuts](#-keyboard-shortcuts)
- [Data Storage](#-data-storage)
- [Configuration](#-configuration)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🌟 Overview

MoneyTracker is a **professional desktop finance application** built with C# and Windows Forms on .NET 8. It helps you manage your personal finances with a clean, modern dark-themed interface — tracking transactions, visualizing spending patterns, and generating monthly reports.

The app runs **fully offline** with **zero external dependencies**. All data is stored locally as plain CSV files — no database, no cloud, no internet required.

---

## ✨ Features

### 🔐 Authentication
- User registration with validation (username 3–31 chars, password 6+ chars)
- Secure login with specific error messages per field
- Show / hide password toggle (👁)
- Per-user isolated data files
- Auto-backup on sign-out

### 💳 Transaction Management
| Feature | Details |
|---------|---------|
| Add Income | With category, amount, date, note |
| Add Expense | With 8 expense categories |
| Edit Transaction | Full edit with balance correction |
| Delete Transaction | Soft-delete (data preserved) |
| Search | Real-time keyword search |
| Filter | By type, category, and date range |
| Export | One-click CSV export |

**Expense categories:** Food · Transport · Shopping · Bills · Entertainment · Salary · Investment · Other

### 📊 Dashboard
- **4 KPI cards:** Balance · Total Income · Total Expenses · Savings Rate
- **Budget progress bar** — colour-coded (green → yellow → red)
- Quick-action buttons for common tasks
- Recent transactions table (last 10)
- Live expense breakdown donut chart

### 📈 Reports & Analytics
- Bar chart: 6-month income vs expense comparison
- Donut chart: expense breakdown by category with percentages
- Monthly summary table for last 12 months (Net column colour-coded)
- Export reports to CSV

### ⚙️ Settings
- **Dark / Light mode** toggle
- **Currency selection:** USD · EUR · GBP · EGP · SAR
- **Monthly budget** target with progress tracking
- Keyboard shortcut reference

### 🔔 Toast Notifications
Animated in-app notifications for every action:

| Type | Colour | Examples |
|------|--------|---------|
| ✔ Success | Green | Transaction added, Settings saved |
| ℹ Info | Blue | Transaction deleted, Exported |
| ⚠ Warning | Yellow | No selection made |
| ✘ Error | Red | Login failed |

---

## 🖥️ Screenshots & UI

```
┌─────────────────────────────────────────────────────────────┐
│  💰 MoneyTracker          Dashboard    Thursday, May 14 2025 │
├────────────┬────────────────────────────────────────────────┤
│            │  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐         │
│ 💰 Money   │  │ BAL  │ │ INC  │ │ EXP  │ │ SAVE │         │
│   Tracker  │  │$5420 │ │$8000 │ │$2580 │ │ 68%  │         │
│ Finance    │  └──────┘ └──────┘ └──────┘ └──────┘         │
│ Manager    │  ████████████████░░░░  Budget: $2580/$5000     │
│────────────│  [+ Income] [- Expense] [💾 Backup]           │
│ 📊 Dash    │                                                │
│ 💳 Trans   │  Recent Transactions                           │
│ 📈 Reports │  ┌──────────────────────────┬──────────────┐  │
│ ⚙️ Settings│  │  Date   Category  Amount │ Donut Chart  │  │
│────────────│  │ Jan 15  Food    -$45.00  │    (●)       │  │
│ Ahmed      │  │ Jan 14  Salary +$4000    │              │  │
│ $5,420.00  │  └──────────────────────────┴──────────────┘  │
│  Sign Out  │                                                │
└────────────┴────────────────────────────────────────────────┘
```

---

## 🛠️ Tech Stack

| Component | Technology |
|-----------|-----------|
| **Language** | C# 12 |
| **Framework** | .NET 8.0 (Windows) |
| **UI Framework** | Windows Forms |
| **Charts** | GDI+ (drawn from scratch — no libraries) |
| **Storage** | Plain CSV files |
| **IDE** | Visual Studio 2022 |
| **NuGet Packages** | **None** |
| **Database** | **None** |
| **Internet** | **Not required** |

---

## 📁 Project Structure

```
MoneyTracker/
│
├── MoneyTracker.sln                   ← Open this in Visual Studio
│
└── MoneyTracker/
    │
    ├── MoneyTracker.csproj            ← SDK-style project file (.NET 8)
    ├── Program.cs                     ← Entry point
    │
    ├── Models/                        ← Data layer
    │   ├── Enums.cs                   ← TxType, TxCategory, AppTheme, Currency
    │   ├── User.cs                    ← User entity (id, username, balance, budget...)
    │   ├── Transaction.cs             ← Transaction entity (type, category, amount...)
    │   └── Session.cs                 ← Singleton: current logged-in user
    │
    ├── Services/                      ← Business logic layer
    │   ├── StorageService.cs          ← CSV read/write, backup, export
    │   ├── AuthService.cs             ← Register, login, validation
    │   ├── TransactionService.cs      ← CRUD + balance updates
    │   └── AnalyticsService.cs        ← Totals, summaries, trends, charts data
    │
    ├── UI/
    │   ├── Theme.cs                   ← Centralised design system (colours + fonts)
    │   └── Controls/
    │       ├── FlatButton.cs          ← Owner-drawn rounded button (5 styles)
    │       ├── StatCard.cs            ← KPI card with accent bar
    │       ├── Charts.cs              ← BarChart, DonutChart, LineChart (GDI+)
    │       └── Widgets.cs             ← BudgetBar, Toast, DBPanel
    │
    └── Forms/
        ├── LoginForm.cs               ← Login + Register screens
        ├── AddTransactionForm.cs      ← Add / edit transaction
        └── DashboardForm.cs           ← Main app: dashboard, transactions, reports, settings
```

---

## 🏗️ Architecture

The app uses a clean **3-layer architecture**:

```
┌─────────────────────────────────────────┐
│              Forms (UI)                 │  Presentation — display only
├─────────────────────────────────────────┤
│          UI Controls / Theme            │  Custom painted controls
├─────────────────────────────────────────┤
│             Services                    │  Business logic & calculations
├─────────────────────────────────────────┤
│              Models                     │  Data structures (POCOs)
├─────────────────────────────────────────┤
│         CSV Files (data\)               │  Persistent storage
└─────────────────────────────────────────┘
```

### Key Design Decisions

| Decision | Reason |
|----------|--------|
| **GDI+ charts** | Zero dependencies — no NuGet, no runtime issues |
| **CSV storage** | Simple, portable, human-readable — no DB setup needed |
| **SetStyle(UserPaint)** on FlatButton | Prevents WinForms from rendering button text twice (root fix for doubled-text bug) |
| **Parent-chain erase** in FlatButton | FlowLayoutPanel has `BackColor=Transparent` — walking the chain finds the real opaque background |
| **Soft delete** on transactions | Prevents data loss; deleted flag in CSV |
| **Dock-based layout** | Replaces 9999-width hacks; panels resize correctly |

---

## 🚀 Getting Started

### Prerequisites

| Requirement | Version |
|-------------|---------|
| Windows | 10 / 11 |
| Visual Studio | 2022 (any edition) |
| .NET SDK | 8.0+ |

> **Note:** .NET 8 SDK is bundled with Visual Studio 2022 v17.8+. No separate installation needed.

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/your-username/MoneyTracker.git

# 2. Open in Visual Studio
#    Double-click MoneyTracker.sln
#    OR: File → Open → Project/Solution

# 3. Build and Run
#    Press F5   (Debug)
#    Press Ctrl+F5  (Release, no debugger)
```

**That's it.** No NuGet restore, no DB migrations, no config files needed.

---

## 📖 How to Use

### First Launch
1. Run the app → Login screen appears
2. Click **"Create Free Account"**
3. Enter username (3–31 chars), email, and password (6+ chars)
4. Log in with your new account

### Adding Transactions
1. Click **➕ Add Income** or **➖ Add Expense** on the dashboard
2. Select category, enter amount and description
3. Optionally add a note and pick a date
4. Click **ADD TRANSACTION** → balance updates instantly

### Searching & Filtering Transactions
- Go to the **Transactions** tab
- Use the search box for keyword search
- Use dropdowns to filter by type or category
- Use date pickers to narrow by date range
- Click **✖ Clear** to reset all filters

### Viewing Reports
- Go to the **Reports** tab
- Bar chart shows last 6 months of income vs expenses
- Donut chart shows expense breakdown by category
- Monthly table shows last 12 months with net profit/loss

### Settings
- Go to **Settings** tab
- Change theme (Dark/Light)
- Change currency symbol
- Set monthly budget target
- Click **SAVE SETTINGS**

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl + 1` | Go to Dashboard |
| `Ctrl + 2` | Go to Transactions |
| `Ctrl + 3` | Go to Reports |
| `Ctrl + I` | Add Income |
| `Ctrl + E` | Add Expense |
| `Del` | Delete selected transaction |

---

## 💾 Data Storage

All data is stored locally in a `data\` folder next to the `.exe`:

```
data\
├── users.csv              ← All user accounts
├── tx_alice.csv           ← Transactions for user "alice"
├── tx_bob.csv             ← Transactions for user "bob"
└── backups\
    ├── users_20250514_143022.csv
    └── tx_alice_20250514_143022.csv
```

### File Formats

**users.csv**
```
id|username|password|email|balance|theme|currency|budget
1|alice|mypassword|alice@mail.com|5420.50|Dark|USD|5000.00
```

**tx_alice.csv**
```
id|type|category|amount|description|date|note|deleted
1|2|1|45.50|Lunch|2025-01-15|good food|0
2|1|6|5000.00|Monthly salary|2025-01-01||0
3|2|4|120.00|Electricity|2025-01-10||1
```

> Columns: `type` (1=Income, 2=Expense) · `category` (1=Food…8=Other) · `deleted` (0=active, 1=soft-deleted)

### Backup
- Automatic backup runs on every sign-out
- Manual backup: click **💾 Backup** button on the dashboard
- Backups are timestamped and never overwrite each other

### Export
- **Transactions tab** → **📥 Export CSV** → saves all visible transactions
- **Reports tab** → **📥 Export Report CSV** → saves 12-month monthly summary

---

## ⚙️ Configuration

No config files are needed. All preferences are stored per-user in `users.csv`:

| Setting | Options | Default |
|---------|---------|---------|
| Theme | Dark, Light | Dark |
| Currency | USD, EUR, GBP, EGP, SAR | USD |
| Monthly Budget | Any positive number | 5000.00 |

Changes take effect immediately (theme applies on next login).

---

## 🎨 Design System

All colours and fonts are centralised in `UI/Theme.cs`:

### Colours
| Token | Dark Mode | Light Mode | Usage |
|-------|-----------|------------|-------|
| `BgBase` | `#080C18` | `#F4F6FC` | App background |
| `BgSurface` | `#0E1426` | `#FFFFFF` | Panel/form background |
| `BgCard` | `#121A30` | `#EEF2FF` | Card background |
| `Accent` | `#00D278` | same | Primary green |
| `Income` | `#00C36C` | same | Positive amounts |
| `Expense` | `#DC4444` | same | Negative amounts |
| `Warning` | `#FFAF2A` | same | Budget alerts |

### Button Styles
| Style | Colour | Use Case |
|-------|--------|---------|
| `Primary` | Green | Main action (Add, Save) |
| `Secondary` | Card grey | Secondary action (Edit) |
| `Danger` | Red | Destructive (Delete) |
| `Ghost` | Transparent + border | Low emphasis (Clear, Backup) |
| `Outline` | Transparent + green border | Register, Export |

---

## 🧪 Project Metrics

| Metric | Value |
|--------|-------|
| C# files | 16 |
| Lines of code | ~2,200 |
| NuGet packages | **0** |
| External dependencies | **0** |
| Database | **None** |
| Internet required | **No** |
| Min. Visual Studio | 2022 |
| Target .NET | 8.0 Windows |

---

## 🤝 Contributing

Contributions are welcome! To contribute:

1. **Fork** the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make your changes — keep the existing architecture
4. Test that the project builds: `dotnet build`
5. **Submit a pull request**

### Guidelines
- Keep the layered architecture (Models → Services → Forms)
- All colours/fonts must go through `UI/Theme.cs`
- New controls should inherit from the existing GDI+ pattern in `UI/Controls/`
- No new NuGet packages without discussion
- Preserve CSV storage compatibility

---

## 📄 License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 Zeyad Elhabak

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

---

## 👤 Author

Built as a portfolio-level desktop finance application demonstrating:
- Clean layered architecture in C# / .NET 8
- Owner-drawn custom controls with GDI+
- Professional dark UI design without external UI libraries
- Proper WinForms layout patterns (Dock-based, TableLayoutPanel)

---

<div align="center">

**💰 MoneyTracker** · Built with C# & .NET 8 · 2025

*Track your money. Understand your habits. Take control.*

</div>
