import { LayoutPanelLeft, LayoutPanelTop, Moon, Sun, UserCircle } from 'lucide-react'
import { toast } from 'sonner'
import { PageHeader } from '../../components/PageHeader'
import { Badge, Card, CardBody, CardHeader } from '../../components/ui'
import { useAuthStore } from '../../store/auth'
import { useLayoutSettings } from '../../store/layoutSettings'
import { useThemeStore, type Theme } from '../../store/theme'
import { cn } from '../../utils/cn'

const THEME_OPTIONS: { value: Theme; label: string; icon: typeof Sun }[] = [
  { value: 'light', label: 'Light', icon: Sun },
  { value: 'dark', label: 'Dark', icon: Moon },
]

export function SettingsPage() {
  const { navPosition, setNavPosition } = useLayoutSettings()
  const isHeader = navPosition === 'header'
  const { theme, setTheme } = useThemeStore()
  const user = useAuthStore((s) => s.user)
  const expiresAt = useAuthStore((s) => s.expiresAt)

  return (
    <div className="p-6 max-w-2xl flex flex-col gap-4">
      <PageHeader title="Settings" description="Customize how LineageStudio looks and feels." />

      {user && (
        <Card>
          <CardHeader>
            <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">Account</h2>
          </CardHeader>
          <CardBody>
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-indigo-50 dark:bg-indigo-950/50">
                <UserCircle size={22} className="text-indigo-600 dark:text-indigo-400" />
              </div>
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium text-neutral-900 dark:text-neutral-100 truncate">
                    {user.displayName}
                  </span>
                  <Badge color="green">Signed in</Badge>
                </div>
                <div className="text-xs text-neutral-500 truncate">{user.email}</div>
              </div>
            </div>

            <dl className="mt-4 grid grid-cols-1 gap-2 text-xs sm:grid-cols-2">
              <div>
                <dt className="text-neutral-400">User ID</dt>
                <dd className="text-neutral-700 dark:text-neutral-300 font-mono truncate">{user.userId}</dd>
              </div>
              {expiresAt && (
                <div>
                  <dt className="text-neutral-400">Session expires</dt>
                  <dd className="text-neutral-700 dark:text-neutral-300">
                    {new Date(expiresAt).toLocaleString()}
                  </dd>
                </div>
              )}
            </dl>
          </CardBody>
        </Card>
      )}

      <Card>
        <CardHeader>
          <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">Theme</h2>
        </CardHeader>
        <CardBody>
          <div className="grid grid-cols-2 gap-3">
            {THEME_OPTIONS.map((option) => {
              const Icon = option.icon
              const selected = theme === option.value
              return (
                <button
                  key={option.value}
                  onClick={() => {
                    setTheme(option.value)
                    toast.success(`Switched to ${option.label.toLowerCase()} theme`)
                  }}
                  className={cn(
                    'flex items-center gap-2 rounded-lg border p-3 text-left transition-colors',
                    selected
                      ? 'border-indigo-500 ring-2 ring-indigo-500/20 bg-indigo-50/50 dark:bg-indigo-950/30'
                      : 'border-neutral-200 dark:border-neutral-800 hover:border-neutral-300 dark:hover:border-neutral-700',
                  )}
                >
                  <Icon size={18} className={selected ? 'text-indigo-600' : 'text-neutral-400'} />
                  <span className="font-medium text-sm text-neutral-900 dark:text-neutral-100">{option.label}</span>
                  {selected && (
                    <span className="ml-auto text-[10px] font-semibold uppercase tracking-wide text-indigo-600 dark:text-indigo-400">
                      Active
                    </span>
                  )}
                </button>
              )
            })}
          </div>
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">Navigation placement</h2>
        </CardHeader>
        <CardBody>
          <div className="flex items-center justify-between gap-4">
            <div className="flex items-center gap-2">
              <LayoutPanelLeft size={16} className={cn(!isHeader ? 'text-indigo-600' : 'text-neutral-400')} />
              <span
                className={cn(
                  'text-sm font-medium',
                  !isHeader ? 'text-neutral-900 dark:text-neutral-100' : 'text-neutral-400',
                )}
              >
                Sidebar
              </span>
            </div>

            <button
              role="switch"
              aria-checked={isHeader}
              aria-label="Toggle navigation placement between sidebar and top header"
              onClick={() => {
                const next = isHeader ? 'sidebar' : 'header'
                setNavPosition(next)
                toast.success(`Navigation moved to the ${next === 'sidebar' ? 'sidebar' : 'top header'}`)
              }}
              className={cn(
                'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full transition-colors',
                isHeader ? 'bg-indigo-600' : 'bg-neutral-300 dark:bg-neutral-700',
              )}
            >
              <span
                className={cn(
                  'inline-block h-4 w-4 transform rounded-full bg-white shadow transition-transform',
                  isHeader ? 'translate-x-6' : 'translate-x-1',
                )}
              />
            </button>

            <div className="flex items-center gap-2">
              <span
                className={cn(
                  'text-sm font-medium',
                  isHeader ? 'text-neutral-900 dark:text-neutral-100' : 'text-neutral-400',
                )}
              >
                Top header
              </span>
              <LayoutPanelTop size={16} className={cn(isHeader ? 'text-indigo-600' : 'text-neutral-400')} />
            </div>
          </div>

          <p className="mt-3 text-xs text-neutral-500">
            {isHeader
              ? 'Navigation runs across the top as a single bar, giving pages more width.'
              : 'Navigation runs down the left side, grouped into sections.'}
          </p>
        </CardBody>
      </Card>
    </div>
  )
}
