import {
  Boxes,
  Database,
  GitBranch,
  History,
  LayoutGrid,
  Play,
  Radar,
  Settings,
  Waypoints,
  Webhook,
  type LucideIcon,
} from 'lucide-react'

export interface NavItem {
  to: string
  label: string
  icon: LucideIcon
}

export interface NavGroup {
  label: string
  items: NavItem[]
}

export const NAV_GROUPS: NavGroup[] = [
  {
    label: 'Build',
    items: [
      { to: '/applications', label: 'Applications', icon: LayoutGrid },
      { to: '/screens', label: 'Screen Builder', icon: Boxes },
      { to: '/data', label: 'Data Designer', icon: Database },
      { to: '/apis', label: 'API Designer', icon: Webhook },
      { to: '/mappings', label: 'Mapping Designer', icon: Waypoints },
    ],
  },
  {
    label: 'Run & Observe',
    items: [
      { to: '/runtime', label: 'Run', icon: Play },
      { to: '/lineage', label: 'Lineage', icon: GitBranch },
      { to: '/executions', label: 'Execution History', icon: History },
      { to: '/impact', label: 'Impact Analysis', icon: Radar },
    ],
  },
]

export const SETTINGS_NAV_ITEM: NavItem = { to: '/settings', label: 'Settings', icon: Settings }
