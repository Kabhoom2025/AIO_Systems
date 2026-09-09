import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ArrowLeftRight, Monitor, Radar, Server, Webhook } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import { listTables } from '../../services/tables'
import { getColumnImpact, getTableImpact } from '../../services/impact'
import { Card, CardBody, CardHeader, Select, SkeletonList } from '../../components/ui'

export function ImpactAnalysisPage() {
  const { applicationId } = useSelectedApplication()
  const [tableId, setTableId] = useState('')
  const [columnId, setColumnId] = useState('')

  const tablesQuery = useQuery({
    queryKey: ['tables', applicationId],
    queryFn: () => listTables(applicationId!),
    enabled: !!applicationId,
  })

  const tableImpactQuery = useQuery({
    queryKey: ['table-impact', tableId],
    queryFn: () => getTableImpact(tableId),
    enabled: !!tableId,
  })

  const columnImpactQuery = useQuery({
    queryKey: ['column-impact', columnId],
    queryFn: () => getColumnImpact(columnId),
    enabled: !!columnId,
  })

  const selectedTable = tablesQuery.data?.find((t) => t.id === tableId)
  const loadingImpact = tableId ? tableImpactQuery.isLoading : columnId ? columnImpactQuery.isLoading : false

  return (
    <div className="p-6 max-w-3xl">
      <PageHeader
        title="Impact Analysis"
        description="See what depends on a table or column before you change it."
      />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <>
          <div className="flex gap-2 mb-6">
            <Select
              value={tableId}
              onChange={(e) => {
                setTableId(e.target.value)
                setColumnId('')
              }}
            >
              <option value="">Select a table…</option>
              {tablesQuery.data?.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </Select>
            <Select value={columnId} onChange={(e) => setColumnId(e.target.value)} disabled={!selectedTable}>
              <option value="">Whole table</option>
              {selectedTable?.columns.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </Select>
          </div>

          {loadingImpact && <SkeletonList rows={4} />}

          {tableId && !columnId && tableImpactQuery.data && (
            <div className="flex flex-col gap-4">
              <Section icon={Monitor} title={`Which screens use "${tableImpactQuery.data.tableName}"?`}>
                {tableImpactQuery.data.screens.length === 0 && <Empty />}
                {tableImpactQuery.data.screens.map((s) => (
                  <li key={s.id}>{s.name} <span className="text-neutral-500">{s.route}</span></li>
                ))}
              </Section>

              <Section icon={Webhook} title={`Which APIs use "${tableImpactQuery.data.tableName}"?`}>
                {tableImpactQuery.data.apis.length === 0 && <Empty />}
                {tableImpactQuery.data.apis.map((a) => (
                  <li key={a.id}>
                    <span className="uppercase text-xs bg-neutral-100 dark:bg-neutral-800 rounded px-1.5 py-0.5 mr-2">{a.method}</span>
                    {a.path}
                  </li>
                ))}
              </Section>

              <Section icon={Server} title="Which services use it?">
                {tableImpactQuery.data.services.length === 0 && <Empty />}
                {tableImpactQuery.data.services.map((s) => (
                  <li key={s.id}>{s.name}</li>
                ))}
              </Section>

              <Section icon={ArrowLeftRight} title={`Which tables are affected if "${tableImpactQuery.data.tableName}" changes?`}>
                {tableImpactQuery.data.referencingTables.length === 0 && <Empty />}
                {tableImpactQuery.data.referencingTables.map((t) => (
                  <li key={t.id}>{t.name} (has a foreign key into this table)</li>
                ))}
              </Section>

              <Section icon={ArrowLeftRight} title="Which tables does this table depend on?">
                {tableImpactQuery.data.referencedTables.length === 0 && <Empty />}
                {tableImpactQuery.data.referencedTables.map((t) => (
                  <li key={t.id}>{t.name}</li>
                ))}
              </Section>
            </div>
          )}

          {columnId && columnImpactQuery.data && (
            <div className="flex flex-col gap-4">
              <Section icon={Monitor} title={`Which UI fields use "${columnImpactQuery.data.tableName}.${columnImpactQuery.data.columnName}"?`}>
                {columnImpactQuery.data.components.length === 0 && <Empty />}
                {columnImpactQuery.data.components.map((c) => (
                  <li key={c.id}>{c.name} <span className="text-neutral-500">on {c.screenName}</span></li>
                ))}
              </Section>

              <Section icon={Webhook} title="Which API fields use it?">
                {columnImpactQuery.data.apiFields.length === 0 && <Empty />}
                {columnImpactQuery.data.apiFields.map((f) => (
                  <li key={`${f.apiId}-${f.apiField}`}>
                    <span className="uppercase text-xs bg-neutral-100 dark:bg-neutral-800 rounded px-1.5 py-0.5 mr-2">{f.method}</span>
                    {f.path}.{f.apiField}
                  </li>
                ))}
              </Section>

              <Section icon={Server} title="Which service fields use it?">
                {columnImpactQuery.data.serviceFields.length === 0 && <Empty />}
                {columnImpactQuery.data.serviceFields.map((f) => (
                  <li key={`${f.serviceId}-${f.serviceField}`}>{f.serviceName}.{f.serviceField}</li>
                ))}
              </Section>

              <Section icon={ArrowLeftRight} title="What breaks if this column changes?">
                {columnImpactQuery.data.dependentColumns.length === 0 && <Empty />}
                {columnImpactQuery.data.dependentColumns.map((c) => (
                  <li key={c.id}>{c.tableName}.{c.name} (foreign key references this column)</li>
                ))}
              </Section>
            </div>
          )}

          {!tableId && !columnId && (
            <Card>
              <CardBody className="flex flex-col items-center gap-2 py-10 text-center">
                <Radar size={28} className="text-neutral-300 dark:text-neutral-700" />
                <p className="text-sm text-neutral-500">Select a table (or one of its columns) above to see its impact.</p>
              </CardBody>
            </Card>
          )}
        </>
      )}
    </div>
  )
}

function Section({
  icon: Icon,
  title,
  children,
}: {
  icon: React.ComponentType<{ size?: number; className?: string }>
  title: string
  children: React.ReactNode
}) {
  return (
    <Card>
      <CardHeader className="flex items-center gap-2">
        <Icon size={15} className="text-neutral-400" />
        <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">{title}</h2>
      </CardHeader>
      <CardBody>
        <ul className="text-sm flex flex-col gap-1 list-disc list-inside">{children}</ul>
      </CardBody>
    </Card>
  )
}

function Empty() {
  return <li className="text-neutral-500 list-none">Nothing found.</li>
}
