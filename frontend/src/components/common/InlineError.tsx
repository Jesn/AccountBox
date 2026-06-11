import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'

interface InlineErrorProps {
  children: ReactNode
  className?: string
}

export function InlineError({ children, className }: InlineErrorProps) {
  if (!children) {
    return null
  }

  return (
    <div className={cn('rounded-md bg-red-50 p-3 text-sm text-red-800', className)}>
      {children}
    </div>
  )
}