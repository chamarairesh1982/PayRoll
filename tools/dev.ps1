#!/usr/bin/env pwsh
# Sri Lanka Payroll Platform - Development Script (PowerShell)
# Cross-platform development automation

param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    
    [Parameter(Position=1)]
    [string]$Target = "",
    
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$RemainingArgs
)

$ErrorActionPreference = "Stop"
$RootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

# Header
function Show-Header {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║   Sri Lanka Payroll Platform - Development Tool       ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

# Check prerequisites
function Test-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    # Check .NET SDK
    if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error "❌ .NET SDK not found. Please install .NET 8 SDK."
        exit 1
    }
    $dotnetVersion = dotnet --version
    Write-Success "✓ .NET SDK: $dotnetVersion"
    
    # Check Node.js
    if (!(Get-Command node -ErrorAction SilentlyContinue)) {
        Write-Error "❌ Node.js not found. Please install Node.js 20+."
        exit 1
    }
    $nodeVersion = node --version
    Write-Success "✓ Node.js: $nodeVersion"
    
    # Check npm
    if (!(Get-Command npm -ErrorAction SilentlyContinue)) {
        Write-Error "❌ npm not found. Please install npm."
        exit 1
    }
    $npmVersion = npm --version
    Write-Success "✓ npm: $npmVersion"
    
    Write-Host ""
}

# Bootstrap - First time setup
function Invoke-Bootstrap {
    Write-Info "🚀 Bootstrapping development environment..."
    Write-Host ""
    
    Test-Prerequisites
    
    # Restore backend dependencies
    Write-Info "📦 Restoring .NET dependencies..."
    Push-Location "$RootDir"
    dotnet restore PayrollSolution.sln
    Pop-Location
    Write-Success "✓ .NET dependencies restored"
    Write-Host ""
    
    # Restore frontend dependencies
    Write-Info "📦 Installing npm packages..."
    Push-Location "$RootDir/frontend/payroll-web"
    npm install
    Pop-Location
    Write-Success "✓ npm packages installed"
    Write-Host ""
    
    # Restore E2E test dependencies
    Write-Info "📦 Installing Playwright..."
    Push-Location "$RootDir/qa-automation"
    npm install
    npx playwright install
    Pop-Location
    Write-Success "✓ Playwright installed"
    Write-Host ""
    
    # Database setup
    Write-Info "🗄️  Setting up database..."
    Push-Location "$RootDir/backend/Payroll.Api"
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    Pop-Location
    Write-Success "✓ Database created and migrated"
    Write-Host ""
    
    # Seed data
    Write-Info "🌱 Seeding initial data..."
    Push-Location "$RootDir/backend/Payroll.Seeder"
    dotnet run
    Pop-Location
    Write-Success "✓ Data seeded"
    Write-Host ""
    
    Write-Success "✅ Bootstrap complete! Run '.\tools\dev.ps1 run' to start development."
}

# Build all projects
function Invoke-Build {
    Write-Info "🔨 Building all projects..."
    Write-Host ""
    
    # Build backend
    Write-Info "Building backend..."
    Push-Location "$RootDir"
    dotnet build PayrollSolution.sln --configuration Debug
    Pop-Location
    Write-Success "✓ Backend built"
    Write-Host ""
    
    # Build frontend
    Write-Info "Building frontend..."
    Push-Location "$RootDir/frontend/payroll-web"
    npm run build
    Pop-Location
    Write-Success "✓ Frontend built"
    Write-Host ""
    
    Write-Success "✅ Build complete!"
}

# Run tests
function Invoke-Test {
    param([string]$TestTarget = "all")
    
    Write-Info "🧪 Running tests..."
    Write-Host ""
    
    if ($TestTarget -eq "all" -or $TestTarget -eq "backend") {
        Write-Info "Running backend tests..."
        Push-Location "$RootDir/backend/Payroll.Application.Tests"
        dotnet test --logger "console;verbosity=normal"
        Pop-Location
        Write-Success "✓ Backend tests passed"
        Write-Host ""
    }
    
    if ($TestTarget -eq "all" -or $TestTarget -eq "frontend") {
        Write-Info "Running frontend tests..."
        Push-Location "$RootDir/frontend/payroll-web"
        npm run test:ci
        Pop-Location
        Write-Success "✓ Frontend tests passed"
        Write-Host ""
    }
    
    if ($TestTarget -eq "all" -or $TestTarget -eq "e2e") {
        Write-Info "Running E2E tests..."
        Push-Location "$RootDir/qa-automation"
        npx playwright test
        Pop-Location
        Write-Success "✓ E2E tests passed"
        Write-Host ""
    }
    
    Write-Success "✅ All tests passed!"
}

# Lint and format code
function Invoke-Lint {
    param([switch]$CheckOnly)
    
    Write-Info "🎨 Linting code..."
    Write-Host ""
    
    # Lint backend
    Write-Info "Linting backend..."
    Push-Location "$RootDir"
    if ($CheckOnly) {
        dotnet format PayrollSolution.sln --verify-no-changes
    } else {
        dotnet format PayrollSolution.sln
    }
    Pop-Location
    Write-Success "✓ Backend linted"
    Write-Host ""
    
    # Lint frontend
    Write-Info "Linting frontend..."
    Push-Location "$RootDir/frontend/payroll-web"
    if ($CheckOnly) {
        npm run lint
    } else {
        npm run lint:fix
    }
    Pop-Location
    Write-Success "✓ Frontend linted"
    Write-Host ""
    
    Write-Success "✅ Linting complete!"
}

# Run development servers
function Invoke-Run {
    param([string]$RunTarget = "all")
    
    Write-Info "🚀 Starting development servers..."
    Write-Host ""
    
    if ($RunTarget -eq "backend") {
        Write-Info "Starting backend API on http://localhost:52938..."
        Push-Location "$RootDir/backend/Payroll.Api"
        dotnet run --urls http://localhost:52938
        Pop-Location
    }
    elseif ($RunTarget -eq "frontend") {
        Write-Info "Starting frontend on http://localhost:4200..."
        Push-Location "$RootDir/frontend/payroll-web"
        npm start
        Pop-Location
    }
    else {
        Write-Info "Starting both backend and frontend..."
        Write-Info "Backend: http://localhost:52938"
        Write-Info "Frontend: http://localhost:4200"
        Write-Host ""
        Write-Warning "Note: Run backend and frontend in separate terminals:"
        Write-Host "  Terminal 1: .\tools\dev.ps1 run backend"
        Write-Host "  Terminal 2: .\tools\dev.ps1 run frontend"
    }
}

# Clean build artifacts
function Invoke-Clean {
    Write-Info "🧹 Cleaning build artifacts..."
    Write-Host ""
    
    # Clean backend
    Write-Info "Cleaning backend..."
    Push-Location "$RootDir"
    dotnet clean PayrollSolution.sln
    Pop-Location
    Write-Success "✓ Backend cleaned"
    Write-Host ""
    
    # Clean frontend
    Write-Info "Cleaning frontend..."
    Push-Location "$RootDir/frontend/payroll-web"
    if (Test-Path "dist") { Remove-Item -Recurse -Force "dist" }
    if (Test-Path ".angular") { Remove-Item -Recurse -Force ".angular" }
    Pop-Location
    Write-Success "✓ Frontend cleaned"
    Write-Host ""
    
    Write-Success "✅ Clean complete!"
}

# Database reset
function Invoke-DbReset {
    Write-Warning "⚠️  This will DROP and recreate the database!"
    $confirm = Read-Host "Are you sure? (yes/no)"
    
    if ($confirm -ne "yes") {
        Write-Info "Cancelled."
        return
    }
    
    Write-Info "🗄️  Resetting database..."
    Push-Location "$RootDir/backend/Payroll.Api"
    
    # Drop database
    dotnet ef database drop --project ../Payroll.Infrastructure --startup-project . --force
    
    # Recreate and migrate
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    
    Pop-Location
    Write-Success "✓ Database reset"
    Write-Host ""
    
    # Seed data
    Write-Info "🌱 Seeding data..."
    Push-Location "$RootDir/backend/Payroll.Seeder"
    dotnet run
    Pop-Location
    Write-Success "✓ Data seeded"
    Write-Host ""
    
    Write-Success "✅ Database reset complete!"
}

# Create database migration
function Invoke-DbMigrate {
    param([string]$MigrationName)
    
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Error "❌ Migration name is required. Usage: .\tools\dev.ps1 db-migrate 'MigrationName'"
        exit 1
    }
    
    Write-Info "📝 Creating migration: $MigrationName..."
    Push-Location "$RootDir/backend/Payroll.Api"
    dotnet ef migrations add $MigrationName --project ../Payroll.Infrastructure --startup-project .
    Pop-Location
    Write-Success "✓ Migration created"
    Write-Host ""
    
    Write-Info "To apply migration, run: .\tools\dev.ps1 db-update"
}

# Apply database migrations
function Invoke-DbUpdate {
    Write-Info "🗄️  Applying database migrations..."
    Push-Location "$RootDir/backend/Payroll.Api"
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    Pop-Location
    Write-Success "✓ Migrations applied"
}

# Show help
function Show-Help {
    Write-Host "Usage: .\tools\dev.ps1 <command> [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    Write-Host "  bootstrap          First-time setup (install deps, create DB, seed data)"
    Write-Host "  build              Build all projects"
    Write-Host "  test [target]      Run tests (all|backend|frontend|e2e)"
    Write-Host "  lint [--check]     Lint and format code"
    Write-Host "  run [target]       Start dev servers (all|backend|frontend)"
    Write-Host "  clean              Remove build artifacts"
    Write-Host "  db-reset           Drop and recreate database"
    Write-Host "  db-migrate <name>  Create new migration"
    Write-Host "  db-update          Apply pending migrations"
    Write-Host "  help               Show this help"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\tools\dev.ps1 bootstrap"
    Write-Host "  .\tools\dev.ps1 build"
    Write-Host "  .\tools\dev.ps1 test backend"
    Write-Host "  .\tools\dev.ps1 lint"
    Write-Host "  .\tools\dev.ps1 run backend"
    Write-Host "  .\tools\dev.ps1 db-migrate 'AddBonusTable'"
    Write-Host ""
}

# Main execution
Show-Header

switch ($Command.ToLower()) {
    "bootstrap" { Invoke-Bootstrap }
    "build" { Invoke-Build }
    "test" { Invoke-Test -TestTarget $Target }
    "lint" { Invoke-Lint -CheckOnly:($RemainingArgs -contains "--check") }
    "run" { Invoke-Run -RunTarget $Target }
    "clean" { Invoke-Clean }
    "db-reset" { Invoke-DbReset }
    "db-migrate" { Invoke-DbMigrate -MigrationName $Target }
    "db-update" { Invoke-DbUpdate }
    "help" { Show-Help }
    default { 
        Write-Error "Unknown command: $Command"
        Show-Help
        exit 1
    }
}
