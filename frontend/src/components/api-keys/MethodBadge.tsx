import { cn } from '@/lib/utils'

const methodClassName: Record<string, string> = {
  GET: 'bg-blue-100 text-blue-700',
  POST: 'bg-green-100 text-green-700',
  PUT: 'bg-yellow-100 text-yellow-700',
  DELETE: 'bg-red-100 text-red-700',
}

interface MethodBadgeProps {
  method: string
}

export function MethodBadge({ method }: MethodBadgeProps) {
  return (
    <span
      className={cn(
        'px-1.5 sm:px-2 py-0.5 sm:py-1 text-[10px] sm:text-xs font-semibold rounded flex-shrink-0',
        methodClassName[method] ?? 'bg-gray-100 text-gray-700'
      )}
    >
      {method}
    </span>
  )
}