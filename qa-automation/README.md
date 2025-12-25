# 🏦 Sri Lanka Payroll - QA Automation Framework

[![Playwright](https://img.shields.io/badge/Framework-Playwright-2EAD33?style=for-the-badge&logo=playwright&logoColor=white)](https://playwright.dev/)
[![TypeScript](https://img.shields.io/badge/Language-TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Node.js](https://img.shields.io/badge/Runtime-Node.js-339933?style=for-the-badge&logo=node.js&logoColor=white)](https://nodejs.org/)

A robust, enterprise-grade automated testing framework designed for the **Sri Lanka Payroll System**. Built with Playwright and TypeScript, this framework ensures 100% accuracy in statutory calculations (EPF/ETF/APIT) and seamless payroll processing.

---

## 🚀 Quick Start Guide

Follow these exact steps to set up and run the automation suite on your local machine.

### 1️⃣ Prerequisites
Ensure you have the following installed:
*   **Node.js**: `v18.0.0` or higher
*   **Java (JRE/JDK)**: Required for generating Allure Reports
*   **Git**: For version control
*   **Python 3.9+** (Optional: only required for test data generation)

### 2️⃣ Installation
Clone the repository and install dependencies:

```bash
# Navigate to the automation directory
cd qa-automation

# Install Node.js dependencies
npm install

# Install Playwright Browsers
npx playwright install --with-deps
```

### 3️⃣ Environment Configuration
Create a `.env` file from the example:
```bash
cp .env.example .env
```
Update the `.env` file with your local application URL and test credentials.

---

## 🧪 Test Execution

The framework provides multiple scripts to run tests based on your needs.

| Command | Description |
|:---|:---|
| `npm run test:smoke` | 💨 Run critical smoke tests (High Priority) |
| `npm run test:regression` | 🔄 Run full regression suite |
| `npm run test:headed` | 👁️ Run tests in visible browser mode |
| `npm run test:debug` | 🐞 Open Playwright Inspector for debugging |
| `npm run test:employees` | 👤 Run Employee Management specific tests |
| `npm run test:payroll` | 💰 Run Payroll Processing specific tests |

### Running Specific Tests
```bash
# Run a specific test file
npx playwright test tests/ui/auth/login.spec.ts

# Run tests with a specific tag
npx playwright test --grep @smoke
```

---

## 📊 Reports & Analytics

After test execution, you can view detailed reports:

### 🌐 HTML Report (Built-in)
```bash
npx playwright show-report
```

### 📈 Allure Report (Advanced)
> **Note**: Requires Java to be installed on your system.
```bash
# Generate and open dashboard
npm run test:report
```

---

## 🛠️ Recent Framework Fixes

To ensure the framework runs smoothly in this environment, the following fixes were applied:

1.  **Corrected Relative Paths**: All test files now use accurate relative paths to import Page Objects (e.g., `../../../pages/LoginPage`), resolving "Module not found" errors.
2.  **Robust Locators**: `LoginPage.ts` now uses Angular-specific `formcontrolname` selectors for higher reliability during execution.
3.  **Allure CLI Integration**: The reporting script was updated to use `npx allure`, eliminating the need for a global Allure installation.
4.  **Git Optimization**: Added highly specific `.gitignore` files to exclude 700+ temporary test artifacts, keeping your repository clean and lightweight.

---

## 📂 Project Structure

```text
qa-automation/
├── tests/              # 🧪 Test specifications (.spec.ts)
│   ├── ui/             # UI Automation tests
│   └── api/            # API Automation tests
├── pages/              # 🏗️ Page Object Models (POM)
├── fixtures/           # 📦 Static test data and JSONs
├── scripts/            # 📜 Utility scripts (Data Generator)
├── utils/              # 🛠️ Helper functions and calculations
└── config/             # ⚙️ Environment configurations
```

---

## 🔢 Test Data Generation
If you need to generate a fresh set of 200+ employee records with varied Sri Lankan scenarios:

```bash
# Install Python dependencies
pip install -r requirements.txt

# Run the generator
python scripts/generate_test_data.py
```

---

## 📝 Key Deliverables Summary

*   **00-ASSUMPTIONS-AND-SCOPE.md**: Detailed breakdown of payroll compliance rules.
*   **01-TEST-STRATEGY.md**: Comprehensive testing approach and environment matrix.
*   **120+ Test Cases**: Covering Authentication, Employee Master, Statutory, and Bank Exports.
*   **Production Readiness Checklist**: Critical items to verify before go-live.

---

## 📞 Support & Contacts
For issues or framework contributions, contact the **QA Automation Team**.
- **QA Lead**: `qa.lead@company.lk`
- **Domain Expert**: `payroll.expert@company.lk`

---
*Version 1.2 | Updated 2025-12-25*
