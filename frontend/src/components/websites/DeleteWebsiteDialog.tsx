import { useState, useEffect } from 'react'
import { websiteService } from '@/services/websiteService'
import type { WebsiteResponse } from '@/types'
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
 * 支持级联删除保护：
 * - 如果有活跃账号，阻止删除
 * - 如果只有回收站账号，显示确认提示
 */
export function DeleteWebsiteDialog({
  open,
  onOpenChange,
  onSuccess,
  website,
}: DeleteWebsiteDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [needsConfirmation, setNeedsConfirmation] = useState(false)
  const [deletedAccountCount, setDeletedAccountCount] = useState(0)

  // 当对话框打开时重置状态
  useEffect(() => {
    if (open) {
      setError(null)
      setNeedsConfirmation(false)
      setDeletedAccountCount(0)
    }
  }, [open])

  const handleDelete = async (confirmed: boolean = false) => {
    if (!website) {
      setError('网站数据不存在')
      return
    }

    setIsDeleting(true)
    setError(null)

    try {
      const params = confirmed ? '?confirmed=true' : ''
      const response = await websiteService.delete(website.id, params)

      if (response.success) {
        onOpenChange(false)
        setNeedsConfirmation(false)
        setDeletedAccountCount(0)
        onSuccess()
      }
    } catch (err) {
      const error = err as { errorCode?: string; message?: string; details?: { activeAccountCount?: number; deletedAccountCount?: number } }
      console.error('删除网站失败:', error)

      // 检查是否是业务错误（来自后端的错误响应）
      if (error.errorCode === 'ACTIVE_ACCOUNTS_EXIST') {
        setError(
          `无法删除网站：该网站下还有 ${error.details?.activeAccountCount || 0} 个活跃账号。\n请先删除或移至回收站所有账号。`
        )
      } else if (error.errorCode === 'CONFIRMATION_REQUIRED') {
        // 显示确认提示
        setNeedsConfirmation(true)
        setDeletedAccountCount(error.details?.deletedAccountCount || 0)
      } else {
        setError(error.message || '删除网站时发生错误，请重试')
      }
    } finally {
      setIsDeleting(false)
    }
  }

  const handleCancel = () => {
    setError(null)
    setNeedsConfirmation(false)
    setDeletedAccountCount(0)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-red-600" />
            确认删除网站
          </DialogTitle>
          <DialogDescription>此操作无法撤销,请确认是否继续</DialogDescription>
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

          {needsConfirmation && (
            <div className="mt-4 rounded-md bg-red-50 p-4 border border-red-200">
              <p className="text-sm font-medium text-red-800 mb-2">
                ⚠️ 危险操作警告
              </p>
              <p className="text-sm text-red-700">
                回收站中还有{' '}
                <span className="font-bold">{deletedAccountCount}</span>{' '}
                个已删除的账号。 删除网站将永久删除这些账号，此操作无法撤销！
              </p>
            </div>
          )}

          {website && website.activeAccountCount > 0 && !needsConfirmation && (
            <div className="mt-4 rounded-md bg-red-50 p-4 border border-red-200">
              <p className="text-sm font-medium text-red-800 mb-2">
                🚫 无法删除
              </p>
              <p className="text-sm text-red-700">
                该网站下还有{' '}
                <span className="font-bold">{website.activeAccountCount}</span>{' '}
                个活跃账号。 请先将所有账号删除或移至回收站后再删除网站。
              </p>
            </div>
          )}

          {error && (
            <div className="mt-4 rounded-md bg-red-50 p-3 text-sm text-red-800 whitespace-pre-wrap">
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
            {website && website.activeAccountCount > 0 && !needsConfirmation
              ? '关闭'
              : '取消'}
          </Button>
          <Button
            type="button"
            variant="destructive"
            onClick={() => handleDelete(needsConfirmation)}
            disabled={
              isDeleting ||
              (website !== null &&
                website.activeAccountCount > 0 &&
                !needsConfirmation)
            }
          >
            {isDeleting
              ? '删除中...'
              : needsConfirmation
                ? '确认永久删除'
                : '确认删除'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
