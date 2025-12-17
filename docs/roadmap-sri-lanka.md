# Sri Lanka Payroll Delivery Plan

Prioritized implementation roadmap for Sri Lanka rollout. Items grouped by priority band with structured details per task.

## P0 — Must Have for Sri Lanka payroll MVP

### Recurring allowance/deduction engine applied in payroll service
- **Description:** Implement configurable recurring pay items that automatically attach to each payroll run for eligible employees, honoring effective dates and frequency rules.
- **Backend API changes:**
  - CRUD endpoints for recurring pay item definitions and employee assignments.
  - Payroll calculation service to hydrate runs with recurring items before gross-to-net calculations.
  - Validation to prevent overlapping effective periods and incompatible combinations.
- **Frontend screen changes:**
  - Form-based UI to create/edit recurring allowance and deduction templates.
  - Employee assignment screens with filters and bulk actions.
- **Tests required:** Unit tests for frequency/effective date logic; integration tests for payroll run ingestion; API contract tests for CRUD endpoints.
- **Dependencies:** Payroll run calculation pipeline; employee master data availability.
- **Estimated story points:** 13

### Statutory EPF/ETF reporting exports
- **Description:** Generate monthly EPF/ETF contribution statements in required statutory formats for submission.
- **Backend API changes:**
  - Reporting endpoints to produce EPF/ETF export files (CSV/TXT) for a selected period.
  - Calculation service to aggregate contributions per employee and employer.
  - Storage of generated reports with metadata for audit retrieval.
- **Frontend screen changes:**
  - Reporting screen to select period/company and trigger export downloads.
  - History view for previously generated statutory reports.
- **Tests required:** Calculation unit tests; file format validation; integration tests for download endpoints.
- **Dependencies:** Completed EPF/ETF contribution calculations; secure file storage.
- **Estimated story points:** 8

### PAYE calculation engine using configured tax slabs
- **Description:** Implement PAYE computation leveraging configurable tax slabs and reliefs applicable to Sri Lanka.
- **Backend API changes:**
  - Tax configuration endpoints for slabs, reliefs, and thresholds.
  - Payroll engine module to compute PAYE during gross-to-net, supporting mid-period changes.
  - Audit trail entries for applied tax configuration per run.
- **Frontend screen changes:**
  - Admin UI to manage tax slabs/reliefs and effective dates.
  - Visualization of applied tax on payslip preview.
- **Tests required:** Unit tests for slab calculations and edge cases; regression tests on historical scenarios; API contract tests for configuration endpoints.
- **Dependencies:** Employee tax status data; recurring pay items integrated into earnings base.
- **Estimated story points:** 13

### Approval workflow & audit logs
- **Description:** Introduce multi-level approval for payroll runs with complete auditability.
- **Backend API changes:**
  - Workflow state machine endpoints to submit, approve, reject, and rollback payroll runs.
  - Audit log service capturing user, timestamp, actions, and diffs for approvals.
  - Permissions/roles enforcement on workflow actions.
- **Frontend screen changes:**
  - Approval dashboard showing pending runs, history, and action buttons.
  - Audit log view embedded in payroll run detail.
- **Tests required:** Workflow state transition tests; authorization tests; audit log persistence checks; end-to-end approval flow tests.
- **Dependencies:** Authentication/authorization; payroll run lifecycle events.
- **Estimated story points:** 8

### Bank export file with at least one template
- **Description:** Produce bank-ready salary disbursement files conforming to at least one local bank format.
- **Backend API changes:**
  - Bank export generation endpoint with template-based formatter (e.g., CSV/fixed width).
  - Template configuration to map payroll outputs to bank fields.
  - Storage and status tracking for generated exports.
- **Frontend screen changes:**
  - Export wizard to select bank template, period, and accounts.
  - Download/history page with status and regeneration options.
- **Tests required:** File format unit tests; integration tests generating exports from payroll runs; validation for mandatory banking fields.
- **Dependencies:** Finalized payroll net pay amounts; bank template definitions; approvals gating before export.
- **Estimated story points:** 5

### Payslip PDF generation
- **Description:** Generate employee payslips as PDFs with earnings, deductions, taxes, employer contributions, and YTD figures.
- **Backend API changes:**
  - Payslip rendering service producing PDFs per employee and bulk for a payroll run.
  - Endpoint for secure payslip retrieval with authorization checks.
  - Template configuration for branding and localization.
- **Frontend screen changes:**
  - Payslip preview within payroll run detail; bulk download option.
  - Employee self-view linkage (future P2 dependency acknowledged).
- **Tests required:** Rendering snapshot tests; integration tests ensuring correct amounts/labels; security tests for access controls.
- **Dependencies:** Finalized payroll calculations; storage for generated PDFs; branding assets.
- **Estimated story points:** 8

## P1 — Next delivery

### GL posting endpoint and export
- **Description:** Enable generation of general ledger postings from payroll runs and export in accounting-friendly format.
- **Backend API changes:**
  - Endpoint to compute GL entries per chart of accounts mapping.
  - Export generator (CSV/JSON) and optional push to external accounting systems.
  - Configuration for cost centers and account mappings.
- **Frontend screen changes:**
  - GL configuration UI for account and cost center mappings.
  - Export/download interface and status tracking.
- **Tests required:** Mapping correctness unit tests; integration tests on GL entry generation; contract tests for exports.
- **Dependencies:** Approved payroll runs; chart of accounts data; cost center hierarchy.
- **Estimated story points:** 8

### Full leave integration (impact on earnings/no-pay)
- **Description:** Integrate leave balances and approvals into payroll to adjust earnings and calculate no-pay days.
- **Backend API changes:**
  - Connector to leave service for approved leave data per period.
  - Payroll engine adjustments to prorate earnings/deductions based on no-pay days.
  - Audit records for leave-driven adjustments.
- **Frontend screen changes:**
  - Payroll run detail showing leave impacts on pay.
  - Configuration screen for leave-pay rules (encashment, unpaid leave handling).
- **Tests required:** Integration tests with mocked leave service; unit tests for prorating logic; regression on edge cases (partial periods).
- **Dependencies:** Leave service availability; payroll calculation pipeline; employee calendars.
- **Estimated story points:** 13

### Multi-company/branch filters
- **Description:** Support running and viewing payroll segregated by company and branch for multi-entity groups.
- **Backend API changes:**
  - Scoping filters on payroll, reporting, and exports by company/branch identifiers.
  - Access control updates to enforce entity scoping.
- **Frontend screen changes:**
  - Filter controls on payroll lists, reports, and exports.
  - Context switcher in navigation to set active company/branch.
- **Tests required:** Authorization tests for entity scoping; filter propagation tests; regression on reporting queries.
- **Dependencies:** Entity master data; role/permission model supporting entity scopes.
- **Estimated story points:** 5

### Angular UI screens for recurring pay items config
- **Description:** Build Angular screens to manage recurring allowance/deduction templates and assignments.
- **Backend API changes:** None beyond P0 recurring items APIs (reuse).
- **Frontend screen changes:**
  - CRUD forms and list views for recurring pay items.
  - Bulk assignment actions with validation feedback.
- **Tests required:** UI unit tests for forms; e2e tests for CRUD flows; accessibility checks.
- **Dependencies:** Recurring pay item APIs from P0.
- **Estimated story points:** 5

### Angular UI screens for EPF/ETF/tax rule setup
- **Description:** Provide UI to configure statutory contributions and tax rules.
- **Backend API changes:** None beyond P0 tax/EPF/ETF config endpoints (reuse).
- **Frontend screen changes:**
  - Forms for slabs, rates, thresholds, and effective dates.
  - Validation and preview of impact before publishing.
- **Tests required:** UI validation tests; e2e scenarios for rule creation/edit; snapshot tests for rule summaries.
- **Dependencies:** Configuration endpoints for tax and statutory contributions.
- **Estimated story points:** 5

### Angular UI screens for payroll approvals
- **Description:** UI workflows for approvers to review, approve, or reject payroll runs with comments.
- **Backend API changes:** None beyond P0 workflow APIs (reuse).
- **Frontend screen changes:**
  - Approval inbox, run detail with line items, action modals, and audit history.
- **Tests required:** e2e approval flow tests; permission-based UI visibility tests.
- **Dependencies:** Approval workflow APIs; audit log endpoints.
- **Estimated story points:** 3

### Angular UI screens for bank export runner
- **Description:** Frontend to generate and download bank export files using available templates.
- **Backend API changes:** None beyond P0 bank export APIs (reuse).
- **Frontend screen changes:**
  - Export initiation form (template selection, date, accounts) and history list with statuses.
- **Tests required:** e2e export generation tests; UI validation for required fields.
- **Dependencies:** Bank export service and templates from P0.
- **Estimated story points:** 3

## P2 — Nice to have

### Employee self-service (view payslip)
- **Description:** Allow employees to securely access their own payslip PDFs.
- **Backend API changes:**
  - Authenticated endpoint scoped to employee identity for payslip retrieval.
  - Optional tokenized download links with expiry.
- **Frontend screen changes:**
  - Employee portal page listing payslips with download/view options.
- **Tests required:** Security/authorization tests; e2e employee view; accessibility checks.
- **Dependencies:** Payslip generation and storage; employee authentication.
- **Estimated story points:** 5

### HR dashboard
- **Description:** Provide HR dashboard with key payroll metrics (cost, headcount, statutory contributions, approval status).
- **Backend API changes:**
  - Aggregation endpoints for metrics and trends.
  - Caching for dashboard queries.
- **Frontend screen changes:**
  - Dashboard widgets/cards with filters by company/branch and period.
- **Tests required:** API aggregation tests; UI snapshot and interaction tests.
- **Dependencies:** Reporting data availability; multi-company filters.
- **Estimated story points:** 5

### Scheduled payroll auto-runner
- **Description:** Scheduler to auto-initiate payroll runs based on configured calendars and notify stakeholders.
- **Backend API changes:**
  - Scheduling service with cron-like configuration and job dispatcher.
  - Notifications via email/Slack/webhooks on run initiation/completion.
- **Frontend screen changes:**
  - Scheduler configuration UI and status view for upcoming/completed runs.
- **Tests required:** Scheduler job tests; integration tests for notifications; failure recovery scenarios.
- **Dependencies:** Approval workflow; calendar configuration; notification channels.
- **Estimated story points:** 8

### Email payslip dispatch
- **Description:** Email payslip PDFs to employees with secure access controls and delivery tracking.
- **Backend API changes:**
  - Email dispatch service integrating with payslip storage and templating.
  - Delivery status tracking and retry logic.
- **Frontend screen changes:**
  - Admin controls to trigger/send payslip emails and view delivery statuses.
- **Tests required:** Email sending mocks; attachment and link security tests; end-to-end dispatch flow.
- **Dependencies:** Payslip generation; employee email data; notification infrastructure.
- **Estimated story points:** 5
