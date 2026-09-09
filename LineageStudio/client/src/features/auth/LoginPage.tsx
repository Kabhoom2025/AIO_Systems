import { useState } from 'react'
import { useNavigate, useLocation, Navigate } from 'react-router-dom'
import { LogIn, Eye, EyeOff } from 'lucide-react'
import { toast } from 'sonner'
import { useAuthStore } from '../../store/auth'
import { Button, Card, CardBody, Input } from '../../components/ui'
import { extractErrorMessage } from '../../utils/errors'

export function LoginPage() {
  const token = useAuthStore((s) => s.token)
  const login = useAuthStore((s) => s.login)
  const isLoading = useAuthStore((s) => s.isLoading)
  const navigate = useNavigate()
  const location = useLocation()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Already signed in - don't show the login form again.
  if (token) {
    const from = (location.state as { from?: string } | null)?.from ?? '/applications'
    return <Navigate to={from} replace />
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-neutral-50 dark:bg-neutral-950 p-4">
      <div className="w-full max-w-sm">
        <div className="flex items-center justify-center gap-2 mb-6">
          <div className="h-8 w-8 rounded-md bg-gradient-to-br from-indigo-500 to-violet-600" />
          <span className="text-lg font-semibold text-neutral-900 dark:text-neutral-100 tracking-tight">
            LineageStudio
          </span>
        </div>

        <Card>
          <CardBody>
            <h1 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100 mb-1">Sign in</h1>
            <p className="text-xs text-neutral-500 mb-4">Use your LineageStudio account to continue.</p>

            <form
              className="flex flex-col gap-3"
              onSubmit={async (e) => {
                e.preventDefault()
                setError(null)
                try {
                  await login(email, password)
                  toast.success('Signed in')
                  const from = (location.state as { from?: string } | null)?.from ?? '/applications'
                  navigate(from, { replace: true })
                } catch (err) {
                  setError(extractErrorMessage(err))
                }
              }}
            >
              <label className="flex flex-col gap-1 text-xs text-neutral-500">
                Email
                <Input
                  type="email"
                  autoComplete="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  autoFocus
                />
              </label>
              <div className="flex flex-col gap-1 text-xs text-neutral-500">
                <label htmlFor="login-password">Password</label>
                <div className="relative">
                  <Input
                    id="login-password"
                    type={showPassword ? 'text' : 'password'}
                    autoComplete="current-password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    className="pr-9"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((v) => !v)}
                    className="absolute inset-y-0 right-0 flex items-center px-2.5 text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300"
                    tabIndex={-1}
                    aria-label={showPassword ? 'Hide password' : 'Show password'}
                  >
                    {showPassword ? <EyeOff size={15} /> : <Eye size={15} />}
                  </button>
                </div>
              </div>

              {error && <p className="text-sm text-red-600">{error}</p>}

              <Button type="submit" variant="primary" loading={isLoading} className="justify-center mt-1">
                <LogIn size={14} /> Sign in
              </Button>
            </form>

            <p className="text-[11px] text-neutral-400 mt-4 text-center">
              Demo login: admin@lineagestudio.local / Admin@123
            </p>
          </CardBody>
        </Card>
      </div>
    </div>
  )
}
