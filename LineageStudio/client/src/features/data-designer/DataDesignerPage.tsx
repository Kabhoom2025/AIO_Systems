import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ChevronDown, ChevronRight, Database, Plus, Table2, Trash2, X } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import {
  addColumn,
  createTable,
  deleteColumn,
  deleteTable,
  emptyColumn,
  listLatestTableRows,
  listTables,
} from '../../services/tables'
import type { ColumnDefinition, DataTable } from '../../types'
import { extractErrorMessage } from '../../utils/errors'
import { Badge, Button, Card, CardBody, CardHeader, EmptyState, Input, SkeletonList, confirmAction } from '../../components/ui'
import { ColumnFieldsEditor } from './ColumnFieldsEditor'

export function DataDesignerPage() {
  const { applicationId } = useSelectedApplication()
  const queryClient = useQueryClient()

  const [showCreateForm, setShowCreateForm] = useState(false)
  const [tableName, setTableName] = useState('')
  const [columns, setColumns] = useState<ColumnDefinition[]>([{ ...emptyColumn(), isPrimaryKey: false }])
  const [formError, setFormError] = useState<string | null>(null)

  const tablesQuery = useQuery({
    queryKey: ['tables', applicationId],
    queryFn: () => listTables(applicationId!),
    enabled: !!applicationId,
  })

  useEffect(() => {
    if (tablesQuery.data && tablesQuery.data.length === 0) setShowCreateForm(true)
  }, [tablesQuery.data])

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['tables', applicationId] })

  const createMutation = useMutation({
    mutationFn: (input: { name: string; columns: ColumnDefinition[] }) => createTable(applicationId!, input),
    onSuccess: (table) => {
      setTableName('')
      setColumns([{ ...emptyColumn() }])
      setFormError(null)
      setShowCreateForm(false)
      invalidate()
      toast.success(`Table "${table.name}" created in PostgreSQL`)
    },
    onError: (err) => setFormError(extractErrorMessage(err)),
  })

  const deleteTableMutation = useMutation({
    mutationFn: (tableId: string) => deleteTable(applicationId!, tableId),
    onSuccess: () => {
      invalidate()
      toast.success('Table dropped')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const deleteColumnMutation = useMutation({
    mutationFn: ({ tableId, columnId }: { tableId: string; columnId: string }) =>
      deleteColumn(applicationId!, tableId, columnId),
    onSuccess: () => {
      invalidate()
      toast.success('Column removed')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const addColumnMutation = useMutation({
    mutationFn: ({ tableId, column }: { tableId: string; column: ColumnDefinition }) =>
      addColumn(applicationId!, tableId, column),
    onSuccess: () => {
      invalidate()
      toast.success('Column added')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  return (
    <div className="p-6">
      <PageHeader
        title="Data Designer"
        description="Create real PostgreSQL tables and columns for your application."
        actions={
          applicationId &&
          tablesQuery.data &&
          tablesQuery.data.length > 0 && (
            <Button variant="primary" onClick={() => setShowCreateForm((v) => !v)}>
              {showCreateForm ? <X size={14} /> : <Plus size={14} />}
              {showCreateForm ? 'Cancel' : 'New table'}
            </Button>
          )
        }
      />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <>
          {showCreateForm && (
            <Card className="mb-8 max-w-3xl">
              <form
                className="flex flex-col gap-3 p-4"
                onSubmit={(e) => {
                  e.preventDefault()
                  createMutation.mutate({ name: tableName, columns })
                }}
              >
                <div className="flex gap-2 items-center">
                  <Input
                    className="flex-1"
                    placeholder="table_name"
                    value={tableName}
                    onChange={(e) => setTableName(e.target.value)}
                    required
                  />
                  <Button type="submit" variant="primary" loading={createMutation.isPending}>
                    Create table
                  </Button>
                </div>

                <div className="flex flex-col gap-2">
                  {columns.map((column, index) => (
                    <ColumnFieldsEditor
                      key={index}
                      column={column}
                      existingTables={tablesQuery.data ?? []}
                      onChange={(next) => setColumns(columns.map((c, i) => (i === index ? next : c)))}
                      onRemove={columns.length > 1 ? () => setColumns(columns.filter((_, i) => i !== index)) : undefined}
                    />
                  ))}
                </div>

                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="self-start"
                  onClick={() => setColumns([...columns, emptyColumn()])}
                >
                  <Plus size={14} /> Add column
                </Button>

                {formError && <p className="text-sm text-red-600">{formError}</p>}
              </form>
            </Card>
          )}

          {tablesQuery.isLoading && <SkeletonList rows={3} />}

          {tablesQuery.data && tablesQuery.data.length === 0 && !showCreateForm && (
            <EmptyState icon={Database} title="No tables yet" description="Create your first table above." />
          )}

          <div className="grid grid-cols-1 xl:grid-cols-2 gap-4 items-start">
            {tablesQuery.data?.map((table) => (
              <Card key={table.id}>
                <CardHeader className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Table2 size={16} className="text-neutral-400" />
                    <span className="font-medium text-neutral-900 dark:text-neutral-100">{table.name}</span>
                    <span className="text-xs text-neutral-500 font-mono">{table.schemaName}</span>
                  </div>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={async () => {
                      const confirmed = await confirmAction({
                        title: `Drop table "${table.name}"?`,
                        description: 'This deletes the real PostgreSQL table and all its data. This cannot be undone.',
                        confirmLabel: 'Drop table',
                        danger: true,
                      })
                      if (confirmed) deleteTableMutation.mutate(table.id)
                    }}
                  >
                    <Trash2 size={14} /> Drop table
                  </Button>
                </CardHeader>

                <CardBody>
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-left text-neutral-500">
                        <th className="pr-4 pb-2 font-medium">Column</th>
                        <th className="pr-4 pb-2 font-medium">Type</th>
                        <th className="pr-4 pb-2 font-medium">Flags</th>
                        <th />
                      </tr>
                    </thead>
                    <tbody>
                      {table.columns.map((column) => (
                        <tr key={column.id} className="border-t border-neutral-100 dark:border-neutral-900">
                          <td className="py-1.5 pr-4 font-mono text-xs">{column.name}</td>
                          <td className="py-1.5 pr-4 text-neutral-600 dark:text-neutral-400">
                            {column.dataType}
                            {column.length ? `(${column.length})` : ''}
                          </td>
                          <td className="py-1.5 pr-4">
                            <div className="flex flex-wrap gap-1">
                              {column.isPrimaryKey && <Badge color="blue">PK</Badge>}
                              {column.isForeignKey && <Badge color="amber">FK</Badge>}
                              {column.isUnique && <Badge>UNIQUE</Badge>}
                              {column.isIndexed && <Badge>INDEX</Badge>}
                              {!column.isNullable && <Badge>NOT NULL</Badge>}
                            </div>
                          </td>
                          <td className="py-1.5 text-right">
                            <button
                              className="text-xs text-red-600 hover:underline"
                              onClick={() => deleteColumnMutation.mutate({ tableId: table.id, columnId: column.id })}
                            >
                              Remove
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>

                  <AddColumnRow
                    existingTables={tablesQuery.data ?? []}
                    onAdd={(column) => addColumnMutation.mutate({ tableId: table.id, column })}
                  />

                  <LatestRowsPanel applicationId={applicationId!} table={table} />
                </CardBody>
              </Card>
            ))}
          </div>
        </>
      )}
    </div>
  )
}

function AddColumnRow({
  existingTables,
  onAdd,
}: {
  existingTables: DataTable[]
  onAdd: (column: ColumnDefinition) => void
}) {
  const [open, setOpen] = useState(false)
  const [column, setColumn] = useState<ColumnDefinition>(emptyColumn())

  if (!open) {
    return (
      <Button variant="ghost" size="sm" className="mt-2" onClick={() => setOpen(true)}>
        <Plus size={14} /> Add column
      </Button>
    )
  }

  return (
    <div className="mt-2 flex flex-col gap-2">
      <ColumnFieldsEditor column={column} existingTables={existingTables} onChange={setColumn} />
      <div className="flex gap-2">
        <Button
          variant="primary"
          size="sm"
          onClick={() => {
            onAdd(column)
            setColumn(emptyColumn())
            setOpen(false)
          }}
        >
          Add
        </Button>
        <Button variant="ghost" size="sm" onClick={() => setOpen(false)}>
          Cancel
        </Button>
      </div>
    </div>
  )
}

/** "What data has actually flowed into the database" - the real, most-recently-written rows of
 *  this table, polled while the panel is open so a submission elsewhere shows up here live. */
function LatestRowsPanel({ applicationId, table }: { applicationId: string; table: DataTable }) {
  const [open, setOpen] = useState(false)

  const rowsQuery = useQuery({
    queryKey: ['latest-rows', applicationId, table.id],
    queryFn: () => listLatestTableRows(applicationId, table.id, 10),
    enabled: open,
    refetchInterval: open ? 4000 : false,
  })

  const columnNames = table.columns.map((c) => c.name)

  return (
    <div className="mt-3 border-t border-neutral-100 dark:border-neutral-900 pt-2">
      <button
        className="flex items-center gap-1 text-xs font-medium text-neutral-600 dark:text-neutral-300 hover:underline"
        onClick={() => setOpen((v) => !v)}
      >
        {open ? <ChevronDown size={12} /> : <ChevronRight size={12} />}
        {open ? 'Hide latest data' : 'View latest data'}
      </button>

      {open && (
        <div className="mt-2">
          {rowsQuery.isLoading && <p className="text-xs text-neutral-400">Loading…</p>}

          {rowsQuery.data && rowsQuery.data.length === 0 && (
            <p className="text-xs text-neutral-400">No rows yet - nothing has flowed in.</p>
          )}

          {rowsQuery.data && rowsQuery.data.length > 0 && (
            <div className="overflow-x-auto rounded border border-neutral-100 dark:border-neutral-900">
              <table className="w-full text-xs">
                <thead>
                  <tr className="text-left text-neutral-500 bg-neutral-50 dark:bg-neutral-900/50">
                    {columnNames.map((name) => (
                      <th key={name} className="px-2 py-1 font-mono font-medium whitespace-nowrap">
                        {name}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {rowsQuery.data.map((row, i) => (
                    <tr key={i} className="border-t border-neutral-100 dark:border-neutral-900">
                      {columnNames.map((name) => (
                        <td key={name} className="px-2 py-1 whitespace-nowrap">
                          {row[name] === null || row[name] === undefined ? (
                            <span className="text-neutral-400">∅</span>
                          ) : (
                            String(row[name])
                          )}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <p className="mt-1 text-[10px] text-neutral-400">Newest first · refreshes every few seconds</p>
        </div>
      )}
    </div>
  )
}
