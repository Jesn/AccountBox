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
import { AlertTriangle } from 'lucide-react'
import type { DeletedAccountResponse } from '@/services/recycleBinService'

interface PermanentDeleteDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  account: DeletedAccountResponse | null
  onConfirm: (account: DeletedAccountResponse) => Promise<void>
}

/**
 * 永久删除账号确认对话框
 * 显示警告信息并要求用户二次确认操作
 */
export function PermanentDeleteDialog({
  open,
  onOpenChange,
  account,
  onConfirm,
}: PermanentDeleteDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)

  const handleConfirm = async () => {
    if (!account) return

    setIsDeleting(true)
    try {
      await onConfirm(account)
      onOpenChange(false)
    } catch (error) {
      // 错误已在父组件处理
      console.error('永久删除失败:', error)
    } finally {
      setIsDeleting(false)
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
            <AlertTriangle className="h-5 w-5 text-red-600" />
            确认永久删除
          </DialogTitle>
          <DialogDescription>
            此操作无法撤销，请谨慎操作
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-red-50 p-4 border border-red-200">
            <p className="text-sm font-medium text-red-800 mb-3">
              ⚠️ 危险操作警告
            </p>
            <div className="text-sm text-red-700 space-y-2">
              <p>
                您即将永久删除账号:{' '}
                <span className="font-bold">{account.username}</span>
              </p>
              <p className="text-gray-600">
                网站: {account.websiteDisplayName || account.websiteDomain}
              </p>
              {account.tags && (
                <p className="text-gray-600">标签: {account.tags}</p>
              )}
              <p className="font-semibold mt-3">此操作将:</p>
              <ul className="list-disc list-inside space-y-1 pl-2">
                <li>从数据库中物理删除该账号数据</li>
                <li>删除加密的密码和备注</li>
                <li>无法通过任何方式恢复</li>
              </ul>
            </div>
          </div>

          <div className="mt-4 rounded-md bg-yellow-50 p-4 border border-yellow-200">
            <p className="text-sm text-yellow-800">
              💡 提示: 如果您可能需要该账号信息，建议先恢复后再删除，或者保存相关信息后再删除。
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            disabled={isDeleting}
          >
            取消
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={handleConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? '删除中...' : '确认永久删除'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
