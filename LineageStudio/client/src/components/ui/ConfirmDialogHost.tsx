import { AlertTriangle } from 'lucide-react'
import { useConfirmStore } from './confirmStore'
import { Button } from './Button'

export function ConfirmDialogHost() {
  const { isOpen, title, description, confirmLabel, danger, resolve } = useConfirmStore()

  if (!isOpen) return null

  const settle = (value: boolean) => {
    resolve?.(value)
    useConfirmStore.setState({ isOpen: false, resolve: null })
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
      onClick={() => settle(false)}
    >
      <div
        role="alertdialog"
        aria-modal="true"
        className="w-full max-w-sm rounded-lg bg-white dark:bg-neutral-900 border border-neutral-200 dark:border-neutral-800 shadow-xl p-5"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start gap-3">
          {danger && <AlertTriangle size={20} className="text-red-600 shrink-0 mt-0.5" />}
          <div>
            <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">{title}</h2>
            {description && <p className="mt-1 text-sm text-neutral-500">{description}</p>}
          </div>
        </div>
        <div className="mt-4 flex justify-end gap-2">
          <Button variant="ghost" size="sm" onClick={() => settle(false)}>
            Cancel
          </Button>
          <Button variant={danger ? 'danger' : 'primary'} size="sm" onClick={() => settle(true)} autoFocus>
            {confirmLabel ?? 'Confirm'}
          </Button>
        </div>
      </div>
    </div>
  )
}
