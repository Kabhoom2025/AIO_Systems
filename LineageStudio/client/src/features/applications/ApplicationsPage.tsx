import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { CircleOff, ExternalLink, LayoutGrid, Plus, Rocket, Search, Trash2 } from 'lucide-react'
import { toast } from 'sonner'
import {
  createApplication,
  deleteApplication,
  listApplications,
  publishApplication,
  unpublishApplication,
} from '../../services/applications'
import { extractErrorMessage } from '../../utils/errors'
import { PageHeader } from '../../components/PageHeader'
import { Badge, Button, Card, EmptyState, Input, SkeletonList, confirmAction } from '../../components/ui'

export function ApplicationsPage() {
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [search, setSearch] = useState('')
  const [formError, setFormError] = useState<string | null>(null)

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['applications'],
    queryFn: listApplications,
  })

  const filtered = useMemo(
    () => (data ?? []).filter((app) => app.name.toLowerCase().includes(search.trim().toLowerCase())),
    [data, search],
  )

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['applications'] })

  const createMutation = useMutation({
    mutationFn: createApplication,
    onSuccess: (app) => {
      setName('')
      setDescription('')
      setFormError(null)
      invalidate()
      toast.success(`Created "${app.name}"`)
    },
    onError: (err) => setFormError(extractErrorMessage(err)),
  })

  const deleteMutation = useMutation({
    mutationFn: deleteApplication,
    onSuccess: () => {
      invalidate()
      toast.success('Application deleted')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const publishMutation = useMutation({
    mutationFn: publishApplication,
    onSuccess: () => {
      invalidate()
      toast.success('Application published')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  const unpublishMutation = useMutation({
    mutationFn: unpublishApplication,
    onSuccess: () => {
      invalidate()
      toast.success('Application unpublished')
    },
    onError: (err) => toast.error(extractErrorMessage(err)),
  })

  return (
    <div className="p-6 max-w-3xl">
      <PageHeader
        title="Applications"
        description="Create, publish, and manage the applications you're building."
      />

      <Card className="mb-6">
        <form
          className="flex flex-col gap-2 p-4"
          onSubmit={(e) => {
            e.preventDefault()
            createMutation.mutate({ name, description: description || undefined })
          }}
        >
          <div className="flex gap-2">
            <Input
              className="flex-1"
              placeholder="Application name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
            />
            <Input
              className="flex-1"
              placeholder="Description (optional)"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
            <Button type="submit" variant="primary" loading={createMutation.isPending}>
              <Plus size={14} /> Create
            </Button>
          </div>
          {formError && <p className="text-sm text-red-600">{formError}</p>}
        </form>
      </Card>

      {isLoading && <SkeletonList rows={3} />}

      {isError && (
        <p className="text-sm text-red-600">Could not reach the API ({extractErrorMessage(error)}).</p>
      )}

      {data && data.length > 0 && (
        <div className="mb-3 relative">
          <Search size={14} className="absolute left-2.5 top-1/2 -translate-y-1/2 text-neutral-400" />
          <Input
            className="w-full pl-8"
            placeholder="Search applications…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      )}

      {data && data.length === 0 && (
        <EmptyState
          icon={LayoutGrid}
          title="No applications yet"
          description="Create your first application above to start designing screens, tables, and APIs."
        />
      )}

      {data && data.length > 0 && filtered.length === 0 && (
        <EmptyState icon={Search} title="No applications match your search" />
      )}

      {filtered.length > 0 && (
        <Card>
          <ul className="divide-y divide-neutral-200 dark:divide-neutral-800">
            {filtered.map((app) => (
              <li key={app.id} className="p-4 flex items-center justify-between gap-4">
                <div>
                  <div className="font-medium text-neutral-900 dark:text-neutral-100 flex items-center gap-2">
                    <Link to="/screens" className="hover:underline">
                      {app.name}
                    </Link>
                    <Badge color={app.isPublished ? 'green' : 'neutral'}>
                      {app.isPublished ? 'Published' : 'Draft'}
                    </Badge>
                    {app.isPublished && (
                      <a
                        href={`/apps/${app.id}`}
                        target="_blank"
                        rel="noreferrer"
                        className="flex items-center gap-1 text-xs text-indigo-600 dark:text-indigo-400 hover:underline"
                      >
                        <ExternalLink size={12} /> Open app
                      </a>
                    )}
                  </div>
                  {app.description && <div className="text-sm text-neutral-500 mt-0.5">{app.description}</div>}
                </div>
                <div className="flex gap-1 shrink-0">
                  {app.isPublished ? (
                    <Button
                      variant="ghost"
                      size="sm"
                      loading={unpublishMutation.isPending}
                      onClick={async () => {
                        const confirmed = await confirmAction({
                          title: `Unpublish "${app.name}"?`,
                          description: 'Its screens and APIs stop being reachable at runtime until you publish again.',
                          confirmLabel: 'Unpublish',
                        })
                        if (confirmed) unpublishMutation.mutate(app.id)
                      }}
                    >
                      <CircleOff size={14} /> Unpublish
                    </Button>
                  ) : (
                    <Button
                      variant="ghost"
                      size="sm"
                      loading={publishMutation.isPending}
                      onClick={async () => {
                        const confirmed = await confirmAction({
                          title: `Publish "${app.name}"?`,
                          description: 'This makes its screens and APIs live and reachable at runtime.',
                          confirmLabel: 'Publish',
                        })
                        if (confirmed) publishMutation.mutate(app.id)
                      }}
                    >
                      <Rocket size={14} /> Publish
                    </Button>
                  )}
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={async () => {
                      const confirmed = await confirmAction({
                        title: `Delete "${app.name}"?`,
                        description: 'This permanently deletes the application and everything in it. This cannot be undone.',
                        confirmLabel: 'Delete',
                        danger: true,
                      })
                      if (confirmed) deleteMutation.mutate(app.id)
                    }}
                  >
                    <Trash2 size={14} /> Delete
                  </Button>
                </div>
              </li>
            ))}
          </ul>
        </Card>
      )}
    </div>
  )
}
