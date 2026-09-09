import { useLayoutEffect } from 'react'
import { QueryClientProvider } from '@tanstack/react-query'
import { Navigate, Route, BrowserRouter, Routes } from 'react-router-dom'
import { Toaster } from 'sonner'
import { queryClient } from './app/queryClient'
import { Sidebar } from './components/Sidebar'
import { Header } from './components/Header'
import { ConfirmDialogHost } from './components/ui'
import { DemoTourModal } from './components/DemoTourModal'
import { RequireAuth } from './components/RequireAuth'
import { LoginPage } from './features/auth/LoginPage'
import { PublicAppPage } from './features/public-app/PublicAppPage'
import { useLayoutSettings } from './store/layoutSettings'
import { useThemeStore } from './store/theme'
import { ApplicationsPage } from './features/applications/ApplicationsPage'
import { DataDesignerPage } from './features/data-designer/DataDesignerPage'
import { ScreenBuilderPage } from './features/screen-builder/ScreenBuilderPage'
import { ApiDesignerPage } from './features/api-designer/ApiDesignerPage'
import { MappingDesignerPage } from './features/mapping-designer/MappingDesignerPage'
import { RuntimePage } from './features/runtime/RuntimePage'
import { LineagePage } from './features/lineage/LineagePage'
import { ExecutionHistoryPage } from './features/executions/ExecutionHistoryPage'
import { ExecutionDetailPage } from './features/executions/ExecutionDetailPage'
import { ImpactAnalysisPage } from './features/impact-analysis/ImpactAnalysisPage'
import { SettingsPage } from './features/settings/SettingsPage'

const routes = (
  <Routes>
    <Route path="/" element={<Navigate to="/applications" replace />} />
    <Route path="/applications" element={<ApplicationsPage />} />
    <Route path="/screens" element={<ScreenBuilderPage />} />
    <Route path="/data" element={<DataDesignerPage />} />
    <Route path="/apis" element={<ApiDesignerPage />} />
    <Route path="/mappings" element={<MappingDesignerPage />} />
    <Route path="/runtime" element={<RuntimePage />} />
    <Route path="/lineage" element={<LineagePage />} />
    <Route path="/executions" element={<ExecutionHistoryPage />} />
    <Route path="/executions/:executionId" element={<ExecutionDetailPage />} />
    <Route path="/impact" element={<ImpactAnalysisPage />} />
    <Route path="/settings" element={<SettingsPage />} />
  </Routes>
)

function App() {
  const navPosition = useLayoutSettings((s) => s.navPosition)
  const theme = useThemeStore((s) => s.theme)

  useLayoutEffect(() => {
    document.documentElement.classList.toggle('dark', theme === 'dark')
  }, [theme])

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/apps/:applicationId/*" element={<PublicAppPage />} />
          <Route
            path="/*"
            element={
              <RequireAuth>
                {navPosition === 'sidebar' ? (
                  <div className="flex min-h-screen bg-white dark:bg-neutral-950">
                    <Sidebar />
                    <main className="flex-1 overflow-auto">{routes}</main>
                  </div>
                ) : (
                  <div className="flex flex-col h-screen bg-white dark:bg-neutral-950">
                    <Header />
                    <main className="flex-1 overflow-auto">{routes}</main>
                  </div>
                )}
              </RequireAuth>
            }
          />
        </Routes>
        <Toaster richColors position="bottom-right" closeButton />
        <ConfirmDialogHost />
        <DemoTourModal />
      </BrowserRouter>
    </QueryClientProvider>
  )
}

export default App
