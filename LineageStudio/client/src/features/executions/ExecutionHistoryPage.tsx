import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { History, Search } from 'lucide-react'
import { listAllExecutions } from '../../services/lineage'
import { PageHeader } from '../../components/PageHeader'
import { Card, EmptyState, Input, Select, SkeletonList, StatusBadge } from '../../components/ui'
import type { ExecutionStatus } from '../../types'

const STATUSES: ExecutionStatus[] = ['Success', 'Failed', 'Running', 'Skipped', 'Pending']

export function ExecutionHistoryPage() {
  const executionsQuery = useQuery({ queryKey: ['all-executions'], queryFn: listAllExecutions })
  const [appFilter, setAppFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [search, setSearch] = useState('')

  const applicationNames = useMemo(
    () => Array.from(new Set((executionsQuery.data ?? []).map((e) => e.applicationName).filter(Boolean))) as string[],
    [executionsQuery.data],
  )

  const filtered = useMemo(
    () =>
      (executionsQuery.data ?? []).filter((e) => {
        if (appFilter && e.applicationName !== appFilter) return false
        if (statusFilter && e.status !== statusFilter) return false
        if (search) {
          const q = search.trim().toLowerCase()
          if (!e.id.toLowerCase().includes(q) && !e.correlationId.toLowerCase().includes(q)) return false
        }
        return true
      }),
    [executionsQuery.data, appFilter, statusFilter, search],
  )

  return (
    <div className="p-6 max-w-4xl">
      <PageHeader title="Execution History" description="Every past execution, across every application." />

      {executionsQuery.isLoading && <SkeletonList rows={4} />}

      {executionsQuery.data?.length === 0 && (
        <EmptyState
          icon={History}
          title="No executions yet"
          description="Go to Run and save something first."
        />
      )}

      {executionsQuery.data && executionsQuery.data.length > 0 && (
        <div className="flex flex-wrap gap-2 mb-4">
          <div className="relative">
            <Search size={14} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-neutral-400" />
            <Input
              className="pl-8 w-56"
              placeholder="Search execution or correlation ID…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Select value={appFilter} onChange={(e) => setAppFilter(e.target.value)}>
            <option value="">All applications</option>
            {applicationNames.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </Select>
          <Select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">All statuses</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </Select>
        </div>
      )}

      {executionsQuery.data && executionsQuery.data.length > 0 && filtered.length === 0 && (
        <EmptyState icon={Search} title="No executions match your filters" />
      )}

      {filtered.length > 0 && (
        <Card>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-neutral-500 border-b border-neutral-200 dark:border-neutral-800">
                <th className="pr-4 py-2 pl-4 font-medium">Execution ID</th>
                <th className="pr-4 py-2 font-medium">Application</th>
                <th className="pr-4 py-2 font-medium">Started</th>
                <th className="pr-4 py-2 font-medium">Duration</th>
                <th className="py-2 pr-4 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((execution) => (
                <tr key={execution.id} className="border-t border-neutral-100 dark:border-neutral-900">
                  <td className="py-2 pr-4 pl-4">
                    <Link to={`/executions/${execution.id}`} className="font-mono text-xs text-indigo-600 dark:text-indigo-400 hover:underline">
                      {execution.id.slice(0, 8)}
                    </Link>
                  </td>
                  <td className="py-2 pr-4">{execution.applicationName ?? execution.applicationId}</td>
                  <td className="py-2 pr-4">{new Date(execution.startedAt).toLocaleString()}</td>
                  <td className="py-2 pr-4">{execution.durationMs ?? '…'} ms</td>
                  <td className="py-2 pr-4">
                    <StatusBadge status={execution.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}
    </div>
  )
}
