import { useEffect, useMemo, useState } from 'react'
import { useQueries, useQuery } from '@tanstack/react-query'
import { Link, useLocation, useParams } from 'react-router-dom'
import { CheckCircle2, LayoutGrid, Loader2, XCircle } from 'lucide-react'
import { toast } from 'sonner'
import {
  executePublicApi,
  getPublicApp,
  listPublicComponents,
  listPublicMappings,
  listPublicScreens,
  listPublicTableRows,
  listPublicTables,
} from '../../services/publicApp'
import type { RuntimeExecutionResult } from '../../services/runtime'
import type { DataColumn, DataTable, UiComponent } from '../../types'
import { extractErrorMessage } from '../../utils/errors'
import { Button, Card, CardBody, EmptyState, Input, Select } from '../../components/ui'

const NON_FIELD_TYPES = new Set(['Button', 'Table', 'Card', 'Form', 'Container', 'Label'])
const TEXTUAL_TYPES = new Set(['Varchar', 'Text'])

function inputTypeFor(componentType: string): string {
  switch (componentType) {
    case 'Email':
      return 'email'
    case 'Number':
      return 'number'
    case 'Date':
      return 'date'
    case 'Checkbox':
      return 'checkbox'
    default:
      return 'text'
  }
}

interface ForeignKeyInfo {
  referencedTable: DataTable
  valueColumn: DataColumn
  labelColumnName: string
}

function findColumn(tables: DataTable[], columnId: string): { table: DataTable; column: DataColumn } | null {
  for (const table of tables) {
    const column = table.columns.find((c) => c.id === columnId)
    if (column) return { table, column }
  }
  return null
}

/** If the mapped column is a foreign key, resolves which table/column to pick a related row from,
 *  and which of that table's columns makes the best human-readable label for each row. */
function resolveForeignKey(tables: DataTable[], column: DataColumn): ForeignKeyInfo | null {
  if (!column.isForeignKey || !column.referencesTableId) return null
  const referencedTable = tables.find((t) => t.id === column.referencesTableId)
  if (!referencedTable) return null

  const valueColumn = referencedTable.columns.find((c) => c.id === column.referencesColumnId)
    ?? referencedTable.columns.find((c) => c.isPrimaryKey)
  if (!valueColumn) return null

  const labelColumn =
    referencedTable.columns.find((c) => c.name.toLowerCase() === 'name' && c.id !== valueColumn.id) ??
    referencedTable.columns.find((c) => TEXTUAL_TYPES.has(c.dataType) && !c.isPrimaryKey && c.id !== valueColumn.id)

  return { referencedTable, valueColumn, labelColumnName: labelColumn?.name ?? valueColumn.name }
}

/**
 * The end-user-facing surface for a published application - no LineageStudio login. It renders
 * the screen's real components as an actual form (not the builder's generic Run-page form), wires
 * each field to whichever API its mappings point to, and submits via the public, unauthenticated
 * runtime endpoint. A field mapped to a foreign-key column gets a picker of real related rows
 * (e.g. "which customer") instead of asking someone to type a raw UUID by hand. Reachable at
 * /apps/:applicationId/<screen route>.
 */
export function PublicAppPage() {
  const { applicationId = '' } = useParams()
  const location = useLocation()

  const appQuery = useQuery({
    queryKey: ['public-app', applicationId],
    queryFn: () => getPublicApp(applicationId),
    enabled: !!applicationId,
    retry: false,
  })

  const screensQuery = useQuery({
    queryKey: ['public-screens', applicationId],
    queryFn: () => listPublicScreens(applicationId),
    enabled: !!applicationId && appQuery.isSuccess,
  })

  const mappingsQuery = useQuery({
    queryKey: ['public-mappings', applicationId],
    queryFn: () => listPublicMappings(applicationId),
    enabled: !!applicationId && appQuery.isSuccess,
  })

  const tablesQuery = useQuery({
    queryKey: ['public-tables', applicationId],
    queryFn: () => listPublicTables(applicationId),
    enabled: !!applicationId && appQuery.isSuccess,
  })

  // Small demo-scale apps only - fetches every table's rows up front so any field's FK picker
  // (however many tables it ends up referencing) is ready without a chain of sequential fetches.
  const tableRowsQueries = useQueries({
    queries: (tablesQuery.data ?? []).map((table) => ({
      queryKey: ['public-table-rows', applicationId, table.id],
      queryFn: () => listPublicTableRows(applicationId, table.id),
      enabled: !!tablesQuery.data,
    })),
  })

  const rowsByTableId = useMemo(() => {
    const map = new Map<string, Record<string, unknown>[]>()
    ;(tablesQuery.data ?? []).forEach((table, i) => {
      map.set(table.id, tableRowsQueries[i]?.data ?? [])
    })
    return map
  }, [tablesQuery.data, tableRowsQueries])

  const prefix = `/apps/${applicationId}`
  const requestedRoute = location.pathname.startsWith(prefix) ? location.pathname.slice(prefix.length) || '/' : '/'
  const screen =
    screensQuery.data?.find((s) => s.route === requestedRoute) ?? screensQuery.data?.[0] ?? null

  const componentsQuery = useQuery({
    queryKey: ['public-components', applicationId, screen?.id],
    queryFn: () => listPublicComponents(applicationId, screen!.id),
    enabled: !!screen,
  })

  const [values, setValues] = useState<Record<string, string>>({})
  const [result, setResult] = useState<RuntimeExecutionResult | null>(null)
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    setValues({})
    setResult(null)
    setSubmitError(null)
  }, [screen?.id])

  const screenMappings = useMemo(() => {
    if (!componentsQuery.data || !mappingsQuery.data) return []
    const componentIds = new Set(componentsQuery.data.map((c) => c.id))
    return mappingsQuery.data.filter((m) => componentIds.has(m.sourceComponentId))
  }, [componentsQuery.data, mappingsQuery.data])

  // A screen's components can only ever really submit to one API in practice - pick whichever
  // API most of this screen's mappings actually point to, so one stray mapping can't hijack it.
  const apiId = useMemo(() => {
    const counts = new Map<string, number>()
    for (const m of screenMappings) counts.set(m.apiId, (counts.get(m.apiId) ?? 0) + 1)
    let best: string | null = null
    let bestCount = 0
    for (const [id, count] of counts) {
      if (count > bestCount) {
        best = id
        bestCount = count
      }
    }
    return best
  }, [screenMappings])

  interface Field {
    component: UiComponent
    apiField: string
    fk: ForeignKeyInfo | null
  }

  const fields = useMemo<Field[]>(() => {
    if (!componentsQuery.data || !tablesQuery.data) return []
    return componentsQuery.data
      .filter((c) => !NON_FIELD_TYPES.has(c.type))
      .map((component) => {
        const mapping = screenMappings.find((m) => m.sourceComponentId === component.id)
        if (!mapping || mapping.apiId !== apiId) return null
        const columnMatch = findColumn(tablesQuery.data!, mapping.columnId)
        const fk = columnMatch ? resolveForeignKey(tablesQuery.data!, columnMatch.column) : null
        return { component, apiField: mapping.apiField, fk }
      })
      .filter((f): f is Field => f !== null)
      .sort((a, b) => a.component.position.y - b.component.position.y)
  }, [componentsQuery.data, tablesQuery.data, screenMappings, apiId])

  if (appQuery.isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-neutral-50 dark:bg-neutral-950">
        <Loader2 className="animate-spin text-neutral-400" size={24} />
      </div>
    )
  }

  if (appQuery.isError || !appQuery.data) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-neutral-50 dark:bg-neutral-950 p-6">
        <EmptyState
          icon={LayoutGrid}
          title="This app isn't available"
          description="It may not exist, or hasn't been published yet."
        />
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-neutral-50 dark:bg-neutral-950">
      <header className="border-b border-neutral-200 dark:border-neutral-800 bg-white dark:bg-neutral-900 px-6 py-4">
        <h1 className="text-lg font-semibold text-neutral-900 dark:text-neutral-100">{appQuery.data.name}</h1>
        {appQuery.data.description && (
          <p className="text-sm text-neutral-500 mt-0.5">{appQuery.data.description}</p>
        )}

        {screensQuery.data && screensQuery.data.length > 1 && (
          <nav className="flex gap-1 mt-3 flex-wrap">
            {screensQuery.data.map((s) => (
              <Link
                key={s.id}
                to={`${prefix}${s.route}`}
                className={`rounded-md px-2.5 py-1 text-sm transition-colors ${
                  s.id === screen?.id
                    ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900'
                    : 'text-neutral-600 hover:bg-neutral-100 dark:text-neutral-300 dark:hover:bg-neutral-800'
                }`}
              >
                {s.name}
              </Link>
            ))}
          </nav>
        )}
      </header>

      <main className="p-6 max-w-lg mx-auto">
        {!screen && (
          <EmptyState icon={LayoutGrid} title="No screens yet" description="This app has nothing to show yet." />
        )}

        {screen && (componentsQuery.isLoading || tablesQuery.isLoading) && (
          <p className="text-sm text-neutral-400">Loading…</p>
        )}

        {screen && componentsQuery.data && tablesQuery.data && fields.length === 0 && (
          <EmptyState
            icon={LayoutGrid}
            title="Nothing to fill in here"
            description="This screen isn't wired to an API yet."
          />
        )}

        {screen && fields.length > 0 && (
          <Card>
            <CardBody>
              <form
                className="flex flex-col gap-3"
                onSubmit={async (e) => {
                  e.preventDefault()
                  if (!apiId) return
                  setSubmitting(true)
                  setSubmitError(null)
                  try {
                    const formData: Record<string, unknown> = {}
                    for (const { component, apiField, fk } of fields) {
                      const raw = values[component.id] ?? ''
                      formData[apiField] = !fk && component.type === 'Number' && raw !== '' ? Number(raw) : raw
                    }
                    const r = await executePublicApi(applicationId, apiId, formData)
                    setResult(r)
                    if (r.success) toast.success('Submitted')
                    else toast.error(`Failed: ${r.errorCode}`)
                  } catch (err) {
                    setSubmitError(extractErrorMessage(err))
                  } finally {
                    setSubmitting(false)
                  }
                }}
              >
                {fields.map(({ component, fk }) => (
                  <label key={component.id} className="text-sm flex flex-col gap-1">
                    <span className="text-xs text-neutral-500">{component.name}</span>
                    {fk ? (
                      <Select
                        required
                        value={values[component.id] ?? ''}
                        onChange={(e) => setValues((v) => ({ ...v, [component.id]: e.target.value }))}
                      >
                        <option value="">Select {fk.referencedTable.name}…</option>
                        {(rowsByTableId.get(fk.referencedTable.id) ?? []).map((row) => {
                          const value = String(row[fk.valueColumn.name] ?? '')
                          const label = row[fk.labelColumnName]
                          return (
                            <option key={value} value={value}>
                              {label != null ? String(label) : value}
                            </option>
                          )
                        })}
                      </Select>
                    ) : (
                      <Input
                        type={inputTypeFor(component.type)}
                        required
                        value={values[component.id] ?? ''}
                        onChange={(e) => setValues((v) => ({ ...v, [component.id]: e.target.value }))}
                      />
                    )}
                  </label>
                ))}

                <Button type="submit" variant="primary" loading={submitting} className="justify-center mt-1">
                  Submit
                </Button>
                {submitError && <p className="text-sm text-red-600">{submitError}</p>}
              </form>
            </CardBody>
          </Card>
        )}

        {result && (
          <Card
            className={`mt-4 ${
              result.success
                ? 'border-green-300 bg-green-50 dark:bg-green-950 dark:border-green-800'
                : 'border-red-300 bg-red-50 dark:bg-red-950 dark:border-red-800'
            }`}
          >
            <CardBody>
              <span
                className={`flex items-center gap-1.5 font-semibold text-sm ${
                  result.success ? 'text-green-700 dark:text-green-300' : 'text-red-700 dark:text-red-300'
                }`}
              >
                {result.success ? <CheckCircle2 size={16} /> : <XCircle size={16} />}
                {result.success ? 'Submitted successfully' : 'Submission failed'}
              </span>
              {!result.success && (
                <p className="text-sm text-red-700 dark:text-red-300 mt-1">
                  [{result.errorCode}] {result.errorMessage}
                </p>
              )}
            </CardBody>
          </Card>
        )}
      </main>
    </div>
  )
}
