import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import type { DeletedAccountResponse } from '@/services/recycleBinService'
import { RotateCcw, Trash2 } from 'lucide-react'

interface RecycleBinListProps {
  deletedAccounts: DeletedAccountResponse[]
  isLoading: boolean
  onRestore: (account: DeletedAccountResponse) => void
  onPermanentlyDelete: (account: DeletedAccountResponse) => void
}

/**
 * 回收站列表组件
 * 显示已删除账号列表,支持恢复和永久删除操作
 */
export function RecycleBinList({
  deletedAccounts,
  isLoading,
  onRestore,
  onPermanentlyDelete,
}: RecycleBinListProps) {
  // 格式化删除时间
  const formatDeletedAt = (deletedAt?: string) => {
    if (!deletedAt) return '未知'
    const date = new Date(deletedAt)
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  // 加载状态
  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600">加载中...</p>
        </CardContent>
      </Card>
    )
  }

  // 空状态
  if (deletedAccounts.length === 0) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600">回收站是空的</p>
          <p className="text-sm text-gray-500 mt-2">已删除的账号将在这里显示</p>
        </CardContent>
      </Card>
    )
  }

  // 已删除账号列表
  return (
    <div className="grid gap-4">
      {deletedAccounts.map((account) => (
        <Card key={account.id}>
          <CardContent className="p-4">
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <h3 className="text-lg font-semibold">{account.username}</h3>
                <p className="text-sm text-gray-600">
                  {account.websiteDisplayName || account.websiteDomain}
                </p>
                <p className="text-sm text-gray-500 mt-1">
                  删除时间: {formatDeletedAt(account.deletedAt)}
                </p>
                {account.tags && (
                  <p className="text-sm text-gray-500 mt-1">
                    标签: {account.tags}
                  </p>
                )}
                {account.notes && (
                  <p className="text-sm text-gray-500 mt-1 truncate max-w-md">
                    备注: {account.notes}
                  </p>
                )}
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => onRestore(account)}
                  title="恢复此账号"
                >
                  <RotateCcw className="mr-2 h-4 w-4" />
                  恢复
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => onPermanentlyDelete(account)}
                  title="永久删除此账号"
                >
                  <Trash2 className="mr-2 h-4 w-4" />
                  永久删除
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
