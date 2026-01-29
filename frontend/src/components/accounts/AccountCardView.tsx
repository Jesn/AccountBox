import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { CopyButton } from '@/components/common/CopyButton'
import { AccountStatusBadge } from '@/components/accounts/AccountStatusBadge'
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import type { AccountResponse } from '@/services/accountService'
import { Eye, EyeOff, Edit, Trash2, CheckCircle, XCircle } from 'lucide-react'
import { useState } from 'react'

interface AccountCardViewProps {
  accounts: AccountResponse[]
  onEdit: (account: AccountResponse) => void
  onDelete: (account: AccountResponse) => void
  onEnable?: (account: AccountResponse) => void
  onDisable?: (account: AccountResponse) => void
}

/**
 * 账号卡片视图组件（移动端）
 * 以卡片形式显示账号列表，适合小屏幕设备
 */
export function AccountCardView({
  accounts,
  onEdit,
  onDelete,
  onEnable,
  onDisable,
}: AccountCardViewProps) {
  const [visiblePasswords, setVisiblePasswords] = useState<Set<number>>(
    new Set()
  )

  const togglePasswordVisibility = (accountId: number) => {
    setVisiblePasswords((prev) => {
      const newSet = new Set(prev)
      if (newSet.has(accountId)) {
        newSet.delete(accountId)
      } else {
        newSet.add(accountId)
      }
      return newSet
    })
  }

  if (accounts.length === 0) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600">暂无账号</p>
        </CardContent>
      </Card>
    )
  }

  return (
    <TooltipProvider>
      <div className="space-y-4">
        {accounts.map((account) => {
          const isPasswordVisible = visiblePasswords.has(account.id)
          const displayPassword = isPasswordVisible
            ? account.password
            : '••••••••'

          return (
            <Card key={account.id} className="overflow-hidden">
              <CardContent className="p-4">
                {/* 用户名和状态 */}
                <div className="flex items-start justify-between mb-3">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <h3 className="font-medium text-base truncate">
                        {account.username}
                      </h3>
                      <AccountStatusBadge status={account.status} />
                    </div>
                    {account.tags && (
                      <p className="text-sm text-gray-600 truncate">
                        标签: {account.tags}
                      </p>
                    )}
                  </div>
                  <CopyButton
                    text={account.username}
                    successMessage="用户名已复制"
                    className="h-8 w-8 p-0 flex-shrink-0"
                  />
                </div>

                {/* 密码 */}
                <div className="mb-3">
                  <label className="text-xs text-gray-500 mb-1 block">密码</label>
                  <div className="flex items-center gap-2">
                    <div className="flex-1 font-mono text-sm bg-gray-50 px-3 py-2 rounded border">
                      {displayPassword}
                    </div>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => togglePasswordVisibility(account.id)}
                      className="h-8 w-8 flex-shrink-0"
                    >
                      {isPasswordVisible ? (
                        <EyeOff className="h-4 w-4" />
                      ) : (
                        <Eye className="h-4 w-4" />
                      )}
                    </Button>
                    <CopyButton
                      text={account.password}
                      successMessage="密码已复制"
                      className="h-8 w-8 p-0 flex-shrink-0"
                    />
                  </div>
                </div>

                {/* 备注 */}
                {account.notes && (
                  <div className="mb-3">
                    <label className="text-xs text-gray-500 mb-1 block">备注</label>
                    <p className="text-sm text-gray-700 line-clamp-2">
                      {account.notes}
                    </p>
                  </div>
                )}

                {/* 扩展字段 */}
                {account.extendedData &&
                  Object.keys(account.extendedData).length > 0 && (
                    <div className="mb-3">
                      <label className="text-xs text-gray-500 mb-1 block">
                        扩展字段
                      </label>
                      <div className="space-y-1">
                        {Object.entries(account.extendedData).map(
                          ([key, value]) => (
                            <div
                              key={key}
                              className="text-sm bg-gray-50 px-2 py-1 rounded"
                            >
                              <span className="text-gray-600">{key}:</span>{' '}
                              <span className="text-gray-900">
                                {typeof value === 'string'
                                  ? value
                                  : JSON.stringify(value)}
                              </span>
                            </div>
                          )
                        )}
                      </div>
                    </div>
                  )}

                {/* 时间信息 */}
                <div className="text-xs text-gray-500 mb-3 space-y-1">
                  <div>
                    创建: {new Date(account.createdAt).toLocaleString('zh-CN')}
                  </div>
                  <div>
                    更新: {new Date(account.updatedAt).toLocaleString('zh-CN')}
                  </div>
                </div>

                {/* 操作按钮 */}
                <div className="flex gap-2 pt-3 border-t">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onEdit(account)}
                    className="flex-1"
                  >
                    <Edit className="h-4 w-4 mr-1" />
                    编辑
                  </Button>

                  {account.status === 'Active' && onDisable && (
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => onDisable(account)}
                        >
                          <XCircle className="h-4 w-4" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>禁用账号</TooltipContent>
                    </Tooltip>
                  )}

                  {account.status === 'Disabled' && onEnable && (
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => onEnable(account)}
                        >
                          <CheckCircle className="h-4 w-4" />
                        </Button>
                      </TooltipTrigger>
                      <TooltipContent>启用账号</TooltipContent>
                    </Tooltip>
                  )}

                  <Tooltip>
                    <TooltipTrigger asChild>
                      <Button
                        variant="destructive"
                        size="sm"
                        onClick={() => onDelete(account)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </TooltipTrigger>
                    <TooltipContent>删除账号</TooltipContent>
                  </Tooltip>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>
    </TooltipProvider>
  )
}
