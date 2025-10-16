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
import type { AccountResponse } from '@/services/accountService'
import { Eye, EyeOff, Copy } from 'lucide-react'
import { useState } from 'react'

interface AccountListProps {
  accounts: AccountResponse[]
  onEdit: (account: AccountResponse) => void
  onDelete: (account: AccountResponse) => void
}

/**
 * 账号列表组件
 * 显示某网站下的账号列表，支持查看密码、复制密码、编辑和删除
 */
export function AccountList({ accounts, onEdit, onDelete }: AccountListProps) {
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

  const copyToClipboard = async (text: string) => {
    try {
      await navigator.clipboard.writeText(text)
      // 这里可以添加一个 toast 提示
    } catch (err) {
      console.error('复制失败:', err)
    }
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
    <div className="rounded-md border overflow-x-auto">
      <Table className="text-sm">
        <TableHeader>
          <TableRow>
            <TableHead className="h-10">用户名</TableHead>
            <TableHead className="h-10">密码</TableHead>
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
                  <code className="text-sm font-mono">
                    {visiblePasswords.has(account.id)
                      ? account.password
                      : '••••••••'}
                  </code>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => togglePasswordVisibility(account.id)}
                  >
                    {visiblePasswords.has(account.id) ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => copyToClipboard(account.password)}
                  >
                    <Copy className="h-4 w-4" />
                  </Button>
                </div>
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
                <div className="flex gap-2 justify-end">
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
  )
}
