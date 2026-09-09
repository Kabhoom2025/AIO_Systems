import { Handle, Position } from '@xyflow/react'
import type { ExecutionStatus, LineageNodeType } from '../../types'
import { LINEAGE_NODE_ICONS } from './lineageNodeIcons'

const STATUS_BORDER: Record<ExecutionStatus, string> = {
  Pending: 'border-neutral-300 dark:border-neutral-700',
  Running: 'border-blue-500',
  Success: 'border-green-500',
  Failed: 'border-red-500',
  Skipped: 'border-neutral-300 dark:border-neutral-700 border-dashed',
}

export function LineageGraphNode({
  data,
}: {
  data: { label: string; nodeType: LineageNodeType; status: ExecutionStatus }
}) {
  const Icon = LINEAGE_NODE_ICONS[data.nodeType]

  return (
    <div
      className={`flex items-center gap-2 rounded-md border-2 px-2.5 py-1.5 bg-white dark:bg-neutral-900 text-xs shadow-sm ${
        STATUS_BORDER[data.status]
      } ${data.status === 'Skipped' ? 'opacity-50' : ''}`}
    >
      <Handle type="target" position={Position.Left} className="!bg-neutral-400" />
      <Icon size={13} className="shrink-0 text-neutral-400" />
      <span className="truncate max-w-[160px] text-neutral-800 dark:text-neutral-200">{data.label}</span>
      {data.status === 'Running' && <span className="h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse shrink-0" />}
      <Handle type="source" position={Position.Right} className="!bg-neutral-400" />
    </div>
  )
}
