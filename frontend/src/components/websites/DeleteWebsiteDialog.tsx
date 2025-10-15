import { useState } from 'react'
import { websiteService } from '@/services/websiteService'
import type { WebsiteResponse } from '@/services/websiteService'
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

interface DeleteWebsiteDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  website: WebsiteResponse | null
}

/**
 * 删除网站确认对话框
 * 显示警告信息并要求用户确认删除操作
 */
export function DeleteWebsiteDialog({
  open,
  onOpenChange,
  onSuccess,
  website,
}: DeleteWebsiteDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleDelete = async () => {
    if (!website) {
      setError('网站数据不存在')
      return
    }

    setIsDeleting(true)
    setError(null)

    try {
      const response = await websiteService.delete(website.id)

      if (response.success) {
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '删除失败')
      }
    } catch (err) {
      console.error('删除网站失败:', err)
      setError('删除网站时发生错误，请重试')
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
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-red-600" />
            确认删除网站
          </DialogTitle>
          <DialogDescription>
            此操作无法撤销,请确认是否继续
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-yellow-50 p-4 border border-yellow-200">
            <p className="text-sm font-medium text-yellow-800 mb-2">
              您即将删除以下网站:
            </p>
            <div className="text-sm text-yellow-700 space-y-1">
              <p>
                <span className="font-semibold">网站名称:</span>{' '}
                {website?.displayName || website?.domain}
              </p>
              <p>
                <span className="font-semibold">域名:</span> {website?.domain}
              </p>
              {website && (
                <>
                  <p>
                    <span className="font-semibold">活跃账号:</span>{' '}
                    {website.activeAccountCount} 个
                  </p>
                  {website.deletedAccountCount > 0 && (
                    <p>
                      <span className="font-semibold">回收站账号:</span>{' '}
                      {website.deletedAccountCount} 个
                    </p>
                  )}
                </>
              )}
            </div>
          </div>

          {website && website.activeAccountCount > 0 && (
            <div className="mt-4 rounded-md bg-red-50 p-4 border border-red-200">
              <p className="text-sm font-medium text-red-800">
                ⚠️ 警告: 该网站下还有 {website.activeAccountCount} 个活跃账号,
                删除网站将同时删除所有关联账号!
              </p>
            </div>
          )}

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
            {isDeleting ? '删除中...' : '确认删除'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
