import { Page, Locator } from '@playwright/test';

export class LoginPage {
    readonly page: Page;
    readonly usernameInput: Locator;
    readonly passwordInput: Locator;
    readonly loginButton: Locator;
    readonly errorMessage: Locator;
    readonly welcomeMessage: Locator;

    constructor(page: Page) {
        this.page = page;
        this.usernameInput = page.locator('input[formcontrolname="username"], input[name="username"], input[type="email"]');
        this.passwordInput = page.locator('input[formcontrolname="password"], input[name="password"], input[type="password"]');
        this.loginButton = page.locator('button[type="submit"], button:has-text("Login")');
        this.errorMessage = page.locator('.error-message, .p-toast-message-error, .alert-danger');
        this.welcomeMessage = page.locator('.welcome-message, h1, .dashboard-title');
    }

    async goto() {
        await this.page.goto('/login');
    }

    async login(username: string, password: string) {
        await this.usernameInput.fill(username);
        await this.passwordInput.fill(password);
        await this.loginButton.click();

        // Wait for navigation
        await this.page.waitForLoadState('networkidle');
    }

    async loginAsAdmin() {
        await this.login(
            process.env.ADMIN_USERNAME || 'admin@test.lk',
            process.env.ADMIN_PASSWORD || 'Test@1234'
        );
    }

    async loginAsPayrollManager() {
        await this.login(
            process.env.PAYROLL_MGR_USERNAME || 'payroll.mgr@test.lk',
            process.env.PAYROLL_MGR_PASSWORD || 'Test@1234'
        );
    }

    async loginAsHRManager() {
        await this.login(
            process.env.HR_MGR_USERNAME || 'hr.mgr@test.lk',
            process.env.HR_MGR_PASSWORD || 'Test@1234'
        );
    }

    async isLoggedIn(): Promise<boolean> {
        try {
            await this.page.waitForSelector('.user-menu, .avatar, [data-testid="user-menu"]', { timeout: 5000 });
            return true;
        } catch {
            return false;
        }
    }

    async getErrorMessage(): Promise<string> {
        return await this.errorMessage.textContent() || '';
    }

    async logout() {
        await this.page.click('.user-menu, .avatar, [data-testid="user-menu"]');
        await this.page.click('text=Logout, text=Sign Out');
        await this.page.waitForURL('**/login');
    }
}
