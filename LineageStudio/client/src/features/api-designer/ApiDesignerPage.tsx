import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { ArrowRight, Plus, Server, Trash2, Webhook } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import { listTables } from '../../services/tables'
import {
  createApi,
  createService,
  deleteApi,
  deleteService,
  listApis,
  listServices,
  type ApiEndpointInput,
} from '../../services/apiDesigner'
import type { HttpMethod } from '../../types'
import { extractErrorMessage } from '../../utils/errors'
import { Button, Card, CardBody, CardHeader, EmptyState, Input, Select, Textarea, confirmAction } from '../../components/ui'
import { cn } from '../../utils/cn'

const HTTP_METHODS: HttpMethod[] = ['Get', 'Post', 'Put', 'Patch', 'Delete']

const METHOD_BADGE_CLASSES: Record<HttpMethod, string> = {
  Get: 'bg-blue-100 text-blue-800 dark:bg-blue-950 dark:text-blue-300',
  Post: 'bg-green-100 text-green-800 dark:bg-green-950 dark:text-green-300',
  Put: 'bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300',
  Patch: 'bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300',
  Delete: 'bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-300',
}

function MethodBadge({ method }: { method: HttpMethod }) {
  return (
    <span className={cn('font-mono text-[10px] font-semibold uppercase rounded px-1.5 py-0.5', METHOD_BADGE_CLASSES[method])}>
      {method}
    </span>
  )
}

export function ApiDesignerPage() {
  const { applicationId } = useSelectedApplication()
  const queryClient = useQueryClient()

  const tablesQuery = useQuery({
    queryKey: ['tables', applicationId],
    queryFn: () => listTables(applicationId!),
    enabled: !!applicationId,
  })
  const servicesQuery = useQuery({
    queryKey: ['services', applicationId],
    queryFn: () => listServices(applicationId!),
    enabled: !!applicationId,
  })
  const apisQuery = useQuery({
    queryKey: ['apis', applicationId],
    queryFn: () => listApis(applicationId!),
    enabled: !!applicationId,
  })

  const [serviceName, setServiceName] = useState('')
  const [serviceTableId, setServiceTableId] = useState('')
  const [serviceError, setServiceError] = useState<string | null>(null)

  const createServiceMutation = useMutation({
    mutationFn: () =>
      createService(applicationId!, { name: serviceName, tableId: serviceTableId || null, description: null }),
    onSuccess: (service) => {
      setServiceName('')
      setServiceTableId('')
      setServiceError(null)
      queryClient.invalidateQueries({ queryKey: ['services', applicationId] })
      toast.success(`Service "${service.name}" created`)
    },
    onError: (err) => setServiceError(extractErrorMessage(err)),
  })

  const deleteServiceMutation = useMutation({
    mutationFn: (serviceId: string) => deleteService(applicationId!, serviceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['services', applicationId] })
      toast.success('Service deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const [method, setMethod] = useState<HttpMethod>('Get')
  const [path, setPath] = useState('')
  const [apiServiceId, setApiServiceId] = useState('')
  const [apiTableId, setApiTableId] = useState('')
  const [requestSchema, setRequestSchema] = useState('')
  const [responseSchema, setResponseSchema] = useState('')
  const [apiError, setApiError] = useState<string | null>(null)

  const createApiMutation = useMutation({
    mutationFn: () => {
      const input: ApiEndpointInput = {
        method,
        path,
        requestSchemaJson: requestSchema.trim() || null,
        responseSchemaJson: responseSchema.trim() || null,
        serviceId: apiServiceId || null,
        tableId: apiTableId || null,
      }
      return createApi(applicationId!, input)
    },
    onSuccess: (api) => {
      setPath('')
      setRequestSchema('')
      setResponseSchema('')
      setApiError(null)
      queryClient.invalidateQueries({ queryKey: ['apis', applicationId] })
      toast.success(`${api.method.toUpperCase()} ${api.path} created`)
    },
    onError: (err) => setApiError(extractErrorMessage(err)),
  })

  const deleteApiMutation = useMutation({
    mutationFn: (apiId: string) => deleteApi(applicationId!, apiId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['apis', applicationId] })
      toast.success('API deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  return (
    <div className="p-6 max-w-4xl">
      <PageHeader title="API Designer" description="Define services and the API endpoints that call them." />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <div className="flex flex-col gap-8">
          <Card>
            <CardHeader className="flex items-center gap-2">
              <Server size={16} className="text-neutral-400" />
              <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">Services</h2>
            </CardHeader>
            <CardBody>
              <form
                className="flex gap-2 items-center mb-3"
                onSubmit={(e) => {
                  e.preventDefault()
                  createServiceMutation.mutate()
                }}
              >
                <Input
                  placeholder="ServiceName"
                  value={serviceName}
                  onChange={(e) => setServiceName(e.target.value)}
                  required
                />
                <Select value={serviceTableId} onChange={(e) => setServiceTableId(e.target.value)}>
                  <option value="">No table</option>
                  {tablesQuery.data?.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.name}
                    </option>
                  ))}
                </Select>
                <Button type="submit" variant="primary" loading={createServiceMutation.isPending}>
                  <Plus size={14} /> Add service
                </Button>
              </form>
              {serviceError && <p className="text-sm text-red-600 mb-2">{serviceError}</p>}

              {servicesQuery.data && servicesQuery.data.length === 0 && (
                <EmptyState icon={Server} title="No services yet" />
              )}

              <ul className="flex flex-col gap-1">
                {servicesQuery.data?.map((service) => (
                  <li
                    key={service.id}
                    className="flex items-center justify-between text-sm border-t border-neutral-100 dark:border-neutral-900 py-2 first:border-t-0"
                  >
                    <span className="flex items-center gap-1.5">
                      {service.name}
                      {service.tableId && (
                        <span className="text-neutral-500 flex items-center gap-1">
                          <ArrowRight size={12} />
                          {tablesQuery.data?.find((t) => t.id === service.tableId)?.name}
                        </span>
                      )}
                    </span>
                    <button
                      className="text-xs text-red-600 hover:underline flex items-center gap-1"
                      onClick={() => deleteServiceMutation.mutate(service.id)}
                    >
                      <Trash2 size={12} /> Delete
                    </button>
                  </li>
                ))}
              </ul>
            </CardBody>
          </Card>

          <Card>
            <CardHeader className="flex items-center gap-2">
              <Webhook size={16} className="text-neutral-400" />
              <h2 className="text-sm font-semibold text-neutral-700 dark:text-neutral-300">APIs</h2>
            </CardHeader>
            <CardBody>
              <form
                className="flex flex-col gap-2 mb-4 rounded-md border border-neutral-200 dark:border-neutral-800 p-3 bg-neutral-50/50 dark:bg-neutral-900/30"
                onSubmit={(e) => {
                  e.preventDefault()
                  createApiMutation.mutate()
                }}
              >
                <div className="flex gap-2 flex-wrap">
                  <Select value={method} onChange={(e) => setMethod(e.target.value as HttpMethod)}>
                    {HTTP_METHODS.map((m) => (
                      <option key={m} value={m}>
                        {m.toUpperCase()}
                      </option>
                    ))}
                  </Select>
                  <Input
                    className="flex-1 min-w-[160px]"
                    placeholder="/customer"
                    value={path}
                    onChange={(e) => setPath(e.target.value)}
                    required
                  />
                  <Select value={apiServiceId} onChange={(e) => setApiServiceId(e.target.value)}>
                    <option value="">No service</option>
                    {servicesQuery.data?.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </Select>
                  <Select value={apiTableId} onChange={(e) => setApiTableId(e.target.value)}>
                    <option value="">No table</option>
                    {tablesQuery.data?.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.name}
                      </option>
                    ))}
                  </Select>
                </div>
                <div className="flex gap-2">
                  <Textarea
                    className="flex-1 h-16"
                    placeholder="request schema (JSON)"
                    value={requestSchema}
                    onChange={(e) => setRequestSchema(e.target.value)}
                  />
                  <Textarea
                    className="flex-1 h-16"
                    placeholder="response schema (JSON)"
                    value={responseSchema}
                    onChange={(e) => setResponseSchema(e.target.value)}
                  />
                </div>
                <Button type="submit" variant="primary" size="sm" className="self-start" loading={createApiMutation.isPending}>
                  <Plus size={14} /> Add API
                </Button>
                {apiError && <p className="text-sm text-red-600">{apiError}</p>}
              </form>

              {apisQuery.data && apisQuery.data.length === 0 && (
                <EmptyState icon={Webhook} title="No APIs yet" />
              )}

              <ul className="flex flex-col gap-1">
                {apisQuery.data?.map((api) => (
                  <li
                    key={api.id}
                    className="flex items-center justify-between text-sm border-t border-neutral-100 dark:border-neutral-900 py-2 first:border-t-0"
                  >
                    <span className="flex items-center gap-2">
                      <MethodBadge method={api.method} />
                      {api.path}
                    </span>
                    <button
                      className="text-xs text-red-600 hover:underline flex items-center gap-1"
                      onClick={async () => {
                        const confirmed = await confirmAction({
                          title: `Delete ${api.method.toUpperCase()} ${api.path}?`,
                          confirmLabel: 'Delete',
                          danger: true,
                        })
                        if (confirmed) deleteApiMutation.mutate(api.id)
                      }}
                    >
                      <Trash2 size={12} /> Delete
                    </button>
                  </li>
                ))}
              </ul>
            </CardBody>
          </Card>
        </div>
      )}
    </div>
  )
}
