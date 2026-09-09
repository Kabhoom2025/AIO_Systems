import { useEffect, useState } from 'react'
import { Trash2 } from 'lucide-react'
import type { UiComponent } from '../../types'
import { Button, Input, Textarea } from '../../components/ui'
import { COMPONENT_ICONS } from './componentIcons'

interface ComponentPropertiesPanelProps {
  component: UiComponent
  width: number
  onStartResize: (e: React.MouseEvent) => void
  onSave: (input: {
    name: string
    dataBinding: string | null
    propertiesJson: string | null
    validationJson: string | null
  }) => void
  onDelete: () => void
  saving: boolean
}

export function ComponentPropertiesPanel({
  component,
  width,
  onStartResize,
  onSave,
  onDelete,
  saving,
}: ComponentPropertiesPanelProps) {
  const [name, setName] = useState(component.name)
  const [dataBinding, setDataBinding] = useState(component.dataBinding ?? '')
  const [propertiesText, setPropertiesText] = useState(JSON.stringify(component.properties ?? {}, null, 2))
  const [validationText, setValidationText] = useState(
    component.validation ? JSON.stringify(component.validation, null, 2) : '',
  )
  const [jsonError, setJsonError] = useState<string | null>(null)

  useEffect(() => {
    setName(component.name)
    setDataBinding(component.dataBinding ?? '')
    setPropertiesText(JSON.stringify(component.properties ?? {}, null, 2))
    setValidationText(component.validation ? JSON.stringify(component.validation, null, 2) : '')
    setJsonError(null)
  }, [component.id])

  const Icon = COMPONENT_ICONS[component.type]

  return (
    <div
      style={{ width }}
      className="relative shrink-0 border-l border-neutral-200 dark:border-neutral-800 p-4 flex flex-col gap-3 overflow-y-auto"
    >
      <div
        onMouseDown={onStartResize}
        className="absolute left-0 top-0 h-full w-1 cursor-col-resize hover:bg-indigo-400/50 active:bg-indigo-500/60"
      />

      <div className="flex items-center gap-2 text-xs uppercase text-neutral-500">
        <Icon size={14} />
        {component.type}
      </div>

      <label className="text-xs text-neutral-500 flex flex-col gap-1">
        Name
        <Input value={name} onChange={(e) => setName(e.target.value)} />
      </label>

      <label className="text-xs text-neutral-500 flex flex-col gap-1">
        Data binding (field name)
        <Input value={dataBinding} onChange={(e) => setDataBinding(e.target.value)} />
      </label>

      <label className="text-xs text-neutral-500 flex flex-col gap-1">
        Properties (JSON)
        <Textarea className="h-24" value={propertiesText} onChange={(e) => setPropertiesText(e.target.value)} />
      </label>

      <label className="text-xs text-neutral-500 flex flex-col gap-1">
        Validation (JSON)
        <Textarea className="h-20" value={validationText} onChange={(e) => setValidationText(e.target.value)} />
      </label>

      {jsonError && <p className="text-xs text-red-600">{jsonError}</p>}

      <div className="flex gap-2 mt-2">
        <Button
          variant="primary"
          size="sm"
          loading={saving}
          onClick={() => {
            try {
              if (propertiesText.trim()) JSON.parse(propertiesText)
              if (validationText.trim()) JSON.parse(validationText)
            } catch {
              setJsonError('Properties/Validation must be valid JSON.')
              return
            }
            setJsonError(null)
            onSave({
              name,
              dataBinding: dataBinding.trim() || null,
              propertiesJson: propertiesText.trim() || null,
              validationJson: validationText.trim() || null,
            })
          }}
        >
          Save
        </Button>
        <Button variant="danger" size="sm" onClick={onDelete}>
          <Trash2 size={14} /> Delete
        </Button>
      </div>
    </div>
  )
}
