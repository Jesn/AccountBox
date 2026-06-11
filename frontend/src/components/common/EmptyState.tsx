import type { ReactNode } from 'react'
import { Card, CardContent } from '@/components/ui/card'
import { cn } from '@/lib/utils'

interface EmptyStateProps {
  title: ReactNode
  description?: ReactNode
  action?: ReactNode
  icon?: ReactNode
  className?: string
}

export function EmptyState({ title, description, action, icon, className }: EmptyStateProps) {
  return (
    <Card>
      <CardContent className={cn('py-12 text-center', className)}>
        {icon && <div className="mb-4 flex justify-center text-gray-400">{icon}</div>}
        <p className="text-gray-600 mb-2">{title}</p>
        {description && <p className="text-sm text-gray-500 mb-4">{description}</p>}
        {action}
      </CardContent>
    </Card>
  )
}