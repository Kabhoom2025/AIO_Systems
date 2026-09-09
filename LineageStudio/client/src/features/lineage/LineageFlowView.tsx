import { useMemo, useState } from 'react'
import { Background, Controls, MiniMap, ReactFlow, ReactFlowProvider, type Edge, type Node, type NodeTypes } from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import type { LineageEvent, LineageExecution, LineageNodeType } from '../../types'
import { layoutWithDagre } from '../../utils/dagreLayout'
import { Card, CardBody, StatusBadge } from '../../components/ui'
import { LineageGraphNode } from './LineageGraphNode'
import { LINEAGE_NODE_ICONS } from './lineageNodeIcons'

const nodeTypes: NodeTypes = { lineageNode: LineageGraphNode }

interface NodeState {
  nodeId: string
  nodeType: LineageNodeType
  label: string
  hasMetadataLabel: boolean
  latestEvent: LineageEvent
}

function labelFor(nodeType: LineageNodeType, nodeId: string, metadata: Record<string, unknown> | null): string {
  if (metadata) {
    if (typeof metadata.name === 'string') return metadata.name as string
    if (typeof metadata.method === 'string' && typeof metadata.path === 'string') return `${metadata.method} ${metadata.path}`
    if (typeof metadata.apiField === 'string') return metadata.apiField as string
  }
  const [, id] = nodeId.split(':')
  return id ? `${nodeType} ${id.slice(0, 8)}` : nodeType
}

function computeGraph(events: LineageEvent[]) {
  const byNode = new Map<string, NodeState>()
  const eventsById = new Map(events.map((e) => [e.id, e]))

  // The *label* comes from whichever event for a node carries the richest metadata - usually
  // its first ("STARTED") event, since the paired COMPLETED/FAILED event is typically
  // metadata-free - while *status* always tracks the most recent event, so a node never loses
  // its descriptive name just because it finished.
  for (const event of events) {
    // ExecutionCompleted is a terminal marker for the whole run (nodeId "execution:{id}"), not
    // a real pipeline node - the execution's own status badge already shows this; including it
    // here would render a confusing extra box with no meaningful label.
    if (event.eventType === 'ExecutionCompleted') continue

    const existing = byNode.get(event.nodeId)
    const hasMetadata = event.metadata !== null && Object.keys(event.metadata).length > 0

    if (!existing) {
      byNode.set(event.nodeId, {
        nodeId: event.nodeId,
        nodeType: event.nodeType,
        label: labelFor(event.nodeType, event.nodeId, event.metadata),
        hasMetadataLabel: hasMetadata,
        latestEvent: event,
      })
      continue
    }

    byNode.set(event.nodeId, {
      ...existing,
      label: hasMetadata && !existing.hasMetadataLabel ? labelFor(event.nodeType, event.nodeId, event.metadata) : existing.label,
      hasMetadataLabel: existing.hasMetadataLabel || hasMetadata,
      latestEvent: new Date(event.startedAt) >= new Date(existing.latestEvent.startedAt) ? event : existing.latestEvent,
    })
  }

  const rawNodes: Node[] = Array.from(byNode.values()).map((state) => ({
    id: state.nodeId,
    type: 'lineageNode',
    position: { x: 0, y: 0 },
    data: { label: state.label, nodeType: state.nodeType, status: state.latestEvent.status },
  }))

  const edgeKeys = new Set<string>()
  const edges: Edge[] = []
  for (const event of events) {
    if (event.eventType === 'ExecutionCompleted' || !event.parentEventId) continue
    const parent = eventsById.get(event.parentEventId)
    if (!parent || parent.nodeId === event.nodeId || !byNode.has(event.nodeId)) continue

    const key = `${parent.nodeId}->${event.nodeId}`
    if (edgeKeys.has(key)) continue
    edgeKeys.add(key)

    edges.push({
      id: key,
      source: parent.nodeId,
      target: event.nodeId,
      animated: event.status === 'Running',
    })
  }

  const nodes = layoutWithDagre(rawNodes, edges, { direction: 'LR', nodeWidth: 200, nodeHeight: 40 })
  return { nodes, edges, byNode }
}

interface LineageFlowViewProps {
  execution: LineageExecution
  events: LineageEvent[]
}

export function LineageFlowView({ execution, events }: LineageFlowViewProps) {
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null)
  const { nodes, edges, byNode } = useMemo(() => computeGraph(events), [events])
  const selected = selectedNodeId ? byNode.get(selectedNodeId) : null
  const DetailIcon = selected ? LINEAGE_NODE_ICONS[selected.nodeType] : null

  return (
    <div className="flex gap-4">
      <div style={{ height: 480 }} className="flex-1 rounded-lg border border-neutral-200 dark:border-neutral-800">
        <ReactFlowProvider>
          <ReactFlow
            nodes={nodes}
            edges={edges}
            nodeTypes={nodeTypes}
            onNodeClick={(_e, node) => setSelectedNodeId(node.id)}
            onPaneClick={() => setSelectedNodeId(null)}
            fitView
            fitViewOptions={{ maxZoom: 1 }}
          >
            <Background />
            <Controls />
            <MiniMap pannable zoomable />
          </ReactFlow>
        </ReactFlowProvider>
      </div>

      {selected && DetailIcon && (
        <Card className="w-72 shrink-0 h-fit">
          <CardBody>
            <div className="flex items-center gap-2 font-semibold mb-2">
              <DetailIcon size={15} className="text-neutral-400" />
              {selected.label}
            </div>
            <dl className="flex flex-col gap-1.5 text-xs">
              <div className="flex items-center justify-between">
                <dt className="text-neutral-500">Node type</dt>
                <dd>{selected.nodeType}</dd>
              </div>
              <div className="flex items-center justify-between">
                <dt className="text-neutral-500">Status</dt>
                <dd>
                  <StatusBadge status={selected.latestEvent.status} />
                </dd>
              </div>
              <div className="flex items-center justify-between">
                <dt className="text-neutral-500">Last event</dt>
                <dd>{selected.latestEvent.eventType}</dd>
              </div>
              {selected.latestEvent.durationMs !== null && (
                <div className="flex items-center justify-between">
                  <dt className="text-neutral-500">Duration</dt>
                  <dd>{selected.latestEvent.durationMs} ms</dd>
                </div>
              )}
              <div className="flex items-center justify-between">
                <dt className="text-neutral-500">Execution</dt>
                <dd className="font-mono text-[10px]">{execution.id.slice(0, 8)}…</dd>
              </div>
              <div className="flex items-center justify-between">
                <dt className="text-neutral-500">Correlation</dt>
                <dd className="font-mono text-[10px]">{execution.correlationId.slice(0, 8)}…</dd>
              </div>
              {selected.latestEvent.errorCode && (
                <div className="mt-2 rounded bg-red-50 dark:bg-red-950 p-2 text-red-700 dark:text-red-300">
                  [{selected.latestEvent.errorCode}] {selected.latestEvent.errorMessage}
                </div>
              )}
            </dl>
          </CardBody>
        </Card>
      )}
    </div>
  )
}
