import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../pages/LoginPage';
import { PayrollPage } from '../../../pages/PayrollPage';

test.describe('Statutory Compliance Tests @regression @critical', () => {
    let loginPage: LoginPage;
    let payrollPage: PayrollPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        payrollPage = new PayrollPage(page);

        await loginPage.goto();
        await loginPage.loginAsPayrollManager();
        await payrollPage.goto();
    });

    test('TC-065: Calculate APIT for high earner - Tax slab validation', async ({ page }) => {
        // Employee with monthly salary LKR 300,000
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('High Earner');

        /**
         * APIT Calculation for LKR 300,000 monthly (LKR 3,600,000 annual):
         * 
         * Tax-free: LKR 1,200,000 (LKR 0 tax)
         * Next LKR 500,000 @ 6% = LKR 30,000
         * Next LKR 500,000 @ 12% = LKR 60,000
         * Next LKR 500,000 @ 18% = LKR 90,000
         * Remaining LKR 900,000 @ 24% = LKR 216,000
         * 
         * Total Annual Tax: LKR 396,000
         * Monthly APIT: LKR 396,000 / 12 = LKR 33,000
         */

        const grossSalary = 300000;
        const expectedMonthlyAPIT = 33000;

        expect(payslipData.grossSalary).toBe(grossSalary);
        expect(payslipData.apit).toBeCloseTo(expectedMonthlyAPIT, 0);
    });

    test('TC-066: Calculate APIT at tax slab boundary - LKR 100,000/month', async ({ page }) => {
        // Employee at LKR 100,000/month (LKR 1,200,000/year - tax-free threshold)
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Threshold Employee');

        // At exactly LKR 1,200,000 annual, APIT should be 0
        expect(payslipData.grossSalary).toBe(100000);
        expect(payslipData.apit).toBe(0);
    });

    test('TC-067: Calculate APIT just above tax-free threshold', async ({ page }) => {
        // Employee at LKR 105,000/month (LKR 1,260,000/year)
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Above Threshold');

        /**
         * Annual: LKR 1,260,000
         * Tax-free: LKR 1,200,000
         * Taxable: LKR 60,000 @ 6% = LKR 3,600
         * Monthly APIT: LKR 3,600 / 12 = LKR 300
         */

        const expectedMonthlyAPIT = 300;
        expect(payslipData.apit).toBeCloseTo(expectedMonthlyAPIT, 0);
    });

    test('TC-068: Verify EPF employee contribution - 8%', async ({ page }) => {
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const summary = await payrollPage.getPayrollSummary();

        // EPF Employee should be 8% of total gross
        const expectedEPFEmployee = summary.totalGross * 0.08;
        expect(summary.totalEPFEmployee).toBeCloseTo(expectedEPFEmployee, 0);
    });

    test('TC-069: Verify EPF employer contribution - 12%', async ({ page }) => {
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const summary = await payrollPage.getPayrollSummary();

        // EPF Employer should be 12% of total gross
        const expectedEPFEmployer = summary.totalGross * 0.12;
        expect(summary.totalEPFEmployer).toBeCloseTo(expectedEPFEmployer, 0);
    });

    test('TC-070: Verify ETF employer contribution - 3%', async ({ page }) => {
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const summary = await payrollPage.getPayrollSummary();

        // ETF should be 3% of total gross
        const expectedETF = summary.totalGross * 0.03;
        expect(summary.totalETF).toBeCloseTo(expectedETF, 0);
    });

    test('TC-071: Verify EPF calculation includes all earnings', async ({ page }) => {
        // EPF should be calculated on Basic + Allowances
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Regular Employee');

        const epfBase = payslipData.basicSalary + payslipData.allowances;
        const expectedEPFEmployee = epfBase * 0.08;

        expect(payslipData.epfEmployee).toBeCloseTo(expectedEPFEmployee, 0);
    });

    test('TC-072: Generate EPF/ETF statutory report', async ({ page }) => {
        // Navigate to reports
        await page.goto('/reports/statutory');

        // Select month and generate EPF report
        await page.selectOption('select[name="month"]', 'January');
        await page.selectOption('select[name="year"]', '2025');
        await page.click('button:has-text("Generate EPF Report")');

        // Verify report is generated
        await page.waitForSelector('.report-table, [data-testid="epf-report"]');

        // Verify report contains required columns
        const reportHeaders = await page.locator('th').allTextContents();
        expect(reportHeaders).toContain('EPF Number');
        expect(reportHeaders).toContain('Employee Name');
        expect(reportHeaders).toContain('Employee Contribution');
        expect(reportHeaders).toContain('Employer Contribution');
    });

    test('TC-073: Verify APIT with dependents relief', async ({ page }) => {
        // Employee with 2 dependents (LKR 50,000 relief per dependent)
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('Employee With Dependents');

        /**
         * If gross is LKR 200,000/month (LKR 2,400,000/year)
         * Relief: LKR 50,000 × 2 = LKR 100,000
         * Taxable: LKR 2,400,000 - LKR 1,200,000 - LKR 100,000 = LKR 1,100,000
         * 
         * Tax:
         * First LKR 500,000 @ 6% = LKR 30,000
         * Next LKR 500,000 @ 12% = LKR 60,000
         * Remaining LKR 100,000 @ 18% = LKR 18,000
         * Total: LKR 108,000
         * Monthly: LKR 9,000
         */

        const expectedMonthlyAPIT = 9000;
        expect(payslipData.apit).toBeCloseTo(expectedMonthlyAPIT, 0);
    });

    test('TC-074: Verify statutory calculations for mid-month joiner', async ({ page }) => {
        // Mid-month joiner should have prorated EPF/ETF/APIT
        await payrollPage.clickRunPayroll();
        await payrollPage.selectMonth('January');
        await payrollPage.selectYear('2025');
        await payrollPage.calculatePayroll();

        const payslipData = await payrollPage.getEmployeePayslipData('MidMonth Joiner');

        // If prorated salary is 42,667 (16 days out of 30)
        const proratedGross = payslipData.grossSalary;
        const expectedEPF = proratedGross * 0.08;

        expect(payslipData.epfEmployee).toBeCloseTo(expectedEPF, 0);
    });
});
