import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { HUB_BASE_URL } from './api'
import { useAuthStore } from '../store/auth'

let connectionPromise: Promise<HubConnection> | null = null

function getConnection(): Promise<HubConnection> {
  if (!connectionPromise) {
    const connection = new HubConnectionBuilder()
      .withUrl(`${HUB_BASE_URL}/lineage`, {
        // The hub requires the same bearer token as the REST API, but a browser can't set a
        // custom header on the WebSocket upgrade request - SignalR sends this as a query param
        // instead (see Program.cs's JwtBearerEvents.OnMessageReceived for the server side).
        accessTokenFactory: () => useAuthStore.getState().token ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connectionPromise = connection.start().then(() => connection)
  }
  return connectionPromise
}

/** Call after logging out so the next login starts a fresh, correctly-authenticated connection
 * instead of reusing one built with the old (or no) token. */
export async function resetConnection(): Promise<void> {
  if (!connectionPromise) return
  const connection = await connectionPromise.catch(() => null)
  connectionPromise = null
  await connection?.stop()
}

export async function joinApplicationGroup(applicationId: string): Promise<HubConnection> {
  const connection = await getConnection()
  await connection.invoke('JoinApplicationGroup', applicationId)
  return connection
}

export async function leaveApplicationGroup(applicationId: string): Promise<void> {
  if (!connectionPromise) return
  const connection = await connectionPromise
  if (connection.state === 'Connected') {
    await connection.invoke('LeaveApplicationGroup', applicationId)
  }
}
