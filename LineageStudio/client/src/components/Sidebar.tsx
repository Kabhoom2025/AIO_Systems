import { NavLink } from 'react-router-dom'
import { LogOut, PlayCircle } from 'lucide-react'
import { cn } from '../utils/cn'
import { useAuthStore } from '../store/auth'
import { useDemoTourStore } from '../store/demoTour'
import { confirmAction } from './ui'
import { NAV_GROUPS, SETTINGS_NAV_ITEM } from './navigation'

export function Sidebar() {
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)
  const openDemo = useDemoTourStore((s) => s.open)

  return (
    <aside className="w-60 shrink-0 border-r border-neutral-200 dark:border-neutral-800 h-screen sticky top-0 flex flex-col bg-neutral-50/50 dark:bg-neutral-950">
      <div className="px-4 py-4 flex items-center gap-2">
        <div className="h-6 w-6 rounded-md bg-gradient-to-br from-indigo-500 to-violet-600" />
        <span className="font-semibold text-neutral-900 dark:text-neutral-100 tracking-tight">
          LineageStudio
        </span>
      </div>

      <nav className="flex-1 flex flex-col gap-4 px-3 pb-4 overflow-y-auto">
        {NAV_GROUPS.map((group) => (
          <div key={group.label}>
            <div className="px-2 pb-1 text-[11px] font-semibold uppercase tracking-wider text-neutral-400 dark:text-neutral-600">
              {group.label}
            </div>
            <div className="flex flex-col gap-0.5">
              {group.items.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) =>
                    cn(
                      'flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm transition-colors',
                      isActive
                        ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900 font-medium'
                        : 'text-neutral-600 hover:bg-neutral-200/60 dark:text-neutral-300 dark:hover:bg-neutral-800',
                    )
                  }
                >
                  <item.icon size={16} className="shrink-0" />
                  {item.label}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>

      <div className="px-3 pb-3 border-t border-neutral-200 dark:border-neutral-800 pt-3 flex flex-col gap-0.5">
        <button
          onClick={openDemo}
          className="flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm text-neutral-600 hover:bg-neutral-200/60 dark:text-neutral-300 dark:hover:bg-neutral-800 transition-colors"
        >
          <PlayCircle size={16} className="shrink-0" />
          Show demo
        </button>

        <NavLink
          to={SETTINGS_NAV_ITEM.to}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm transition-colors',
              isActive
                ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900 font-medium'
                : 'text-neutral-600 hover:bg-neutral-200/60 dark:text-neutral-300 dark:hover:bg-neutral-800',
            )
          }
        >
          <SETTINGS_NAV_ITEM.icon size={16} className="shrink-0" />
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
            className="flex items-center gap-2.5 rounded-md px-2.5 py-1.5 text-sm text-neutral-600 hover:bg-neutral-200/60 dark:text-neutral-300 dark:hover:bg-neutral-800 transition-colors"
            title={user.email}
          >
            <LogOut size={16} className="shrink-0" />
            <span className="truncate">Log out ({user.displayName})</span>
          </button>
        )}
      </div>
    </aside>
  )
}
