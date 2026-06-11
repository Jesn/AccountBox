import type { ReactNode } from 'react'
import { Card, CardContent } from '@/components/ui/card'
import { cn } from '@/lib/utils'

interface LoadingStateProps {
  message?: ReactNode
  className?: string
  card?: boolean
}

export function LoadingState({
  message = '加载中...',
  className,
  card = true,
}: LoadingStateProps) {
  const content = (
    <div className={cn('py-12 text-center', className)}>
      <p className="text-gray-600">{message}</p>
    </div>
  )

  if (!card) {
    return content
  }

  return (
    <Card>
      <CardContent>{content}</CardContent>
    </Card>
  )
}