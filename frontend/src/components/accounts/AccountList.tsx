import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
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
import { Eye, EyeOff, CheckCircle, XCircle } from 'lucide-react'
import { useState } from 'react'

interface AccountListProps {
  accounts: AccountResponse[]
  onEdit: (account: AccountResponse) => void
  onDelete: (account: AccountResponse) => void
  onEnable?: (account: AccountResponse) => void
  onDisable?: (account: AccountResponse) => void
}

/**
 * 账号列表组件
 * 显示某网站下的账号列表，支持查看密码、复制密码、编辑和删除
 */
export function AccountList({
  accounts,
  onEdit,
  onDelete,
  onEnable,
  onDisable,
}: AccountListProps) {
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
      <div className="rounded-md border">
        <Table className="text-sm">
          <TableHeader>
            <TableRow>
              <TableHead className="h-10">用户名</TableHead>
              <TableHead className="h-10">密码</TableHead>
              <TableHead className="h-10">状态</TableHead>
              <TableHead className="hidden md:table-cell h-10">标签</TableHead>
              <TableHead className="hidden lg:table-cell h-10">备注</TableHead>
              <TableHead className="hidden xl:table-cell h-10">
                创建时间
              </TableHead>
              <TableHead className="hidden xl:table-cell h-10">
                更新时间
              </TableHead>
              <TableHead className="text-right h-10">操作</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
          {accounts.map((account) => (
            <TableRow key={account.id}>
              {/* 用户名 */}
              <TableCell className="font-medium py-2 px-3">
                {account.username}
              </TableCell>

              {/* 密码（可切换显示/隐藏）*/}
              <TableCell className="py-2 px-3">
                <div className="flex items-center gap-2">
                  {visiblePasswords.has(account.id) &&
                  account.password.length > 8 ? (
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <code className="text-sm font-mono block cursor-help">
                          {account.password.substring(0, 8)}...
                        </code>
                      </TooltipTrigger>
                      <TooltipContent>
                        <p className="font-mono text-xs max-w-md break-all">
                          {account.password}
                        </p>
                      </TooltipContent>
                    </Tooltip>
                  ) : (
                    <code className="text-sm font-mono block">
                      {visiblePasswords.has(account.id)
                        ? account.password
                        : '••••••••'}
                    </code>
                  )}
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => togglePasswordVisibility(account.id)}
                    className="h-8 w-8 p-0 flex-shrink-0"
                    title={
                      visiblePasswords.has(account.id)
                        ? '隐藏密码'
                        : '显示密码'
                    }
                  >
                    {visiblePasswords.has(account.id) ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                  <CopyButton
                    text={account.password}
                    successMessage="密码已复制到剪贴板"
                    className="h-8 w-8 p-0 flex-shrink-0"
                    title="复制密码"
                  />
                </div>
              </TableCell>

              {/* 状态 */}
              <TableCell className="py-2 px-3">
                <AccountStatusBadge status={account.status} />
              </TableCell>

              {/* 标签 */}
              <TableCell className="hidden md:table-cell py-2 px-3">
                {account.tags || '-'}
              </TableCell>

              {/* 备注（截断显示）*/}
              <TableCell
                className="hidden lg:table-cell max-w-xs truncate py-2 px-3"
                title={account.notes || ''}
              >
                {account.notes || '-'}
              </TableCell>

              {/* 创建时间 */}
              <TableCell className="hidden xl:table-cell text-sm text-gray-500 py-2 px-3">
                {new Date(account.createdAt).toLocaleString('zh-CN')}
              </TableCell>

              {/* 更新时间 */}
              <TableCell className="hidden xl:table-cell text-sm text-gray-500 py-2 px-3">
                {new Date(account.updatedAt).toLocaleString('zh-CN')}
              </TableCell>

              {/* 操作 */}
              <TableCell className="text-right py-2 px-3">
                <div className="flex gap-2 justify-end items-center whitespace-nowrap">
                  {account.status === 'Disabled' && onEnable && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => onEnable(account)}
                      title="启用账号"
                    >
                      <CheckCircle className="h-4 w-4 mr-1" />
                      启用
                    </Button>
                  )}
                  {account.status === 'Active' && onDisable && (
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => onDisable(account)}
                      title="禁用账号"
                    >
                      <XCircle className="h-4 w-4 mr-1" />
                      禁用
                    </Button>
                  )}
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onEdit(account)}
                  >
                    编辑
                  </Button>
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => onDelete(account)}
                  >
                    删除
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
    </TooltipProvider>
  )
}
