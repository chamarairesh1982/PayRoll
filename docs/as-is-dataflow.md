# As-Is Payroll Data Flow and Data Dictionary

## Core payroll entities / tables
- **Employee** – master data with personal details, status flags, and base salary used as the default earning for payroll.【F:backend/Payroll.Domain/Employees/Employee.cs†L6-L167】
- **AttendanceRecord** – hours worked for an employee within a date range; used to compute no-pay deductions.【F:backend/Payroll.Domain/Attendance/AttendanceRecord.cs†L6-L10】
- **PayRun** – payroll batch with period, pay date, status, and generated payslips.【F:backend/Payroll.Domain/Payroll/PayRun.cs†L6-L25】
- **PaySlip** – per-employee payroll result with earnings, deductions, net pay, and statutory amounts.【F:backend/Payroll.Domain/Payroll/PaySlip.cs†L6-L22】
- **EarningLine / DeductionLine** – itemized earnings and deductions attached to a payslip (e.g., basic pay, OT, no-pay, tax).【F:backend/Payroll.Domain/Payroll/EarningLine.cs†L1-L12】【F:backend/Payroll.Domain/Payroll/DeductionLine.cs†L1-L11】
- **OvertimeRecord** – approved overtime hours by type; pulled into payslip earnings when unlocked for payroll.【F:backend/Payroll.Domain/Overtime/OvertimeRecord.cs†L6-L19】
- **Loan / LoanRepayment** – employee loan schedule and installments, reduced during payroll runs.【F:backend/Payroll.Domain/Loans/Loan.cs†L6-L15】【F:backend/Payroll.Domain/Loans/LoanRepayment.cs†L1-L12】
- **AllowanceType / DeductionType** – catalog of recurring earning/deduction definitions with EPF/ETF/tax flags (configuration).【F:backend/Payroll.Domain/PayrollConfig/AllowanceType.cs†L6-L15】【F:backend/Payroll.Domain/PayrollConfig/DeductionType.cs†L6-L14】
- **TaxRuleSet / TaxSlab** – PAYE tax tables with effective dates and rate slabs.【F:backend/Payroll.Domain/PayrollConfig/TaxRuleSet.cs†L6-L14】【F:backend/Payroll.Domain/PayrollConfig/TaxSlab.cs†L6-L13】
- **EpfEtfRuleSet** – statutory EPF/ETF rates, thresholds, and effective dates.【F:backend/Payroll.Domain/PayrollConfig/EpfEtfRuleSet.cs†L6-L17】
- **LeaveRequest** – captures leave type, dates, approval status; currently not integrated into payroll calculations.【F:backend/Payroll.Domain/Leave/LeaveRequest.cs†L6-L22】
- **Shift** – shift definition (start/end times) referenced by attendance but not yet linked in payroll math.【F:backend/Payroll.Domain/Attendance/Shift.cs†L1-L7】

## Key as-is flows (validated)
- **Employee onboarding** – HR creates or updates employees (code, base salary, dates).【F:backend/Payroll.Application/Services/EmployeeService.cs†L21-L137】
- **Attendance & overtime capture** – attendance records and approved overtime entries are stored and later filtered by pay period during pay-run generation.【F:backend/Payroll.Application/Services/PayrollService.cs†L167-L189】
- **Payroll run** – admin triggers pay-run creation for a period; system selects employees, gathers attendance/OT/loans, and generates payslips with EPF/ETF and PAYE applied.【F:backend/Payroll.Application/Services/PayrollService.cs†L37-L221】【F:backend/Payroll.Application/Services/PayrollService.cs†L224-L592】
- **Payslip retrieval** – payslips (with earnings/deductions breakdown) can be queried per pay-run.【F:backend/Payroll.Application/Services/PayrollService.cs†L156-L165】【F:backend/Payroll.Application/Services/PayrollService.cs†L551-L572】
- **Status changes** – pay-runs can be recalculated, locked when posted, and queried with pagination.【F:backend/Payroll.Application/Services/PayrollService.cs†L71-L155】

## Missing or partial areas
- **Recurring allowances/deductions** – placeholders exist but the earning/deduction load is TODO, so no catalog-driven recurring components are applied yet.【F:backend/Payroll.Application/Services/PayrollService.cs†L238-L309】
- **Approvals & audit** – pay-run approval is a status flag only; no workflow, approver tracking, or audit log store is implemented.【F:backend/Payroll.Application/Services/PayrollService.cs†L87-L135】【F:backend/Payroll.Domain/Common/AuditableEntity.cs†L1-L10】
- **Reporting** – statutory or financial reports are stubbed without output generation.【F:backend/Payroll.Application/Services/ReportService.cs†L1-L11】
- **Leave impact on payroll** – leave balances and statuses are stored but not factored into earnings/deductions; absences are inferred only from zero-hours attendance entries.【F:backend/Payroll.Domain/Leave/LeaveRequest.cs†L6-L22】【F:backend/Payroll.Application/Services/PayrollService.cs†L271-L305】
- **Banking/export** – no bank file/export integration in the current codebase.【d7b2a8†L1-L2】
- **General ledger integration** – no accounting postings are produced from pay-runs.【ea1ce6†L1-L2】
- **Audit logging** – entities inherit audit fields, but no centralized audit log store is configured beyond created/modified metadata.【F:backend/Payroll.Domain/Common/AuditableEntity.cs†L1-L10】

## Sri Lanka payroll gap matrix (code-based)
| Module / Feature | Current status in code | Why it matters in Sri Lanka | Recommended implementation | Priority |
| --- | --- | --- | --- | --- |
| Recurring Allowances/Deductions engine (catalog-driven) | **Partial** – Allowance/Deduction catalogs exist but payroll applies only basic/OT/loans; fixed allowance/deduction loaders are TODO.【F:backend/Payroll.Domain/PayrollConfig/AllowanceType.cs†L6-L15】【F:backend/Payroll.Application/Services/PayrollService.cs†L238-L309】 | Many Sri Lankan payrolls use fixed/variable recurring components (travel, phone, attendance-based allowances, union fees) that affect EPF/ETF/PAYE bases. | Implement per-employee assignments to catalog items with effective dates, map flags to EPF/ETF/tax bases, and inject them in pay-slip calculations. | P0 |
| PayRun lifecycle (Draft → Calculated → Reviewed → Approved → Locked) | **Partial** – Status enum includes stages, but lifecycle is unguarded; status can be set directly and locking only occurs on `Posted`.【F:backend/Payroll.Domain/Payroll/PayRunStatus.cs†L1-L9】【F:backend/Payroll.Application/Services/PayrollService.cs†L87-L135】 | Separation of duties and auditability require controlled transitions, maker-checker approval, and lock on approval/posting. | Add state machine or validation around transitions, capture reviewer/approver identities and timestamps, and enforce lock on Approved/Posted. | P0 |
| Audit log store (who changed what, when, before/after values) | **Missing** – Entities carry `Created/Modified` fields but no change history store or hooks exist.【F:backend/Payroll.Domain/Common/AuditableEntity.cs†L1-L10】【F:backend/Payroll.Infrastructure/Persistence/PayrollDbContext.cs†L1-L35】 | Payroll data changes (rates, pay-runs, approvals) must be traceable for compliance and dispute resolution. | Add audit log entity + interceptor to capture changes with user, timestamp, and before/after values; surface review UI. | P0 |
| Statutory outputs: EPF/ETF schedules + PAYE summary | **Partial** – EPF/ETF/PAYE amounts are calculated per payslip but reporting service is stubbed with TODO.【F:backend/Payroll.Application/Services/PayrollService.cs†L354-L442】【F:backend/Payroll.Application/Services/ReportService.cs†L1-L11】 | Sri Lankan employers must file monthly EPF/ETF returns and PAYE remittance schedules. | Build report generators that aggregate payslip data into statutory schedule formats (CSV/PDF) for submission. | P0 |
| Bank file export (configurable templates) | **Missing** – No bank export or template code is present.【d7b2a8†L1-L2】 | Salary disbursement commonly requires bank-specific CSV/TXT formats (e.g., Sampath/BOC) with account validation. | Add bank profile/config templates, allow mapping of net pay to export lines, and generate files per bank requirements. | P0/P1 |
| Payslip PDF generation | **Missing** – Payslips are exposed as DTOs only; no renderer or PDF output exists.【F:backend/Payroll.Application/Services/PayrollService.cs†L551-L572】【379054†L1-L1】 | Employees need payslips in printable form with statutory components for proof of income and audits. | Add payslip view templates and PDF generation (e.g., using Razor/QuestPDF) with secure delivery. | P1 |
| Leave integration rules (no-pay vs approved leave) | **Partial** – Leave requests exist but payroll ignores leave status; no-pay is inferred solely from zero-hour attendance.【F:backend/Payroll.Domain/Leave/LeaveRequest.cs†L6-L22】【F:backend/Payroll.Application/Services/PayrollService.cs†L258-L305】 | Approved leave (paid/unpaid) affects entitlement and no-pay deductions; SL labour rules require correct handling. | Integrate leave records into payroll calculations, differentiating paid leave, unpaid leave, and holidays; adjust no-pay logic accordingly. | P1 |
| GL summary export | **Missing** – No ledger/GL export code or journal mapping exists.【ea1ce6†L1-L2】 | Payroll must post to accounting (salary expense, EPF/ETF, PAYE liabilities) for financial statements. | Add GL mapping configuration and batch export (CSV/API) summarizing payroll journals by cost buckets. | P1/P2 |
| Multi-company/branch/cost center | **Missing** – Employee and payroll entities lack company/branch/cost-center fields or filters.【F:backend/Payroll.Domain/Employees/Employee.cs†L6-L167】【7d957c†L1-L1】 | Organizations often run multiple entities/branches; payroll must segment calculations and reports per entity/cost center. | Introduce organization/branch/cost-center references on employees and pay-runs, scope queries accordingly, and expose filters in reports/exports. | P1 |

## As-is DFD (Level 0)
```mermaid
graph TD
    HR[Admin/HR]
    Emp[Employee]
    Bank[Bank]
    IRD[IRD]
    EPF[EPF/ETF]
    ACC[Accounting]

    subgraph PayrollSystem
      P1[Employee Setup]
      P2[Attendance Import]
      P3[Payroll Calculation]
      P4[Approvals]
      P5[Payslip Generation]
      P6[Reporting]
      P7[Banking Export]
    end

    HR --> P1
    Emp --> P2
    Emp --> P1
    P1 -->|Employees| D1[(Employees)]
    P2 -->|Attendance| D2[(Attendance)]
    P3 -->|PayRuns| D3[(PayRuns)]
    P3 -->|PayrollRules| D4[(PayrollRules/TaxTables)]
    P3 -->|Audit| D5[(AuditLogs)]
    P3 --> P5
    P4 --> P3
    P5 -->|Payslip| Emp
    P6 --> HR
    P7 --> Bank
    P6 --> IRD
    P6 --> EPF
    P6 --> ACC
    D1 --> P3
    D2 --> P3
    D3 --> P4
    D4 --> P3
```

## As-is DFD (Level 1 – Payroll run)
```mermaid
graph TD
    HR[Admin/HR]
    Emp[Employee]
    subgraph DataStores
      D1[(Employees)]
      D2[(Attendance)]
      D3[(Overtime)]
      D4[(Loans)]
      D5[(PayrollRules: EPF/ETF, Tax)]
      D6[(PayRuns)]
      D7[(PaySlips)]
    end

    HR -->|Create pay run request| PR1[Select employees & period]
    PR1 -->|Active employees| D1
    PR1 --> PR2[Load period attendance]
    D2 --> PR2
    PR1 --> PR3[Load approved overtime]
    D3 --> PR3
    PR1 --> PR4[Load active loans]
    D4 --> PR4
    PR2 --> PR5[Compute basic & no-pay]
    PR3 --> PR6[Add OT earnings]
    PR4 --> PR7[Apply loan deductions]
    PR5 --> PR8[Apply statutory (EPF/ETF, PAYE)]
    PR6 --> PR8
    PR7 --> PR8
    D5 --> PR8
    PR8 --> PR9[Assemble payslips]
    PR9 -->|Payslips| D7
    PR9 -->|Pay run summary| D6
    HR -->|Review/lock| PR10[Set status]
    PR10 --> D6
    D7 -->|View payslip| Emp
```

## Data dictionary (as-is)
| Table / Aggregate | Key fields | Description | Relationships |
| --- | --- | --- | --- |
| Employees | `Id`, `EmployeeCode`, `NicNumber` | Master data including personal details, employment dates, and `BaseSalary`. | Referenced by PaySlips, AttendanceRecords, LeaveRequests, OvertimeRecords, Loans. 【F:backend/Payroll.Domain/Employees/Employee.cs†L6-L167】|
| AttendanceRecords | `Id`, `EmployeeId`, `Period(Start,End)`, `HoursWorked` | Captures work hours per period; used for no-pay calculation. | Links to Employees via `EmployeeId`. 【F:backend/Payroll.Domain/Attendance/AttendanceRecord.cs†L6-L10】|
| LeaveRequests | `Id`, `EmployeeId`, `LeaveType`, `StartDate`, `EndDate`, `Status` | Leave applications with approval metadata; not yet integrated into payroll amounts. | Links to Employees; approvals tracked via `ApprovedById`. 【F:backend/Payroll.Domain/Leave/LeaveRequest.cs†L6-L22】|
| OvertimeRecords | `Id`, `EmployeeId`, `Date`, `Hours`, `Type`, `Status`, `IsLockedForPayroll` | Approved overtime entries feeding payslip earnings. | Links to Employees; filtered by pay period when generating payslips. 【F:backend/Payroll.Domain/Overtime/OvertimeRecord.cs†L6-L19】【F:backend/Payroll.Application/Services/PayrollService.cs†L182-L220】|
| Loans | `Id`, `EmployeeId`, `PrincipalAmount`, `OutstandingPrincipal`, `InstallmentAmount`, `Status` | Employee loans with repayment schedule. | Has many LoanRepayments; referenced when applying deductions. 【F:backend/Payroll.Domain/Loans/Loan.cs†L6-L15】【F:backend/Payroll.Application/Services/PayrollService.cs†L186-L220】|
| LoanRepayments | `Id`, `LoanId`, `DueDate`, `Amount`, `IsPaid` | Planned installments for a loan. | Belongs to Loan via `LoanId`. 【F:backend/Payroll.Domain/Loans/LoanRepayment.cs†L1-L12】|
| PayRuns | `Id`, `Code`, `PeriodStart`, `PeriodEnd`, `PayDate`, `Status`, `IsLocked` | Payroll batch header for a set of payslips. | Contains many PaySlips. 【F:backend/Payroll.Domain/Payroll/PayRun.cs†L6-L25】|
| PaySlips | `Id`, `PayRunId`, `EmployeeId`, `BasicSalary`, `TotalEarnings`, `TotalDeductions`, `NetPay`, `EmployeeEpf`, `EmployerEpf`, `EmployerEtf`, `PayeTax` | Payroll result per employee with earnings/deductions collections. | Belongs to PayRun and Employee; has many EarningLines and DeductionLines. 【F:backend/Payroll.Domain/Payroll/PaySlip.cs†L6-L22】|
| EarningLines | `Id`, `PaySlipId`, `Code`, `Description`, `Amount`, `IsEpfApplicable`, `IsEtfApplicable`, `IsTaxable` | Itemized earnings (basic pay, OT, allowances). | Belongs to PaySlip; included in taxable/EPF/ETF bases. 【F:backend/Payroll.Domain/Payroll/EarningLine.cs†L1-L12】|
| DeductionLines | `Id`, `PaySlipId`, `Code`, `Description`, `Amount`, `IsPreTax`, `IsPostTax` | Itemized deductions (no-pay, loans, PAYE, EPF). | Belongs to PaySlip; pre/post-tax flags drive calculations. 【F:backend/Payroll.Domain/Payroll/DeductionLine.cs†L1-L11】【F:backend/Payroll.Application/Services/PayrollService.cs†L285-L508】|
| AllowanceTypes | `Id`, `Code`, `Name`, `Basis`, `IsEpfApplicable`, `IsEtfApplicable`, `IsTaxable` | Catalog of allowance definitions (recurring earnings). | Used for configuration; not yet linked to payslip generation. 【F:backend/Payroll.Domain/PayrollConfig/AllowanceType.cs†L6-L15】|
| DeductionTypes | `Id`, `Code`, `Name`, `Basis`, `IsPreTax`, `IsPostTax` | Catalog of deduction definitions (recurring deductions). | Used for configuration; not yet linked to payslip generation. 【F:backend/Payroll.Domain/PayrollConfig/DeductionType.cs†L6-L14】|
| EpfEtfRuleSets | `Id`, `Name`, `EffectiveFrom`, `EffectiveTo`, `EmployeeEpfRate`, `EmployerEpfRate`, `EmployerEtfRate`, thresholds | Statutory contribution rules applied during payroll. | Used when computing EPF/ETF bases and amounts. 【F:backend/Payroll.Domain/PayrollConfig/EpfEtfRuleSet.cs†L6-L17】【F:backend/Payroll.Application/Services/PayrollService.cs†L408-L457】|
| TaxRuleSets | `Id`, `Name`, `YearOfAssessment`, `EffectiveFrom/To`, `IsDefault` | PAYE rule header with effective period. | Has many TaxSlabs; selected per pay date. 【F:backend/Payroll.Domain/PayrollConfig/TaxRuleSet.cs†L6-L14】【F:backend/Payroll.Application/Services/PayrollService.cs†L194-L197】|
| TaxSlabs | `Id`, `TaxRuleSetId`, `FromAmount`, `ToAmount`, `RatePercent`, `Order` | Defines progressive tax brackets. | Belongs to TaxRuleSet; iterated to compute PAYE. 【F:backend/Payroll.Domain/PayrollConfig/TaxSlab.cs†L6-L13】【F:backend/Payroll.Application/Services/PayrollService.cs†L459-L508】|
| Audit (implicit) | `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `IsActive` | Audit metadata inherited by most aggregates. | Present on AuditableEntity descendants; no centralized log. 【F:backend/Payroll.Domain/Common/AuditableEntity.cs†L1-L10】|
