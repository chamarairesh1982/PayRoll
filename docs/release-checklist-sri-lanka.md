You are in repo root (PayRoll).

Task: Create a new markdown file documenting the Sri Lanka Payroll release checklist.

File to create:
- docs/release-checklist-sri-lanka.md

Rules:
- Do NOT modify any existing code.
- Do NOT refactor other docs.
- Only create this one markdown file.
- Use clear headings, checklists, and consistent formatting.

Content requirements:
1) Title: "Sri Lanka Payroll – Release & UAT Checklist"
2) Short intro explaining purpose (UAT + production readiness).
3) Sections in this exact order:
   1. Environment and access
   2. Master data readiness
   3. Workflow and controls (maker-checker)
   4. Payroll calculation UAT scenarios
   5. Attendance + leave reconciliation
   6. Overtime (OT)
   7. EPF/ETF calculations and exports
   8. PAYE/APIT engine and configuration
   9. Payslip PDF/HTML
   10. Bank export
   11. GL Posting
   12. APIT reports and certificates
   13. Data + seeding verification
   14. Non-functional readiness
   15. Go-live checklist
4) Each section must use markdown checkboxes:
   - [ ] item
5) Keep wording neutral and implementation-agnostic (works for Dev/UAT/Prod).
6) Use professional, audit-friendly language suitable for HR, Finance, and IT sign-off.
7) End with a short “Sign-off” section:
   - HR Representative
   - Finance Representative
   - IT/Payroll Admin
   - Date

Populate the checklist using the following content (adapt formatting, do not paraphrase heavily):

ENVIRONMENT
- Dev/UAT/Prod environments defined
- Roles configured (Maker, Approver, Payroll Admin, Finance)
- Entity scoping tested (if enabled)
- Audit logs enabled

MASTER DATA
- Company profile complete
- Pay calendar configured
- Pay components catalog complete
- Employee records validated (NIC, salary, bank, EPF)

WORKFLOW
- Draft → Prepared → Approved → Locked works
- Unauthorized transitions blocked
- Status history visible
- Locked runs immutable

PAYROLL CALCULATION
- Base salary scenario
- Recurring allowance/deduction scenario
- Retro/adjustment handling
- Rounding consistency

ATTENDANCE + LEAVE
- Full-day unpaid leave → no-pay
- Half-day unpaid leave → 0.5 no-pay
- Paid leave → no no-pay
- Attendance absent → no-pay
- Conflict detection (present + leave)

OVERTIME
- OT rules and multipliers
- Approval workflow
- OT earnings included
- Locking after payroll

EPF/ETF
- Contributable base correctness
- Rate effective dating
- Export gating after lock
- File validation

PAYE/APIT
- Slab CRUD validation
- Relief handling
- Preview vs payslip match
- Locked immutability

PAYSLIPS
- Single & bulk PDF generation
- Content correctness
- Authenticity hash
- Secure download

BANK EXPORT
- Locked gating
- Bank validation errors
- History and regeneration
- Sample upload verification

GL POSTING
- Balanced journals
- Mapping validation
- Finance approval
- Export success

APIT REPORTS
- Monthly aggregation
- Annual aggregation
- Employee certificates
- History and regeneration

SEEDING
- Master seed
- Scenario seed
- Idempotency
- Reset protection

NON-FUNCTIONAL
- Performance test
- Concurrency safety
- Security/masking
- Logging and backup

GO-LIVE
- Production data verified
- Dry run completed
- Statutory sign-off
- Bank sign-off
- Support plan

After creating the file:
- Output only the file path created.
- Do not include explanations.
