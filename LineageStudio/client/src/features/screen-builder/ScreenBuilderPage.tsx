import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Background,
  Controls,
  MiniMap,
  ReactFlow,
  ReactFlowProvider,
  type Node,
  type OnNodeDrag,
  type NodeTypes,
} from '@xyflow/react'
import '@xyflow/react/dist/style.css'
import { toast } from 'sonner'
import { MonitorPlay, Plus, Trash2 } from 'lucide-react'
import { ApplicationPicker } from '../../components/ApplicationPicker'
import { useSelectedApplication } from '../../store/selectedApplication'
import { createScreen, deleteScreen, listScreens } from '../../services/screens'
import { createComponent, deleteComponent, listComponents, updateComponent } from '../../services/components'
import type { ComponentType, UiComponent } from '../../types'
import { extractErrorMessage } from '../../utils/errors'
import { Button, EmptyState, Input, confirmAction, useResizablePanel } from '../../components/ui'
import { cn } from '../../utils/cn'
import { ComponentNode } from './ComponentNode'
import { ComponentPropertiesPanel } from './ComponentPropertiesPanel'
import { COMPONENT_ICONS } from './componentIcons'
import { SCREEN_TEMPLATES, type ScreenTemplate } from './screenTemplates'

const COMPONENT_TYPES: ComponentType[] = [
  'Text', 'Input', 'Number', 'Email', 'Select', 'Checkbox', 'Radio',
  'Date', 'Button', 'Table', 'Card', 'Form', 'Label', 'Container',
]

const nodeTypes: NodeTypes = { component: ComponentNode }

export function ScreenBuilderPage() {
  const { applicationId } = useSelectedApplication()
  const queryClient = useQueryClient()
  const propertiesPanel = useResizablePanel(288, { min: 240, max: 480, edge: 'left' })

  const [selectedScreenId, setSelectedScreenId] = useState<string | null>(null)
  const [selectedComponentId, setSelectedComponentId] = useState<string | null>(null)
  const [newScreenName, setNewScreenName] = useState('')
  const [newScreenRoute, setNewScreenRoute] = useState('')
  const [screenFormError, setScreenFormError] = useState<string | null>(null)

  const screensQuery = useQuery({
    queryKey: ['screens', applicationId],
    queryFn: () => listScreens(applicationId!),
    enabled: !!applicationId,
  })

  const screenId = selectedScreenId ?? screensQuery.data?.[0]?.id ?? null

  const componentsQuery = useQuery({
    queryKey: ['components', applicationId, screenId],
    queryFn: () => listComponents(applicationId!, screenId!),
    enabled: !!applicationId && !!screenId,
  })

  const invalidateComponents = () =>
    queryClient.invalidateQueries({ queryKey: ['components', applicationId, screenId] })

  const createScreenMutation = useMutation({
    mutationFn: (input: { name: string; route: string }) => createScreen(applicationId!, input),
    onSuccess: (screen) => {
      setNewScreenName('')
      setNewScreenRoute('')
      setScreenFormError(null)
      setSelectedScreenId(screen.id)
      queryClient.invalidateQueries({ queryKey: ['screens', applicationId] })
      toast.success(`Screen "${screen.name}" created`)
    },
    onError: (err) => setScreenFormError(extractErrorMessage(err)),
  })

  const deleteScreenMutation = useMutation({
    mutationFn: (id: string) => deleteScreen(applicationId!, id),
    onSuccess: () => {
      setSelectedScreenId(null)
      queryClient.invalidateQueries({ queryKey: ['screens', applicationId] })
      toast.success('Screen deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const createFromTemplateMutation = useMutation({
    mutationFn: async (template: ScreenTemplate) => {
      const route = `${template.routeBase}-${Math.random().toString(36).slice(2, 7)}`
      const screen = await createScreen(applicationId!, { name: template.screenName, route })
      for (const component of template.components) {
        await createComponent(applicationId!, {
          screenId: screen.id,
          type: component.type,
          name: component.name,
          positionX: component.x,
          positionY: component.y,
          dataBinding: component.dataBinding ?? null,
        })
      }
      return screen
    },
    onSuccess: (screen, template) => {
      setSelectedScreenId(screen.id)
      setSelectedComponentId(null)
      queryClient.invalidateQueries({ queryKey: ['screens', applicationId] })
      queryClient.invalidateQueries({ queryKey: ['components', applicationId, screen.id] })
      toast.success(`"${template.label}" created`)
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const createComponentMutation = useMutation({
    mutationFn: (type: ComponentType) =>
      createComponent(applicationId!, {
        screenId: screenId!,
        type,
        name: type,
        positionX: 80 + Math.random() * 300,
        positionY: 80 + Math.random() * 200,
      }),
    onSuccess: invalidateComponents,
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const moveComponentMutation = useMutation({
    mutationFn: ({ component, x, y }: { component: UiComponent; x: number; y: number }) =>
      updateComponent(applicationId!, component.id, {
        name: component.name,
        positionX: x,
        positionY: y,
        dataBinding: component.dataBinding,
        propertiesJson: component.properties ? JSON.stringify(component.properties) : null,
        validationJson: component.validation ? JSON.stringify(component.validation) : null,
      }),
    onSuccess: invalidateComponents,
  })

  const saveComponentMutation = useMutation({
    mutationFn: ({
      component,
      input,
    }: {
      component: UiComponent
      input: { name: string; dataBinding: string | null; propertiesJson: string | null; validationJson: string | null }
    }) =>
      updateComponent(applicationId!, component.id, {
        name: input.name,
        positionX: component.position.x,
        positionY: component.position.y,
        dataBinding: input.dataBinding,
        propertiesJson: input.propertiesJson,
        validationJson: input.validationJson,
      }),
    onSuccess: () => {
      invalidateComponents()
      toast.success('Component saved')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const deleteComponentMutation = useMutation({
    mutationFn: (componentId: string) => deleteComponent(applicationId!, componentId),
    onSuccess: () => {
      setSelectedComponentId(null)
      invalidateComponents()
      toast.success('Component deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const nodes: Node[] = useMemo(
    () =>
      (componentsQuery.data ?? []).map((component) => ({
        id: component.id,
        type: 'component',
        position: component.position,
        data: { component },
        selected: component.id === selectedComponentId,
      })),
    [componentsQuery.data, selectedComponentId],
  )

  const onNodeDragStop: OnNodeDrag = (_event, node) => {
    const component = componentsQuery.data?.find((c) => c.id === node.id)
    if (component) {
      moveComponentMutation.mutate({ component, x: node.position.x, y: node.position.y })
    }
  }

  const selectedComponent = componentsQuery.data?.find((c) => c.id === selectedComponentId) ?? null

  return (
    <div className="flex h-full">
      <div className="w-64 shrink-0 border-r border-neutral-200 dark:border-neutral-800 p-4 flex flex-col gap-4 overflow-y-auto">
        <div>
          <div className="flex items-center gap-2 mb-3">
            <MonitorPlay size={18} className="text-indigo-600" />
            <h1 className="text-base font-semibold text-neutral-900 dark:text-neutral-100">Screen Builder</h1>
          </div>
          <ApplicationPicker />
        </div>

        {applicationId && (
          <>
            <div>
              <div className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400 mb-1.5">
                Quick start templates
              </div>
              <div className="flex flex-col gap-1 mb-3">
                {SCREEN_TEMPLATES.map((template) => {
                  const Icon = template.icon
                  const isPending =
                    createFromTemplateMutation.isPending && createFromTemplateMutation.variables?.id === template.id
                  return (
                    <button
                      key={template.id}
                      disabled={createFromTemplateMutation.isPending}
                      onClick={() => createFromTemplateMutation.mutate(template)}
                      className="flex items-center gap-2 rounded-md border border-neutral-300 dark:border-neutral-700 px-2.5 py-1.5 text-left hover:bg-neutral-100 dark:hover:bg-neutral-800 hover:border-indigo-400 transition-colors disabled:opacity-50"
                    >
                      <Icon size={15} className="text-indigo-600 shrink-0" />
                      <span className="min-w-0">
                        <span className="block text-xs font-medium text-neutral-800 dark:text-neutral-200">
                          {isPending ? 'Creating…' : template.label}
                        </span>
                        <span className="block text-[11px] text-neutral-500 truncate">{template.description}</span>
                      </span>
                    </button>
                  )
                })}
              </div>
            </div>

            <div>
              <div className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400 mb-1.5">
                Screens
              </div>
              <ul className="flex flex-col gap-0.5 mb-2">
                {screensQuery.data?.map((screen) => (
                  <li key={screen.id}>
                    <button
                      className={cn(
                        'w-full text-left rounded-md px-2.5 py-1.5 text-sm transition-colors',
                        screen.id === screenId
                          ? 'bg-neutral-900 text-white dark:bg-white dark:text-neutral-900'
                          : 'text-neutral-700 dark:text-neutral-300 hover:bg-neutral-100 dark:hover:bg-neutral-800',
                      )}
                      onClick={() => {
                        setSelectedScreenId(screen.id)
                        setSelectedComponentId(null)
                      }}
                    >
                      {screen.name}
                      <span className="block text-xs opacity-70">{screen.route}</span>
                    </button>
                  </li>
                ))}
              </ul>

              <form
                className="flex flex-col gap-1.5"
                onSubmit={(e) => {
                  e.preventDefault()
                  createScreenMutation.mutate({ name: newScreenName, route: newScreenRoute })
                }}
              >
                <Input
                  placeholder="Screen name"
                  value={newScreenName}
                  onChange={(e) => setNewScreenName(e.target.value)}
                  required
                />
                <Input
                  placeholder="/route"
                  value={newScreenRoute}
                  onChange={(e) => setNewScreenRoute(e.target.value)}
                  required
                />
                <Button type="submit" variant="primary" size="sm" loading={createScreenMutation.isPending}>
                  <Plus size={14} /> Add screen
                </Button>
                {screenFormError && <p className="text-xs text-red-600">{screenFormError}</p>}
              </form>

              {screenId && (
                <Button
                  variant="danger"
                  size="sm"
                  className="mt-2 w-full"
                  onClick={async () => {
                    const confirmed = await confirmAction({
                      title: 'Delete this screen?',
                      description: 'All its components will be deleted too. This cannot be undone.',
                      confirmLabel: 'Delete screen',
                      danger: true,
                    })
                    if (confirmed) deleteScreenMutation.mutate(screenId)
                  }}
                >
                  <Trash2 size={14} /> Delete selected screen
                </Button>
              )}
            </div>

            {screenId && (
              <div>
                <div className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400 mb-1.5">
                  Components palette
                </div>
                <div className="grid grid-cols-2 gap-1">
                  {COMPONENT_TYPES.map((type) => {
                    const Icon = COMPONENT_ICONS[type]
                    return (
                      <button
                        key={type}
                        className="flex items-center gap-1.5 rounded-md border border-neutral-300 dark:border-neutral-700 px-2 py-1.5 text-xs text-neutral-700 dark:text-neutral-300 hover:bg-neutral-100 dark:hover:bg-neutral-800 hover:border-indigo-400 transition-colors"
                        onClick={() => createComponentMutation.mutate(type)}
                      >
                        <Icon size={13} className="text-neutral-400 shrink-0" />
                        {type}
                      </button>
                    )
                  })}
                </div>
              </div>
            )}
          </>
        )}
      </div>

      <div className="flex-1">
        {screenId ? (
          <ReactFlowProvider>
            <ReactFlow
              nodes={nodes}
              edges={[]}
              nodeTypes={nodeTypes}
              onNodeDragStop={onNodeDragStop}
              onNodeClick={(_event, node) => setSelectedComponentId(node.id)}
              onPaneClick={() => setSelectedComponentId(null)}
              fitView
              fitViewOptions={{ maxZoom: 1 }}
            >
              <Background />
              <Controls />
              <MiniMap pannable zoomable />
            </ReactFlow>
          </ReactFlowProvider>
        ) : (
          <EmptyState
            icon={MonitorPlay}
            title="No screen selected"
            description="Create or select a screen from the sidebar to start designing."
          />
        )}
      </div>

      {selectedComponent && (
        <ComponentPropertiesPanel
          component={selectedComponent}
          width={propertiesPanel.width}
          onStartResize={propertiesPanel.startDrag}
          saving={saveComponentMutation.isPending}
          onSave={(input) => saveComponentMutation.mutate({ component: selectedComponent, input })}
          onDelete={async () => {
            const confirmed = await confirmAction({
              title: `Delete component "${selectedComponent.name}"?`,
              confirmLabel: 'Delete',
              danger: true,
            })
            if (confirmed) deleteComponentMutation.mutate(selectedComponent.id)
          }}
        />
      )}
    </div>
  )
}
