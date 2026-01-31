import { test, expect } from '@playwright/test';
import { signIn } from '../helpers/auth';

test.describe('Profile component (authenticated)', () => {

    test('profile-card renders and actions navigate', async ({ page }) => {
        // 1. Sign in
        await signIn(page);

        // 2. Go to profile page
        await page.goto('/profile', { waitUntil: 'load' })

        // 3. Verify the web component is present
        const profileCard = page.locator('profile-card');
        await expect(profileCard).toBeVisible();

        // 4. Ensure card contains expected info
        await expect(
            profileCard.locator('profile-info', { hasText: 'Email' })
        ).toBeVisible();

        await expect(
            profileCard.locator('profile-info', { hasText: 'Phone' })
        ).toBeVisible();

        await expect(
            profileCard.locator('profile-info', { hasText: 'Member Since' })
        ).toBeVisible();

        await expect(
            profileCard.locator('profile-info', { hasText: 'Last Login' })
        ).toBeVisible();

        // 5. Check "Edit Profile" navigation
        const editButton = profileCard.getByRole('button', {name:'Edit Profile'});
        await expect(editButton).toBeVisible();
        await Promise.all([
            page.waitForURL(/\/Profile\/Edit/i),
            editButton.click(),
        ]);
        await page.goBack({ waitUntil: 'load' });

        // 6. Check "Change Password" navigation
        const changePassword = profileCard.getByRole('button', { name: 'Change Password' });
        await expect(changePassword).toBeVisible();
        await Promise.all([
            page.waitForURL(/\/Profile\/ChangePassword/i),
            changePassword.click()
        ])
        await page.goBack({ waitUntil: 'load' });
    })
})