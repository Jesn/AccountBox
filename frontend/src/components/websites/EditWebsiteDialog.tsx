import { useState, useEffect } from 'react'
import { websiteService } from '@/services/websiteService'
import type {
  UpdateWebsiteRequest,
  WebsiteResponse,
} from '@/services/websiteService'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

interface EditWebsiteDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  website: WebsiteResponse | null
}

/**
 * 编辑网站对话框组件
 * 允许用户修改网站的域名、显示名称和标签
 */
export function EditWebsiteDialog({
  open,
  onOpenChange,
  onSuccess,
  website,
}: EditWebsiteDialogProps) {
  const [domain, setDomain] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [tags, setTags] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // 当对话框打开或网站数据变化时，更新表单
  useEffect(() => {
    if (website) {
      setDomain(website.domain)
      setDisplayName(website.displayName || '')
      setTags(website.tags || '')
    }
  }, [website])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!website) {
      setError('网站数据不存在')
      return
    }

    // 基础验证
    if (!domain.trim()) {
      setError('域名不能为空')
      return
    }

    setIsSubmitting(true)
    try {
      const request: UpdateWebsiteRequest = {
        domain: domain.trim(),
        displayName: displayName.trim() || undefined,
        tags: tags.trim() || undefined,
      }

      const response = await websiteService.update(website.id, request)

      if (response.success) {
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '更新失败')
      }
    } catch (err) {
      console.error('更新网站失败:', err)
      setError('更新网站时发生错误，请重试')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setError(null)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>编辑网站</DialogTitle>
            <DialogDescription>
              修改网站的域名、显示名称和标签
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="edit-domain">
                域名 <span className="text-red-500">*</span>
              </Label>
              <Input
                id="edit-domain"
                placeholder="example.com"
                value={domain}
                onChange={(e) => setDomain(e.target.value)}
                disabled={isSubmitting}
                autoFocus
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="edit-displayName">显示名称（可选）</Label>
              <Input
                id="edit-displayName"
                placeholder="示例网站"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                disabled={isSubmitting}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="edit-tags">标签（可选）</Label>
              <Textarea
                id="edit-tags"
                placeholder="工作, 重要"
                value={tags}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                  setTags(e.target.value)
                }
                disabled={isSubmitting}
                rows={3}
              />
            </div>

            {error && (
              <div className="rounded-md bg-red-50 p-3 text-sm text-red-800">
                {error}
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleCancel}
              disabled={isSubmitting}
            >
              取消
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? '保存中...' : '保存'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
