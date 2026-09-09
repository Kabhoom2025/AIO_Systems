import { useCallback, useRef, useState } from 'react'

/** A draggable-width side panel. `edge: 'left'` means the drag handle sits on the panel's left
 * side and dragging left/right grows/shrinks it (used for a panel anchored to the right of the
 * screen); use 'right' for a panel anchored to the left. */
export function useResizablePanel(initialWidth: number, options?: { min?: number; max?: number; edge?: 'left' | 'right' }) {
  const { min = 200, max = 600, edge = 'left' } = options ?? {}
  const [width, setWidth] = useState(initialWidth)
  const dragState = useRef<{ startX: number; startWidth: number } | null>(null)

  const onMouseMove = useCallback(
    (e: MouseEvent) => {
      if (!dragState.current) return
      const delta = e.clientX - dragState.current.startX
      const signed = edge === 'left' ? -delta : delta
      const next = Math.min(max, Math.max(min, dragState.current.startWidth + signed))
      setWidth(next)
    },
    [edge, min, max],
  )

  const onMouseUp = useCallback(() => {
    dragState.current = null
    window.removeEventListener('mousemove', onMouseMove)
    window.removeEventListener('mouseup', onMouseUp)
  }, [onMouseMove])

  const startDrag = useCallback(
    (e: React.MouseEvent) => {
      dragState.current = { startX: e.clientX, startWidth: width }
      window.addEventListener('mousemove', onMouseMove)
      window.addEventListener('mouseup', onMouseUp)
    },
    [width, onMouseMove, onMouseUp],
  )

  return { width, startDrag }
}
