import { create } from 'zustand'

interface ConfirmOptions {
  title: string
  description?: string
  confirmLabel?: string
  danger?: boolean
}

interface ConfirmState extends ConfirmOptions {
  isOpen: boolean
  resolve: ((value: boolean) => void) | null
}

const useConfirmStore = create<ConfirmState>(() => ({
  isOpen: false,
  title: '',
  description: undefined,
  confirmLabel: undefined,
  danger: false,
  resolve: null,
}))

export function confirmAction(options: ConfirmOptions): Promise<boolean> {
  return new Promise((resolve) => {
    useConfirmStore.setState({ ...options, isOpen: true, resolve })
  })
}

export { useConfirmStore }
