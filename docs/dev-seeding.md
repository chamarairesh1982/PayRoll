# Developer Data Seeding

## Prerequisites
- .NET 8 SDK
- SQL Server instance reachable by the connection string
- Database connection string configured via one of:
  - `backend/Payroll.Api/appsettings.json` → `Database:ConnectionString`
  - `backend/Payroll.Seeder/appsettings.json`
  - Environment variables (e.g. `Database__ConnectionString`)

## Commands
```bash
dotnet run --project backend/Payroll.Seeder -- --mode master
```
Seeds the minimal Sri Lanka master data.

```bash
dotnet run --project backend/Payroll.Seeder -- --mode scenarios
```
Seeds master data plus QA scenario data.

```bash
dotnet run --project backend/Payroll.Seeder -- --mode reset
```
Deletes existing data from core payroll tables and reseeds master + scenario data. **Dev only.**

## Master data inserted (Sri Lanka baseline)
- **Organization:**
  - Company `SLPAY` (name includes EPF/ETF registration placeholders)
  - Branches `HQ`, `KDY`
  - Cost centers `CORP`, `HQ-OPS`, `KDY-OPS`
- **Payroll settings:**
  - Working days/hours, no-pay calculation basis, OT multipliers, rounding, caps
- **Earning/Deduction catalog:**
  - Allowances: `BASIC`, `ALW`, `OT`
  - Deductions: `EPF_EE`, `EPF_ER`, `ETF_ER`, `PAYE`, `NOPAY`
- **Statutory defaults:**
  - EPF/ETF rule set (rates are sample defaults, configurable in the UI/API)
- **Tax slabs (sample values):**
  - `Sri Lanka PAYE (Sample)` rule set with slabs for YA 2025 and effective dates
- **Bank templates (available in code):**
  - `HNB`, `BOC`, `Commercial` (from `Payroll.Application.BankExports.BankExportTemplateResolver`)
  - Field mappings follow the template definitions in `Payroll.Application.BankExports` (sample layouts)

## Scenario data inserted
### Pay run lifecycle
- Draft pay run: `PR-APR25-DRAFT`
- Prepared pay run: `PR-APR25-PREP` (with approval history)
- Approved pay run: `PR-APR25-APPR` (with approval history)
- Locked pay run with valid bank data: `PR-APR25-LOCK` (with approval history, payslips)
- Locked pay run with invalid bank data: `PR-APR25-LOCK-BAD` (with approval history, payslip missing bank details)

### Employees
- Active salaried, hourly, and high-tax employees
- One inactive employee
- One employee missing bank details
- Mix of branches/cost centers for multi-entity coverage

### Recurring items
- Monthly allowance rule (`RR-ALW-MON`)
- Temporary deduction rule with start/end (`RR-DED-TEMP`)
- Inactive rule (`RR-INACTIVE`)
- Mid-month recurring allowance to exercise proration logic (`EMP-SAL` + `ALW_TRAN`)

### PAYE
- `EMP-TAX` payslip with taxable income crossing slab thresholds

### EPF/ETF
- `EMP-EPF` payslip with contributable basic salary + non-contributable allowance (`ALW_NC`)

### Bank export validation
- `PR-APR25-LOCK` uses employees with complete HNB bank fields
- `PR-APR25-LOCK-BAD` includes `EMP-NOBANK` to trigger missing bank detail validation

### OT
- Approved OT entries (weekday + weekend) and one pending OT entry for `EMP-HR`

### Attendance no-pay
- `EMP-HR` has one full-day absence and one half-day attendance entry

## Reset guidance
The `reset` mode deletes data from payroll operational tables (employees, pay runs, pay slips, recurring items, attendance, overtime, tax rules, etc.) before reseeding. It is intended **only for local development** and should not be used in shared or production databases.
