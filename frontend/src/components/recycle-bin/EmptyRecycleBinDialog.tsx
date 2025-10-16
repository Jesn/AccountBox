import { useState } from 'react'
import { recycleBinService } from '@/services/recycleBinService'
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

interface EmptyRecycleBinDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  totalCount: number
  websiteId?: number
}

/**
 * 清空回收站确认对话框
 * 显示强警告信息并要求用户二次确认操作
 */
export function EmptyRecycleBinDialog({
  open,
  onOpenChange,
  onSuccess,
  totalCount,
  websiteId,
}: EmptyRecycleBinDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleEmpty = async () => {
    setIsDeleting(true)
    setError(null)

    try {
      const response = await recycleBinService.emptyRecycleBin(websiteId)

      if (response.success) {
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '清空回收站失败')
      }
    } catch (err) {
      console.error('清空回收站失败:', err)
      setError('清空回收站时发生错误,请重试')
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
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-red-600" />
            确认清空回收站
          </DialogTitle>
          <DialogDescription>
            此操作将永久删除所有账号,无法撤销!
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-red-50 p-4 border border-red-200">
            <p className="text-sm font-medium text-red-800 mb-3">
              ⚠️ 危险操作警告
            </p>
            <div className="text-sm text-red-700 space-y-2">
              <p>
                您即将永久删除回收站中的{' '}
                <span className="font-bold">{totalCount}</span> 个账号
              </p>
              <p className="font-semibold">此操作将:</p>
              <ul className="list-disc list-inside space-y-1 pl-2">
                <li>从数据库中物理删除所有账号数据</li>
                <li>删除所有加密的密码和备注</li>
                <li>无法通过任何方式恢复</li>
              </ul>
            </div>
          </div>

          <div className="mt-4 rounded-md bg-yellow-50 p-4 border border-yellow-200">
            <p className="text-sm text-yellow-800">
              💡 提示: 如果您只想删除特定账号,请在列表中单独操作。
              清空回收站将删除所有已删除的账号。
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
            onClick={handleEmpty}
            disabled={isDeleting || totalCount === 0}
          >
            {isDeleting ? '删除中...' : '确认清空回收站'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
