import { useState } from 'react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { RotateCcw } from 'lucide-react'
import type { DeletedAccountResponse } from '@/services/recycleBinService'

interface RestoreAccountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  account: DeletedAccountResponse | null
  onConfirm: (account: DeletedAccountResponse) => Promise<void>
}

/**
 * 恢复账号确认对话框
 * 显示账号信息并要求用户确认恢复操作
 */
export function RestoreAccountDialog({
  open,
  onOpenChange,
  account,
  onConfirm,
}: RestoreAccountDialogProps) {
  const [isRestoring, setIsRestoring] = useState(false)

  const handleConfirm = async () => {
    if (!account) return

    setIsRestoring(true)
    try {
      await onConfirm(account)
      onOpenChange(false)
    } catch (error) {
      // 错误已在父组件处理
      console.error('恢复失败:', error)
    } finally {
      setIsRestoring(false)
    }
  }

  const handleCancel = () => {
    onOpenChange(false)
  }

  if (!account) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <RotateCcw className="h-5 w-5 text-blue-600" />
            确认恢复账号
          </DialogTitle>
          <DialogDescription>
            确认要恢复此账号吗？
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-blue-50 p-4 border border-blue-200">
            <p className="text-sm font-medium text-blue-800 mb-3">
              📋 账号信息
            </p>
            <div className="text-sm text-blue-700 space-y-2">
              <p>
                用户名:{' '}
                <span className="font-bold">{account.username}</span>
              </p>
              <p className="text-gray-600">
                网站: {account.websiteDisplayName || account.websiteDomain}
              </p>
              {account.tags && (
                <p className="text-gray-600">标签: {account.tags}</p>
              )}
              {account.notes && (
                <p className="text-gray-600">备注: {account.notes}</p>
              )}
            </div>
          </div>

          <p className="text-sm text-gray-600 mt-4">
            恢复后，此账号将重新出现在账号列表中。
          </p>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isRestoring}
          >
            取消
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={isRestoring}
          >
            {isRestoring ? '恢复中...' : '确认恢复'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
