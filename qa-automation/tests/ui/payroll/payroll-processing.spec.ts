import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pages/LoginPage';
import { PayrollPage } from '../../pages/PayrollPage';

test.describe('Payroll Processing Tests @smoke @regression', () => {
    let loginPage: LoginPage;
    let payrollPage: PayrollPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        payrollPage = new PayrollPage(page);

        // Login as Payroll Manager
        await loginPage.goto();
        await loginPage.loginAsPayrollManager();
        await payrollPage.goto();
    });

    test('TC-045: Run monthly payroll - Full month @smoke', async ({ page }) => {
        // Select payroll period
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.selectDepartment('All');

        // Calculate payroll
        await payrollPage.calculatePayroll();

        // Verify payroll summary
        const summary = await payrollPage.getPayrollSummary();

        expect(summary.totalEmployees).toBeGreaterThan(0);
        expect(summary.totalGross).toBeGreaterThan(0);
        expect(summary.totalNetPay).toBeGreaterThan(0);

        // Verify EPF calculations (8% employee, 12% employer)
        const expectedEPFEmployee = summary.totalGross * 0.08;
        const expectedEPFEmployer = summary.totalGross * 0.12;

        expect(summary.totalEPFEmployee).toBeCloseTo(expectedEPFEmployee, 0);
        expect(summary.totalEPFEmployer).toBeCloseTo(expectedEPFEmployer, 0);

        // Verify ETF calculation (3% employer)
        const expectedETF = summary.totalGross * 0.03;
        expect(summary.totalETF).toBeCloseTo(expectedETF, 0);

        // Verify Net Pay = Gross - Deductions
        const expectedNetPay = summary.totalGross - summary.totalDeductions;
        expect(summary.totalNetPay).toBeCloseTo(expectedNetPay, 0);

        // Approve payroll
        await payrollPage.approvePayroll();

        // Verify status changed to Approved
        const status = await payrollPage.getPayrollStatus();
        expect(status).toContain('Approved');
    });

    test('TC-046: Calculate payroll for specific department', async ({ page }) => {
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.selectDepartment('IT');

        await payrollPage.calculatePayroll();

        const summary = await payrollPage.getPayrollSummary();
        expect(summary.totalEmployees).toBeGreaterThan(0);

        // Verify only IT department employees are included
        // This would require checking the payroll table
    });

    test('TC-047: Verify proration for mid-month joiner', async ({ page }) => {
        // Assume we have an employee who joined on 15th Jan
        // For a 30-day month, they should get 16/30 of their salary

        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        // View payslip for mid-month joiner
        const payslipData = await payrollPage.getEmployeePayslipData('MidMonth Joiner');

        // If basic salary is 80,000 and joined on 15th (16 working days out of 30)
        const fullMonthSalary = 80000;
        const workingDays = 16;
        const totalDays = 30;
        const expectedProrated = (fullMonthSalary / totalDays) * workingDays;

        expect(payslipData.basicSalary).toBeCloseTo(expectedProrated, 0);
    });

    test('TC-048: Verify no-pay leave deduction', async ({ page }) => {
        // Employee with 5 days no-pay leave
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('NoPayLeave Employee');

        // If basic salary is 60,000 and 5 days NPL out of 22 working days
        const fullMonthSalary = 60000;
        const nplDays = 5;
        const workingDays = 22;
        const expectedSalary = fullMonthSalary - ((fullMonthSalary / workingDays) * nplDays);

        expect(payslipData.basicSalary).toBeCloseTo(expectedSalary, 0);
    });

    test('TC-049: Verify overtime calculation - Normal OT', async ({ page }) => {
        // Employee with 20 hours normal OT at 1.5x rate
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Overtime Employee');

        // If hourly rate is 500 and 20 hours OT at 1.5x
        const hourlyRate = 500;
        const otHours = 20;
        const otMultiplier = 1.5;
        const expectedOTPay = hourlyRate * otHours * otMultiplier;

        // OT should be included in allowances or shown separately
        expect(payslipData.allowances).toBeGreaterThanOrEqual(expectedOTPay);
    });

    test('TC-050: Verify loan deduction', async ({ page }) => {
        // Employee with active loan - monthly deduction 10,000
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Loan Employee');

        const expectedLoanDeduction = 10000;

        // Loan deduction should be included in total deductions
        expect(payslipData.totalDeductions).toBeGreaterThanOrEqual(expectedLoanDeduction);
    });

    test('TC-051: Re-run payroll scenario', async ({ page }) => {
        // First run
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const firstRunSummary = await payrollPage.getPayrollSummary();

        // Re-run payroll
        await payrollPage.reRunPayroll();
        await page.waitForTimeout(5000); // Wait for recalculation

        const secondRunSummary = await payrollPage.getPayrollSummary();

        // Summary should be identical (assuming no data changes)
        expect(secondRunSummary.totalGross).toBe(firstRunSummary.totalGross);
        expect(secondRunSummary.totalNetPay).toBe(firstRunSummary.totalNetPay);
    });

    test('TC-052: Month-end close scenario', async ({ page }) => {
        // Run payroll for the month
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();
        await payrollPage.approvePayroll();

        // Verify status is Approved/Closed
        const status = await payrollPage.getPayrollStatus();
        expect(status).toMatch(/Approved|Closed/i);

        // Try to edit - should not be allowed
        // This would require checking if edit button is disabled
        const editButton = page.locator('button:has-text("Edit")');
        if (await editButton.isVisible()) {
            expect(await editButton.isDisabled()).toBeTruthy();
        }
    });

    test('TC-053: Verify payroll cannot run without employees', async ({ page }) => {
        // Select a department with no employees or future month with no data
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('December');
        await payrollPage.selectYear('2026'); // Future date
        await payrollPage.calculatePayroll();

        // Should show error or zero employees
        const summary = await payrollPage.getPayrollSummary();
        expect(summary.totalEmployees).toBe(0);
    });

    test('TC-054: Verify payroll summary totals accuracy', async ({ page }) => {
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const summary = await payrollPage.getPayrollSummary();

        // Total Deductions should equal sum of all deduction components
        const calculatedDeductions =
            summary.totalEPFEmployee +
            summary.totalAPIT;
        // + other deductions

        // Allow small rounding difference
        expect(summary.totalDeductions).toBeCloseTo(calculatedDeductions, 0);

        // Net Pay should equal Gross - Deductions
        const calculatedNetPay = summary.totalGross - summary.totalDeductions;
        expect(summary.totalNetPay).toBeCloseTo(calculatedNetPay, 0);
    });
});
