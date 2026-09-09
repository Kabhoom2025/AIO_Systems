import { create } from 'zustand'

interface SelectedApplicationState {
  applicationId: string | null
  setApplicationId: (id: string | null) => void
}

export const useSelectedApplication = create<SelectedApplicationState>((set) => ({
  applicationId: null,
  setApplicationId: (id) => set({ applicationId: id }),
}))
