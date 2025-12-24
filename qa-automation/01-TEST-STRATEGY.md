# Sri Lanka Payroll System - Test Strategy

## Document Version: 1.0
## Date: 2025-12-24
## Owner: QA Automation Team

---

## 1. EXECUTIVE SUMMARY

This test strategy defines the comprehensive approach for validating the Sri Lanka Payroll system for production readiness. The strategy encompasses automated UI testing, API validation, database checks, and statutory compliance verification across 8 core modules with 200 test data records and 120+ test scenarios.

**Key Objectives**:
- Achieve 95%+ pass rate for critical business flows
- Validate 100% statutory calculation accuracy
- Ensure zero P1/P2 defects at release
- Establish repeatable automation framework for CI/CD

---

## 2. TEST LEVELS

### 2.1 Unit Testing (Out of Scope for QA)
- **Owner**: Development Team
- **Coverage**: Individual functions, components
- **Target**: 80% code coverage
- **Tools**: Jest (Frontend), xUnit/NUnit (Backend)

### 2.2 Integration Testing
- **Owner**: QA Team
- **Scope**: API contract validation, service integration
- **Coverage**: All REST endpoints, database transactions
- **Tools**: Playwright API Testing, Postman/Newman
- **Focus Areas**:
  - Employee service ↔ Payroll service integration
  - Payroll engine ↔ Statutory calculation service
  - Report service ↔ Database queries
  - Authentication service ↔ All modules

### 2.3 System Testing (Primary Focus)
- **Owner**: QA Team
- **Scope**: End-to-end business workflows
- **Coverage**: All 8 modules, 120+ test cases
- **Tools**: Playwright (UI), API tests, SQL validation
- **Test Types**:
  - Functional testing
  - Regression testing
  - Smoke testing
  - Sanity testing
  - Security testing (basic)
  - Performance smoke testing

### 2.4 User Acceptance Testing (UAT)
- **Owner**: Business Users + QA Support
- **Scope**: Business validation, usability
- **Duration**: 2 weeks
- **Participants**: Payroll Manager, HR Manager, Accountant

---

## 3. TEST TYPES & COVERAGE

### 3.1 Functional Testing

#### 3.1.1 Positive Testing
- **Coverage**: Happy path scenarios for all features
- **Examples**:
  - Create employee with valid data
  - Run payroll for full month
  - Generate payslip successfully
  - Export bank file with correct format

#### 3.1.2 Negative Testing
- **Coverage**: Invalid inputs, boundary conditions
- **Examples**:
  - Create employee with missing required fields
  - Run payroll with no employees
  - Generate payslip for non-existent employee
  - Export bank file with invalid date range

#### 3.1.3 Boundary Testing
- **Coverage**: Edge cases, limits
- **Examples**:
  - Salary at tax slab boundaries (LKR 500,000, 1,000,000)
  - Maximum overtime hours (100 hours/month)
  - Minimum salary (LKR 10,000)
  - Date boundaries (month-end, year-end)

### 3.2 Regression Testing
- **Trigger**: Every code deployment
- **Suite Size**: 80+ test cases
- **Execution Time**: < 45 minutes
- **Automation**: 100%
- **Coverage**:
  - Core payroll calculations
  - Statutory compliance
  - Critical user journeys
  - Previously fixed defects

### 3.3 Smoke Testing
- **Trigger**: Every build
- **Suite Size**: 20 test cases
- **Execution Time**: < 10 minutes
- **Automation**: 100%
- **Coverage**:
  - Login functionality
  - Employee CRUD basics
  - Payroll run (simple case)
  - Report generation
  - Critical API health checks

### 3.4 Sanity Testing
- **Trigger**: After bug fixes
- **Suite Size**: 15-20 test cases
- **Execution Time**: < 15 minutes
- **Focus**: Affected modules only

### 3.5 Security Testing

#### 3.5.1 Authentication & Authorization
- **Test Cases**: 15 cases
- **Coverage**:
  - Login with valid/invalid credentials
  - Session timeout validation
  - Role-based access control (RBAC)
  - Password complexity rules
  - Brute force protection

#### 3.5.2 Data Security
- **Test Cases**: 10 cases
- **Coverage**:
  - SQL injection attempts
  - XSS prevention
  - Sensitive data masking (salary, bank accounts)
  - Audit trail completeness

### 3.6 Performance Testing

#### 3.6.1 Performance Smoke Tests
- **Scope**: Response time validation
- **Test Cases**: 8 cases
- **Metrics**:
  - Page load time: < 3 seconds
  - API response time: < 500ms
  - Payroll run (200 employees): < 2 minutes
  - Report generation: < 10 seconds

#### 3.6.2 Load Testing (Future Phase)
- **Scope**: Concurrent user simulation
- **Target**: 50 concurrent users
- **Tools**: JMeter/K6

### 3.7 Accessibility Testing
- **Standard**: WCAG 2.1 Level A
- **Tool**: Axe DevTools, Playwright Accessibility
- **Coverage**: All user-facing pages
- **Test Cases**: 12 cases
- **Focus**:
  - Keyboard navigation
  - Screen reader compatibility
  - Color contrast
  - Form labels

### 3.8 Compatibility Testing
- **Browsers**: Chrome (latest), Edge (latest), Firefox (latest)
- **Resolutions**: 1920x1080, 1366x768, 1280x1024
- **OS**: Windows 10/11 (primary)

---

## 4. TEST ENVIRONMENTS

### 4.1 Environment Matrix

| Environment | Purpose | Stability | Data Refresh | Access |
|-------------|---------|-----------|--------------|--------|
| **DEV** | Development testing | Low | On-demand | Dev + QA |
| **QA** | Automated testing | High | Weekly | QA Only |
| **UAT** | Business validation | High | Bi-weekly | Business + QA |
| **STAGING** | Pre-production | Very High | Monthly | QA + Ops |
| **PROD** | Production | Critical | N/A | Smoke only |

### 4.2 Environment Configuration

#### QA Environment (Primary Automation Target)
```
Frontend URL: https://qa-payroll.company.lk
API Base URL: https://qa-api-payroll.company.lk/api/v1
Database: SQL Server (qa-payroll-db.company.lk)
Test Users:
  - admin@test.lk (Super Admin)
  - payroll.mgr@test.lk (Payroll Manager)
  - hr.mgr@test.lk (HR Manager)
  - accountant@test.lk (Accountant)
  - employee@test.lk (Employee)
```

### 4.3 Data Management Strategy
- **Test Data Isolation**: Each test suite uses unique employee IDs (prefix: AUTO_)
- **Data Cleanup**: Automated cleanup after test execution
- **Data Seeding**: Pre-seeded master data (departments, banks, tax rules)
- **Data Refresh**: Weekly full refresh from production-like dataset

---

## 5. ENTRY & EXIT CRITERIA

### 5.1 Entry Criteria (Test Execution)

#### For Smoke Testing
- ✅ Build deployed successfully to target environment
- ✅ Environment health check passed
- ✅ Database migrations completed
- ✅ Test data seeded

#### For Regression Testing
- ✅ All smoke tests passed
- ✅ Code review completed
- ✅ Unit test pass rate ≥ 90%
- ✅ No P1 defects open in target build

#### For UAT
- ✅ All regression tests passed (≥ 90%)
- ✅ Test summary report reviewed
- ✅ Known issues documented
- ✅ UAT environment stable

### 5.2 Exit Criteria

#### For Test Cycle Completion
- ✅ All planned test cases executed
- ✅ Pass rate ≥ 90% (regression), ≥ 95% (smoke)
- ✅ All P1/P2 defects resolved or deferred with approval
- ✅ Test execution report published
- ✅ Defect metrics within acceptable limits

#### For Production Release (Go/No-Go)
- ✅ UAT sign-off received
- ✅ Zero P1 defects open
- ✅ P2 defects ≤ 2 (with workarounds)
- ✅ Statutory calculations validated by domain expert
- ✅ Security scan completed (no high-severity issues)
- ✅ Performance benchmarks met
- ✅ Audit trail verified
- ✅ Rollback plan documented

---

## 6. TEST AUTOMATION APPROACH

### 6.1 Automation Framework Architecture

```
qa-automation/
├── tests/
│   ├── ui/                    # Playwright UI tests
│   │   ├── auth/
│   │   ├── employees/
│   │   ├── payroll/
│   │   ├── reports/
│   │   └── admin/
│   ├── api/                   # API tests
│   │   ├── employees/
│   │   ├── payroll/
│   │   └── statutory/
│   └── e2e/                   # End-to-end scenarios
├── pages/                     # Page Object Models
├── fixtures/                  # Test data & fixtures
├── utils/                     # Helper functions
├── config/                    # Environment configs
└── reports/                   # Test execution reports
```

### 6.2 Tooling Stack

| Layer | Tool | Purpose |
|-------|------|---------|
| **UI Automation** | Playwright | Browser automation, cross-browser testing |
| **API Testing** | Playwright API / Newman | REST API validation |
| **Test Runner** | Playwright Test | Test execution, parallel runs |
| **Reporting** | Allure / Playwright HTML | Test reports, screenshots, videos |
| **CI/CD** | GitHub Actions | Automated test execution |
| **Data Generation** | Python (Faker) | Test data generation |
| **DB Validation** | SQL queries via Playwright | Database assertions |
| **Accessibility** | Axe-core | WCAG compliance checks |

### 6.3 Automation Principles

#### Test Design Patterns
- **Page Object Model (POM)**: Separate page logic from test logic
- **Data-Driven Testing**: Parameterized tests with external data
- **Keyword-Driven**: Reusable action keywords
- **Hybrid Approach**: Combine POM + Data-Driven + Keywords

#### Best Practices
1. **Stable Selectors**: Use data-testid attributes (recommended to dev team)
2. **Explicit Waits**: Avoid hard-coded sleeps, use smart waits
3. **Independent Tests**: Each test can run standalone
4. **Idempotent Tests**: Tests can be re-run without side effects
5. **Meaningful Assertions**: Clear expected vs actual messages
6. **Screenshot on Failure**: Auto-capture for debugging
7. **Video Recording**: For critical flows (optional)
8. **Retry Logic**: Auto-retry flaky tests (max 2 retries)

### 6.4 Test Data Strategy

#### Test Data Categories
1. **Static Data**: Master data (departments, banks, tax rules)
2. **Dynamic Data**: Generated per test run (employees, payrolls)
3. **Negative Data**: Invalid/edge case data
4. **Performance Data**: Large datasets (200+ employees)

#### Data Generation Approach
- **Tool**: Python script with Faker library
- **Output**: JSON/CSV files
- **Seeding**: API-based data seeding before test execution
- **Cleanup**: API-based cleanup after test execution

---

## 7. TEST EXECUTION STRATEGY

### 7.1 Execution Modes

#### Local Execution
```bash
# Run all tests
npm run test

# Run smoke suite
npm run test:smoke

# Run specific module
npm run test:employees

# Run in headed mode (debug)
npm run test:debug
```

#### CI/CD Execution
- **Trigger**: On every pull request + merge to main
- **Parallel Execution**: 4 workers
- **Retry**: Failed tests retry once
- **Artifacts**: Screenshots, videos, HTML report

### 7.2 Test Scheduling

| Suite | Frequency | Duration | Trigger |
|-------|-----------|----------|---------|
| **Smoke** | Every commit | 10 min | CI/CD |
| **Regression** | Daily (nightly) | 45 min | Scheduled |
| **Full Suite** | Weekly | 90 min | Scheduled |
| **Performance** | Weekly | 30 min | Scheduled |
| **Security** | Bi-weekly | 20 min | Manual |

### 7.3 Defect Management

#### Defect Severity Classification
- **P1 (Critical)**: System crash, data loss, security breach, payroll calculation error
- **P2 (High)**: Major feature broken, workaround exists
- **P3 (Medium)**: Minor feature issue, cosmetic with functional impact
- **P4 (Low)**: Cosmetic, typos, minor UI issues

#### Defect Workflow
1. Test fails → Screenshot captured
2. QA logs defect in Azure DevOps / Jira
3. Dev fixes → Code review → Merge
4. QA re-tests → Closes defect or reopens

---

## 8. RISK MANAGEMENT

### 8.1 Testing Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Environment instability** | High | Health checks, retry logic, backup environment |
| **Test data corruption** | Medium | Data isolation, automated cleanup, version control |
| **Flaky tests** | Medium | Stable selectors, explicit waits, retry mechanism |
| **Incomplete requirements** | High | Assumptions documented, SME validation |
| **Statutory rule changes** | High | Parameterized rules, quick update process |
| **Resource unavailability** | Medium | Cross-training, documentation |

### 8.2 Quality Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Incorrect statutory calculations** | Critical | Domain expert validation, parallel calculation verification |
| **Data security breach** | Critical | Security testing, penetration testing (future) |
| **Performance degradation** | High | Performance smoke tests, load testing (future) |
| **Audit trail gaps** | High | Comprehensive audit log validation |

---

## 9. ROLES & RESPONSIBILITIES

| Role | Responsibilities |
|------|------------------|
| **QA Lead** | Test strategy, planning, reporting, stakeholder communication |
| **QA Automation Engineer** | Framework development, test automation, CI/CD integration |
| **QA Manual Tester** | Exploratory testing, UAT support, defect verification |
| **Payroll Domain Expert** | Statutory validation, business rule verification |
| **DevOps Engineer** | Environment setup, CI/CD pipeline, monitoring |
| **Development Team** | Unit tests, bug fixes, testability improvements |

---

## 10. DELIVERABLES & TIMELINE

### 10.1 Test Artifacts

| Artifact | Owner | Delivery |
|----------|-------|----------|
| Test Strategy | QA Lead | Week 1 |
| Test Cases (120+) | QA Team | Week 2 |
| Test Data (200 records) | QA Automation | Week 2 |
| Automation Framework | QA Automation | Week 3 |
| Automated Tests (40+) | QA Automation | Week 4 |
| Test Execution Report | QA Team | Weekly |
| Readiness Report | QA Lead | Pre-release |
| Go/No-Go Recommendation | QA Lead | Release day |

### 10.2 Milestones

- **Week 1**: Strategy finalized, test cases designed
- **Week 2**: Test data generated, framework setup
- **Week 3**: 50% automation complete
- **Week 4**: 100% automation complete, regression suite ready
- **Week 5**: UAT support, final validation
- **Week 6**: Production readiness assessment

---

## 11. SUCCESS METRICS

### 11.1 Test Execution Metrics
- **Test Coverage**: ≥ 85% of user stories
- **Automation Coverage**: ≥ 70% of regression tests
- **Pass Rate**: ≥ 95% (smoke), ≥ 90% (regression)
- **Defect Detection Rate**: ≥ 80% defects found in QA
- **Defect Leakage**: < 10% defects found in UAT/PROD

### 11.2 Quality Metrics
- **Defect Density**: ≤ 2 defects per module
- **Critical Defects**: Zero at release
- **Test Execution Time**: Regression < 45 min
- **Mean Time to Detect (MTTD)**: < 24 hours
- **Mean Time to Resolve (MTTR)**: < 48 hours (P1), < 5 days (P2)

### 11.3 Automation Metrics
- **Automation ROI**: Time saved vs manual execution
- **Test Stability**: < 5% flaky test rate
- **Maintenance Effort**: < 10% time on test maintenance

---

## 12. CONTINUOUS IMPROVEMENT

### 12.1 Retrospective Activities
- **Weekly**: Test execution review, flaky test analysis
- **Bi-weekly**: Automation framework improvements
- **Monthly**: Test strategy review, metrics analysis
- **Quarterly**: Tool evaluation, process optimization

### 12.2 Knowledge Management
- **Documentation**: Confluence/Wiki for test approach
- **Training**: Onboarding for new QA team members
- **Best Practices**: Shared coding standards, review checklist

---

**Document Status**: APPROVED
**Next Step**: Proceed to Test Case Catalogue (Section C)
