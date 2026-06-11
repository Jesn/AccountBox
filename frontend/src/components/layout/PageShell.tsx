import type { ReactNode } from 'react'
import { cn } from '@/lib/utils'

interface PageShellProps {
  children: ReactNode
  maxWidth?: string
  className?: string
  contentClassName?: string
}

export function PageShell({
  children,
  maxWidth = 'max-w-7xl',
  className,
  contentClassName,
}: PageShellProps) {
  return (
    <div className={cn('min-h-screen bg-gray-50 p-4 md:p-6 lg:p-8', className)}>
      <div className={cn('mx-auto', maxWidth, contentClassName)}>{children}</div>
    </div>
  )
}