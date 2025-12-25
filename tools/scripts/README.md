# Development Scripts

Cross-platform development automation scripts for the Sri Lanka Payroll Platform.

## Usage

### Windows (PowerShell)
```powershell
.\tools\dev.ps1 <command> [options]
```

### Linux/macOS (Bash)
```bash
./tools/dev.sh <command> [options]
```

## Available Commands

### `bootstrap`
First-time setup. Installs all dependencies, creates database, runs migrations, and seeds data.

```bash
.\tools\dev.ps1 bootstrap
```

### `build`
Build all projects (backend + frontend).

```bash
.\tools\dev.ps1 build
```

### `test`
Run all tests or specific test suites.

```bash
# All tests
.\tools\dev.ps1 test

# Backend only
.\tools\dev.ps1 test backend

# Frontend only
.\tools\dev.ps1 test frontend

# E2E only
.\tools\dev.ps1 test e2e
```

### `lint`
Check and fix code style issues.

```bash
# Check and fix
.\tools\dev.ps1 lint

# Check only (no fixes)
.\tools\dev.ps1 lint --check
```

### `run`
Start development servers.

```bash
# Both backend and frontend
.\tools\dev.ps1 run

# Backend only
.\tools\dev.ps1 run backend

# Frontend only
.\tools\dev.ps1 run frontend
```

### `clean`
Remove build artifacts and temporary files.

```bash
.\tools\dev.ps1 clean
```

### `db-reset`
Drop and recreate the database with fresh migrations and seed data.

```bash
.\tools\dev.ps1 db-reset
```

### `db-migrate`
Create a new database migration.

```bash
.\tools\dev.ps1 db-migrate "MigrationName"
```

## Examples

```bash
# First time setup
.\tools\dev.ps1 bootstrap

# Daily development
.\tools\dev.ps1 build
.\tools\dev.ps1 test
.\tools\dev.ps1 run

# Before committing
.\tools\dev.ps1 lint
.\tools\dev.ps1 test

# Database work
.\tools\dev.ps1 db-migrate "AddBonusTable"
.\tools\dev.ps1 db-reset
```

## Requirements

- .NET 8 SDK
- Node.js 20+
- SQL Server 2019+
- PowerShell 7+ (Windows) or Bash (Linux/macOS)

## Troubleshooting

If scripts fail to execute:

### Windows
```powershell
# Enable script execution
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Linux/macOS
```bash
# Make script executable
chmod +x tools/dev.sh
```
