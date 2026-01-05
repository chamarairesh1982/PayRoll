# AI Agent Operating Manual

> **Purpose**: This document guides AI coding assistants (GitHub Copilot, Cursor, Codex, Antigravity, etc.) on how to work effectively in this repository.

## 🎯 Repository Overview

**Sri Lanka Payroll Platform** - A world-class SaaS payroll system with clean architecture, domain-driven design, and multi-tenant capabilities.

- **Tech Stack**: .NET 8, Angular 18+, SQL Server, Playwright
- **Architecture**: Clean Architecture + DDD + CQRS patterns
- **Target Market**: Sri Lanka (with internationalization support)

## 📁 Repository Structure

```
/
├── src/                          # Source code (monorepo)
│   ├── apps/                     # Deployable applications
│   │   ├── web/                  # Angular SPA (frontend)
│   │   └── api/                  # ASP.NET Core API (backend)
│   ├── services/                 # Background services & workers
│   │   └── (future: jobs, integrations)
│   └── libs/                     # Shared libraries
│       ├── domain/               # Core domain models
│       ├── application/          # Use cases & business logic
│       ├── infrastructure/       # Data access & external services
│       ├── shared/               # Common utilities
│       └── contracts/            # DTOs, events, API contracts
├── tests/                        # All test projects
│   ├── unit/                     # Unit tests
│   ├── integration/              # Integration tests
│   └── e2e/                      # End-to-end tests (Playwright)
├── infra/                        # Infrastructure as Code
│   ├── docker/                   # Docker configurations
│   ├── bicep/                    # Azure Bicep templates
│   └── terraform/                # Terraform (if needed)
├── docs/                         # Documentation
│   ├── adr/                      # Architecture Decision Records
│   ├── architecture/             # Architecture diagrams
│   ├── api/                      # API documentation
│   └── product/                  # Product requirements
├── tools/                        # Development tools & scripts
│   ├── scripts/                  # Cross-platform scripts
│   └── generators/               # Code generators
└── .github/                      # GitHub workflows & templates
    ├── workflows/                # CI/CD pipelines
    └── PULL_REQUEST_TEMPLATE.md
```

## 🚀 Quick Start Commands

### Bootstrap (First Time Setup)
```bash
# Windows
.\tools\dev.ps1 bootstrap

# Linux/macOS
./tools/dev.sh bootstrap
```

### Daily Development
```bash
# Build everything
.\tools\dev.ps1 build

# Run tests
.\tools\dev.ps1 test

# Lint & format
.\tools\dev.ps1 lint

# Run locally
.\tools\dev.ps1 run
```

## 🏗️ Architecture Principles

### 1. **Clean Architecture Layers**
```
┌─────────────────────────────────────┐
│         Presentation Layer          │  ← Angular SPA, API Controllers
├─────────────────────────────────────┤
│        Application Layer            │  ← Use Cases, DTOs, Validators
├─────────────────────────────────────┤
│          Domain Layer               │  ← Entities, Value Objects, Domain Services
├─────────────────────────────────────┤
│       Infrastructure Layer          │  ← EF Core, External APIs, File Storage
└─────────────────────────────────────┘
```

**Dependency Rule**: Inner layers NEVER depend on outer layers.

### 2. **Domain-Driven Design Boundaries**

**Bounded Contexts:**
- `Employees` - Employee management, profiles, documents
- `Payroll` - Pay runs, payslips, calculations
- `PayrollConfig` - Tax rules, allowances, deductions, banks
- `Organizations` - Companies, branches, cost centers
- `Leave` - Leave requests, balances, encashments
- `Loans` - Employee loans, repayments
- `Overtime` - OT entries, rules, calculations
- `GeneralLedger` - GL accounts, mappings, journal entries
- `Auditing` - Audit trails, compliance logs

### 3. **Naming Conventions**

#### Backend (.NET)
- **Namespaces**: `Payroll.{Layer}.{BoundedContext}`
  - Example: `Payroll.Application.Employees.Commands`
- **Files**: PascalCase (e.g., `EmployeeService.cs`)
- **Classes**: PascalCase, descriptive (e.g., `CreateEmployeeCommandHandler`)
- **Interfaces**: Prefix with `I` (e.g., `IEmployeeRepository`)
- **DTOs**: Suffix with purpose (e.g., `EmployeeDto`, `CreateEmployeeRequest`)

#### Frontend (Angular)
- **Components**: kebab-case (e.g., `employee-list.component.ts`)
- **Services**: kebab-case + `.service` (e.g., `employee.service.ts`)
- **Modules**: kebab-case + `.module` (e.g., `employees.module.ts`)
- **Folders**: kebab-case, feature-based (e.g., `src/app/features/employees/`)

#### Tests
- **Unit Tests**: `{ClassName}.Tests.cs` or `{component-name}.spec.ts`
- **Integration Tests**: `{Feature}IntegrationTests.cs`
- **E2E Tests**: `{feature}.spec.ts` in `/tests/e2e/`

### 4. **File Organization**

#### Backend Project Structure
```
Payroll.Application/
├── Employees/
│   ├── Commands/
│   │   ├── CreateEmployee/
│   │   │   ├── CreateEmployeeCommand.cs
│   │   │   ├── CreateEmployeeCommandHandler.cs
│   │   │   └── CreateEmployeeCommandValidator.cs
│   │   └── UpdateEmployee/
│   ├── Queries/
│   │   ├── GetEmployee/
│   │   └── ListEmployees/
│   ├── DTOs/
│   │   └── EmployeeDto.cs
│   └── Mappings/
│       └── EmployeeMappingProfile.cs
```

#### Frontend Feature Structure
```
src/app/features/employees/
├── components/
│   ├── employee-list/
│   ├── employee-form/
│   └── employee-detail/
├── pages/
│   ├── employees-list-page/
│   └── employee-edit-page/
├── services/
│   └── employee.service.ts
├── models/
│   └── employee.model.ts
└── employees-routing.module.ts
```

## 🤖 AI Agent Guidelines

### DO ✅

1. **Follow Existing Patterns**
   - Study similar features before creating new ones
   - Use CQRS pattern for backend (Commands/Queries)
   - Use reactive forms in Angular

2. **Maintain Clean Architecture**
   - Domain layer has NO external dependencies
   - Application layer depends only on Domain
   - Infrastructure implements interfaces from Application

3. **Write Tests**
   - Unit tests for business logic
   - Integration tests for database operations
   - E2E tests for critical user flows

4. **Use Dependency Injection**
   - Backend: Register services in `DependencyInjection.cs`
   - Frontend: Use Angular's DI system

5. **Follow Sri Lanka Compliance**
   - EPF/ETF calculations per Sri Lankan law
   - Tax slabs according to Inland Revenue Department
   - Support for Sri Lankan date formats, currency (LKR)

6. **Document Decisions**
   - Create ADR for architectural changes
   - Update API documentation for new endpoints
   - Add inline comments for complex business logic

### DON'T ❌

1. **Don't Break Dependency Rules**
   - Never reference Infrastructure from Domain
   - Never reference Application from Domain
   - Never bypass the repository pattern

2. **Don't Hardcode**
   - Use configuration files (`appsettings.json`, `environment.ts`)
   - Use constants/enums for magic strings
   - Use dependency injection, not `new` keyword

3. **Don't Skip Validation**
   - Validate all inputs (FluentValidation in backend, Validators in Angular)
   - Sanitize user inputs
   - Handle edge cases

4. **Don't Ignore Errors**
   - Use proper exception handling
   - Log errors appropriately
   - Return meaningful error messages

5. **Don't Mix Concerns**
   - Keep controllers thin (delegation only)
   - Keep components focused (single responsibility)
   - Separate business logic from presentation

## 📝 Common Tasks

### Adding a New Feature

1. **Backend**:
   ```bash
   # 1. Add entity to Domain layer
   # 2. Create repository interface in Application
   # 3. Implement repository in Infrastructure
   # 4. Create Commands/Queries in Application
   # 5. Add API controller in Api layer
   # 6. Write tests
   ```

2. **Frontend**:
   ```bash
   # 1. Generate feature module
   ng generate module features/{feature-name} --routing
   
   # 2. Create components
   ng generate component features/{feature-name}/components/{component-name}
   
   # 3. Create service
   ng generate service features/{feature-name}/services/{feature-name}
   
   # 4. Add routes
   # 5. Write tests
   ```

### Adding a New Migration

```bash
# From /src/apps/api directory
dotnet ef migrations add {MigrationName} --project ../../libs/infrastructure --startup-project .

# Apply migration
dotnet ef database update --project ../../libs/infrastructure --startup-project .
```

### Running Specific Tests

```bash
# Backend unit tests
dotnet test tests/unit/Payroll.Application.Tests

# Frontend unit tests
cd src/apps/web && npm run test

# E2E tests
cd tests/e2e && npx playwright test
```

## 🔧 Configuration Management

### Environment Variables

**Backend** (`appsettings.{Environment}.json`):
- `Database:ConnectionString` - SQL Server connection
- `Jwt:SecretKey` - JWT signing key
- `Cors:AllowedOrigins` - CORS origins
- `Storage:BasePath` - File storage path

**Frontend** (`environment.{env}.ts`):
- `apiUrl` - Backend API URL
- `production` - Production flag
- `features` - Feature flags

### Feature Flags

Use feature flags for gradual rollouts:
```typescript
// Frontend
if (environment.features.newPayrollEngine) {
  // Use new engine
}
```

```csharp
// Backend
if (_featureManager.IsEnabledAsync("NewPayrollEngine").Result) {
  // Use new engine
}
```

## 🐛 Debugging

### Backend
- Use Visual Studio debugger or `dotnet run --launch-profile Development`
- Check logs in `logs/` directory
- Use Swagger UI at `/swagger` for API testing

### Frontend
- Use Angular DevTools browser extension
- Check browser console for errors
- Use `ng serve --source-map` for debugging

### Database
- Use SQL Server Management Studio or Azure Data Studio
- Check migration history: `SELECT * FROM __EFMigrationsHistory`
- View logs: `SELECT * FROM AuditEvents ORDER BY TimestampUtc DESC`

## 📊 Performance Guidelines

1. **Database**:
   - Use indexes on frequently queried columns
   - Avoid N+1 queries (use `.Include()` or projections)
   - Use pagination for large datasets

2. **API**:
   - Use async/await consistently
   - Implement caching for static data
   - Use compression for responses

3. **Frontend**:
   - Use OnPush change detection
   - Lazy load feature modules
   - Use trackBy in *ngFor loops

## 🔒 Security Guidelines

1. **Authentication**: JWT-based, stored in httpOnly cookies
2. **Authorization**: Role-based + permission-based
3. **Input Validation**: Server-side validation is mandatory
4. **SQL Injection**: Use parameterized queries (EF Core does this)
5. **XSS**: Angular sanitizes by default, but be careful with `innerHTML`
6. **CSRF**: Use anti-forgery tokens for state-changing operations

## 📦 Dependency Management

### Backend
- Use `dotnet add package` for NuGet packages
- Keep packages up-to-date (security patches)
- Document why each package is needed

### Frontend
- Use `npm install --save` or `--save-dev`
- Commit `package-lock.json`
- Audit dependencies: `npm audit`

## 🚢 Deployment

### Environments
- **Development**: Local development
- **Test**: QA testing environment
- **Staging**: Pre-production
- **Production**: Live system

### CI/CD Pipeline
- **Build**: Compile, lint, test
- **Test**: Run all tests
- **Package**: Create artifacts
- **Deploy**: Deploy to target environment

See `.github/workflows/` for pipeline definitions.

## 📚 Additional Resources

- [DEVELOPMENT.md](./DEVELOPMENT.md) - Development setup guide
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Detailed architecture documentation
- [docs/adr/](./docs/adr/) - Architecture Decision Records
- [API Documentation](./docs/api/) - API reference

## 🆘 Getting Help

1. Check existing documentation
2. Search closed issues/PRs
3. Ask in team chat
4. Create a GitHub issue with:
   - Clear description
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details

---

**Last Updated**: 2025-12-25  
**Maintained By**: Development Team
