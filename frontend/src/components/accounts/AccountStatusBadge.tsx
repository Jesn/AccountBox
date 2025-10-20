import type { Account } from '@/types/common'

interface AccountStatusBadgeProps {
  status: Account['status']
  className?: string
}

/**
 * 账号状态标识组件
 */
export function AccountStatusBadge({
  status,
  className = '',
}: AccountStatusBadgeProps) {
  if (status === 'Active') {
    return (
      <span
        className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800 ${className}`}
      >
        活跃
      </span>
    )
  }

  return (
    <span
      className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800 ${className}`}
    >
      已禁用
    </span>
  )
}
