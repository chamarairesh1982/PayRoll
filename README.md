# Sri Lanka Payroll Platform

> A world-class SaaS payroll system built with Clean Architecture, Domain-Driven Design, and AI-agent friendly development practices.

[![CI/CD](https://github.com/chamarairesh1982/PayRoll/workflows/CI%2FCD%20Pipeline/badge.svg)](https://github.com/chamarairesh1982/PayRoll/actions)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-18+-DD0031)](https://angular.io/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [SQL Server 2019+](https://www.microsoft.com/sql-server/sql-server-downloads)

### One-Command Setup

```bash
# Windows (PowerShell)
.\tools\dev.ps1 bootstrap

# Linux/macOS (Bash)
./tools/dev.sh bootstrap
```

This will:
✅ Install all dependencies  
✅ Create and migrate database  
✅ Seed initial data  
✅ Verify installation  

### Run Locally

```bash
# Start both backend and frontend
.\tools\dev.ps1 run

# Or run separately:
# Terminal 1 - Backend API (http://localhost:52938)
.\tools\dev.ps1 run backend

# Terminal 2 - Frontend SPA (http://localhost:4200)
.\tools\dev.ps1 run frontend
```

## 📚 Documentation

- **[DEVELOPMENT.md](./DEVELOPMENT.md)** - Development setup, workflows, and commands
- **[AGENTS.md](./AGENTS.md)** - AI agent operating manual (for Copilot, Cursor, etc.)
- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - System architecture and design decisions
- **[docs/adr/](./docs/adr/)** - Architecture Decision Records

## 🏗️ Architecture

### Clean Architecture + DDD

```
┌─────────────────────────────────────┐
│      Presentation (Angular SPA)     │
├─────────────────────────────────────┤
│      API Layer (ASP.NET Core)       │
├─────────────────────────────────────┤
│    Application (Use Cases, DTOs)    │
├─────────────────────────────────────┤
│    Domain (Entities, Business Logic)│
├─────────────────────────────────────┤
│  Infrastructure (EF Core, Services) │
└─────────────────────────────────────┘
```

### Bounded Contexts
- **Employees** - Employee management
- **Payroll** - Pay runs, payslips, calculations
- **PayrollConfig** - Tax rules, allowances, deductions
- **Organizations** - Companies, branches, cost centers
- **Leave** - Leave management
- **Loans** - Employee loans
- **Overtime** - OT tracking
- **GeneralLedger** - GL integration
- **Auditing** - Audit trail

## 🛠️ Technology Stack

### Backend
- **.NET 8** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping

### Frontend
- **Angular 18+** - SPA framework
- **PrimeNG** - UI components
- **RxJS** - Reactive programming
- **TypeScript** - Type safety

### Testing
- **xUnit** - Backend unit tests
- **Jasmine/Karma** - Frontend unit tests
- **Playwright** - E2E tests

## 📋 Development Commands

```bash
# Build everything
.\tools\dev.ps1 build

# Run all tests
.\tools\dev.ps1 test

# Run specific tests
.\tools\dev.ps1 test backend
.\tools\dev.ps1 test frontend
.\tools\dev.ps1 test e2e

# Lint and format code
.\tools\dev.ps1 lint

# Clean build artifacts
.\tools\dev.ps1 clean

# Database operations
.\tools\dev.ps1 db-migrate "MigrationName"
.\tools\dev.ps1 db-update
.\tools\dev.ps1 db-reset
```

## 🗂️ Project Structure

```
PayRoll/
├── backend/                  # .NET backend (current)
│   ├── Payroll.Api          # API controllers
│   ├── Payroll.Application  # Use cases, DTOs
│   ├── Payroll.Domain       # Domain entities
│   ├── Payroll.Infrastructure # Data access
│   └── Payroll.Shared       # Common utilities
├── frontend/                 # Angular frontend (current)
│   └── payroll-web/         # SPA application
├── qa-automation/            # Playwright E2E tests
├── docs/                     # Documentation
│   ├── adr/                 # Architecture decisions
│   ├── architecture/        # Architecture diagrams
│   └── product/             # Product requirements
├── tools/                    # Development tools
│   ├── dev.ps1              # PowerShell dev script
│   └── dev.sh               # Bash dev script
└── .github/                  # GitHub workflows
    └── workflows/           # CI/CD pipelines
```

> **Note**: We're gradually migrating to a `/src` monorepo structure. See [ADR-0001](./docs/adr/0001-monorepo-structure.md) for details.

## 🧪 Testing

### Test Pyramid

```
      /\
     /E2E\         ← 10% (Critical user journeys)
    /------\
   /  API  \       ← 20% (Integration tests)
  /----------\
 /    Unit    \    ← 70% (Business logic)
/--------------\
```

### Running Tests

```bash
# All tests
.\tools\dev.ps1 test

# Backend unit tests
cd backend/Payroll.Application.Tests
dotnet test

# Frontend unit tests
cd frontend/payroll-web
npm test

# E2E tests
cd qa-automation
npx playwright test
```

## 🔐 Security

- **Authentication**: JWT-based
- **Authorization**: Role-based + Permission-based
- **Data Protection**: SQL Server TDE (production)
- **Input Validation**: Server-side validation mandatory
- **Audit Trail**: Immutable audit log

## 🌍 Sri Lanka Compliance

- ✅ EPF (Employees' Provident Fund) calculations
- ✅ ETF (Employees' Trust Fund) calculations
- ✅ PAYE Tax calculations per Inland Revenue Department
- ✅ Support for Sri Lankan date formats
- ✅ Currency: LKR (Sri Lankan Rupee)

## 🤖 AI-Friendly Development

This repository is optimized for AI coding assistants:

- **Clear Structure**: Organized by bounded contexts
- **Consistent Naming**: PascalCase (C#), kebab-case (Angular)
- **Comprehensive Docs**: AGENTS.md guides AI agents
- **Type Safety**: TypeScript + C# strong typing
- **Test Coverage**: High test coverage for confidence

See [AGENTS.md](./AGENTS.md) for AI agent guidelines.

## 🚢 Deployment

### Environments
- **Development**: Local development
- **Test**: QA testing
- **Staging**: Pre-production
- **Production**: Live system

### CI/CD Pipeline
- ✅ Automated build and test
- ✅ Code quality checks
- ✅ Security scanning
- ⏳ Automated deployment (coming soon)

## 📊 Roadmap

### Phase 1: Foundation (Current)
- ✅ Clean architecture setup
- ✅ Core payroll calculations
- ✅ Employee management
- ✅ Basic reporting

### Phase 2: Enhancement (Q1 2026)
- ⏳ Advanced reporting
- ⏳ Leave management
- ⏳ Loan management
- ⏳ Performance optimization

### Phase 3: Scale (Q2 2026)
- 📋 Multi-currency support
- 📋 Advanced GL integration
- 📋 Mobile app
- 📋 Self-service portal

### Phase 4: Enterprise (Q3 2026)
- 📋 Microservices architecture
- 📋 Advanced analytics
- 📋 AI-powered insights
- 📋 International expansion

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Read [DEVELOPMENT.md](./DEVELOPMENT.md)
2. Read [AGENTS.md](./AGENTS.md) for conventions
3. Create a feature branch
4. Make your changes
5. Run tests: `.\tools\dev.ps1 test`
6. Run lint: `.\tools\dev.ps1 lint`
7. Submit a PR using the template

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Documentation**: Check [DEVELOPMENT.md](./DEVELOPMENT.md) and [AGENTS.md](./AGENTS.md)
- **Issues**: Create a [GitHub issue](https://github.com/chamarairesh1982/PayRoll/issues)
- **Discussions**: Use [GitHub Discussions](https://github.com/chamarairesh1982/PayRoll/discussions)

## 🙏 Acknowledgments

- Built with ❤️ for Sri Lankan businesses
- Powered by .NET and Angular
- Inspired by Clean Architecture and DDD principles

---

**Last Updated**: 2025-12-25  
**Version**: 1.0.0  
**Maintained By**: Development Team
