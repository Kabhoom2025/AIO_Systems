import { useParams, Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { getExecution, listEvents } from '../../services/lineage'
import { SkeletonList, StatusBadge } from '../../components/ui'
import { LineageFlowView } from '../lineage/LineageFlowView'

export function ExecutionDetailPage() {
  const { executionId } = useParams<{ executionId: string }>()

  const executionQuery = useQuery({
    queryKey: ['execution', executionId],
    queryFn: () => getExecution(executionId!),
    enabled: !!executionId,
  })
  const eventsQuery = useQuery({
    queryKey: ['lineage-events', executionId],
    queryFn: () => listEvents(executionId!),
    enabled: !!executionId,
  })

  return (
    <div className="p-6">
      <Link
        to="/executions"
        className="text-xs text-neutral-500 hover:text-neutral-700 dark:hover:text-neutral-300 flex items-center gap-1 w-fit"
      >
        <ArrowLeft size={12} /> Back to Execution History
      </Link>

      {executionQuery.isLoading && <SkeletonList rows={2} />}

      {executionQuery.data && (
        <div className="mt-3 mb-4">
          <div className="flex items-center gap-2">
            <h1 className="text-xl font-semibold text-neutral-900 dark:text-neutral-100">
              {executionQuery.data.applicationName ?? executionQuery.data.applicationId}
            </h1>
            <StatusBadge status={executionQuery.data.status} />
          </div>
          <p className="text-sm text-neutral-500 mt-0.5">
            started {new Date(executionQuery.data.startedAt).toLocaleString()}
            {executionQuery.data.durationMs !== null && <> · {executionQuery.data.durationMs} ms</>}
          </p>
          <p className="text-xs text-neutral-400 font-mono">
            execution {executionQuery.data.id} · correlation {executionQuery.data.correlationId}
          </p>
        </div>
      )}

      {executionQuery.data && eventsQuery.data && (
        <LineageFlowView execution={executionQuery.data} events={eventsQuery.data} />
      )}
    </div>
  )
}
