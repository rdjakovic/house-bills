# HouseBills

A Windows desktop app for keeping track of household bills: what's due, what's paid, what repeats every month, and where the money goes over the year.

Built with .NET 10, WPF (Fluent theme) and SQL Server LocalDB. Everything stays on your own PC.

---

## Features

- **Bills** — add, edit and delete bills; record payments (date and amount) or mark a bill unpaid again. Filter by status, due-date range, category or payee. Each bill is shown as **Overdue**, **Due soon** (within 7 days), **Upcoming** or **Paid**, with totals and the outstanding amount.
- **Recurring bills** — templates that repeat weekly, monthly, quarterly or yearly, with an optional end date. Bills are generated automatically for the next 31 days when the app starts (and on demand). Pausing and resuming a template doesn't back-fill the paused period; deleting a generated bill doesn't bring it back.
- **Payees and categories** — names are unique; anything still in use can't be deleted. Seven starter categories are created with the database.
- **Reports** — monthly totals for a year compared with the previous year, and totals per category.
- **English or Srpski** — choose the language under **Settings** (bottom of the menu); it switches immediately and is remembered. Serbian also uses Serbian formats (`1.234,56 RSD`, `24.9.2026.`); English follows your Windows regional settings.
- **Safe for more than one person editing** — if someone else changed a record in the meantime, you get a "reload and try again" message instead of silently overwriting their change.

---

## Installing (for users)

1. Run `HouseBills-Setup-<version>.exe` and follow the wizard. Administrator rights are needed.
2. If Windows shows **"Windows protected your PC"**, click **More info → Run anyway**. The installer isn't code-signed yet. (Copying the installer via USB stick or a network share instead of downloading it avoids this prompt.)
3. Start HouseBills from the Start menu. The first start takes up to ~20 seconds while it creates your database; a "Starting HouseBills…" window is shown meanwhile.

The installer also installs what the app needs, only if it's missing:

- **Microsoft SQL Server Express LocalDB** — the database engine HouseBills stores data in.
- **Microsoft Visual C++ Redistributable** — required by LocalDB.

On a PC without LocalDB, installation can take several minutes. No separate .NET installation is needed.

**Your data**

- Each Windows user on the PC has their own HouseBills data (LocalDB is per user). It lives in your user profile (`C:\Users\<you>\HouseBills.mdf`).
- Uninstalling HouseBills keeps your data and LocalDB, so reinstalling brings your bills back.
- Log files are in `%LOCALAPPDATA%\HouseBills\Logs` (one file per day, kept for 30 days) — useful when something goes wrong.
- Your language choice is stored in `%LOCALAPPDATA%\HouseBills\preferences.json`. Without it, HouseBills starts in Serbian if Windows' display language is Serbian, otherwise in English.

---

## For developers

### Prerequisites

| Tool | Needed for | Install |
|---|---|---|
| Windows 10 (1809+) or 11, x64 | everything (WPF) | — |
| .NET SDK **10.0.401** or later 10.0.x (pinned in `global.json`) | build, run, test | `winget install Microsoft.DotNet.SDK.10` |
| SQL Server **LocalDB** | running the app, LocalDB tests | SQL Server Express installer (LocalDB feature) or the HouseBills installer |
| **Docker Desktop** (running) | integration tests against SQL Server in a container | `winget install Docker.DockerDesktop` |
| **Inno Setup 6** | building the installer only | `winget install JRSoftware.InnoSetup` |

### Get started

```bash
git clone https://github.com/rdjakovic/house-bills.git   # or: git clone git@github.com:rdjakovic/house-bills.git
cd house-bills
dotnet tool restore
dotnet build
dotnet run --project src/HouseBills.Wpf
```

On first start the app creates the `HouseBills` database in `(localdb)\MSSQLLocalDB` and applies all migrations. There is no manual database step when you use LocalDB.

To use a different database without editing files, override the connection string with an environment variable (it takes precedence over `appsettings.json`):

```powershell
$env:ConnectionStrings__HouseBills = "Server=(localdb)\MSSQLLocalDB;Database=HouseBillsDev;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
dotnet run --project src/HouseBills.Wpf
```

### Tests

```bash
dotnet test
```

- Tests use **xUnit v3** on **Microsoft.Testing.Platform** (enabled in `global.json`), with Shouldly and NSubstitute.
- `HouseBills.Infrastructure.Tests` runs SQL Server in Docker via Testcontainers — **Docker Desktop must be running**, otherwise those tests fail with "Docker is either not running". The first run downloads the SQL Server image (~several minutes).
- LocalDB tests (database creation and re-attach) skip themselves when LocalDB isn't installed.
- Tests run with the `en-US` culture (`tests/xunit.runner.json`), so assertions on English texts don't depend on the Windows display language.
- Run one project: `dotnet test --project tests/HouseBills.Domain.Tests`

### Before you commit

```bash
dotnet build -c Release          # warnings are errors
dotnet test
dotnet format --verify-no-changes
```

### Database migrations

Migrations in `src/HouseBills.Infrastructure/Persistence/Migrations` are the source of truth for the schema.

```bash
dotnet ef migrations add <Name> -p src/HouseBills.Infrastructure -s src/HouseBills.Wpf -o Persistence/Migrations
dotnet ef database update       -p src/HouseBills.Infrastructure -s src/HouseBills.Wpf
dotnet ef migrations script --idempotent -p src/HouseBills.Infrastructure -s src/HouseBills.Wpf -o artifacts/migrate.sql
```

- With **LocalDB**, the app applies pending migrations itself at startup (`LocalDbInitializer`), so `database update` is optional.
- With **any other SQL Server** the app never changes the schema; deploy with the idempotent script. See `AGENTS.md` §6.
- Never edit a migration that has already been applied — add a new one.

### Build the installer

```bash
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1 -Version 1.0.0
```

This publishes the app self-contained for win-x64, downloads the LocalDB and VC++ prerequisites once into `artifacts\prereqs` (signatures verified; the LocalDB download asks for administrator approval), and compiles `installer\HouseBills.iss`. Output: `artifacts\installer\HouseBills-Setup-1.0.0.exe` (~135 MB).

Test installer changes on a **clean** Windows (e.g. Windows Sandbox), not only on your dev PC — the dev PC already has runtimes the target PC may not.

### Configuration (`src/HouseBills.Wpf/appsettings.json`)

| Setting | Default | Meaning |
|---|---|---|
| `ConnectionStrings:HouseBills` | LocalDB, database `HouseBills` | Database connection. |
| `Billing:GenerationLookaheadDays` | `31` | How far ahead recurring bills are generated (0–366). |
| `FileLogging:Directory` | `%LOCALAPPDATA%\HouseBills\Logs` | Log folder; environment variables are expanded. |
| `FileLogging:RetainedDays` | `30` | Log files older than this are deleted (1–3650). |
| `UserPreferences:FilePath` | `%LOCALAPPDATA%\HouseBills\preferences.json` | Where the language choice is saved (not in `appsettings.json` by default; handy to override in tests). |
| `Logging:LogLevel` | `Information` | Standard .NET log levels; apply to the log file too. |

Every setting can be overridden with an environment variable, using `__` for `:` (e.g. `Billing__GenerationLookaheadDays=60`).

### Project structure

```
src/
  HouseBills.Domain/          Entities and business rules (bills, recurring schedules, money rules). No dependencies.
  HouseBills.Application/     Services, validation, Result types, repository and report interfaces.
  HouseBills.Infrastructure/  EF Core DbContext, configurations, migrations, repositories, Dapper reports, LocalDB setup.
  HouseBills.Presentation.Resources/  UI texts (Strings.resx + Strings.sr-Latn.resx); no UI-framework dependency.
  HouseBills.Wpf/             WPF views, view models (CommunityToolkit.Mvvm), navigation, dialogs, app host, logging, localization.
tests/                        One test project per layer; Infrastructure tests use real SQL Server.
installer/                    Inno Setup script and build script.
tools/generate-icon.ps1       Regenerates src/HouseBills.Wpf/Assets/HouseBills.ico.
```

Dependencies point inwards (Wpf → Application → Domain; Infrastructure implements Application interfaces and is referenced by the Wpf project only in its composition root, `Hosting/HostBuilderExtensions.cs`). ViewModels depend on Application interfaces only, so the database layer could later be swapped for an API client.

### Texts and translations

All user-facing text is in resource files, English plus a Serbian (Latin) translation next to it:

| Texts | Files | Used as |
|---|---|---|
| UI (buttons, labels, dialogs, enum names) | `src/HouseBills.Presentation.Resources/Strings.resx` + `Strings.sr-Latn.resx` | XAML: `{loc:Tr Key}` (updates live when the language changes); C#: `Strings.Key` |
| Validation and business errors | `src/HouseBills.Application/Resources/Messages.resx` + `Messages.sr-Latn.resx` | C#: `Messages.Key` |

- Add every new key to **both** files. Tests fail if a translation is missing or empty, if `{0}` placeholders differ between the languages, or if XAML uses a key that doesn't exist.
- The `Strings`/`Messages` classes are generated at build time (no Visual Studio designer files). The UI texts live in their own project because WPF's XAML pre-compilation can't see resource classes generated inside the WPF project.
- Show amounts with `Converter={StaticResource Money}` (not `StringFormat=C`), so they use the chosen language's formats.
- Don't rely on `CultureInfo.CurrentUICulture`/`CurrentCulture` for texts or formats in UI code; use `Strings`, `LocalizedStrings.Culture` and `LocalizedStrings.FormattingCulture` (the ambient cultures are async-local and don't reliably follow a language switch).

### Tech stack

.NET 10 · WPF with the Fluent theme · CommunityToolkit.Mvvm · Generic Host (DI, config, logging) · EF Core 10 (SQL Server) · Dapper · Serilog (daily log files) · xUnit v3, Shouldly, NSubstitute, Testcontainers · Inno Setup.

Package versions are pinned centrally in `Directory.Packages.props`. Coding conventions and rules for contributors (and AI agents) are in [`AGENTS.md`](AGENTS.md).

---

## Troubleshooting

| Problem | What to do |
|---|---|
| "Could not load bills…" on start | Check the latest file in `%LOCALAPPDATA%\HouseBills\Logs`. Usually LocalDB isn't installed or the connection string is wrong. |
| `Cannot open database "HouseBills"` although `HouseBills.mdf` exists in your profile | LocalDB lost track of the database. Just start the app — it re-attaches the existing files automatically, keeping your data. |
| `dotnet ef database update` fails with "Cannot create file … already exists" | Same cause as above: start the app once (it re-attaches the database), then run the command again. |
| Infrastructure tests fail with "Docker is either not running" | Start Docker Desktop and wait until `docker info` responds. |
| Installer fails installing LocalDB | The error message names a log file in `%TEMP%` (`HouseBills-LocalDB-install.log` / `HouseBills-VCRedist-install.log`); search it for `Return value 3`. |
