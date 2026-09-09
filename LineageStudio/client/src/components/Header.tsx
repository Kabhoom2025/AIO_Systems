import { NavLink } from 'react-router-dom'
import { LogOut, PlayCircle } from 'lucide-react'
import { cn } from '../utils/cn'
import { useAuthStore } from '../store/auth'
import { useDemoTourStore } from '../store/demoTour'
import { confirmAction } from './ui'
import { NAV_GROUPS, SETTINGS_NAV_ITEM } from './navigation'

export function Header() {
  const allItems = NAV_GROUPS.flatMap((group) => group.items)
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)
  const openDemo = useDemoTourStore((s) => s.open)

  return (
    <header className="sticky top-0 z-10 border-b border-neutral-200 dark:border-neutral-800 bg-white/80 dark:bg-neutral-950/80 backdrop-blur">
      <div className="flex items-center gap-4 px-4 h-14">
        <div className="flex items-center gap-2 shrink-0">
          <div className="h-6 w-6 rounded-md bg-gradient-to-br from-indigo-500 to-violet-600" />
          <span className="font-semibold text-neutral-900 dark:text-neutral-100 tracking-tight whitespace-nowrap">
            LineageStudio
          </span>
        </div>

        <nav className="flex items-center gap-1 overflow-x-auto">
          {allItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-sm whitespace-nowrap transition-colors',
                  isActive
                    ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900 font-medium'
                    : 'text-neutral-600 hover:bg-neutral-100 dark:text-neutral-300 dark:hover:bg-neutral-800',
                )
              }
            >
              <item.icon size={15} className="shrink-0" />
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="ml-auto shrink-0 flex items-center gap-1">
          <button
            onClick={openDemo}
            className="flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-sm text-neutral-600 hover:bg-neutral-100 dark:text-neutral-300 dark:hover:bg-neutral-800 transition-colors"
          >
            <PlayCircle size={15} className="shrink-0" />
            Show demo
          </button>

          <NavLink
            to={SETTINGS_NAV_ITEM.to}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-sm transition-colors',
                isActive
                  ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900 font-medium'
                  : 'text-neutral-600 hover:bg-neutral-100 dark:text-neutral-300 dark:hover:bg-neutral-800',
              )
            }
          >
            <SETTINGS_NAV_ITEM.icon size={15} className="shrink-0" />
            {SETTINGS_NAV_ITEM.label}
          </NavLink>

          {user && (
            <button
              onClick={async () => {
                const confirmed = await confirmAction({
                  title: 'Log out?',
                  description: `You'll need to sign in again to continue using LineageStudio, ${user.displayName}.`,
                  confirmLabel: 'Log out',
                  danger: true,
                })
                if (confirmed) logout()
              }}
              className="flex items-center gap-1.5 rounded-md px-2.5 py-1.5 text-sm text-neutral-600 hover:bg-neutral-100 dark:text-neutral-300 dark:hover:bg-neutral-800 transition-colors"
              title={user.email}
            >
              <LogOut size={15} className="shrink-0" />
              Log out
            </button>
          )}
        </div>
      </div>
    </header>
  )
}
