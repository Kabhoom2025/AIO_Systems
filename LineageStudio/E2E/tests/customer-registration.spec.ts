import { test, expect } from '@playwright/test'

/**
 * Drives the spec's own golden-path demo through the real browser UI, against the real .NET API
 * and real PostgreSQL (via SeedData's "Customer Management" app) - no mocks, no stubbed
 * fetch/XHR. Covers: Applications list shows the published seed app, Run executes a real POST
 * that really inserts a row (SUCCESS), and a second save with the same email really violates the
 * database's UNIQUE constraint (FAILED) - the exact deliberate-failure scenario the spec asks for.
 */

test('Applications page lists the published Customer Management seed app', async ({ page }) => {
  await page.goto('/applications')
  // Scoped to this app's own row - other apps created in this environment can also be published.
  const row = page.getByRole('listitem').filter({ hasText: 'Customer Management' })
  await expect(row).toBeVisible()
  await expect(row.getByText('Published')).toBeVisible()
})

test('Running the seeded Customer Registration API succeeds, then fails on a duplicate email', async ({ page }) => {
  await page.goto('/runtime')

  const appSelect = page.locator('select').first()
  await appSelect.selectOption({ label: 'Customer Management' })

  const apiSelect = page.locator('select').nth(1)
  await apiSelect.selectOption({ label: 'POST /customer' })

  const uniqueEmail = `jane.${Date.now()}@example.com`

  await page.getByLabel(/CustomerForm\.name/i).fill('Jane Doe')
  await page.getByLabel(/CustomerForm\.email/i).fill(uniqueEmail)
  await page.getByLabel(/CustomerForm\.phone/i).fill('555-1234')
  await page.getByRole('button', { name: 'Save' }).click()

  await expect(page.getByText('✓ SUCCESS')).toBeVisible({ timeout: 10_000 })

  // Same email again - the real UNIQUE constraint on customer.email must reject this.
  await page.getByLabel(/CustomerForm\.name/i).fill('John Smith')
  await page.getByLabel(/CustomerForm\.email/i).fill(uniqueEmail)
  await page.getByLabel(/CustomerForm\.phone/i).fill('555-9999')
  await page.getByRole('button', { name: 'Save' }).click()

  await expect(page.getByText('✗ FAILED')).toBeVisible({ timeout: 10_000 })
  // Scoped to the result card's own error paragraph - a toast notification also mentions the
  // error code, so an unscoped text query would match both.
  await expect(page.locator('p', { hasText: /DUPLICATE_KEY/i })).toBeVisible()
})

test('A successful save shows up live in the Lineage view', async ({ page }) => {
  await page.goto('/runtime')

  const appSelect = page.locator('select').first()
  await appSelect.selectOption({ label: 'Customer Management' })
  const apiSelect = page.locator('select').nth(1)
  await apiSelect.selectOption({ label: 'POST /customer' })

  await page.getByLabel(/CustomerForm\.name/i).fill('Live Lineage Test')
  await page.getByLabel(/CustomerForm\.email/i).fill(`live.${Date.now()}@example.com`)
  await page.getByLabel(/CustomerForm\.phone/i).fill('555-0000')
  await page.getByRole('button', { name: 'Save' }).click()
  await expect(page.getByText('✓ SUCCESS')).toBeVisible({ timeout: 10_000 })

  await page.getByRole('link', { name: /view live lineage/i }).click()
  await expect(page).toHaveURL(/\/lineage/)

  // The live flow view renders React Flow nodes for the pipeline this run actually went
  // through - Screen, the API, and the real database write.
  await expect(page.getByText(/Customer Registration/i)).toBeVisible({ timeout: 10_000 })
  await expect(page.getByText(/POST \/customer/i)).toBeVisible()
})
