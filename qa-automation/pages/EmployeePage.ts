import { Page, Locator } from '@playwright/test';

export interface EmployeeData {
    firstName: string;
    lastName: string;
    employeeCode: string;
    nicNumber: string;
    dateOfBirth: string;
    gender: string;
    email: string;
    mobile: string;
    address: string;
    employmentStartDate: string;
    baseSalary: number;
    epfNumber: string;
    bankCode: string;
    bankAccountNumber: string;
}

export class EmployeePage {
    readonly page: Page;
    readonly addEmployeeButton: Locator;
    readonly searchInput: Locator;
    readonly employeeTable: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;
    readonly successMessage: Locator;
    readonly errorMessage: Locator;

    constructor(page: Page) {
        this.page = page;
        this.addEmployeeButton = page.locator('button:has-text("Add"), button:has-text("New Employee"), [data-testid="add-employee"]');
        this.searchInput = page.locator('input[placeholder*="Search"], [data-testid="search-input"]');
        this.employeeTable = page.locator('table, .p-datatable, [data-testid="employee-table"]');
        this.saveButton = page.locator('button:has-text("Create Employee"), button:has-text("Save"), button[type="submit"]');
        this.cancelButton = page.locator('button:has-text("Cancel")');
        this.successMessage = page.locator('.p-toast-message-success, .success-message');
        this.errorMessage = page.locator('.p-toast-message-error, .error-message');
    }

    async goto() {
        await this.page.goto('/employees');
        await this.page.waitForLoadState('networkidle');
    }

    async clickAddEmployee() {
        await this.addEmployeeButton.click();
        await this.page.waitForSelector('form, .employee-form, [data-testid="employee-form"]');
    }

    async fillEmployeeForm(employee: EmployeeData) {
        // Identity & Profile Tab
        await this.page.click('.p-tabview-nav li:has-text("Identity")');
        await this.fillField('employeeCode', employee.employeeCode);
        await this.fillField('firstName', employee.firstName);
        await this.fillField('lastName', employee.lastName);
        await this.fillField('nicNumber', employee.nicNumber);
        await this.fillCalendar('dateOfBirth', employee.dateOfBirth);
        await this.selectDropdown('gender', employee.gender);

        // Employment Tab
        await this.page.click('.p-tabview-nav li:has-text("Employment")');
        await this.fillCalendar('employmentStartDate', employee.employmentStartDate);

        // Financials Tab
        await this.page.click('.p-tabview-nav li:has-text("Financials")');
        await this.fillInputNumber('baseSalary', employee.baseSalary.toString());
        await this.fillField('epfNumber', employee.epfNumber);

        // Use the bank name instead of code for selection if that's what's in the UI
        await this.selectDropdown('bankCode', employee.bankCode);
        // If bank branch is needed, we should add it to the interface
        if ((employee as any).branchCode) {
            await this.selectDropdown('branchCode', (employee as any).branchCode);
        }
        await this.fillField('bankAccountNumber', employee.bankAccountNumber);
    }

    private async fillField(fieldName: string, value: string) {
        const selectors = [
            `input[name="${fieldName}"]`,
            `input[formControlName="${fieldName}"]`,
            `textarea[formControlName="${fieldName}"]`,
            `input[id="${fieldName}"]`,
            `[data-testid="${fieldName}"]`
        ];

        for (const selector of selectors) {
            try {
                const field = this.page.locator(selector).first();
                if (await field.isVisible({ timeout: 1000 })) {
                    await field.fill(value);
                    return;
                }
            } catch {
                continue;
            }
        }
        console.warn(`Field ${fieldName} not found, skipping...`);
    }

    private async fillInputNumber(fieldName: string, value: string) {
        const selectors = [
            `p-inputNumber[formControlName="${fieldName}"] input`,
            `[formControlName="${fieldName}"] input`,
            `input[name="${fieldName}"]`
        ];

        for (const selector of selectors) {
            try {
                const field = this.page.locator(selector).first();
                if (await field.isVisible({ timeout: 1000 })) {
                    await field.fill(value);
                    return;
                }
            } catch {
                continue;
            }
        }
    }

    private async fillCalendar(fieldName: string, value: string) {
        const selectors = [
            `p-calendar[formControlName="${fieldName}"] input`,
            `[formControlName="${fieldName}"] input`
        ];

        for (const selector of selectors) {
            try {
                const field = this.page.locator(selector).first();
                if (await field.isVisible({ timeout: 1000 })) {
                    await field.fill(value);
                    await this.page.keyboard.press('Escape'); // Close calendar overlay
                    return;
                }
            } catch {
                continue;
            }
        }
    }

    private async selectDropdown(fieldName: string, value: string) {
        const selectors = [
            `p-dropdown[formControlName="${fieldName}"]`,
            `[formControlName="${fieldName}"]`,
            `select[name="${fieldName}"]`
        ];

        for (const selector of selectors) {
            try {
                const field = this.page.locator(selector).first();
                if (await field.isVisible({ timeout: 2000 })) {
                    // Handle native select
                    const tagName = await field.evaluate(el => el.tagName);
                    if (tagName === 'SELECT') {
                        await field.selectOption(value);
                        return;
                    }

                    // Handle PrimeNG dropdown
                    await field.click();
                    const item = this.page.locator(`.p-dropdown-item:has-text("${value}"), .p-dropdown-item >> text="${value}"`).first();
                    await item.waitFor({ state: 'visible', timeout: 2000 });
                    await item.click();
                    return;
                }
            } catch {
                continue;
            }
        }

        console.warn(`Dropdown ${fieldName} not found, skipping...`);
    }

    async saveEmployee() {
        await this.saveButton.click();
        await this.page.waitForLoadState('networkidle');
    }

    async searchEmployee(searchTerm: string) {
        await this.searchInput.fill(searchTerm);
        await this.page.waitForTimeout(1000); // Wait for debounce
    }

    async getEmployeeCount(): Promise<number> {
        const rows = await this.employeeTable.locator('tbody tr').count();
        return rows;
    }

    async isEmployeeInList(employeeName: string): Promise<boolean> {
        const row = this.employeeTable.locator(`tr:has-text("${employeeName}")`);
        return await row.count() > 0;
    }

    async clickEditEmployee(employeeName: string) {
        const row = this.employeeTable.locator(`tr:has-text("${employeeName}")`);
        await row.locator('button:has-text("Edit"), .pi-pencil').click();
    }

    async clickDeleteEmployee(employeeName: string) {
        const row = this.employeeTable.locator(`tr:has-text("${employeeName}")`);
        await row.locator('button:has-text("Delete"), .pi-trash').click();

        // Confirm deletion
        await this.page.locator('button:has-text("Yes"), button:has-text("Confirm")').click();
    }

    async getSuccessMessage(): Promise<string> {
        await this.successMessage.waitFor({ state: 'visible', timeout: 5000 });
        return await this.successMessage.textContent() || '';
    }

    async getErrorMessage(): Promise<string> {
        await this.errorMessage.waitFor({ state: 'visible', timeout: 5000 });
        return await this.errorMessage.textContent() || '';
    }

    async exportEmployees() {
        await this.page.locator('button:has-text("Export"), .pi-download').click();
    }

    async importEmployees(filePath: string) {
        await this.page.locator('button:has-text("Import"), .pi-upload').click();
        await this.page.setInputFiles('input[type="file"]', filePath);
        await this.page.locator('button:has-text("Upload")').click();
    }
}
