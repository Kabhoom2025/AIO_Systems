import { Trash2 } from 'lucide-react'
import type { ColumnDataType, ColumnDefinition, DataTable } from '../../types'
import { COLUMN_DATA_TYPES } from '../../types'
import { Input, Select } from '../../components/ui'

interface ColumnFieldsEditorProps {
  column: ColumnDefinition
  onChange: (column: ColumnDefinition) => void
  onRemove?: () => void
  existingTables: DataTable[]
}

const TYPES_WITH_LENGTH: ColumnDataType[] = ['Varchar', 'Decimal']

export function ColumnFieldsEditor({ column, onChange, onRemove, existingTables }: ColumnFieldsEditorProps) {
  const referencedTable = existingTables.find((t) => t.id === column.referencesTableId)

  return (
    <div className="flex flex-col gap-2 rounded-md border border-neutral-200 dark:border-neutral-800 p-3 bg-neutral-50/50 dark:bg-neutral-900/30">
      <div className="flex flex-wrap gap-2 items-center">
        <Input
          className="w-40"
          placeholder="column_name"
          value={column.name}
          onChange={(e) => onChange({ ...column, name: e.target.value })}
        />
        <Select
          value={column.dataType}
          onChange={(e) => onChange({ ...column, dataType: e.target.value as ColumnDataType })}
        >
          {COLUMN_DATA_TYPES.map((t) => (
            <option key={t} value={t}>
              {t}
            </option>
          ))}
        </Select>
        {TYPES_WITH_LENGTH.includes(column.dataType) && (
          <Input
            className="w-20"
            type="number"
            placeholder={column.dataType === 'Varchar' ? '255' : 'precision'}
            value={column.length ?? ''}
            onChange={(e) => onChange({ ...column, length: e.target.value ? Number(e.target.value) : null })}
          />
        )}
        <Input
          className="w-32"
          placeholder="default value"
          value={column.defaultValue ?? ''}
          onChange={(e) => onChange({ ...column, defaultValue: e.target.value || null })}
        />
        {onRemove && (
          <button
            type="button"
            aria-label="Remove column"
            className="text-red-600 hover:text-red-700 ml-auto p-1 rounded hover:bg-red-50 dark:hover:bg-red-950"
            onClick={onRemove}
          >
            <Trash2 size={14} />
          </button>
        )}
      </div>

      <div className="flex flex-wrap gap-4 text-xs text-neutral-600 dark:text-neutral-300">
        <label className="flex items-center gap-1.5">
          <input
            type="checkbox"
            checked={column.isPrimaryKey ?? false}
            onChange={(e) => onChange({ ...column, isPrimaryKey: e.target.checked })}
          />
          Primary key
        </label>
        <label className="flex items-center gap-1.5">
          <input
            type="checkbox"
            checked={!(column.isNullable ?? true)}
            onChange={(e) => onChange({ ...column, isNullable: !e.target.checked })}
          />
          Required
        </label>
        <label className="flex items-center gap-1.5">
          <input
            type="checkbox"
            checked={column.isUnique ?? false}
            onChange={(e) => onChange({ ...column, isUnique: e.target.checked })}
          />
          Unique
        </label>
        <label className="flex items-center gap-1.5">
          <input
            type="checkbox"
            checked={column.isIndexed ?? false}
            onChange={(e) => onChange({ ...column, isIndexed: e.target.checked })}
          />
          Indexed
        </label>
        <label className="flex items-center gap-1.5">
          <input
            type="checkbox"
            checked={column.isForeignKey ?? false}
            onChange={(e) =>
              onChange({
                ...column,
                isForeignKey: e.target.checked,
                referencesTableId: e.target.checked ? column.referencesTableId : null,
                referencesColumnId: e.target.checked ? column.referencesColumnId : null,
              })
            }
          />
          Foreign key
        </label>
      </div>

      {column.isForeignKey && (
        <div className="flex gap-2 items-center text-xs">
          <span className="text-neutral-500">references</span>
          <Select
            value={column.referencesTableId ?? ''}
            onChange={(e) => onChange({ ...column, referencesTableId: e.target.value || null, referencesColumnId: null })}
          >
            <option value="">Select table…</option>
            {existingTables.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </Select>
          <Select
            value={column.referencesColumnId ?? ''}
            onChange={(e) => onChange({ ...column, referencesColumnId: e.target.value || null })}
            disabled={!referencedTable}
          >
            <option value="">Select column…</option>
            {referencedTable?.columns.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
        </div>
      )}
    </div>
  )
}
