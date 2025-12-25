# Development Guide

> **One-command setup, build, test, and run for the Sri Lanka Payroll Platform**

## 🚀 Quick Start

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+ & npm** - [Download](https://nodejs.org/)
- **SQL Server 2019+** or **SQL Server Express** - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Git** - [Download](https://git-scm.com/)

### First-Time Setup (Bootstrap)

```bash
# Windows (PowerShell)
.\tools\dev.ps1 bootstrap

# Linux/macOS (Bash)
./tools/dev.sh bootstrap
```

This will:
1. ✅ Restore .NET dependencies
2. ✅ Install npm packages
3. ✅ Create local database
4. ✅ Run migrations
5. ✅ Seed initial data
6. ✅ Verify installation

## 📋 Daily Development Commands

### Build Everything
```bash
# Windows
.\tools\dev.ps1 build

# Linux/macOS
./tools/dev.sh build
```

### Run Tests
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

### Lint & Format
```bash
# Check and fix code style
.\tools\dev.ps1 lint

# Check only (no fixes)
.\tools\dev.ps1 lint --check
```

### Run Locally
```bash
# Start both backend and frontend
.\tools\dev.ps1 run

# Backend only
.\tools\dev.ps1 run backend

# Frontend only
.\tools\dev.ps1 run frontend
```

### Clean Build Artifacts
```bash
.\tools\dev.ps1 clean
```

## 🏗️ Project Structure

```
PayRoll/
├── src/                          # Source code (future monorepo)
│   ├── apps/
│   │   ├── web/                  # Angular SPA → frontend/payroll-web
│   │   └── api/                  # ASP.NET API → backend/Payroll.Api
│   └── libs/
│       ├── domain/               # → backend/Payroll.Domain
│       ├── application/          # → backend/Payroll.Application
│       ├── infrastructure/       # → backend/Payroll.Infrastructure
│       └── shared/               # → backend/Payroll.Shared
├── tests/                        # Test projects
│   ├── unit/                     # → backend/Payroll.Application.Tests
│   └── e2e/                      # → qa-automation
├── backend/                      # Current .NET projects (legacy location)
├── frontend/                     # Current Angular app (legacy location)
├── qa-automation/                # Playwright E2E tests (legacy location)
├── docs/                         # Documentation
├── tools/                        # Development scripts
└── infra/                        # Infrastructure as Code
```

> **Note**: We're gradually migrating to `/src` structure. Current code remains in `/backend`, `/frontend`, `/qa-automation` until migration is complete.

## 🔧 Development Workflows

### Backend Development

#### 1. Create a New Feature
```bash
# Example: Adding a new "Bonuses" feature
cd backend/Payroll.Application

# Create folder structure
mkdir -p Bonuses/Commands/CreateBonus
mkdir -p Bonuses/Queries/GetBonus
mkdir -p Bonuses/DTOs

# Add entity to Payroll.Domain
# Add repository interface to Payroll.Application/Interfaces
# Implement repository in Payroll.Infrastructure
# Add controller to Payroll.Api
```

#### 2. Add Database Migration
```bash
cd backend/Payroll.Api

# Create migration
dotnet ef migrations add AddBonusesTable --project ../Payroll.Infrastructure --startup-project .

# Apply migration
dotnet ef database update --project ../Payroll.Infrastructure --startup-project .

# Rollback (if needed)
dotnet ef database update PreviousMigrationName --project ../Payroll.Infrastructure --startup-project .
```

#### 3. Run Backend Locally
```bash
cd backend/Payroll.Api
dotnet run --urls http://localhost:52938

# Or with hot reload
dotnet watch run --urls http://localhost:52938
```

#### 4. Test Backend
```bash
# Unit tests
cd backend/Payroll.Application.Tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=html
```

### Frontend Development

#### 1. Generate Components
```bash
cd frontend/payroll-web

# Generate feature module
ng generate module features/bonuses --routing

# Generate component
ng generate component features/bonuses/components/bonus-list

# Generate service
ng generate service features/bonuses/services/bonus
```

#### 2. Run Frontend Locally
```bash
cd frontend/payroll-web

# Development server
npm start
# or
ng serve

# With specific port
ng serve --port 4200

# With proxy to backend
ng serve --proxy-config proxy.conf.json
```

#### 3. Test Frontend
```bash
cd frontend/payroll-web

# Unit tests
npm test
# or
ng test

# E2E tests (from root)
cd qa-automation
npx playwright test
```

#### 4. Build Frontend
```bash
cd frontend/payroll-web

# Development build
ng build

# Production build
ng build --configuration production
```

### Database Management

#### Reset Database
```bash
cd backend/Payroll.Api

# Drop database
dotnet ef database drop --project ../Payroll.Infrastructure --startup-project .

# Recreate and migrate
dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
```

#### Seed Data
```bash
cd backend/Payroll.Seeder
dotnet run
```

#### View Migrations
```bash
cd backend/Payroll.Api
dotnet ef migrations list --project ../Payroll.Infrastructure --startup-project .
```

## 🧪 Testing Strategy

### Test Pyramid

```
        /\
       /E2E\         ← Few, critical user journeys (Playwright)
      /------\
     /  API  \       ← Integration tests (WebApplicationFactory)
    /----------\
   /    Unit    \    ← Many, fast, isolated (xUnit, Jasmine)
  /--------------\
```

### Running Tests

#### Backend Unit Tests
```bash
cd backend/Payroll.Application.Tests
dotnet test --logger "console;verbosity=detailed"
```

#### Frontend Unit Tests
```bash
cd frontend/payroll-web
npm run test:ci  # Headless, single run
npm test         # Watch mode
```

#### E2E Tests
```bash
cd qa-automation

# All tests
npx playwright test

# Specific test file
npx playwright test tests/ui/employees/employee-crud.spec.ts

# Headed mode (see browser)
npx playwright test --headed

# Debug mode
npx playwright test --debug

# Generate report
npx playwright show-report
```

## 🎨 Code Style & Linting

### Backend (.NET)

#### Format Code
```bash
# Format all projects
dotnet format PayrollSolution.sln

# Check only (CI mode)
dotnet format PayrollSolution.sln --verify-no-changes
```

#### EditorConfig
The repository uses `.editorconfig` for consistent formatting:
- Indentation: 4 spaces
- Line endings: CRLF (Windows) / LF (Linux)
- Charset: UTF-8

### Frontend (Angular)

#### Lint
```bash
cd frontend/payroll-web

# Lint TypeScript
npm run lint

# Fix auto-fixable issues
npm run lint:fix
```

#### Format with Prettier
```bash
# Format all files
npm run format

# Check only
npm run format:check
```

## 🔐 Environment Configuration

### Backend Configuration

**appsettings.json** (checked into Git, no secrets):
```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=Payroll;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "PayrollApi",
    "Audience": "PayrollWeb"
  }
}
```

**appsettings.Development.json** (local overrides):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

**User Secrets** (for sensitive data):
```bash
cd backend/Payroll.Api

# Initialize user secrets
dotnet user-secrets init

# Set a secret
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key-here"

# List secrets
dotnet user-secrets list
```

### Frontend Configuration

**environment.ts** (development):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:52938/api',
  features: {
    newPayrollEngine: false
  }
};
```

**environment.prod.ts** (production):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.payroll.lk/api',
  features: {
    newPayrollEngine: true
  }
};
```

## 🐛 Debugging

### Backend (Visual Studio / VS Code)

#### Visual Studio
1. Open `PayrollSolution.sln`
2. Set `Payroll.Api` as startup project
3. Press F5 to debug

#### VS Code
1. Open workspace
2. Use launch configuration in `.vscode/launch.json`
3. Press F5

#### Command Line
```bash
cd backend/Payroll.Api
dotnet run --launch-profile Development
```

### Frontend (Chrome DevTools)

#### Angular DevTools
1. Install [Angular DevTools](https://angular.io/guide/devtools) extension
2. Open browser DevTools (F12)
3. Navigate to "Angular" tab

#### Source Maps
```bash
# Enable source maps for debugging
ng serve --source-map
```

### Database (SQL Server)

#### Tools
- **SQL Server Management Studio (SSMS)**
- **Azure Data Studio** (cross-platform)
- **VS Code SQL Server extension**

#### Connection String
```
Server=localhost;Database=Payroll;Trusted_Connection=True;TrustServerCertificate=True
```

## 📦 Dependency Management

### Backend (NuGet)

```bash
# Add package
dotnet add package PackageName

# Update package
dotnet add package PackageName --version x.y.z

# Remove package
dotnet remove package PackageName

# List packages
dotnet list package

# Check for outdated packages
dotnet list package --outdated
```

### Frontend (npm)

```bash
cd frontend/payroll-web

# Install package
npm install package-name

# Install dev dependency
npm install --save-dev package-name

# Update package
npm update package-name

# Remove package
npm uninstall package-name

# Audit for vulnerabilities
npm audit

# Fix vulnerabilities
npm audit fix
```

## 🚢 Build & Deployment

### Build for Production

#### Backend
```bash
cd backend/Payroll.Api

# Publish
dotnet publish -c Release -o ./publish

# The output will be in ./publish/
```

#### Frontend
```bash
cd frontend/payroll-web

# Build
ng build --configuration production

# Output will be in dist/payroll-web/
```

### Docker (Future)

```bash
# Build backend image
docker build -f infra/docker/Dockerfile.api -t payroll-api:latest .

# Build frontend image
docker build -f infra/docker/Dockerfile.web -t payroll-web:latest .

# Run with docker-compose
docker-compose -f infra/docker/docker-compose.yml up
```

## 🔍 Troubleshooting

### Common Issues

#### 1. "Cannot connect to SQL Server"
```bash
# Check SQL Server is running
# Windows: Services → SQL Server (MSSQLSERVER)
# Or use SQL Server Configuration Manager

# Test connection
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

#### 2. "Migration already applied"
```bash
# Check migration history
dotnet ef migrations list --project ../Payroll.Infrastructure --startup-project .

# If needed, remove last migration
dotnet ef migrations remove --project ../Payroll.Infrastructure --startup-project .
```

#### 3. "Port already in use"
```bash
# Windows: Find process using port
netstat -ano | findstr :52938
taskkill /PID <process-id> /F

# Linux/macOS
lsof -ti:52938 | xargs kill -9
```

#### 4. "npm install fails"
```bash
# Clear cache
npm cache clean --force

# Delete node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Reinstall
npm install
```

#### 5. "Angular CLI not found"
```bash
# Install globally
npm install -g @angular/cli

# Or use npx
npx ng serve
```

## 📚 Additional Resources

- [AGENTS.md](./AGENTS.md) - AI agent operating manual
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Architecture documentation
- [docs/adr/](./docs/adr/) - Architecture Decision Records
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Angular Documentation](https://angular.io/docs)
- [Playwright Documentation](https://playwright.dev/)

## 🆘 Getting Help

1. **Check Documentation**: Start with this guide and AGENTS.md
2. **Search Issues**: Look for similar problems in GitHub issues
3. **Ask Team**: Use team chat or create a GitHub discussion
4. **Create Issue**: If it's a bug, create a detailed issue with:
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (OS, .NET version, Node version)
   - Error messages and stack traces

---

**Last Updated**: 2025-12-25  
**Maintained By**: Development Team
