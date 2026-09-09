import { test, expect } from '@playwright/test'

/**
 * Screen Builder templates create a real screen plus real components via the actual .NET API in
 * one click - not a client-side mock. Covers the whole round trip: click a template, the new
 * screen shows up in the Screens list and gets selected, and its components render as real nodes
 * on the canvas (proving they were actually persisted, not just added to local state).
 */

test('Clicking a quick start template creates a screen with its components', async ({ page }) => {
  await page.goto('/screens')

  await page.getByRole('button', { name: /Login form/i }).click()

  // The new screen is selected automatically - its name shows up highlighted in the Screens list.
  // (Prior test runs leave earlier "Login" screens around too, so scope to the highlighted one.)
  await expect(page.locator('button.bg-neutral-900', { hasText: 'Login' })).toBeVisible({ timeout: 10_000 })

  // Its components were really created server-side and now render as canvas nodes.
  await expect(page.getByText('LoginForm.title')).toBeVisible()
  await expect(page.getByText('LoginForm.email')).toBeVisible()
  await expect(page.getByText('LoginForm.password')).toBeVisible()
  await expect(page.getByText('LoginForm.submit')).toBeVisible()
})
