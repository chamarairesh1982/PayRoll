import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../pages/LoginPage';

test.describe('Authentication Tests @smoke @regression', () => {
    let loginPage: LoginPage;

    test.beforeEach(async ({ page }) => {
        loginPage = new LoginPage(page);
        await loginPage.goto();
    });

    test('TC-001: Login with valid credentials - Admin', async ({ page }) => {
        // Test Steps
        await loginPage.loginAsAdmin();

        // Assertions
        expect(await loginPage.isLoggedIn()).toBeTruthy();
        await expect(page).toHaveURL(/dashboard|home/);
        await expect(loginPage.welcomeMessage).toBeVisible();
    });

    test('TC-002: Login with valid credentials - Payroll Manager', async ({ page }) => {
        await loginPage.loginAsPayrollManager();

        expect(await loginPage.isLoggedIn()).toBeTruthy();
        await expect(page).toHaveURL(/dashboard|home/);
    });

    test('TC-003: Login with valid credentials - HR Manager', async ({ page }) => {
        await loginPage.loginAsHRManager();

        expect(await loginPage.isLoggedIn()).toBeTruthy();
        await expect(page).toHaveURL(/dashboard|home/);
    });

    test('TC-004: Login with invalid username', async ({ page }) => {
        await loginPage.login('invalid@test.lk', 'Test@1234');

        // Should show error message
        await expect(loginPage.errorMessage).toBeVisible();
        const errorText = await loginPage.getErrorMessage();
        expect(errorText.toLowerCase()).toContain('invalid');

        // Should not be logged in
        expect(await loginPage.isLoggedIn()).toBeFalsy();
    });

    test('TC-005: Login with invalid password', async ({ page }) => {
        await loginPage.login('admin@test.lk', 'WrongPassword');

        await expect(loginPage.errorMessage).toBeVisible();
        expect(await loginPage.isLoggedIn()).toBeFalsy();
    });

    test('TC-006: Login with empty credentials', async ({ page }) => {
        await loginPage.login('', '');

        // Form validation should prevent submission
        const isLoggedIn = await loginPage.isLoggedIn();
        expect(isLoggedIn).toBeFalsy();
    });

    test('TC-007: Logout functionality', async ({ page }) => {
        // Login first
        await loginPage.loginAsAdmin();
        expect(await loginPage.isLoggedIn()).toBeTruthy();

        // Logout
        await loginPage.logout();

        // Should redirect to login page
        await expect(page).toHaveURL(/login/);
        expect(await loginPage.isLoggedIn()).toBeFalsy();
    });

    test('TC-008: Session timeout validation', async ({ page }) => {
        await loginPage.loginAsAdmin();
        expect(await loginPage.isLoggedIn()).toBeTruthy();

        // Wait for session timeout (adjust based on actual timeout)
        // This is a placeholder - actual implementation depends on session timeout
        await page.waitForTimeout(30 * 60 * 1000); // 30 minutes

        // Try to access protected page
        await page.goto('/employees');

        // Should redirect to login
        await expect(page).toHaveURL(/login/);
    });

    test('TC-009: Password field masking', async ({ page }) => {
        const passwordField = loginPage.passwordInput;

        await passwordField.fill('TestPassword123');

        // Password should be masked
        const inputType = await passwordField.getAttribute('type');
        expect(inputType).toBe('password');
    });

    test('TC-010: Remember me functionality', async ({ page }) => {
        // Check if remember me checkbox exists
        const rememberMe = page.locator('input[type="checkbox"][name="rememberMe"]');

        if (await rememberMe.isVisible()) {
            await rememberMe.check();
            await loginPage.loginAsAdmin();

            // Close and reopen browser
            await page.context().close();

            // Create new context and check if still logged in
            // This would require actual cookie/storage validation
        }
    });
});
