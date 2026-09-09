import { KeyRound, LayoutDashboard, MailPlus, UserPlus, type LucideIcon } from 'lucide-react'
import type { ComponentType } from '../../types'

export interface ScreenTemplateComponent {
  type: ComponentType
  name: string
  x: number
  y: number
  dataBinding?: string
}

export interface ScreenTemplate {
  id: string
  label: string
  description: string
  icon: LucideIcon
  screenName: string
  routeBase: string
  components: ScreenTemplateComponent[]
}

/**
 * One-click starting points for the canvas - each creates a screen plus a pre-arranged set of
 * components, so a common layout doesn't need to be assembled one component at a time from the
 * palette. Field names deliberately follow the "Screen.field" convention already used by the
 * seeded Customer Management demo, so mappings created afterwards read naturally.
 */
export const SCREEN_TEMPLATES: ScreenTemplate[] = [
  {
    id: 'login-form',
    label: 'Login form',
    description: 'Email, password and a submit button.',
    icon: KeyRound,
    screenName: 'Login',
    routeBase: '/login',
    components: [
      { type: 'Text', name: 'LoginForm.title', x: 60, y: 40 },
      { type: 'Email', name: 'LoginForm.email', x: 60, y: 120, dataBinding: 'LoginForm.email' },
      { type: 'Input', name: 'LoginForm.password', x: 60, y: 200, dataBinding: 'LoginForm.password' },
      { type: 'Button', name: 'LoginForm.submit', x: 60, y: 280 },
    ],
  },
  {
    id: 'contact-form',
    label: 'Contact form',
    description: 'Name, email, message and a submit button.',
    icon: MailPlus,
    screenName: 'Contact',
    routeBase: '/contact',
    components: [
      { type: 'Text', name: 'ContactForm.name', x: 60, y: 40, dataBinding: 'ContactForm.name' },
      { type: 'Email', name: 'ContactForm.email', x: 60, y: 120, dataBinding: 'ContactForm.email' },
      { type: 'Text', name: 'ContactForm.message', x: 60, y: 200, dataBinding: 'ContactForm.message' },
      { type: 'Button', name: 'ContactForm.submit', x: 60, y: 280 },
    ],
  },
  {
    id: 'registration-form',
    label: 'Registration form',
    description: 'Name, email, phone and a submit button.',
    icon: UserPlus,
    screenName: 'Registration',
    routeBase: '/registration',
    components: [
      { type: 'Text', name: 'RegistrationForm.name', x: 60, y: 40, dataBinding: 'RegistrationForm.name' },
      { type: 'Email', name: 'RegistrationForm.email', x: 60, y: 120, dataBinding: 'RegistrationForm.email' },
      { type: 'Input', name: 'RegistrationForm.phone', x: 60, y: 200, dataBinding: 'RegistrationForm.phone' },
      { type: 'Button', name: 'RegistrationForm.submit', x: 60, y: 280 },
    ],
  },
  {
    id: 'dashboard',
    label: 'Dashboard',
    description: 'Summary card plus a data table.',
    icon: LayoutDashboard,
    screenName: 'Dashboard',
    routeBase: '/dashboard',
    components: [
      { type: 'Card', name: 'Dashboard.summary', x: 60, y: 40 },
      { type: 'Table', name: 'Dashboard.table', x: 60, y: 160 },
    ],
  },
]
