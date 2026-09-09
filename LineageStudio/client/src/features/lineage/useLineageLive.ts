import { useEffect } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { joinApplicationGroup, leaveApplicationGroup } from '../../services/signalr'
import type { LineageEventWire, LineageExecutionWire } from '../../services/lineage'

interface UseLineageLiveOptions {
  applicationId: string | null
  onEvent: (evt: LineageEventWire) => void
  onExecutionUpdate: (execution: LineageExecutionWire) => void
}

/** Joins the application's SignalR group and forwards every live push - no polling, no timers.
 * The connection is a lazily-created singleton (see services/signalr.ts) shared across pages. */
export function useLineageLive({ applicationId, onEvent, onExecutionUpdate }: UseLineageLiveOptions) {
  useEffect(() => {
    if (!applicationId) return

    let connection: HubConnection | undefined
    let cancelled = false

    joinApplicationGroup(applicationId).then((conn) => {
      if (cancelled) return
      connection = conn
      conn.on('lineageEvent', onEvent)
      conn.on('lineageExecutionUpdate', onExecutionUpdate)
    })

    return () => {
      cancelled = true
      connection?.off('lineageEvent', onEvent)
      connection?.off('lineageExecutionUpdate', onExecutionUpdate)
      leaveApplicationGroup(applicationId)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [applicationId])
}
