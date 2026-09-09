/** Client-side identifier for optimistic UI state (e.g. a streaming placeholder message). */
export function v4(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID();
  }
  return `client-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
