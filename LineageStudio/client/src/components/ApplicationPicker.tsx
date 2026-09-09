import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { LayoutGrid } from 'lucide-react'
import { listApplications } from '../services/applications'
import { useSelectedApplication } from '../store/selectedApplication'
import { Select } from './ui'

export function ApplicationPicker() {
  const { data, isLoading } = useQuery({ queryKey: ['applications'], queryFn: listApplications })
  const { applicationId, setApplicationId } = useSelectedApplication()

  useEffect(() => {
    if (!applicationId && data && data.length > 0) {
      setApplicationId(data[0].id)
    }
  }, [applicationId, data, setApplicationId])

  if (isLoading) {
    return <div className="h-8 w-56 rounded-md bg-neutral-100 dark:bg-neutral-800 animate-pulse" />
  }

  if (!data || data.length === 0) {
    return (
      <Link
        to="/applications"
        className="inline-flex items-center gap-1.5 text-sm text-indigo-600 dark:text-indigo-400 hover:underline"
      >
        <LayoutGrid size={14} />
        Create an application first
      </Link>
    )
  }

  return (
    <label className="flex items-center gap-2 text-sm">
      <span className="text-neutral-500 whitespace-nowrap">Application</span>
      <Select value={applicationId ?? ''} onChange={(e) => setApplicationId(e.target.value)} className="min-w-48">
        {data.map((app) => (
          <option key={app.id} value={app.id}>
            {app.name}
          </option>
        ))}
      </Select>
    </label>
  )
}
