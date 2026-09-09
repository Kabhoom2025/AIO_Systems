import { Database, GitBranch, LayoutGrid, MonitorPlay, Play, Waypoints, Webhook, type LucideIcon } from 'lucide-react'

export interface DemoStep {
  title: string
  icon: LucideIcon
  route: string
  routeLabel: string
  points: string[]
}

/** The end-to-end pipeline every LineageStudio app follows, in the order you'd actually build it. */
export const DEMO_STEPS: DemoStep[] = [
  {
    title: 'Create an application',
    icon: LayoutGrid,
    route: '/applications',
    routeLabel: 'Applications',
    points: [
      'An application is the container for everything below - its screens, APIs, tables and mappings.',
      'Create one, then Publish it once its screens and APIs should be reachable at runtime.',
    ],
  },
  {
    title: 'Design your data',
    icon: Database,
    route: '/data',
    routeLabel: 'Data Designer',
    points: [
      'Define tables and columns - this is the real PostgreSQL schema your data lands in.',
      'Do this first: every mapping later needs a column to point to.',
    ],
  },
  {
    title: 'Build a screen',
    icon: MonitorPlay,
    route: '/screens',
    routeLabel: 'Screen Builder',
    points: [
      'Drag components onto the canvas from the palette, or click a Quick start template for a ready-made form in one click.',
      'Each component is a field a user will fill in, e.g. "CustomerForm.email".',
    ],
  },
  {
    title: 'Define an API',
    icon: Webhook,
    route: '/apis',
    routeLabel: 'API Designer',
    points: [
      'Create the API endpoint (method + path) a screen submits to.',
      'Optionally attach a service if the data passes through a service layer before reaching the database.',
    ],
  },
  {
    title: 'Wire it all together',
    icon: Waypoints,
    route: '/mappings',
    routeLabel: 'Mapping Designer',
    points: [
      'Map each field end-to-end: Component field -> API field -> (optional Service field) -> Column.',
      'Add an optional transformation (Trim, Uppercase, DateConversion, ...) if a value needs converting along the way.',
    ],
  },
  {
    title: 'Run it for real',
    icon: Play,
    route: '/runtime',
    routeLabel: 'Run',
    points: [
      'Pick an API - a form is generated automatically from its mappings.',
      'Fill it in and Save - this really executes the API and really writes to PostgreSQL.',
    ],
  },
  {
    title: 'Watch the data flow',
    icon: GitBranch,
    route: '/lineage',
    routeLabel: 'Lineage',
    points: [
      'Every execution streams live over SignalR as a flow diagram: screen -> API -> service -> database.',
      'Click any node for its status, duration and error details. Execution History and Impact Analysis dig even further.',
    ],
  },
]
