import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export type NavPosition = 'sidebar' | 'header'

interface LayoutSettingsState {
  navPosition: NavPosition
  setNavPosition: (position: NavPosition) => void
}

export const useLayoutSettings = create<LayoutSettingsState>()(
  persist(
    (set) => ({
      navPosition: 'sidebar',
      setNavPosition: (navPosition) => set({ navPosition }),
    }),
    { name: 'lineagestudio-layout-settings' },
  ),
)
