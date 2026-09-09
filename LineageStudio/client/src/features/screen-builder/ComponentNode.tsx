import { Handle, Position } from '@xyflow/react'
import type { UiComponent } from '../../types'
import { COMPONENT_ICONS } from './componentIcons'

export function ComponentNode({ data, selected }: { data: { component: UiComponent }; selected: boolean }) {
  const { component } = data
  const Icon = COMPONENT_ICONS[component.type]

  return (
    <div
      className={`flex items-center gap-2.5 rounded-md border px-3 py-2 bg-white dark:bg-neutral-900 text-sm min-w-[150px] shadow-sm transition-shadow ${
        selected
          ? 'border-indigo-500 ring-2 ring-indigo-500/30'
          : 'border-neutral-300 dark:border-neutral-700 hover:shadow-md'
      }`}
    >
      <Handle type="target" position={Position.Top} className="!bg-neutral-400" />
      <div className="flex h-7 w-7 shrink-0 items-center justify-center rounded bg-indigo-50 dark:bg-indigo-950 text-indigo-600 dark:text-indigo-400">
        <Icon size={14} />
      </div>
      <div className="min-w-0">
        <div className="font-medium text-neutral-900 dark:text-neutral-100 truncate">{component.name}</div>
        <div className="text-xs text-neutral-500">{component.type}</div>
      </div>
      <Handle type="source" position={Position.Bottom} className="!bg-neutral-400" />
    </div>
  )
}
