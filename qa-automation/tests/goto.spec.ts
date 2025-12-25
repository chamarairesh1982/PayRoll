import { test, expect } from '@playwright/test';

test('goto test', async ({ page }) => {
    await page.goto('https://playwright.dev');
    await expect(page).toHaveTitle(/Playwright/);
});
