import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';

dotenv.config();

export default defineConfig({
    testDir: './tests',

    /* Maximum time one test can run */
    timeout: 60 * 1000,

    /* Test execution settings */
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 1,
    workers: process.env.CI ? 4 : undefined,

    /* Reporter configuration */
    reporter: [
        ['html', { outputFolder: 'playwright-report', open: 'never' }],
        ['json', { outputFile: 'test-results/results.json' }],
        ['junit', { outputFile: 'test-results/junit.xml' }],
        ['list'],
        ['allure-playwright']
    ],

    /* Shared settings for all projects */
    use: {
        /* Base URL */
        baseURL: process.env.BASE_URL || 'http://localhost:4200',

        /* API base URL */
        extraHTTPHeaders: {
            'Accept': 'application/json',
        },

        /* Collect trace when retrying the failed test */
        trace: 'on-first-retry',

        /* Screenshot on failure */
        screenshot: 'only-on-failure',

        /* Video on failure */
        video: 'retain-on-failure',

        /* Viewport */
        viewport: { width: 1920, height: 1080 },

        /* Ignore HTTPS errors */
        ignoreHTTPSErrors: true,

        /* Action timeout */
        actionTimeout: 15000,

        /* Navigation timeout */
        navigationTimeout: 30000,
    },

    /* Configure projects for major browsers */
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'firefox',
            use: { ...devices['Desktop Firefox'] },
        },
        {
            name: 'webkit',
            use: { ...devices['Desktop Safari'] },
        },

        /* Mobile viewports */
        {
            name: 'Mobile Chrome',
            use: { ...devices['Pixel 5'] },
        },
        {
            name: 'Mobile Safari',
            use: { ...devices['iPhone 12'] },
        },
    ],

    /* Run local dev server before starting tests */
    // webServer: {
    //   command: 'npm run start',
    //   url: 'http://localhost:4200',
    //   reuseExistingServer: !process.env.CI,
    // },
});
