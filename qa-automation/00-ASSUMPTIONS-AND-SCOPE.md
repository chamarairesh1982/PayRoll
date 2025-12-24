# Sri Lanka Payroll System - QA Assumptions & Scope Boundaries

## Document Version: 1.0
## Date: 2025-12-24
## Author: Senior QA Automation Architect

---

## 1. SYSTEM ASSUMPTIONS

### 1.1 Technical Architecture
- **Frontend**: Angular-based web application (confirmed from codebase)
- **Backend**: REST API endpoints (assumed .NET/Node.js)
- **Database**: SQL Server or PostgreSQL (assumed relational DB)
- **Authentication**: JWT-based token authentication
- **Deployment**: Cloud-hosted (Azure/AWS assumed)

### 1.2 Sri Lanka Statutory Compliance Assumptions

#### EPF (Employees' Provident Fund)
- **Employee Contribution**: 8% of gross salary
- **Employer Contribution**: 12% of gross salary
- **Ceiling**: No maximum limit (all earnings included)
- **Calculation Base**: Basic + Allowances (excluding one-time payments)

#### ETF (Employees' Trust Fund)
- **Employer Contribution**: 3% of gross salary
- **Employee Contribution**: 0%
- **Calculation Base**: Same as EPF

#### APIT (Advanced Personal Income Tax)
- **Tax Year**: April to March
- **Tax-Free Allowance**: LKR 1,200,000 per annum (LKR 100,000 per month)
- **Tax Slabs** (2024/2025 assumed):
  - First LKR 500,000: 6%
  - Next LKR 500,000: 12%
  - Next LKR 500,000: 18%
  - Above LKR 1,500,000: 24%
- **Relief**: LKR 50,000 per qualifying dependent (max 3)

#### Statutory Reporting
- **EPF/ETF Returns**: Monthly submission (C1, C2, C3 forms)
- **APIT Returns**: Monthly submission
- **Bank File Format**: Standard CEFTS or custom bank format

### 1.3 Payroll Business Rules Assumptions

#### Salary Components
- **Basic Salary**: 40-60% of gross
- **Fixed Allowances**: Transport, Mobile, Meal
- **Variable Allowances**: Overtime, Incentives, Bonuses
- **Deductions**: Loans, Advances, Welfare, Fines

#### Attendance & Leave
- **Working Days**: 22-26 days per month (configurable)
- **No-Pay Leave**: Deduct (Basic + Allowances) / Working Days × Absent Days
- **Overtime**: 1.5x for weekdays, 2x for weekends, 2.5x for holidays
- **Leave Types**: Annual, Casual, Medical, No-Pay

#### Payroll Processing
- **Frequency**: Monthly
- **Cut-off Date**: Last day of month
- **Payment Date**: 1st-5th of following month
- **Proration**: Daily basis for mid-month joiners/leavers

### 1.4 User Roles & Permissions
- **Super Admin**: Full system access
- **Payroll Manager**: Run payroll, approve, generate reports
- **HR Manager**: Employee CRUD, leave management
- **Accountant**: View reports, bank file generation
- **Employee**: View own payslip only

---

## 2. SCOPE BOUNDARIES

### 2.1 IN SCOPE

#### Modules to Test
1. ✅ **Employee Master Management**
   - CRUD operations
   - Bulk import/export
   - Employee search and filtering
   - Bank account validation

2. ✅ **Earnings & Deductions Configuration**
   - Fixed allowances setup
   - Variable earnings
   - Loan/Advance management
   - Deduction rules

3. ✅ **Payroll Processing**
   - Monthly payroll run
   - Proration calculations
   - Statutory calculations (EPF/ETF/APIT)
   - Payroll approval workflow
   - Re-run/reversal scenarios

4. ✅ **Payslip Generation**
   - Individual payslip view
   - Bulk payslip download
   - Email distribution
   - Format validation

5. ✅ **Statutory Compliance**
   - EPF/ETF calculation accuracy
   - APIT calculation with tax slabs
   - Monthly return generation
   - Form C1/C2/C3 outputs

6. ✅ **Bank File Export**
   - CEFTS format validation
   - Bank-specific formats
   - File integrity checks
   - Reconciliation

7. ✅ **Reports & Analytics**
   - Payroll summary reports
   - Department-wise cost analysis
   - Statutory reports
   - Audit trail reports

8. ✅ **Security & Access Control**
   - Role-based access
   - Audit logging
   - Session management
   - Password policies

#### Test Types
- ✅ Functional Testing (UI + API)
- ✅ Regression Testing
- ✅ Smoke Testing
- ✅ Sanity Testing
- ✅ Data Validation Testing
- ✅ Security Testing (Basic)
- ✅ Performance Smoke Testing
- ✅ Accessibility Testing (WCAG 2.1 Level A)

### 2.2 OUT OF SCOPE

#### Excluded from Current Phase
- ❌ **Advanced Performance Testing**: Load testing with 1000+ concurrent users
- ❌ **Penetration Testing**: Deep security audits (OWASP Top 10 full assessment)
- ❌ **Mobile App Testing**: If mobile apps exist
- ❌ **Integration Testing**: Third-party HR systems, biometric devices
- ❌ **Disaster Recovery Testing**: Backup/restore procedures
- ❌ **Multi-Currency Payroll**: Only LKR supported
- ❌ **International Tax Compliance**: Only Sri Lanka statutory
- ❌ **Advanced Leave Management**: Complex leave accrual rules
- ❌ **Time & Attendance Integration**: Biometric/RFID systems

---

## 3. TEST DATA ASSUMPTIONS

### 3.1 Employee Distribution (200 Records)
- **Departments**: IT (40), Finance (30), HR (20), Operations (50), Sales (40), Admin (20)
- **Salary Ranges**:
  - Entry Level: LKR 30,000 - 50,000 (60 employees)
  - Mid Level: LKR 50,001 - 100,000 (80 employees)
  - Senior Level: LKR 100,001 - 200,000 (40 employees)
  - Management: LKR 200,001 - 500,000 (20 employees)

### 3.2 Special Cases Distribution
- **Mid-Month Joiners**: 20 employees (joined 10th-25th of month)
- **Mid-Month Leavers**: 10 employees (left 5th-20th of month)
- **Overtime Cases**: 30 employees (10-50 hours OT)
- **No-Pay Leave**: 20 employees (1-10 days NPL)
- **Loan Repayments**: 20 employees (active loans)
- **Invalid Data**: 15 employees (missing bank, invalid NIC, etc.)
- **Tax Edge Cases**: 10 employees (near tax slab boundaries)
- **Missing Bank Details**: 10 employees

### 3.3 Data Generation Rules
- **NIC Format**: Old (9 digits + V) or New (12 digits)
- **Bank Accounts**: Valid Sri Lankan bank account numbers (15-18 digits)
- **EPF Numbers**: 7-digit unique numbers
- **Email**: Unique company emails
- **Phone**: Valid Sri Lankan mobile numbers (+94 7X XXX XXXX)

---

## 4. ENVIRONMENT ASSUMPTIONS

### 4.1 Test Environments
1. **DEV**: Development environment (unstable, frequent changes)
2. **QA**: Dedicated testing environment (stable for automation)
3. **UAT**: User acceptance testing (production-like)
4. **PROD**: Production (smoke tests only)

### 4.2 Test Data Management
- **Data Refresh**: QA environment reset weekly
- **Test Users**: Dedicated test accounts per role
- **Data Isolation**: Each test suite uses unique data sets
- **Cleanup**: Automated cleanup after test execution

---

## 5. ACCEPTANCE CRITERIA

### 5.1 Test Coverage Targets
- **Functional Coverage**: ≥ 85% of user stories
- **Code Coverage**: ≥ 70% (unit + integration)
- **API Coverage**: 100% of critical endpoints
- **UI Coverage**: 100% of critical user journeys

### 5.2 Quality Gates
- **Pass Rate**: ≥ 95% for smoke suite
- **Pass Rate**: ≥ 90% for regression suite
- **Defect Density**: ≤ 2 critical defects per module
- **Performance**: Page load ≤ 3 seconds
- **Accessibility**: Zero critical WCAG violations

### 5.3 Production Readiness Criteria
1. ✅ All critical test scenarios pass
2. ✅ Zero P1/P2 defects open
3. ✅ Statutory calculations validated by domain expert
4. ✅ Security scan completed (no high-severity issues)
5. ✅ Performance baseline established
6. ✅ Audit trail verified
7. ✅ Backup/restore tested
8. ✅ User acceptance sign-off received

---

## 6. RISKS & MITIGATION

### 6.1 Identified Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Statutory rule changes mid-project | High | Medium | Parameterize tax rules, quick update mechanism |
| Test data quality issues | High | Low | Automated data validation, peer review |
| Environment instability | Medium | Medium | Retry logic, environment health checks |
| Incomplete requirements | High | Low | Assumptions documented, SME validation |
| API contract changes | Medium | Medium | Contract testing, version management |
| Performance degradation | Medium | Low | Performance smoke tests in CI/CD |

### 6.2 Dependencies
- **External**: Government statutory rate updates
- **Internal**: Backend API stability, database schema finalization
- **Tools**: Playwright, Allure, CI/CD pipeline access

---

## 7. CLARIFICATION QUESTIONS (5 MAX)

### Questions for Stakeholders:
1. **Statutory Rates**: Are the assumed EPF (8%/12%), ETF (3%), and APIT slabs correct for 2024/2025?
2. **Bank File Format**: Which specific bank format(s) should be supported (CEFTS, Commercial Bank, BOC, etc.)?
3. **Payroll Frequency**: Is monthly the only frequency, or do we need to support weekly/bi-weekly?
4. **User Roles**: Are the 5 assumed roles (Super Admin, Payroll Manager, HR Manager, Accountant, Employee) complete?
5. **Performance SLA**: What are the expected response times for payroll run (e.g., 1000 employees in X minutes)?

### Assumptions if No Response:
- Use stated statutory rates with configuration flexibility
- Support generic CEFTS + 2 major bank formats
- Monthly payroll only
- Use defined 5 roles
- Performance target: Process 200 employees in < 2 minutes

---

## 8. DELIVERABLE CHECKLIST

- [x] Assumptions documented
- [x] Scope boundaries defined
- [ ] Test strategy created
- [ ] Test cases catalogued (120+ cases)
- [ ] Test data generator (200 records)
- [ ] Automation framework setup
- [ ] 40+ automated tests implemented
- [ ] Execution guide provided
- [ ] Readiness report template
- [ ] Go/No-Go recommendation

---

**Document Status**: APPROVED FOR EXECUTION
**Next Step**: Proceed to Test Strategy (Section B)
