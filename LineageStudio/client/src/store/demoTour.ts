import { create } from 'zustand'

interface DemoTourState {
  isOpen: boolean
  stepIndex: number
  open: () => void
  close: () => void
  next: (lastIndex: number) => void
  back: () => void
  goToStep: (index: number) => void
}

export const useDemoTourStore = create<DemoTourState>((set) => ({
  isOpen: false,
  stepIndex: 0,
  open: () => set({ isOpen: true, stepIndex: 0 }),
  close: () => set({ isOpen: false }),
  next: (lastIndex) => set((s) => ({ stepIndex: Math.min(s.stepIndex + 1, lastIndex) })),
  back: () => set((s) => ({ stepIndex: Math.max(s.stepIndex - 1, 0) })),
  goToStep: (index) => set({ stepIndex: index }),
}))
