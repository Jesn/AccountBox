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
  const [visiblePasswords, setVisiblePasswords] = useState<Set<number>>(new Set())

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
    <div className="grid gap-4">
      {accounts.map((account) => (
        <Card key={account.id}>
          <CardContent className="p-4">
            <div className="flex justify-between items-start">
              <div className="flex-1 space-y-2">
                <div>
                  <label className="text-sm font-medium text-gray-500">用户名</label>
                  <p className="text-base font-medium">{account.username}</p>
                </div>

                <div>
                  <label className="text-sm font-medium text-gray-500">密码</label>
                  <div className="flex items-center gap-2">
                    <code className="text-base font-mono">
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
                </div>

                {account.notes && (
                  <div>
                    <label className="text-sm font-medium text-gray-500">备注</label>
                    <p className="text-sm text-gray-700 whitespace-pre-wrap">
                      {account.notes}
                    </p>
                  </div>
                )}

                {account.tags && (
                  <div>
                    <label className="text-sm font-medium text-gray-500">标签</label>
                    <p className="text-sm text-gray-600">{account.tags}</p>
                  </div>
                )}

                <div className="flex gap-4 text-xs text-gray-500">
                  <span>创建时间: {new Date(account.createdAt).toLocaleString('zh-CN')}</span>
                  <span>更新时间: {new Date(account.updatedAt).toLocaleString('zh-CN')}</span>
                </div>
              </div>

              <div className="flex gap-2">
                <Button variant="outline" size="sm" onClick={() => onEdit(account)}>
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
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
