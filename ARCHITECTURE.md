# Architecture Overview

> **Sri Lanka Payroll Platform** - A world-class SaaS payroll system built with Clean Architecture and Domain-Driven Design principles.

## 🎯 System Overview

### Vision
Build a scalable, maintainable, and compliant payroll platform for Sri Lankan businesses with the flexibility to expand to other markets.

### Key Principles
1. **Clean Architecture** - Clear separation of concerns with dependency inversion
2. **Domain-Driven Design** - Business logic in the domain layer, organized by bounded contexts
3. **CQRS Pattern** - Separate read and write operations for scalability
4. **Event Sourcing** - Audit trail for compliance and debugging
5. **Multi-Tenancy** - Support multiple companies in a single deployment
6. **API-First** - RESTful API with OpenAPI documentation

## 🏗️ Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌──────────────────┐              ┌──────────────────┐    │
│  │   Angular SPA    │              │  API Controllers │    │
│  │  (payroll-web)   │◄────────────►│  (Payroll.Api)   │    │
│  └──────────────────┘              └──────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Use Cases (Commands & Queries)                    │    │
│  │  - CreateEmployeeCommandHandler                    │    │
│  │  - GetEmployeeQueryHandler                         │    │
│  │  - CalculatePayrollCommandHandler                  │    │
│  │                                                     │    │
│  │  DTOs, Validators, Mappings, Interfaces            │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Entities, Value Objects, Domain Services          │    │
│  │  - Employee, PayRun, PaySlip                       │    │
│  │  - Money, DateRange, TaxSlab                       │    │
│  │  - PayrollCalculationService                       │    │
│  │                                                     │    │
│  │  Business Rules & Domain Events                    │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Data Access (EF Core)                             │    │
│  │  - PayrollDbContext                                │    │
│  │  - Repositories                                    │    │
│  │                                                     │    │
│  │  External Services                                 │    │
│  │  - Email, SMS, File Storage, PDF Generation        │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rule
**Inner layers NEVER depend on outer layers.**
- Domain has NO dependencies
- Application depends only on Domain
- Infrastructure implements interfaces from Application
- API/Web depends on Application (not Infrastructure directly)

## 📦 Bounded Contexts

The system is organized into the following bounded contexts:

### 1. **Employees** 
Employee lifecycle management, profiles, documents, and personal information.

**Key Entities:**
- `Employee` - Core employee entity
- `EmployeeTaxProfile` - Tax-related settings
- `EmployeePayItem` - Allowances and deductions
- `EmployeeRecurringPayItem` - Recurring pay items

**Responsibilities:**
- Employee CRUD operations
- Profile management
- Document storage
- Employment history

### 2. **Payroll**
Pay run execution, payslip generation, and salary calculations.

**Key Entities:**
- `PayRun` - Monthly/periodic payroll execution
- `PaySlip` - Individual employee payslip
- `EarningLine` - Salary components (basic, allowances, OT)
- `DeductionLine` - Deductions (tax, EPF, loans)

**Responsibilities:**
- Payroll calculation engine
- Payslip generation
- Statutory calculations (EPF, ETF, Tax)
- Bank file exports

### 3. **PayrollConfig**
System configuration for payroll rules, tax slabs, and master data.

**Key Entities:**
- `AllowanceType` - Allowance definitions
- `DeductionType` - Deduction definitions
- `TaxRuleSet` - Tax calculation rules
- `EpfEtfRuleSet` - EPF/ETF rules
- `Bank`, `BankBranch` - Banking information

**Responsibilities:**
- Tax slab management
- EPF/ETF rate configuration
- Allowance/deduction definitions
- Rule versioning

### 4. **Organizations**
Company structure, branches, and cost centers.

**Key Entities:**
- `Company` - Multi-tenant company entity
- `Branch` - Physical locations
- `CostCenter` - Cost allocation units

**Responsibilities:**
- Organizational hierarchy
- Multi-tenancy support
- Cost center management

### 5. **Leave**
Leave management, balances, and encashments.

**Key Entities:**
- `LeaveRequest` - Leave applications
- `LeaveTypeDefinition` - Leave types (annual, sick, etc.)
- `LeaveEncashmentRequest` - Leave encashment

**Responsibilities:**
- Leave balance tracking
- Leave approval workflow
- Leave encashment calculations

### 6. **Loans**
Employee loan management and repayments.

**Key Entities:**
- `Loan` - Loan records
- `LoanRepayment` - Repayment schedule

**Responsibilities:**
- Loan disbursement
- Automatic deductions
- Repayment tracking

### 7. **Overtime**
Overtime entry and calculations.

**Key Entities:**
- `OTEntry` - Overtime records
- `OTRule` - Overtime calculation rules

**Responsibilities:**
- OT hour tracking
- OT rate calculations
- OT approval workflow

### 8. **GeneralLedger**
GL integration and journal entries.

**Key Entities:**
- `GlAccount` - Chart of accounts
- `GlMapping` - Payroll to GL mappings
- `GlJournalBatch` - Journal entry batches
- `GlJournalLine` - Individual journal lines

**Responsibilities:**
- GL account mapping
- Journal entry generation
- Export to accounting systems

### 9. **Auditing**
Comprehensive audit trail for compliance.

**Key Entities:**
- `AuditEvent` - Immutable audit records

**Responsibilities:**
- Change tracking
- Compliance reporting
- Tamper-proof audit trail

## 🔄 Data Flow

### Typical Payroll Run Flow

```
1. User initiates pay run
   │
   ├─► API Controller receives request
   │
   ├─► CreatePayRunCommandHandler validates input
   │
   ├─► PayrollCalculationService calculates:
   │   ├─► Basic salary
   │   ├─► Allowances (from EmployeePayItems)
   │   ├─► Overtime (from OTEntries)
   │   ├─► Deductions (EPF, Tax, Loans)
   │   └─► Net pay
   │
   ├─► PaySlip entities created
   │
   ├─► GL journal entries generated
   │
   ├─► Audit events logged
   │
   └─► Response returned to client
```

### Authentication & Authorization Flow

```
1. User logs in (username/password)
   │
   ├─► AuthController validates credentials
   │
   ├─► JWT token generated with claims:
   │   ├─► UserId
   │   ├─► CompanyId (tenant)
   │   ├─► Roles
   │   └─► Permissions
   │
   ├─► Token returned to client
   │
   └─► Client includes token in Authorization header
       │
       ├─► API validates token
       │
       ├─► CurrentUserService extracts claims
       │
       └─► Authorization policies enforced
```

## 🗄️ Database Design

### Key Design Decisions

1. **Multi-Tenancy**: `CompanyId` on most tables for data isolation
2. **Soft Deletes**: `IsActive` flag instead of hard deletes
3. **Audit Columns**: `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy` on all entities
4. **Immutable Audit Trail**: `AuditEvents` table is append-only
5. **Versioned Rules**: `RulePackageVersion` for historical tax/EPF rules

### Entity Relationships

```
Company
  ├─► Branches
  │     └─► CostCenters
  │
  ├─► Employees
  │     ├─► EmployeeTaxProfile
  │     ├─► EmployeePayItems
  │     ├─► EmployeeRecurringPayItems
  │     ├─► LeaveRequests
  │     ├─► Loans
  │     └─► OTEntries
  │
  └─► PayRuns
        ├─► PaySlips
        │     ├─► EarningLines
        │     └─► DeductionLines
        │
        ├─► GlJournalBatches
        │     └─► GlJournalLines
        │
        └─► PayRunBankExports
```

## 🔐 Security Architecture

### Authentication
- **JWT Tokens**: Stateless authentication
- **Token Storage**: HttpOnly cookies (XSS protection)
- **Token Expiry**: 1 hour (configurable)
- **Refresh Tokens**: 7 days (future enhancement)

### Authorization
- **Role-Based**: Admin, Manager, Employee
- **Permission-Based**: Fine-grained permissions (e.g., `payroll:run`, `employee:create`)
- **Multi-Tenant**: Users can only access their company's data

### Data Protection
- **Encryption at Rest**: SQL Server TDE (production)
- **Encryption in Transit**: HTTPS/TLS
- **Sensitive Data**: User secrets for connection strings, API keys
- **Password Hashing**: BCrypt with salt

## 📊 Performance Considerations

### Database Optimization
- **Indexes**: On foreign keys, frequently queried columns
- **Pagination**: All list endpoints support paging
- **Projections**: Use DTOs to avoid over-fetching
- **Caching**: Static data (tax slabs, allowance types) cached

### API Optimization
- **Async/Await**: All I/O operations are asynchronous
- **Response Compression**: Gzip compression enabled
- **Rate Limiting**: Prevent abuse (future enhancement)

### Frontend Optimization
- **Lazy Loading**: Feature modules loaded on demand
- **OnPush Change Detection**: Reduce change detection cycles
- **Virtual Scrolling**: For large lists
- **Service Workers**: Offline support (future enhancement)

## 🚀 Scalability Strategy

### Horizontal Scaling
- **Stateless API**: Can run multiple instances behind load balancer
- **Database**: Read replicas for reporting queries
- **File Storage**: Azure Blob Storage / S3 (future)

### Vertical Scaling
- **Database**: Upgrade SQL Server tier as needed
- **API**: Increase CPU/memory allocation

### Future Enhancements
- **Message Queue**: RabbitMQ/Azure Service Bus for async processing
- **Background Jobs**: Hangfire for scheduled tasks
- **Microservices**: Split into smaller services if needed
- **Event Sourcing**: Full event sourcing for audit trail

## 🧪 Testing Strategy

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

### Test Types

1. **Unit Tests** (xUnit, Jasmine)
   - Domain logic
   - Application handlers
   - Validators
   - Services

2. **Integration Tests** (WebApplicationFactory)
   - API endpoints
   - Database operations
   - External service mocks

3. **E2E Tests** (Playwright)
   - Critical user flows
   - Cross-browser testing
   - Visual regression (future)

## 📈 Monitoring & Observability

### Logging
- **Structured Logging**: Serilog with JSON output
- **Log Levels**: Debug, Info, Warning, Error, Critical
- **Log Sinks**: Console, File, Application Insights (future)

### Metrics (Future)
- **Application Metrics**: Request count, duration, errors
- **Business Metrics**: Payroll runs, employee count, active users
- **Infrastructure Metrics**: CPU, memory, disk, network

### Tracing (Future)
- **Distributed Tracing**: OpenTelemetry
- **Correlation IDs**: Track requests across services

## 🔄 CI/CD Pipeline

### Build Pipeline
1. **Restore**: Dependencies (NuGet, npm)
2. **Build**: Compile code
3. **Lint**: Code style checks
4. **Test**: Run all tests
5. **Package**: Create artifacts

### Deployment Pipeline
1. **Deploy to Test**: Automated deployment
2. **Smoke Tests**: Basic health checks
3. **Deploy to Staging**: Manual approval
4. **Integration Tests**: Full test suite
5. **Deploy to Production**: Manual approval
6. **Health Checks**: Verify deployment

## 📚 Technology Stack

### Backend
- **.NET 8** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **MediatR** - CQRS implementation
- **Serilog** - Logging
- **xUnit** - Unit testing

### Frontend
- **Angular 18+** - SPA framework
- **PrimeNG** - UI component library
- **RxJS** - Reactive programming
- **TypeScript** - Type safety
- **Jasmine/Karma** - Unit testing
- **Playwright** - E2E testing

### Database
- **SQL Server 2019+** - Primary database
- **Redis** - Caching (future)

### Infrastructure
- **Azure** - Cloud platform (future)
- **Docker** - Containerization (future)
- **Kubernetes** - Orchestration (future)

## 🗺️ Roadmap

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

## 📖 References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Microsoft .NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [Angular Architecture Guide](https://angular.io/guide/architecture)

---

**Last Updated**: 2025-12-25  
**Version**: 1.0  
**Maintained By**: Architecture Team
