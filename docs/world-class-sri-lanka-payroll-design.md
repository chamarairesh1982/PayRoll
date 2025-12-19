# World-Class Sri Lanka Payroll – Product & Architecture Design

This document defines the feature set, domain model, workflows, compliance, and technical architecture to build the best payroll system for Sri Lanka, while staying extensible for other countries.

## Product principles
- **Compliance-first**: statutory correctness, auditability, and traceable calculations.
- **Maker-checker by default**: no silent edits after approval/lock.
- **Explainable payroll**: every amount is drill-down traceable to rules + source data.
- **Multi-entity ready**: company/branch/cost center segmentation and consolidated outputs.
- **Extensible rules**: configurable, effective-dated, versioned rule engine.
- **Automation + safety**: imports, validations, reconciliation, and safe retries (idempotent).
- **Great UX**: guided workflows, previews, and discrepancy alerts before finalization.

---

## 1) Core payroll workflow (PayRun lifecycle)
### States
- Draft → Prepared → Approved → Locked → (Exported) → (Posted)
Optional: Reopened (controlled) for corrections.

### Roles
- Maker: create/edit Draft, Prepare
- Approver: Approve/Reject
- Payroll Admin: Lock/Unlock (special permission), manage configs
- Finance: export bank/GL posting approvals

### Rules
- No pay calculations are “final” until **Locked**
- **Bank export, Payslip PDFs, GL postings** require Locked
- Reopen creates a new “revision” (no destructive edits), preserves full audit trail.

### Outputs
- PayRun summary
- Employee payslips
- Statutory reports
- Bank export
- GL journal entries

---

## 2) Sri Lanka statutory engine (EPF/ETF/PAYE/APIT)
### EPF/ETF
- Configurable rates (employee/employer), effective-dated
- Contributable base mapping by pay component
- Threshold validation + warning flags
- Monthly export formats + summaries

### PAYE/APIT
- Slab-based tax engine with effective-dated slabs
- Reliefs/rebates + tax-free allowances
- Mid-year changes supported (effective dates)
- Monthly and annual reports + employee certificates

### Statutory traceability
Each statutory number must be traceable:
- contributable earnings
- taxable earnings
- applied slab/rate version
- adjustments (retro, proration)

---

## 3) Earnings & deductions catalog (rule-driven)
### Pay components
- Earnings: Basic, Allowances, OT, Bonus, Commission, Encashment
- Deductions: No-pay, advances, loans, welfare, PAYE
- Contributions: EPF/ETF employer/employee

### Component metadata
- taxable? contributable (EPF/ETF)? included in OT base? included in daily rate base?
- effective date ranges
- GL mapping

---

## 4) Recurring items engine (best practice)
### Rules
- frequency: monthly/weekly/biweekly/one-off
- start/end dates + pause/resume
- proration (join/leave mid-period)
- eligibility: employee group, grade, branch, contract type
- idempotent application per (ruleId, employeeId, payPeriod)

### Retro adjustments
When rule changes effective in past period:
- generate retro delta lines in next run (with link to original period)

### UX
- rule list, templates, assignments
- preview impact for selected employees + period
- conflict detection (overlaps, duplicates)

---

## 5) Attendance + Leave integration (no-pay accuracy)
### Inputs
- attendance: in/out, shifts, late, absent, half-day
- leave: approved leave types (paid/unpaid/half-day)

### Reconciliation logic
- single source of truth per day: attendance vs leave
- no double deduction
- configurable precedence rules (leave overrides attendance absent if approved)

### Outputs
- no-pay deduction lines with day-level trace
- leave encashment lines
- audit drill-down for each day

---

## 6) Overtime (OT) engine
### OT rules
- weekday/weekend/holiday multipliers
- rounding rules (e.g., 15-min blocks)
- caps (daily/monthly)
- approval workflow for OT entries

### Calculation
- derive OT pay from hourly rate or configured base
- lock OT entries once payrun is locked

---

## 7) Bank export engine (Sri Lanka templates)
### Templates
- HNB / BOC / Commercial / Sampath / DFCC (expandable)
- CSV/TXT/fixed-width support
- bank-specific validations
- per-run export status tracking: Pending → Generated → Downloaded → Submitted

### Enhancements
- beneficiary verification checks (account number length, bank codes)
- “failed rows” report downloadable
- re-generation produces new version, preserves previous versions

---

## 8) Payslip generation & distribution
### PDF/HTML
- company branding + bilingual support (EN/SI optional)
- breakdown: earnings, deductions, statutory, YTD totals
- authenticity: signature hash + generated timestamp

### Distribution
- employee self-service portal (P2)
- secure email links (expiring tokens)
- bulk download for HR/Admin

---

## 9) General Ledger posting
### Capabilities
- configurable COA mapping per component
- cost center split support
- balanced journals (debit/credit)
- export formats (CSV, JSON)
- posting approval workflow (Finance)

---

## 10) Audit, compliance, and security (world-class)
### Audit log (append-only, tamper-evident)
- actor, timestamp, entity, action, before/after JSON
- hash chain per entity stream for tamper-evidence
- export for compliance audits

### Permissions model
- RBAC + entity scope (company/branch/cost center)
- field-level masking for sensitive data (NIC, bank)
- segregation of duties (maker != approver)

### Data integrity
- optimistic concurrency on payrun/payslip edits
- idempotency keys for generation endpoints
- immutable “locked” records; changes via revisions only

---

## 11) Reporting & analytics
### Standard reports
- payroll register, bank register, statutory summaries
- cost analysis by branch/cost center
- employee earnings history
- variance report: current vs previous run

### Best-in-class extras
- anomaly detection alerts (spikes, missing bank, negative net pay)
- reconciliation report (attendance vs leave vs no-pay)

---

## 12) Architecture & technical design
### Suggested modules
- Payroll.Domain (entities, rules, invariants)
- Payroll.Application (use cases, workflows)
- Payroll.Infrastructure (EF, integrations)
- Payroll.Api (controllers)
- Payroll.Seeder (master + scenario seeds)
- Frontend Angular modules:
  - Payroll Runs
  - Config (Tax, EPF/ETF, Components)
  - Approvals
  - Exports
  - Reports
  - Audit

### Design patterns
- Effective-dated configuration tables
- Event-like history tables (status changes, adjustments)
- Calculation explanation model (line-level provenance)

---

## 13) MVP scope recommendation (Sri Lanka launch)
### P0 must-have
- PayRun workflow + approvals + audit
- Earnings/deductions catalog
- Recurring items engine (CRUD + apply + preview)
- EPF/ETF calc + export
- PAYE calc (slabs) + payslip
- Bank export (at least one template)
- Payslip PDF export
- Master data + seeding scripts

### P1
- Leave & attendance full reconciliation
- OT engine
- GL posting
- Multi-company/branch/cost center advanced consolidation
- APIT certificates & annual reports

### P2
- Employee self-service
- Scheduler auto-runs + notifications
- Email dispatch + tracking
- Analytics dashboard

---

## 14) Implementation approach (safe incremental)
1. Lock down domain invariants (workflow, immutable locked runs)
2. Build effective-dated config framework (tax + statutory + components)
3. Implement payroll calculation with explanation output
4. Add exports (bank, statutory) gated by Locked
5. Add payslip PDF and audit tamper-evidence
6. Add OT + leave/attendance reconciliation
7. Add GL posting and advanced reporting
