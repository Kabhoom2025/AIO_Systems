import { create } from "zustand";
import { persist } from "zustand/middleware";

export type ThemeMode = "light" | "dark";

interface ThemeState {
  mode: ThemeMode;
  toggleMode: () => void;
  setMode: (mode: ThemeMode) => void;
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      mode: "light",
      toggleMode: () => set({ mode: get().mode === "light" ? "dark" : "light" }),
      setMode: (mode) => set({ mode }),
    }),
    {
      name: "projectflow-theme",
    }
  )
);
