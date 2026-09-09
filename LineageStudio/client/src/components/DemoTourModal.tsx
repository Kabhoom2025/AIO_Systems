import { ArrowRight, X } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useDemoTourStore } from '../store/demoTour'
import { DEMO_STEPS } from './demoSteps'
import { Button } from './ui'
import { cn } from '../utils/cn'

export function DemoTourModal() {
  const isOpen = useDemoTourStore((s) => s.isOpen)
  const stepIndex = useDemoTourStore((s) => s.stepIndex)
  const close = useDemoTourStore((s) => s.close)
  const next = useDemoTourStore((s) => s.next)
  const back = useDemoTourStore((s) => s.back)
  const goToStep = useDemoTourStore((s) => s.goToStep)
  const navigate = useNavigate()

  if (!isOpen) return null

  const step = DEMO_STEPS[stepIndex]
  const Icon = step.icon
  const isFirst = stepIndex === 0
  const isLast = stepIndex === DEMO_STEPS.length - 1

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" onClick={close}>
      <div
        role="dialog"
        aria-modal="true"
        className="w-full max-w-md rounded-lg bg-white dark:bg-neutral-900 border border-neutral-200 dark:border-neutral-800 shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start gap-3 p-5 pb-3">
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-indigo-50 dark:bg-indigo-950/50">
            <Icon size={18} className="text-indigo-600 dark:text-indigo-400" />
          </div>
          <div className="min-w-0 flex-1">
            <div className="text-[11px] font-semibold uppercase tracking-wider text-neutral-400">
              Step {stepIndex + 1} of {DEMO_STEPS.length}
            </div>
            <h2 className="text-sm font-semibold text-neutral-900 dark:text-neutral-100">{step.title}</h2>
          </div>
          <button
            onClick={close}
            aria-label="Close demo"
            className="shrink-0 rounded-md p-1 text-neutral-400 hover:bg-neutral-100 dark:hover:bg-neutral-800 hover:text-neutral-600 dark:hover:text-neutral-300"
          >
            <X size={16} />
          </button>
        </div>

        <div className="px-5 pb-2">
          <ul className="flex flex-col gap-2">
            {step.points.map((point) => (
              <li key={point} className="flex gap-2 text-sm text-neutral-600 dark:text-neutral-300">
                <span className="mt-1.5 h-1 w-1 shrink-0 rounded-full bg-indigo-500" />
                {point}
              </li>
            ))}
          </ul>
        </div>

        <div className="flex items-center justify-center gap-1.5 py-3">
          {DEMO_STEPS.map((s, i) => (
            <button
              key={s.title}
              aria-label={`Go to step ${i + 1}`}
              onClick={() => goToStep(i)}
              className={cn(
                'h-1.5 rounded-full transition-all',
                i === stepIndex ? 'w-4 bg-indigo-600' : 'w-1.5 bg-neutral-300 dark:bg-neutral-700',
              )}
            />
          ))}
        </div>

        <div className="flex items-center justify-between gap-2 border-t border-neutral-200 dark:border-neutral-800 p-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              navigate(step.route)
              close()
            }}
          >
            Open {step.routeLabel} <ArrowRight size={14} />
          </Button>

          <div className="flex gap-2">
            <Button variant="secondary" size="sm" onClick={back} disabled={isFirst}>
              Back
            </Button>
            {isLast ? (
              <Button variant="primary" size="sm" onClick={close}>
                Finish
              </Button>
            ) : (
              <Button variant="primary" size="sm" onClick={() => next(DEMO_STEPS.length - 1)}>
                Next
              </Button>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
