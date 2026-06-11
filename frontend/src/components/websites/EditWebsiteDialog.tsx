import { useState, useEffect } from 'react'
import { websiteService } from '@/services/websiteService'
import type {
  UpdateWebsiteRequest,
  WebsiteResponse,
} from '@/types'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { WebsiteForm } from '@/components/websites/WebsiteForm'

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
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>编辑网站</DialogTitle>
            <DialogDescription>
              修改网站的域名、显示名称和标签
            </DialogDescription>
          </DialogHeader>

          <WebsiteForm
            idPrefix="edit-website"
            domain={domain}
            displayName={displayName}
            tags={tags}
            error={error}
            disabled={isSubmitting}
            autoFocus
            onDomainChange={setDomain}
            onDisplayNameChange={setDisplayName}
            onTagsChange={setTags}
          />

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
