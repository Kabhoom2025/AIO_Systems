import { useEffect, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { GitBranch, Radio } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import { fromWire, listEvents, listExecutions, type LineageEventWire } from '../../services/lineage'
import type { LineageEvent } from '../../types'
import { Card, EmptyState, SkeletonList, StatusBadge } from '../../components/ui'
import { cn } from '../../utils/cn'
import { useLineageLive } from './useLineageLive'
import { LineageFlowView } from './LineageFlowView'

export function LineagePage() {
  const { applicationId } = useSelectedApplication()
  const [selectedExecutionId, setSelectedExecutionId] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const executionsQuery = useQuery({
    queryKey: ['lineage-executions', applicationId],
    queryFn: () => listExecutions(applicationId!),
    enabled: !!applicationId,
  })

  const eventsQuery = useQuery({
    queryKey: ['lineage-events', selectedExecutionId],
    queryFn: () => listEvents(selectedExecutionId!),
    enabled: !!selectedExecutionId,
  })

  useEffect(() => {
    // Landing on this page fresh (e.g. via "View live lineage" right after a save) should show
    // that execution immediately - not wait for a *future* live push, which is all the
    // onExecutionUpdate handler below reacts to.
    if (!selectedExecutionId && executionsQuery.data && executionsQuery.data.length > 0) {
      setSelectedExecutionId(executionsQuery.data[0].id)
    }
  }, [executionsQuery.data, selectedExecutionId])

  useLineageLive({
    applicationId,
    onExecutionUpdate: (execution) => {
      queryClient.invalidateQueries({ queryKey: ['lineage-executions', applicationId] })
      // Follow a newly-started execution live, so "Save" on the Run page shows up here without
      // any manual clicking - this is the "watch it run" path the spec calls for.
      setSelectedExecutionId((current) => current ?? execution.id)
    },
    onEvent: (wire: LineageEventWire) => {
      if (wire.executionId !== selectedExecutionId) return
      queryClient.setQueryData<LineageEvent[]>(['lineage-events', selectedExecutionId], (existing) => {
        const event = fromWire(wire)
        if (!existing) return [event]
        if (existing.some((e) => e.id === event.id)) return existing
        return [...existing, event]
      })
    },
  })

  const selectedExecution = executionsQuery.data?.find((e) => e.id === selectedExecutionId)

  return (
    <div className="p-6">
      <PageHeader
        title="Lineage"
        description="Watch data flow live through your app - screen, API, transformations, service, database."
        actions={
          <span className="flex items-center gap-1.5 text-xs text-neutral-500">
            <Radio size={12} className="text-green-500" /> Live via SignalR
          </span>
        }
      />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <div className="flex gap-6">
          <div className="w-72 shrink-0">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-neutral-400 mb-2">Executions</h2>
            {executionsQuery.isLoading && <SkeletonList rows={4} />}
            {executionsQuery.data?.length === 0 && (
              <EmptyState icon={GitBranch} title="No executions yet" description="Go to Run and save something first." />
            )}
            <ul className="flex flex-col gap-1 max-h-[calc(100vh-13rem)] overflow-y-auto pr-1">
              {executionsQuery.data?.map((execution) => (
                <li key={execution.id}>
                  <button
                    className={cn(
                      'w-full text-left rounded-md px-2.5 py-2 text-sm transition-colors',
                      execution.id === selectedExecutionId
                        ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900'
                        : 'hover:bg-neutral-100 dark:hover:bg-neutral-800',
                    )}
                    onClick={() => setSelectedExecutionId(execution.id)}
                  >
                    <div className="flex justify-between items-center">
                      <StatusBadge status={execution.status} />
                      <span className="text-xs opacity-70">{execution.durationMs ?? '…'} ms</span>
                    </div>
                    <div className="text-xs opacity-70 mt-1">{new Date(execution.startedAt).toLocaleString()}</div>
                  </button>
                </li>
              ))}
            </ul>
          </div>

          <div className="flex-1">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-neutral-400 mb-2">Live flow</h2>
            {!selectedExecutionId && (
              <Card>
                <EmptyState icon={GitBranch} title="Select an execution to inspect it" />
              </Card>
            )}
            {selectedExecution && eventsQuery.data && (
              <LineageFlowView execution={selectedExecution} events={eventsQuery.data} />
            )}
          </div>
        </div>
      )}
    </div>
  )
}
