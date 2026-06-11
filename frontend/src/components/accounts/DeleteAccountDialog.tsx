import { useState } from 'react'
import { accountService } from '@/services/accountService'
import type { AccountResponse } from '@/types'
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

interface DeleteAccountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  account: AccountResponse | null
}

/**
 * 删除账号确认对话框
 * 显示警告信息并要求用户确认删除操作（软删除）
 */
export function DeleteAccountDialog({
  open,
  onOpenChange,
  onSuccess,
  account,
}: DeleteAccountDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleDelete = async () => {
    if (!account) {
      setError('账号数据不存在')
      return
    }

    setIsDeleting(true)
    setError(null)

    try {
      const response = await accountService.delete(account.id)

      if (response.success) {
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '删除失败')
      }
    } catch (err) {
      console.error('删除账号失败:', err)
      setError('删除账号时发生错误，请重试')
    } finally {
      setIsDeleting(false)
    }
  }

  const handleCancel = () => {
    setError(null)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-orange-600" />
            确认删除账号
          </DialogTitle>
          <DialogDescription>
            账号将被移至回收站，可在需要时恢复
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-orange-50 p-4 border border-orange-200">
            <p className="text-sm font-medium text-orange-800 mb-2">
              您即将删除以下账号:
            </p>
            <div className="text-sm text-orange-700 space-y-1">
              <p>
                <span className="font-semibold">网站:</span>{' '}
                {account?.websiteDomain}
              </p>
              <p>
                <span className="font-semibold">用户名:</span>{' '}
                {account?.username}
              </p>
              {account?.notes && (
                <p>
                  <span className="font-semibold">备注:</span>{' '}
                  {account.notes.substring(0, 50)}
                  {account.notes.length > 50 ? '...' : ''}
                </p>
              )}
            </div>
          </div>

          <div className="mt-4 rounded-md bg-blue-50 p-4 border border-blue-200">
            <p className="text-sm text-blue-800">
              💡 提示:
              这是软删除操作，账号将被移至回收站。您可以在回收站中恢复此账号，或永久删除。
            </p>
          </div>

          {error && (
            <div className="mt-4 rounded-md bg-red-50 p-3 text-sm text-red-800">
              {error}
            </div>
          )}
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
            onClick={handleDelete}
            disabled={isDeleting}
          >
            {isDeleting ? '删除中...' : '移至回收站'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
