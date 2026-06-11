import type { ReactNode } from 'react'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface PageHeaderBackAction {
  label?: string
  title?: string
  onClick: () => void
  variant?: 'default' | 'outline' | 'ghost' | 'link' | 'destructive' | 'secondary'
}

interface PageHeaderProps {
  title: ReactNode
  description?: ReactNode
  icon?: ReactNode
  backAction?: PageHeaderBackAction
  actions?: ReactNode
  meta?: ReactNode
  className?: string
  titleClassName?: string
  descriptionClassName?: string
}

export function PageHeader({
  title,
  description,
  icon,
  backAction,
  actions,
  meta,
  className,
  titleClassName,
  descriptionClassName,
}: PageHeaderProps) {
  return (
    <div className={cn('mb-6', className)}>
      <div className="flex items-start justify-between gap-3 mb-4">
        <div className="flex items-center gap-2 flex-1 min-w-0">
          {backAction && (
            <Button
              variant={backAction.variant ?? 'ghost'}
              size="sm"
              onClick={backAction.onClick}
              title={backAction.title ?? backAction.label ?? '返回'}
              className="-ml-2 flex-shrink-0"
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              {backAction.label ?? '返回'}
            </Button>
          )}

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 min-w-0">
              {icon}
              <h1 className={cn('text-xl sm:text-2xl md:text-3xl font-bold truncate', titleClassName)}>
                {title}
              </h1>
              {meta}
            </div>
            {description && (
              <p className={cn('text-xs sm:text-sm text-muted-foreground mt-1 hidden sm:block', descriptionClassName)}>
                {description}
              </p>
            )}
          </div>
        </div>

        {actions && <div className="flex gap-1.5 flex-shrink-0">{actions}</div>}
      </div>
    </div>
  )
}