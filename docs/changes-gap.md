**1. PayRun lifecycle & approvals**

Priority: P0

What to change: Implement a multi-stage payroll workflow (draft → prepared → approved → locked) with approver assignments, timestamps, comments, and enforced state transitions/permissions to provide maker-checker controls before payments are finalized.

Where to change: Backend payroll domain (e.g., backend services handling pay-run entities, workflow/authorization modules), frontend admin/payroll UI for approval actions, and persistence/migration scripts.

Acceptance criteria:

Pay-runs have discrete workflow states with allowed transitions and enforced permissions.

Approver identities, timestamps, and comments are recorded per transition.

Unauthorized transitions are blocked and surfaced to users.

UI supports viewing status history and performing approvals/locks.

**2. Multi-company/Branch/Cost center support**

Priority: P0

What to change: Introduce organizational hierarchy on employees and pay-runs to segment data by company/branch/cost center, scoping rules/reports and approvals accordingly, with options for consolidated vs. per-branch outputs.

Where to change: Core data models and schemas for organizations/employees/pay-runs, authorization/segmentation logic, report queries, and related UI filters/selectors.

Acceptance criteria:

Employees and pay-runs can be tagged to company/branch/cost center entities.

Permissions and workflows respect the assigned entity scope.

Reports and exports can be generated per entity and consolidated.

UI offers filters/selectors to view and approve by entity.

**3. Recurring allowances & deductions engine**

Priority: P0

What to change: Deliver a recurring engine with APIs to manage recurrence rules (frequency, start/stop, eligibility, proration, EPF/ETF/tax flags), simulate upcoming amounts, and inject idempotent recurring lines into payslips during pay-run generation with retro adjustments, plus HR UI to manage and preview them.

Where to change: Payroll calculation services, recurrence scheduling components, payslip generation pipeline, REST/GraphQL API layer, and HR-facing UI forms/previews.

Acceptance criteria:

CRUD + simulation APIs for recurring allowance/deduction rules exist.

Pay-run generation auto-includes recurring lines with idempotency and retro handling.

UI supports creating/updating rules and previewing impact before approval.

EPF/ETF/tax applicability flags are honored in calculations.

**4. Leave integration into payroll math**

Priority: P0

What to change: Incorporate approved leave data into payroll to calculate no-pay deductions, leave encashment, and partial-day handling, reconciling with attendance to avoid double deductions.

Where to change: Payroll calculation engine, leave/attendance data ingestion and reconciliation modules, payslip generation logic, and related UI summaries.

Acceptance criteria:

Approved leave affects earnings/deductions per configured rules (including partial days).

No-pay and encashment amounts appear on payslips with traceable calculations.

Attendance reconciliation prevents duplicate deductions for the same absence.

Tests/QA scenarios cover edge cases (half-days, overlapping leave/attendance).

**5. Bank export files (HNB/BOC/Commercial templates)**

Priority: P0

What to change: Implement bank-specific salary export templates (CSV/TXT) with account validation, net pay amounts, per-pay-run file generation, and export status tracking to meet Sri Lankan bank upload requirements.

Where to change: Payment/export service layer, template generation utilities, validation routines, and payroll UI for initiating/download tracking.

Acceptance criteria:

Configurable exports for HNB/BOC/Commercial formats generate correctly per pay-run.

Employee bank details are validated before export; failures are reported.

Export status (pending/generated/downloaded) is tracked per pay-run.

Generated files pass sample bank upload checks.

**6. Audit log store**

Priority: P0

What to change: Add an append-only audit log capturing user actions (pay-run create/update, payslip adjustments, approvals) with before/after values, plus search/export APIs for compliance audits.

Where to change: Cross-cutting audit logging component, database schema for audit events, API endpoints for querying/exporting logs, and admin UI for viewing audits.

Acceptance criteria:

All key payroll actions are recorded with actor, timestamp, entity, and before/after values.

Audit records are append-only and tamper-evident.

Search/filter/export of audit logs is available via API/UI.

Logs can be correlated to pay-runs and payslip changes.

**7. Overtime rule configuration**

Priority: P1

What to change: Build an OT rule engine supporting rate multipliers by type/weekday/holiday, rounding rules, caps, and auto-calculation of OT earnings from approved hours, locking OT records for payroll once approved.

Where to change: Overtime configuration models, payroll calculation engine, approval workflow for OT records, and UI for rule setup and review.

Acceptance criteria:

OT rules can be configured for multipliers, rounding, and caps.

Approved OT hours are converted to earnings automatically per rule.

Locked OT entries cannot be modified during payroll.

UI displays applied rules and computed OT amounts.

**8. Statutory EPF/ETF reporting**

Priority: P1

What to change: Add generators for EPF/ETF summaries per pay-run/month with CSV/PDF exports, validations against statutory thresholds, and mapping of earning lines to the contributable base for Sri Lankan filings.

Where to change: Reporting services, calculation mapping for EPF/ETF bases, export templates, and admin reporting UI.

Acceptance criteria:

EPF/ETF reports produce correct per-run and monthly summaries.

CSV/PDF exports match Sri Lankan schedule formats (e.g., C, R3).

Validation flags contributions outside thresholds.

Earnings are correctly mapped to EPF/ETF contributable amounts.

**9. PAYE/APIT reporting and certificates**

Priority: P1

What to change: Extend tax engine with monthly/annual APIT reports, certificate generation, and support for reliefs/rebates compliant with Sri Lankan rules.

Where to change: Tax computation module, reporting/export services, certificate generation utilities, and payroll UI for tax documents.

Acceptance criteria:

Monthly and annual APIT reports generate with correct totals per employee.

Certificates can be generated and exported for employees.

Reliefs/rebates are configurable and reflected in calculations and reports.

Reports/export formats align with IRD filing requirements.

**10. Payslip PDF export**

Priority: P1

What to change: Enable PDF/HTML payslip generation with earning/deduction breakdowns, statutory amounts, digital signature, and email/download distribution for employees.

Where to change: Payslip presentation service, document rendering utilities, email delivery workflows, and employee self-service UI.

Acceptance criteria:

Payslips render as PDF/HTML with full breakdown and statutory details.

Digital signature or authenticity markers are included.

Employees can download or receive payslips via email securely.

Generated documents match statutory format requirements.

11. General Ledger (accounting posting)

Priority: P1

What to change: Map earnings/deductions to GL accounts and produce balanced journal entries per pay-run (debit/credit) with exports for downstream accounting systems.

Where to change: Accounting integration module, payroll posting logic, export adapters, and finance-facing UI/reports.

Acceptance criteria:

Each pay-run produces balanced journal entries tied to GL account mappings.

Earnings/deductions can be configured to specific accounts.

Exports are available in formats consumable by target accounting systems.

Finance users can review and approve postings before export.
