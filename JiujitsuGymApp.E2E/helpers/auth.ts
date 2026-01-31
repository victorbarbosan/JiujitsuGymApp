import type { Page } from '@playwright/test';

export async function signIn(page: Page) {
    if (!process.env.TEST_USER_EMAIL || !process.env.TEST_USER_PASSWORD) {
        throw new Error('Missing TEST_USER credentials');
    }

    await page.goto('/Account/Login', { waitUntil: 'load' });
    await page.fill('input[name="Email"]', process.env.TEST_USER_EMAIL);
    await page.fill('input[name="Password"]', process.env.TEST_USER_PASSWORD);

    await Promise.all([
        page.waitForNavigation({ waitUntil: 'networkidle' }),
        page.click('button[type="submit"]'),
    ]);
}