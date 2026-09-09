import { CheckCircle2, CircleDashed, CircleSlash, Loader2, XCircle } from 'lucide-react'
import { cn } from '../../utils/cn'
import type { ExecutionStatus } from '../../types'

interface BadgeProps {
  children: React.ReactNode
  color?: 'neutral' | 'green' | 'red' | 'blue' | 'amber'
  className?: string
}

const COLOR_CLASSES: Record<NonNullable<BadgeProps['color']>, string> = {
  neutral: 'bg-neutral-100 text-neutral-700 dark:bg-neutral-800 dark:text-neutral-300',
  green: 'bg-green-100 text-green-800 dark:bg-green-950 dark:text-green-300',
  red: 'bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-300',
  blue: 'bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-300',
  amber: 'bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300',
}

export function Badge({ children, color = 'neutral', className }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium',
        COLOR_CLASSES[color],
        className,
      )}
    >
      {children}
    </span>
  )
}

const STATUS_CONFIG: Record<ExecutionStatus, { color: BadgeProps['color']; label: string; icon: React.ReactNode }> = {
  Pending: { color: 'neutral', label: 'Pending', icon: <CircleDashed size={12} /> },
  Running: { color: 'blue', label: 'Running', icon: <Loader2 size={12} className="animate-spin" /> },
  Success: { color: 'green', label: 'Success', icon: <CheckCircle2 size={12} /> },
  Failed: { color: 'red', label: 'Failed', icon: <XCircle size={12} /> },
  Skipped: { color: 'neutral', label: 'Skipped', icon: <CircleSlash size={12} /> },
}

export function StatusBadge({ status }: { status: ExecutionStatus }) {
  const config = STATUS_CONFIG[status]
  return (
    <Badge color={config.color}>
      {config.icon}
      {config.label}
    </Badge>
  )
}
