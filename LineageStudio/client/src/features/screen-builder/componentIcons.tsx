import {
  Calendar,
  CheckSquare,
  ChevronDownSquare,
  CreditCard,
  Hash,
  LayoutTemplate,
  ListChecks,
  Mail,
  MousePointerClick,
  RectangleHorizontal,
  Square,
  SquareStack,
  Tag,
  Table as TableIcon,
  Type,
  type LucideIcon,
} from 'lucide-react'
import type { ComponentType } from '../../types'

export const COMPONENT_ICONS: Record<ComponentType, LucideIcon> = {
  Text: Type,
  Input: RectangleHorizontal,
  Number: Hash,
  Email: Mail,
  Select: ChevronDownSquare,
  Checkbox: CheckSquare,
  Radio: ListChecks,
  Date: Calendar,
  Button: MousePointerClick,
  Table: TableIcon,
  Card: CreditCard,
  Form: LayoutTemplate,
  Label: Tag,
  Container: Square,
}

export const SCREEN_BUILDER_ICON = SquareStack
