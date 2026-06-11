import { useState } from 'react'
import { websiteService } from '@/services/websiteService'
import type { CreateWebsiteRequest } from '@/types'
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
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>添加网站</DialogTitle>
            <DialogDescription>
              输入网站的域名和可选的显示名称、标签
            </DialogDescription>
          </DialogHeader>

          <WebsiteForm
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
              {isSubmitting ? '创建中...' : '创建'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
