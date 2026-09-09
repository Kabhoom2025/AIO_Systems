import { test as setup, expect } from '@playwright/test'

const authFile = 'playwright/.auth/user.json'

/**
 * The app is now gated behind real JWT login (Platform.Api/AuthController). This setup project
 * signs in once as the seeded demo admin and saves the resulting localStorage auth state so every
 * other test project starts already authenticated, instead of repeating the login UI flow per test.
 */
setup('authenticate as the seeded demo admin', async ({ page }) => {
  await page.goto('/login')
  await page.getByLabel('Email').fill('admin@lineagestudio.local')
  await page.getByLabel('Password', { exact: true }).fill('Admin@123')
  await page.getByRole('button', { name: 'Sign in' }).click()

  await expect(page).toHaveURL(/\/applications/)
  await page.context().storageState({ path: authFile })
})
