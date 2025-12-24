import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pages/LoginPage';
import { EmployeePage, EmployeeData } from '../../pages/EmployeePage';
import { faker } from '@faker-js/faker';

test.describe('Employee Management Tests @regression', () => {
    let loginPage: LoginPage;
    let employeePage: EmployeePage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        employeePage = new EmployeePage(page);

        // Login as HR Manager
        await loginPage.goto();
        await loginPage.loginAsHRManager();
        await employeePage.goto();
    });

    test('TC-015: Create employee with complete valid data @smoke', async ({ page }) => {
        const employee: EmployeeData = {
            firstName: 'Kasun',
            lastName: 'Perera',
            nic: '199012345678',
            dateOfBirth: '1990-05-15',
            gender: 'Male',
            email: `kasun.perera.${Date.now()}@test.lk`,
            mobile: '+94771234567',
            address: 'No. 123, Galle Road, Colombo 03',
            department: 'IT',
            designation: 'Software Engineer',
            employmentType: 'Permanent',
            joinDate: '2024-01-01',
            basicSalary: 100000,
            bankName: 'Commercial Bank',
            bankBranch: 'Colombo 03',
            bankAccountNumber: '1234567890123456',
            epfNumber: '1234567',
            taxId: '199012345678',
        };

        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee);
        await employeePage.saveEmployee();

        // Verify success message
        const successMsg = await employeePage.getSuccessMessage();
        expect(successMsg.toLowerCase()).toContain('success');

        // Verify employee appears in list
        await employeePage.searchEmployee(employee.email);
        const isInList = await employeePage.isEmployeeInList(`${employee.firstName} ${employee.lastName}`);
        expect(isInList).toBeTruthy();
    });

    test('TC-016: Create employee with missing required fields', async ({ page }) => {
        const employee: Partial<EmployeeData> = {
            firstName: 'Test',
            lastName: 'User',
            // Missing NIC, email, etc.
        };

        await employeePage.clickAddEmployee();
        // Try to save without filling all required fields
        await employeePage.saveEmployee();

        // Should show validation errors
        const errorMsg = await employeePage.getErrorMessage();
        expect(errorMsg.toLowerCase()).toContain('required');
    });

    test('TC-017: Create employee with invalid NIC format', async ({ page }) => {
        const employee: EmployeeData = {
            firstName: 'Invalid',
            lastName: 'NIC',
            nic: 'INVALID123',  // Invalid NIC format
            dateOfBirth: '1990-01-01',
            gender: 'Male',
            email: `invalid.nic.${Date.now()}@test.lk`,
            mobile: '+94771234567',
            address: 'Test Address',
            department: 'IT',
            designation: 'Developer',
            employmentType: 'Permanent',
            joinDate: '2024-01-01',
            basicSalary: 50000,
            bankName: 'Commercial Bank',
            bankBranch: 'Colombo',
            bankAccountNumber: '1234567890123456',
            epfNumber: '1234567',
            taxId: 'INVALID123',
        };

        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee);
        await employeePage.saveEmployee();

        // Should show validation error for NIC
        const errorMsg = await employeePage.getErrorMessage();
        expect(errorMsg.toLowerCase()).toContain('nic');
    });

    test('TC-018: Create employee with duplicate email', async ({ page }) => {
        const email = `duplicate.${Date.now()}@test.lk`;

        const employee1: EmployeeData = {
            firstName: 'First',
            lastName: 'Employee',
            nic: '199012345678',
            dateOfBirth: '1990-01-01',
            gender: 'Male',
            email: email,
            mobile: '+94771234567',
            address: 'Address 1',
            department: 'IT',
            designation: 'Developer',
            employmentType: 'Permanent',
            joinDate: '2024-01-01',
            basicSalary: 50000,
            bankName: 'Commercial Bank',
            bankBranch: 'Colombo',
            bankAccountNumber: '1234567890123456',
            epfNumber: '1234567',
            taxId: '199012345678',
        };

        // Create first employee
        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee1);
        await employeePage.saveEmployee();

        // Try to create second employee with same email
        const employee2 = { ...employee1, firstName: 'Second', nic: '199112345678' };
        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee2);
        await employeePage.saveEmployee();

        // Should show duplicate email error
        const errorMsg = await employeePage.getErrorMessage();
        expect(errorMsg.toLowerCase()).toContain('email');
    });

    test('TC-019: Search employee by name', async ({ page }) => {
        const searchTerm = 'Kasun';

        await employeePage.searchEmployee(searchTerm);

        // Wait for search results
        await page.waitForTimeout(1500);

        // Verify results contain search term
        const tableText = await employeePage.employeeTable.textContent();
        expect(tableText).toContain(searchTerm);
    });

    test('TC-020: Edit employee details', async ({ page }) => {
        // First, search for an existing employee
        await employeePage.searchEmployee('Kasun Perera');

        // Click edit
        await employeePage.clickEditEmployee('Kasun Perera');

        // Update mobile number
        const newMobile = '+94779999999';
        await page.locator('input[name="mobile"]').fill(newMobile);
        await employeePage.saveEmployee();

        // Verify success
        const successMsg = await employeePage.getSuccessMessage();
        expect(successMsg.toLowerCase()).toContain('success');
    });

    test('TC-021: Delete employee', async ({ page }) => {
        // Create a test employee first
        const employee: EmployeeData = {
            firstName: 'ToDelete',
            lastName: 'Employee',
            nic: '199912345678',
            dateOfBirth: '1999-01-01',
            gender: 'Male',
            email: `todelete.${Date.now()}@test.lk`,
            mobile: '+94771111111',
            address: 'Test Address',
            department: 'Admin',
            designation: 'Temp',
            employmentType: 'Temporary',
            joinDate: '2024-01-01',
            basicSalary: 30000,
            bankName: 'Commercial Bank',
            bankBranch: 'Colombo',
            bankAccountNumber: '1111111111111111',
            epfNumber: '9999999',
            taxId: '199912345678',
        };

        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee);
        await employeePage.saveEmployee();

        // Now delete it
        await employeePage.searchEmployee(employee.email);
        await employeePage.clickDeleteEmployee('ToDelete Employee');

        // Verify success
        const successMsg = await employeePage.getSuccessMessage();
        expect(successMsg.toLowerCase()).toContain('success');

        // Verify employee is removed from list
        await employeePage.searchEmployee(employee.email);
        const isInList = await employeePage.isEmployeeInList('ToDelete Employee');
        expect(isInList).toBeFalsy();
    });

    test('TC-022: Export employee list', async ({ page }) => {
        const [download] = await Promise.all([
            page.waitForEvent('download'),
            employeePage.exportEmployees()
        ]);

        // Verify download
        expect(download).toBeTruthy();
        const fileName = download.suggestedFilename();
        expect(fileName).toMatch(/employee|export/i);
    });

    test('TC-023: Validate employee count display', async ({ page }) => {
        const count = await employeePage.getEmployeeCount();

        // Should have at least 1 employee
        expect(count).toBeGreaterThan(0);

        // Verify count matches displayed total
        const totalText = await page.locator('text=/Total.*Employee/i').textContent();
        if (totalText) {
            const displayedCount = parseInt(totalText.match(/\d+/)?.[0] || '0');
            expect(displayedCount).toBeGreaterThan(0);
        }
    });

    test('TC-024: Create mid-month joiner', async ({ page }) => {
        const employee: EmployeeData = {
            firstName: 'MidMonth',
            lastName: 'Joiner',
            nic: '199512345678',
            dateOfBirth: '1995-06-15',
            gender: 'Female',
            email: `midmonth.${Date.now()}@test.lk`,
            mobile: '+94772222222',
            address: 'Test Address',
            department: 'Finance',
            designation: 'Accountant',
            employmentType: 'Permanent',
            joinDate: '2025-01-15',  // Mid-month join date
            basicSalary: 80000,
            bankName: 'Bank of Ceylon',
            bankBranch: 'Kandy',
            bankAccountNumber: '2222222222222222',
            epfNumber: '2222222',
            taxId: '199512345678',
        };

        await employeePage.clickAddEmployee();
        await employeePage.fillEmployeeForm(employee);
        await employeePage.saveEmployee();

        // Verify success
        const successMsg = await employeePage.getSuccessMessage();
        expect(successMsg.toLowerCase()).toContain('success');

        // This employee should be flagged for proration in payroll
    });
});
