import { useState, useEffect } from 'react'
import { apiKeyService } from '@/services/apiKeyService'
import { websiteService, type WebsiteResponse } from '@/services/websiteService'
import type { CreateApiKeyRequest, ApiKeyScopeType } from '@/types/ApiKey'
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
import { Checkbox } from '@/components/ui/checkbox'

interface CreateApiKeyDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
}

/**
 * 创建API密钥对话框组件
 */
export function CreateApiKeyDialog({
  open,
  onOpenChange,
  onSuccess,
}: CreateApiKeyDialogProps) {
  const [name, setName] = useState('')
  const [scopeType, setScopeType] = useState<ApiKeyScopeType>('All')
  const [selectedWebsiteIds, setSelectedWebsiteIds] = useState<number[]>([])
  const [websites, setWebsites] = useState<WebsiteResponse[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [isLoadingWebsites, setIsLoadingWebsites] = useState(false)

  // 加载网站列表
  useEffect(() => {
    if (open && scopeType === 'Specific') {
      loadWebsites()
    }
  }, [open, scopeType])

  const loadWebsites = async () => {
    setIsLoadingWebsites(true)
    try {
      const response = await websiteService.getAll(1, 100) // 获取前100个网站
      if (response.success && response.data) {
        setWebsites(response.data.items)
      }
    } catch (err) {
      console.error('加载网站列表失败:', err)
    } finally {
      setIsLoadingWebsites(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    // 验证
    if (!name.trim()) {
      setError('密钥名称不能为空')
      return
    }

    if (scopeType === 'Specific' && selectedWebsiteIds.length === 0) {
      setError('指定网站作用域时必须选择至少一个网站')
      return
    }

    setIsSubmitting(true)
    try {
      const request: CreateApiKeyRequest = {
        name: name.trim(),
        scopeType,
        websiteIds: scopeType === 'Specific' ? selectedWebsiteIds : undefined,
      }

      await apiKeyService.create(request)

      // 重置表单
      setName('')
      setScopeType('All')
      setSelectedWebsiteIds([])
      onOpenChange(false)
      onSuccess()
    } catch (err) {
      const error = err as { response?: { data?: { error?: { message?: string } } }; message?: string }
      console.error('创建API密钥失败:', error)
      setError(
        error.response?.data?.error?.message || '创建API密钥时发生错误，请重试'
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setName('')
    setScopeType('All')
    setSelectedWebsiteIds([])
    setError(null)
    onOpenChange(false)
  }

  const toggleWebsiteSelection = (websiteId: number) => {
    setSelectedWebsiteIds((prev) =>
      prev.includes(websiteId)
        ? prev.filter((id) => id !== websiteId)
        : [...prev, websiteId]
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px] max-h-[80vh] overflow-y-auto">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>创建API密钥</DialogTitle>
            <DialogDescription>
              创建用于外部API调用的密钥，可配置作用域控制
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="name">
                密钥名称 <span className="text-red-500">*</span>
              </Label>
              <Input
                id="name"
                placeholder="例如：爬虫密钥"
                value={name}
                onChange={(e) => setName(e.target.value)}
                disabled={isSubmitting}
                autoFocus
              />
            </div>

            <div className="grid gap-2">
              <Label>作用域</Label>
              <div className="space-y-2">
                <div className="flex items-center space-x-2">
                  <input
                    type="radio"
                    id="scope-all"
                    name="scopeType"
                    checked={scopeType === 'All'}
                    onChange={() => setScopeType('All')}
                    disabled={isSubmitting}
                    className="h-4 w-4"
                  />
                  <Label
                    htmlFor="scope-all"
                    className="font-normal cursor-pointer"
                  >
                    所有网站
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <input
                    type="radio"
                    id="scope-specific"
                    name="scopeType"
                    checked={scopeType === 'Specific'}
                    onChange={() => setScopeType('Specific')}
                    disabled={isSubmitting}
                    className="h-4 w-4"
                  />
                  <Label
                    htmlFor="scope-specific"
                    className="font-normal cursor-pointer"
                  >
                    指定网站
                  </Label>
                </div>
              </div>
            </div>

            {scopeType === 'Specific' && (
              <div className="grid gap-2">
                <Label>选择网站</Label>
                {isLoadingWebsites ? (
                  <div className="text-sm text-muted-foreground">加载中...</div>
                ) : websites.length === 0 ? (
                  <div className="text-sm text-muted-foreground">
                    暂无网站，请先添加网站
                  </div>
                ) : (
                  <div className="max-h-48 overflow-y-auto border rounded-md p-3 space-y-2">
                    {websites.map((website) => (
                      <div
                        key={website.id}
                        className="flex items-center space-x-2"
                      >
                        <Checkbox
                          id={`website-${website.id}`}
                          checked={selectedWebsiteIds.includes(website.id)}
                          onCheckedChange={() =>
                            toggleWebsiteSelection(website.id)
                          }
                          disabled={isSubmitting}
                        />
                        <Label
                          htmlFor={`website-${website.id}`}
                          className="font-normal cursor-pointer flex-1"
                        >
                          {website.displayName || website.domain}
                        </Label>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}

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
