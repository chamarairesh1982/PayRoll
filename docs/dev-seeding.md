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
Deletes only scenario/demo data (records prefixed with `DEMO_`) and reseeds master + scenario data.
**Dev only** and requires `SEEDER_ALLOW_RESET=true`.

## Master data inserted (Sri Lanka baseline)
- **Organization:**
  - Company `DEMO_LANKA` → "Demo Lanka (Pvt) Ltd"
  - Branches `COL` (Colombo), `KDY` (Kandy)
  - Cost centers `OPS` (Operations), `SALES` (Sales), `FIN` (Finance)
- **Payroll settings:**
  - Working days per month (26), working hours, no-pay calculation basis, OT multipliers, rounding, caps
  - Monthly pay period is implicit (no dedicated pay calendar entity yet; pay runs are seeded as monthly)
- **Earning/Deduction catalog:**
  - Allowances: `BASIC`, `ALLOW_TRANSPORT`, `OT`
  - Deductions: `EPF_EMPLOYEE`, `EPF_EMPLOYER`, `ETF_EMPLOYER`, `PAYE`, `DED_NO_PAY`
- **Statutory defaults:**
  - EPF/ETF rule set (rates are sample defaults, configurable in the UI/API)
- **Tax slabs (sample values):**
  - `Sri Lanka PAYE (Sample)` rule set with slabs for YA 2025 and effective dates
- **Bank templates:**
  - No template entity exists yet; the seeder sets `PayRun.ExportedBank = HNB` on the locked demo pay run.

> Note: component flags like `includeInDailyRateBase` and `includeInOtBase` are not modeled yet, so only taxable/EPF/ETF flags are seeded where the fields exist.
> Employee EPF/ETF registration numbers are not modeled in the current `Employee` entity, so the seeder cannot populate them yet.
> Employee tax profiles (tax exemption/override) are modeled in `EmployeeTaxProfiles` and seeded for demo employees.
> Currency defaults are not modeled in `PayrollSettings`, so the seeder cannot persist LKR defaults yet.

## Scenario data inserted
### Pay run lifecycle
- Draft pay run: `DEMO_PR_2025_04_DRAFT`
- Prepared pay run: `DEMO_PR_2025_04_PREP` (with approval history)
- Approved pay run: `DEMO_PR_2025_04_APPR` (with approval history)
- Locked pay run: `DEMO_PR_2025_04_LOCK` (with approval history, payslips)

### Employees
- `DEMO_EMP_1`: active salaried, complete bank details
- `DEMO_EMP_2`: active salaried, missing bank account number
- `DEMO_EMP_3`: active lower-salary employee
- `DEMO_EMP_4`: inactive employee
- `DEMO_EMP_5`: different branch/cost center (Kandy/Finance)

### Recurring items
- Monthly allowance rule (`DEMO_RR_ALLOW`)
- Temporary deduction rule with start/end (`DEMO_RR_DED_TEMP`)
- Inactive rule (`DEMO_RR_INACTIVE`)
- Mid-month recurring allowance to exercise proration logic (`DEMO_EMP_1` + `ALLOW_TRANSPORT`)

### PAYE
- `DEMO_EMP_1` + `DEMO_EMP_5` payslips include PAYE demo deductions

### EPF/ETF
- `DEMO_EMP_5` payslip includes a non-contributable allowance (`DEMO_ALLOW_NC`)

### Bank export validation
- `DEMO_PR_2025_04_LOCK` includes `DEMO_EMP_2` with missing bank details

### OT
- Approved OT entries (weekday + weekend) and one pending OT entry for `DEMO_EMP_3`

### Attendance no-pay
- `DEMO_EMP_3` has one full-day absence and one half-day attendance entry

### Leave
- Approved annual leave for `DEMO_EMP_1`

## Reset guidance
The `reset` mode removes only scenario records that start with `DEMO_` (employees, pay runs, pay slips, recurring items, OT, attendance, leave, etc.) before reseeding. It is intended **only for local development** and should not be used in shared or production databases.
