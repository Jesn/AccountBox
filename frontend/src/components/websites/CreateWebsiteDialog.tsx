import { useState } from 'react'
import { websiteService } from '@/services/websiteService'
import type { CreateWebsiteRequest } from '@/services/websiteService'
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

interface CreateWebsiteDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

/**
 * 创建网站对话框组件
 * 允许用户输入域名、显示名称和标签创建新网站
 */
export function CreateWebsiteDialog({
  open,
  onOpenChange,
  onSuccess,
}: CreateWebsiteDialogProps) {
  const [domain, setDomain] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [tags, setTags] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    // 基础验证
    if (!domain.trim()) {
      setError('域名不能为空')
      return
    }

    setIsSubmitting(true)
    try {
      const request: CreateWebsiteRequest = {
        domain: domain.trim(),
        displayName: displayName.trim() || undefined,
        tags: tags.trim() || undefined,
      }

      const response = await websiteService.create(request)

      if (response.success) {
        // 重置表单
        setDomain('')
        setDisplayName('')
        setTags('')
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '创建失败')
      }
    } catch (err) {
      console.error('创建网站失败:', err)
      setError('创建网站时发生错误，请重试')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setDomain('')
    setDisplayName('')
    setTags('')
    setError(null)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>添加网站</DialogTitle>
            <DialogDescription>
              输入网站的域名和可选的显示名称、标签
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="domain">
                域名 <span className="text-red-500">*</span>
              </Label>
              <Input
                id="domain"
                placeholder="example.com"
                value={domain}
                onChange={(e) => setDomain(e.target.value)}
                disabled={isSubmitting}
                autoFocus
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="displayName">显示名称（可选）</Label>
              <Input
                id="displayName"
                placeholder="示例网站"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                disabled={isSubmitting}
              />
            </div>

            <div className="grid gap-2">
              <Label htmlFor="tags">标签（可选）</Label>
              <Textarea
                id="tags"
                placeholder="工作, 重要"
                value={tags}
                onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => setTags(e.target.value)}
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
              {isSubmitting ? '创建中...' : '创建'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
