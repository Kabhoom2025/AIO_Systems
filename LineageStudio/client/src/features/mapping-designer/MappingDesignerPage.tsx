import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ReactFlow, ReactFlowProvider, Background, Controls, MiniMap, type Edge, type Node } from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import { toast } from 'sonner'
import { ArrowRight, Plus, Trash2, Waypoints } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { PageHeader } from '../../components/PageHeader'
import { useSelectedApplication } from '../../store/selectedApplication'
import { listComponents } from '../../services/components'
import { listApis, listServices } from '../../services/apiDesigner'
import { listTables } from '../../services/tables'
import { createMapping, deleteMapping, listMappings } from '../../services/mappings'
import type { TransformationType } from '../../types'
import { extractErrorMessage } from '../../utils/errors'
import { layoutWithDagre } from '../../utils/dagreLayout'
import { Badge, Button, Card, CardBody, EmptyState, Input, Select, confirmAction } from '../../components/ui'

const TRANSFORMATIONS: TransformationType[] = [
  'None', 'Trim', 'Uppercase', 'Lowercase', 'Default', 'Concatenate', 'Split', 'DateConversion', 'NumberConversion',
]

const TRANSFORMATIONS_REQUIRING_CONFIG = new Set<TransformationType>([
  'Default', 'Concatenate', 'Split', 'DateConversion', 'NumberConversion',
])

export function MappingDesignerPage() {
  const { applicationId } = useSelectedApplication()
  const queryClient = useQueryClient()

  const componentsQuery = useQuery({
    queryKey: ['components', applicationId, null],
    queryFn: () => listComponents(applicationId!),
    enabled: !!applicationId,
  })
  const apisQuery = useQuery({
    queryKey: ['apis', applicationId],
    queryFn: () => listApis(applicationId!),
    enabled: !!applicationId,
  })
  const servicesQuery = useQuery({
    queryKey: ['services', applicationId],
    queryFn: () => listServices(applicationId!),
    enabled: !!applicationId,
  })
  const tablesQuery = useQuery({
    queryKey: ['tables', applicationId],
    queryFn: () => listTables(applicationId!),
    enabled: !!applicationId,
  })
  const mappingsQuery = useQuery({
    queryKey: ['mappings', applicationId],
    queryFn: () => listMappings(applicationId!),
    enabled: !!applicationId,
  })

  const columns = useMemo(
    () => (tablesQuery.data ?? []).flatMap((t) => t.columns.map((c) => ({ ...c, tableName: t.name }))),
    [tablesQuery.data],
  )

  const [componentId, setComponentId] = useState('')
  const [sourceField, setSourceField] = useState('')
  const [apiId, setApiId] = useState('')
  const [apiField, setApiField] = useState('')
  const [serviceId, setServiceId] = useState('')
  const [serviceField, setServiceField] = useState('')
  const [columnId, setColumnId] = useState('')
  const [transformation, setTransformation] = useState<TransformationType>('None')
  const [transformationConfig, setTransformationConfig] = useState('')
  const [formError, setFormError] = useState<string | null>(null)

  const createMutation = useMutation({
    mutationFn: () =>
      createMapping(applicationId!, {
        sourceComponentId: componentId,
        sourceField,
        apiId,
        apiField,
        serviceId: serviceId || null,
        serviceField: serviceField || null,
        columnId,
        transformation,
        transformationConfigJson: transformationConfig.trim() || null,
      }),
    onSuccess: () => {
      setSourceField('')
      setApiField('')
      setServiceField('')
      setTransformationConfig('')
      setFormError(null)
      queryClient.invalidateQueries({ queryKey: ['mappings', applicationId] })
      toast.success('Mapping created')
    },
    onError: (err) => setFormError(extractErrorMessage(err)),
  })

  const deleteMutation = useMutation({
    mutationFn: (mappingId: string) => deleteMapping(applicationId!, mappingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['mappings', applicationId] })
      toast.success('Mapping deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const { nodes, edges } = useMemo(() => {
    const nodeMap = new Map<string, Node>()
    const flowEdges: Edge[] = []

    function addNode(lane: 'component' | 'api' | 'service' | 'column', id: string, label: string) {
      const key = `${lane}:${id}`
      if (!nodeMap.has(key)) {
        nodeMap.set(key, {
          id: key,
          position: { x: 0, y: 0 },
          data: { label },
          // Tailwind classes (not an inline `style` background) so this actually follows dark
          // mode - a hardcoded `background: white` with no explicit text color left dark-mode
          // text unreadably light-on-white.
          className:
            'rounded-lg border border-neutral-300 dark:border-neutral-700 !bg-white dark:!bg-neutral-900 !text-neutral-800 dark:!text-neutral-200 px-3 py-2 text-xs',
        })
      }
      return key
    }

    for (const mapping of mappingsQuery.data ?? []) {
      const component = componentsQuery.data?.find((c) => c.id === mapping.sourceComponentId)
      const api = apisQuery.data?.find((a) => a.id === mapping.apiId)
      const service = servicesQuery.data?.find((s) => s.id === mapping.serviceId)
      const column = columns.find((c) => c.id === mapping.columnId)

      // Node labels name the *entity* only (a component, the API endpoint, the service, the
      // column) - never a specific mapped field - because one API or service commonly has
      // several field mappings flowing through the same node. Which field goes where is shown
      // on the edges instead, so it's never lost even when nodes are shared.
      const componentKey = addNode('component', mapping.sourceComponentId, component?.name ?? mapping.sourceField)
      const apiKey = addNode('api', mapping.apiId, api ? `${api.method.toUpperCase()} ${api.path}` : mapping.apiId)
      const columnKey = addNode('column', mapping.columnId, column ? `${column.tableName}.${column.name}` : mapping.columnId)

      const transformationSuffix = mapping.transformation !== 'None' ? ` (${mapping.transformation})` : ''
      flowEdges.push({
        id: `${mapping.id}-ca`,
        source: componentKey,
        target: apiKey,
        label: `.${mapping.sourceField} → .${mapping.apiField}${transformationSuffix}`,
        animated: false,
      })

      if (service) {
        const serviceKey = addNode('service', mapping.serviceId!, service.name)
        flowEdges.push({
          id: `${mapping.id}-as`,
          source: apiKey,
          target: serviceKey,
          label: `.${mapping.apiField} → .${mapping.serviceField ?? mapping.apiField}`,
        })
        flowEdges.push({
          id: `${mapping.id}-sc`,
          source: serviceKey,
          target: columnKey,
          label: `.${mapping.serviceField ?? mapping.apiField} → .${column?.name ?? ''}`,
        })
      } else {
        flowEdges.push({
          id: `${mapping.id}-ac`,
          source: apiKey,
          target: columnKey,
          label: `.${mapping.apiField} → .${column?.name ?? ''}`,
        })
      }
    }

    const rawNodes = Array.from(nodeMap.values())
    return { nodes: layoutWithDagre(rawNodes, flowEdges, { direction: 'LR' }), edges: flowEdges }
  }, [mappingsQuery.data, componentsQuery.data, apisQuery.data, servicesQuery.data, columns])

  return (
    <div className="p-6">
      <PageHeader
        title="Mapping Designer"
        description="Wire a UI field to an API field, an optional service field, and a database column."
      />

      <div className="mb-6">
        <ApplicationPicker />
      </div>

      {applicationId && (
        <>
          <Card className="mb-6 max-w-4xl">
            <CardBody>
              <form
                className="flex flex-col gap-2"
                onSubmit={(e) => {
                  e.preventDefault()
                  createMutation.mutate()
                }}
              >
                <div className="flex flex-wrap gap-2 items-center">
                  <Select value={componentId} onChange={(e) => setComponentId(e.target.value)} required>
                    <option value="">Component…</option>
                    {componentsQuery.data?.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </Select>
                  <Input
                    className="w-32"
                    placeholder="source field"
                    value={sourceField}
                    onChange={(e) => setSourceField(e.target.value)}
                    required
                  />
                  <ArrowRight size={14} className="text-neutral-400 shrink-0" />
                  <Select value={apiId} onChange={(e) => setApiId(e.target.value)} required>
                    <option value="">API…</option>
                    {apisQuery.data?.map((a) => (
                      <option key={a.id} value={a.id}>
                        {a.method.toUpperCase()} {a.path}
                      </option>
                    ))}
                  </Select>
                  <Input
                    className="w-32"
                    placeholder="api field"
                    value={apiField}
                    onChange={(e) => setApiField(e.target.value)}
                    required
                  />
                  <ArrowRight size={14} className="text-neutral-400 shrink-0" />
                  <Select value={serviceId} onChange={(e) => setServiceId(e.target.value)}>
                    <option value="">No service</option>
                    {servicesQuery.data?.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </Select>
                  <Input
                    className="w-32"
                    placeholder="service field"
                    value={serviceField}
                    onChange={(e) => setServiceField(e.target.value)}
                    disabled={!serviceId}
                  />
                  <ArrowRight size={14} className="text-neutral-400 shrink-0" />
                  <Select value={columnId} onChange={(e) => setColumnId(e.target.value)} required>
                    <option value="">Column…</option>
                    {columns.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.tableName}.{c.name}
                      </option>
                    ))}
                  </Select>
                </div>

                <div className="flex gap-2 items-center">
                  <Select value={transformation} onChange={(e) => setTransformation(e.target.value as TransformationType)}>
                    {TRANSFORMATIONS.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </Select>
                  {TRANSFORMATIONS_REQUIRING_CONFIG.has(transformation) && (
                    <Input
                      className="flex-1 font-mono text-xs"
                      placeholder='transformation config (JSON), e.g. {"default":"N/A"}'
                      value={transformationConfig}
                      onChange={(e) => setTransformationConfig(e.target.value)}
                    />
                  )}
                  <Button type="submit" variant="primary" className="ml-auto" loading={createMutation.isPending}>
                    <Plus size={14} /> Add mapping
                  </Button>
                </div>
                {formError && <p className="text-sm text-red-600">{formError}</p>}
              </form>
            </CardBody>
          </Card>

          {mappingsQuery.data && mappingsQuery.data.length === 0 && (
            <EmptyState icon={Waypoints} title="No mappings yet" description="Add your first mapping above." />
          )}

          {mappingsQuery.data && mappingsQuery.data.length > 0 && (
            <Card className="mb-6 max-w-4xl">
              <ul className="divide-y divide-neutral-100 dark:divide-neutral-900">
                {mappingsQuery.data.map((mapping) => (
                  <li key={mapping.id} className="flex items-center justify-between text-sm p-3">
                    <span className="flex items-center gap-1.5 flex-wrap">
                      {mapping.sourceField}
                      <ArrowRight size={12} className="text-neutral-400" />
                      {mapping.apiField}
                      {mapping.serviceField && (
                        <>
                          <ArrowRight size={12} className="text-neutral-400" />
                          {mapping.serviceField}
                        </>
                      )}
                      <ArrowRight size={12} className="text-neutral-400" />
                      {columns.find((c) => c.id === mapping.columnId)?.name}
                      {mapping.transformation !== 'None' && <Badge color="blue">{mapping.transformation}</Badge>}
                    </span>
                    <button
                      className="text-xs text-red-600 hover:underline flex items-center gap-1 shrink-0"
                      onClick={async () => {
                        const confirmed = await confirmAction({ title: 'Delete this mapping?', confirmLabel: 'Delete', danger: true })
                        if (confirmed) deleteMutation.mutate(mapping.id)
                      }}
                    >
                      <Trash2 size={12} /> Delete
                    </button>
                  </li>
                ))}
              </ul>
            </Card>
          )}

          <div style={{ height: 440 }} className="rounded-lg border border-neutral-200 dark:border-neutral-800 max-w-5xl">
            <ReactFlowProvider>
              <ReactFlow nodes={nodes} edges={edges} fitView fitViewOptions={{ maxZoom: 1 }}>
                <Background />
                <Controls />
                <MiniMap pannable zoomable />
              </ReactFlow>
            </ReactFlowProvider>
          </div>
        </>
      )}
    </div>
  )
}
