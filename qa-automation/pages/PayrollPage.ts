import { Page, Locator } from '@playwright/test';

export interface PayrollRunData {
    month: string;
    year: string;
    department?: string;
}

export interface PayrollSummary {
    totalEmployees: number;
    totalGross: number;
    totalDeductions: number;
    totalNetPay: number;
    totalEPFEmployee: number;
    totalEPFEmployer: number;
    totalETF: number;
    totalAPIT: number;
}

export class PayrollPage {
    readonly page: Page;
    readonly runPayrollButton: Locator;
    readonly monthDropdown: Locator;
    readonly yearDropdown: Locator;
    readonly departmentDropdown: Locator;
    readonly calculateButton: Locator;
    readonly approveButton: Locator;
    readonly payrollSummary: Locator;
    readonly payrollTable: Locator;
    readonly successMessage: Locator;

    constructor(page: Page) {
        this.page = page;
        this.runPayrollButton = page.locator('button:has-text("Run Payroll"), [data-testid="run-payroll"]');
        this.monthDropdown = page.locator('select[name="month"], p-dropdown[formControlName="month"]');
        this.yearDropdown = page.locator('select[name="year"], p-dropdown[formControlName="year"]');
        this.departmentDropdown = page.locator('select[name="department"], p-dropdown[formControlName="department"]');
        this.calculateButton = page.locator('button:has-text("Calculate"), [data-testid="calculate-payroll"]');
        this.approveButton = page.locator('button:has-text("Approve"), [data-testid="approve-payroll"]');
        this.payrollSummary = page.locator('.payroll-summary, [data-testid="payroll-summary"]');
        this.payrollTable = page.locator('table, .p-datatable');
        this.successMessage = page.locator('.p-toast-message-success');
    }

    async goto() {
        await this.page.goto('/payroll');
        await this.page.waitForLoadState('networkidle');
    }

    async clickRunPayroll() {
        await this.runPayrollButton.click();
        await this.page.waitForSelector('form, .payroll-form');
    }

    async selectMonth(month: string) {
        if (await this.monthDropdown.evaluate(el => el.tagName === 'SELECT')) {
            await this.monthDropdown.selectOption(month);
        } else {
            await this.monthDropdown.click();
            await this.page.locator(`.p-dropdown-item:has-text("${month}")`).click();
        }
    }

    async selectYear(year: string) {
        if (await this.yearDropdown.evaluate(el => el.tagName === 'SELECT')) {
            await this.yearDropdown.selectOption(year);
        } else {
            await this.yearDropdown.click();
            await this.page.locator(`.p-dropdown-item:has-text("${year}")`).click();
        }
    }

    async selectDepartment(department: string) {
        if (await this.departmentDropdown.evaluate(el => el.tagName === 'SELECT')) {
            await this.departmentDropdown.selectOption(department);
        } else {
            await this.departmentDropdown.click();
            await this.page.locator(`.p-dropdown-item:has-text("${department}")`).click();
        }
    }

    async calculatePayroll() {
        await this.calculateButton.click();

        // Wait for calculation to complete
        await this.page.waitForSelector('.payroll-summary, [data-testid="payroll-summary"]', { timeout: 120000 });
    }

    async approvePayroll() {
        await this.approveButton.click();
        await this.page.locator('button:has-text("Confirm"), button:has-text("Yes")').click();
        await this.successMessage.waitFor({ state: 'visible', timeout: 10000 });
    }

    async getPayrollSummary(): Promise<PayrollSummary> {
        const summary = this.payrollSummary;

        const getText = async (label: string): Promise<number> => {
            const text = await summary.locator(`text=${label}`).locator('..').textContent() || '0';
            return parseFloat(text.replace(/[^0-9.-]/g, ''));
        };

        return {
            totalEmployees: await getText('Total Employees'),
            totalGross: await getText('Total Gross'),
            totalDeductions: await getText('Total Deductions'),
            totalNetPay: await getText('Total Net Pay'),
            totalEPFEmployee: await getText('EPF Employee'),
            totalEPFEmployer: await getText('EPF Employer'),
            totalETF: await getText('ETF'),
            totalAPIT: await getText('APIT'),
        };
    }

    async getPayrollStatus(): Promise<string> {
        const statusBadge = this.page.locator('.status-badge, .p-tag, [data-testid="payroll-status"]');
        return await statusBadge.textContent() || '';
    }

    async viewPayslip(employeeName: string) {
        const row = this.payrollTable.locator(`tr:has-text("${employeeName}")`);
        await row.locator('button:has-text("View"), .pi-eye').click();
    }

    async downloadPayslip(employeeName: string) {
        const row = this.payrollTable.locator(`tr:has-text("${employeeName}")`);

        const [download] = await Promise.all([
            this.page.waitForEvent('download'),
            row.locator('button:has-text("Download"), .pi-download').click()
        ]);

        return download;
    }

    async exportBankFile() {
        const [download] = await Promise.all([
            this.page.waitForEvent('download'),
            this.page.locator('button:has-text("Export Bank File"), [data-testid="export-bank-file"]').click()
        ]);

        return download;
    }

    async reRunPayroll() {
        await this.page.locator('button:has-text("Re-run"), [data-testid="rerun-payroll"]').click();
        await this.page.locator('button:has-text("Confirm")').click();
    }

    async getEmployeePayslipData(employeeName: string) {
        await this.viewPayslip(employeeName);

        // Wait for payslip modal/page
        await this.page.waitForSelector('.payslip, [data-testid="payslip"]');

        const payslip = this.page.locator('.payslip, [data-testid="payslip"]');

        const getAmount = async (label: string): Promise<number> => {
            const text = await payslip.locator(`text=${label}`).locator('..').textContent() || '0';
            return parseFloat(text.replace(/[^0-9.-]/g, ''));
        };

        return {
            basicSalary: await getAmount('Basic Salary'),
            allowances: await getAmount('Allowances'),
            grossSalary: await getAmount('Gross Salary'),
            epfEmployee: await getAmount('EPF (8%)'),
            apit: await getAmount('APIT'),
            totalDeductions: await getAmount('Total Deductions'),
            netPay: await getAmount('Net Pay'),
        };
    }
}
