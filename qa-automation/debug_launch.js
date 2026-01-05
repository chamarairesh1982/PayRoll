const { chromium } = require('@playwright/test');

(async () => {
    try {
        const browser = await chromium.launch();
        const page = await browser.newPage();
        await page.goto('https://example.com');
        console.log(await page.title());
        await browser.close();
    } catch (e) {
        console.error(e);
        process.exit(1);
    }
})();
