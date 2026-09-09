import { useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { ArrowUpRight, CheckCircle2, Play, XCircle } from 'lucide-react'
import { toast } from 'sonner'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import { listApis } from '../../services/apiDesigner'
import { listMappings } from '../../services/mappings'
import { listComponents } from '../../services/components'
import { executeApi, type RuntimeExecutionResult } from '../../services/runtime'
import { extractErrorMessage } from '../../utils/errors'
import { Button, Card, CardBody, EmptyState, Input, Select } from '../../components/ui'

export function RuntimePage() {
  const { applicationId } = useSelectedApplication()

  const apisQuery = useQuery({
    queryKey: ['apis', applicationId],
    queryFn: () => listApis(applicationId!),
    enabled: !!applicationId,
  })
  const mappingsQuery = useQuery({
    queryKey: ['mappings', applicationId],
    queryFn: () => listMappings(applicationId!),
    enabled: !!applicationId,
  })
  const componentsQuery = useQuery({
    queryKey: ['components', applicationId, null],
    queryFn: () => listComponents(applicationId!),
    enabled: !!applicationId,
  })

  const [apiId, setApiId] = useState('')
  const [values, setValues] = useState<Record<string, string>>({})
  const [result, setResult] = useState<RuntimeExecutionResult | null>(null)
  const [requestError, setRequestError] = useState<string | null>(null)

  const fieldMappings = useMemo(
    () => (mappingsQuery.data ?? []).filter((m) => m.apiId === apiId),
    [mappingsQuery.data, apiId],
  )

  const executeMutation = useMutation({
    mutationFn: () => executeApi(applicationId!, apiId, values),
    onSuccess: (r) => {
      setResult(r)
      setRequestError(null)
      if (r.success) toast.success('Execution succeeded')
      else toast.error(`Execution failed: ${r.errorCode}`)
    },
    onError: (err) => {
      setResult(null)
      setRequestError(extractErrorMessage(err))
    },
  })

  return (
    <div className="p-6 max-w-2xl">
      <PageHeader title="Run" description="Invoke a real API and watch the actual data flow through your app." />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <>
          <div className="mb-4">
            <Select
              value={apiId}
              onChange={(e) => {
                setApiId(e.target.value)
                setValues({})
                setResult(null)
              }}
            >
              <option value="">Select an API to run…</option>
              {apisQuery.data?.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.method.toUpperCase()} {a.path}
                </option>
              ))}
            </Select>
          </div>

          {apiId && fieldMappings.length === 0 && (
            <EmptyState
              icon={Play}
              title="No field mappings for this API yet"
              description="Add mappings in the Mapping Designer first."
            />
          )}

          {apiId && fieldMappings.length > 0 && (
            <Card className="mb-6">
              <form
                className="flex flex-col gap-3 p-4"
                onSubmit={(e) => {
                  e.preventDefault()
                  executeMutation.mutate()
                }}
              >
                {fieldMappings.map((mapping) => {
                  const component = componentsQuery.data?.find((c) => c.id === mapping.sourceComponentId)
                  return (
                    <label key={mapping.id} className="text-sm">
                      <span className="block text-xs text-neutral-500 mb-1">
                        {component?.name ?? mapping.sourceField} ({mapping.apiField})
                      </span>
                      <Input
                        className="w-full"
                        value={values[mapping.apiField] ?? ''}
                        onChange={(e) => setValues({ ...values, [mapping.apiField]: e.target.value })}
                      />
                    </label>
                  )
                })}
                <Button type="submit" variant="primary" className="self-start" loading={executeMutation.isPending}>
                  <Play size={14} /> Save
                </Button>
              </form>
            </Card>
          )}

          {requestError && <p className="text-sm text-red-600 mb-4">{requestError}</p>}

          {result && (
            <Card
              className={
                result.success
                  ? 'border-green-300 bg-green-50 dark:bg-green-950 dark:border-green-800'
                  : 'border-red-300 bg-red-50 dark:bg-red-950 dark:border-red-800'
              }
            >
              <CardBody>
                <div className="flex items-center justify-between mb-2">
                  <span
                    className={`flex items-center gap-1.5 font-semibold ${
                      result.success ? 'text-green-700 dark:text-green-300' : 'text-red-700 dark:text-red-300'
                    }`}
                  >
                    {result.success ? <CheckCircle2 size={16} /> : <XCircle size={16} />}
                    {result.success ? '✓ SUCCESS' : '✗ FAILED'}
                  </span>
                  <Link
                    to="/lineage"
                    className="text-xs underline text-neutral-600 dark:text-neutral-300 flex items-center gap-0.5"
                  >
                    View live lineage <ArrowUpRight size={12} />
                  </Link>
                </div>

                {!result.success && (
                  <p className="text-sm text-red-700 dark:text-red-300 mb-2">
                    [{result.errorCode}] {result.errorMessage}
                  </p>
                )}

                {result.fields.length > 0 && (
                  <table className="w-full text-xs mb-2">
                    <thead>
                      <tr className="text-left text-neutral-500">
                        <th className="pr-4 pb-1 font-medium">API field</th>
                        <th className="pr-4 pb-1 font-medium">Column</th>
                        <th className="pr-4 pb-1 font-medium">Raw</th>
                        <th className="pb-1 font-medium">Transformed</th>
                      </tr>
                    </thead>
                    <tbody>
                      {result.fields.map((f) => (
                        <tr key={f.apiField} className="border-t border-black/10">
                          <td className="pr-4 py-1">{f.apiField}</td>
                          <td className="pr-4 py-1">{f.columnName}</td>
                          <td className="pr-4 py-1 text-neutral-500">{f.rawValue ?? '∅'}</td>
                          <td className="py-1">{f.transformedValue ?? '∅'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}

                {result.rows.length > 0 && (
                  <pre className="text-xs bg-black/5 dark:bg-white/5 rounded p-2 overflow-auto">
                    {JSON.stringify(result.rows, null, 2)}
                  </pre>
                )}
              </CardBody>
            </Card>
          )}
        </>
      )}
    </div>
  )
}
