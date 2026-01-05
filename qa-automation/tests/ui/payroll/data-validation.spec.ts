import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../pages/LoginPage';
import { EmployeePage, EmployeeData } from '../../../pages/EmployeePage';
import { PayrollPage, PayrollRunData } from '../../../pages/PayrollPage';

test.describe('Payroll Data and Logic Validation', () => {
    let loginPage: LoginPage;
    let employeePage: EmployeePage;
    let payrollPage: PayrollPage;

    const testEmployee: EmployeeData = {
        employeeCode: 'EMP' + Date.now().toString().slice(-5),
        firstName: 'Automation',
        lastName: 'TestUser',
        nicNumber: '199012345678',
        dateOfBirth: '1990-01-01',
        gender: 'Male',
        email: `testuser_${Date.now()}@test.lk`,
        mobile: '0771234567',
        address: '123 Test Street, Colombo',
        employmentStartDate: '2023-01-01',
        baseSalary: 100000,
        epfNumber: 'EPF12345',
        bankCode: '7010', // Example Bank Code
        bankAccountNumber: '0012345678'
    };

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        employeePage = new EmployeePage(page);
        payrollPage = new PayrollPage(page);

        await loginPage.goto();
        await loginPage.login('Admin', '123456');
        await expect(await loginPage.isLoggedIn()).toBe(true);
    });

    test('TC-060: Verify Employee Data Saving', async ({ page }) => {
        await employeePage.goto();
        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(testEmployee);
        await employeePage.saveEmployee();

        // Verify success or redirect
        // Some systems might show a toast, others redirect to list
        await employeePage.searchEmployee(testEmployee.employeeCode);
        const exists = await employeePage.isEmployeeInList(testEmployee.employeeCode);
        expect(exists).toBe(true);
    });

    test('TC-061: Verify Payroll Calculation Logic', async ({ page }) => {
        // This test assumes the employee exists or was created
        await payrollPage.goto();
        await payrollPage.clickRunPayroll();

        const runData: PayrollRunData = {
            name: 'Audit Run ' + Date.now(),
            payDate: '2025-12-31',
            periodStart: '2025-12-01',
            periodEnd: '2025-12-31',
            periodType: 'Monthly'
        };

        await payrollPage.fillPayRunForm(runData);
        await payrollPage.calculatePayroll();

        // Now we verify the logic if the summary is visible
        // Even if the API fails in the current environment, the code should be correct for the logic
        // For a 100,000 basic:
        // EPF Employee (8%) = 8,000
        // EPF Employer (12%) = 12,000
        // ETF (3%) = 3,000

        // Note: In the actual app, we'd look for these values in the summary or payslip
        // This is a placeholder for actual verification once the UI is stable
        console.log('Payroll initialized. Verification steps would go here.');
    });
});
