import { chromium } from 'playwright'

const browser = await chromium.launch()
const context = await browser.newContext()
const page = await context.newPage()
page.on('console', (msg) => console.log('CONSOLE:', msg.text()))

await page.goto('http://localhost:4299/apps/7d6bd77c-0d41-43f9-95c3-61e1d8e590c0/customers')
await page.waitForTimeout(1000)

await page.getByRole('link', { name: 'Products' }).click()
await page.waitForTimeout(1000)
await page.screenshot({ path: 'products.png', fullPage: true })
console.log('--- products html ---')
console.log(await page.locator('main').innerText())

await page.getByRole('link', { name: 'Orders' }).click()
await page.waitForTimeout(1000)
await page.screenshot({ path: 'orders.png', fullPage: true })
console.log('--- orders html ---')
console.log(await page.locator('main').innerText())

await browser.close()
console.log('done')
