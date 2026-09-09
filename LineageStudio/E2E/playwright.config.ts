import { defineConfig, devices } from '@playwright/test'

/**
 * Starts both the real .NET API and the real Vite dev server (not mocks) so these tests exercise
 * the actual PostgreSQL-backed runtime/lineage engine end to end, matching the spec's "no
 * prototype" requirement. `reuseExistingServer` lets this run against servers you already have up.
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: 'list',
  use: {
    // A dedicated port (not the usual 4209) so this never collides with a dev server you
    // already have running locally.
    baseURL: 'http://localhost:4299',
    trace: 'retain-on-failure',
  },
  projects: [
    { name: 'setup', testMatch: /auth\.setup\.ts/ },
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], storageState: 'playwright/.auth/user.json' },
      dependencies: ['setup'],
    },
  ],
  webServer: [
    {
      command: 'dotnet run --no-launch-profile',
      cwd: '../src/Platform.Api',
      url: 'http://localhost:5009/',
      env: { ASPNETCORE_ENVIRONMENT: 'Development', ASPNETCORE_URLS: 'http://localhost:5009' },
      reuseExistingServer: true,
      timeout: 60_000,
    },
    {
      command: 'npm run dev -- --port 4299 --strictPort',
      cwd: '../client',
      url: 'http://localhost:4299/',
      // Bypasses the ApiGateway (not started for local E2E runs) and talks to Platform.Api
      // directly - docker-compose/production still go through the gateway as normal.
      env: {
        VITE_API_BASE_URL: 'http://localhost:5009/api',
        VITE_HUB_BASE_URL: 'http://localhost:5009/hubs',
      },
      reuseExistingServer: true,
      timeout: 60_000,
    },
  ],
})
