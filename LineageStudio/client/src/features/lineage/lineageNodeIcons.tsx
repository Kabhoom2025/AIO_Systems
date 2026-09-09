import {
  Archive,
  Columns3,
  Database,
  Monitor,
  Router,
  Server,
  Sparkles,
  SquareStack,
  Table as TableIcon,
  Webhook,
  type LucideIcon,
} from 'lucide-react'
import type { LineageNodeType } from '../../types'

export const LINEAGE_NODE_ICONS: Record<LineageNodeType, LucideIcon> = {
  Screen: Monitor,
  Component: SquareStack,
  Api: Webhook,
  Controller: Router,
  Service: Server,
  Repository: Archive,
  Database: Database,
  Table: TableIcon,
  Column: Columns3,
  Transformation: Sparkles,
}
