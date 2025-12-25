import { test, expect } from '@playwright/test';

test.describe('Smoke Tests @smoke', () => {
    test('TC-001: Basic test - Verify Playwright is working', async ({ page }) => {
        // This is a simple test to verify the framework is set up correctly
        await page.goto('https://playwright.dev');
        await expect(page).toHaveTitle(/Playwright/);
    });

    test('TC-002: Navigate to application', async ({ page }) => {
        // Navigate to the payroll application
        const baseURL = process.env.BASE_URL || 'http://localhost:4200';
        await page.goto(baseURL);

        // Verify page loads
        await expect(page).toHaveURL(baseURL);
    });
});
