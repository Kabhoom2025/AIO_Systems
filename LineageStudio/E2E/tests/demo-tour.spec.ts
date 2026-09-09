import { test, expect } from '@playwright/test'

/**
 * The "Show demo" walkthrough explains the whole app pipeline (Applications -> Data Designer ->
 * Screen Builder -> API Designer -> Mapping Designer -> Run -> Lineage) for anyone unsure how the
 * flow works. Covers opening it, stepping through, and that "Open <page>" really navigates there.
 */

test('Show demo opens a step-by-step walkthrough that can navigate to a page', async ({ page }) => {
  await page.goto('/applications')

  await page.getByRole('button', { name: 'Show demo' }).click()

  await expect(page.getByText('Step 1 of 7')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Create an application' })).toBeVisible()

  await page.getByRole('button', { name: 'Next' }).click()
  await expect(page.getByText('Step 2 of 7')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Design your data' })).toBeVisible()

  await page.getByRole('button', { name: /Open Data Designer/ }).click()
  await expect(page).toHaveURL(/\/data/)
  await expect(page.getByText('Step 1 of 7')).not.toBeVisible()
})
