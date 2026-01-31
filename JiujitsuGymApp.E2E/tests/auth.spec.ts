import { test, expect } from '@playwright/test';

test.describe('Authentication Redirect', () => {

    test('should redirect unauthenticated users to login', async ({ page }) => {
        // 1. Attempt to go to the home page
        await page.goto('/');

        // 2. Verify the URL changed to the login route
        // This confirms your ASP.NET [Authorize] attribute is working
        await expect(page).toHaveURL(/.*login/i);

        // 3. Verify the Login Page specifically loaded
        await expect(page).toHaveTitle(/Login/);
    });

    test('should have a functional login button', async ({ page }) => {
        // 1. Navigate to the actual controller route
        await page.goto('/Account/Login', { waitUntil: 'load' });

        // 2. Prefer a role-based, text-matching locator (more robust)
        const loginButton = page.locator('form').getByRole('button', { name: /login/i });

        // 3. Wait for visibility with an extended timeout
        await expect(loginButton).toBeVisible({ timeout: 10000 });
    });
});