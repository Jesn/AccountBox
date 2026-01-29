import { useState } from 'react'
import { apiKeyService } from '@/services/apiKeyService'
import type { ApiKey } from '@/types/ApiKey'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'

interface DeleteApiKeyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  apiKey: ApiKey | null
  onSuccess: () => void
}

/**
 * 删除API密钥对话框组件
 */
export function DeleteApiKeyDialog({
  open,
  onOpenChange,
  apiKey,
  onSuccess,
}: DeleteApiKeyDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleDelete = async () => {
    if (!apiKey) return

    setError(null)
    setIsDeleting(true)
    try {
      await apiKeyService.delete(apiKey.id)
      onOpenChange(false)
      onSuccess()
    } catch (err) {
      const error = err as { response?: { data?: { error?: { message?: string } } }; message?: string }
      console.error('删除API密钥失败:', error)
      setError(
        error.response?.data?.error?.message || '删除API密钥时发生错误，请重试'
      )
    } finally {
      setIsDeleting(false)
    }
  }

  const handleCancel = () => {
    setError(null)
    onOpenChange(false)
  }

  if (!apiKey) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>删除API密钥</DialogTitle>
          <DialogDescription>
            确定要删除密钥 "{apiKey.name}" 吗？此操作无法撤销。
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-yellow-50 p-3 text-sm text-yellow-800">
            <strong>警告：</strong> 使用此密钥的外部系统将无法继续访问API。
          </div>

          {error && (
            <div className="mt-3 rounded-md bg-red-50 p-3 text-sm text-red-800">
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
            {isDeleting ? '删除中...' : '确认删除'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
