import { useEffect, useState } from 'react'
import { apiKeyService } from '@/services/apiKeyService'
import { websiteService } from '@/services/websiteService'
import type { WebsiteResponse } from '@/types'
import type { ApiKey } from '@/types'
import { Button } from '@/components/ui/button'
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { InlineError } from '@/components/common/InlineError'
import { LoadingState } from '@/components/common/LoadingState'
import { ResponsiveActionButton } from '@/components/common/ResponsiveActionButton'
import { CreateApiKeyDialog } from '@/components/api-keys/CreateApiKeyDialog'
import { DeleteApiKeyDialog } from '@/components/api-keys/DeleteApiKeyDialog'
import { ApiKeyList } from '@/components/api-keys/ApiKeyList'
import { APP_CONFIG } from '@/lib/constants'
import { Plus, RefreshCw, BookOpen } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

/**
 * API密钥管理页面
 */
export function ApiKeysPage() {
  const navigate = useNavigate()
  const [apiKeys, setApiKeys] = useState<ApiKey[]>([])
  const [websites, setWebsites] = useState<WebsiteResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [selectedApiKey, setSelectedApiKey] = useState<ApiKey | null>(null)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setIsLoading(true)
    setError(null)
    try {
      // 并行加载 API 密钥和网站列表
      const [keysData, websitesData] = await Promise.all([
        apiKeyService.getAll(),
        websiteService.getAll(1, APP_CONFIG.MAX_WEBSITE_OPTIONS),
      ])
      setApiKeys(keysData)
      if (websitesData.data) {
        setWebsites(websitesData.data.items)
      }
    } catch (err) {
      const error = err as { message?: string }
      console.error('加载数据失败:', error)
      setError(error.message || '加载数据失败，请重试')
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateSuccess = () => {
    loadData()
  }

  const handleDeleteClick = (apiKey: ApiKey) => {
    setSelectedApiKey(apiKey)
    setIsDeleteDialogOpen(true)
  }

  const handleDeleteSuccess = () => {
    loadData()
    setSelectedApiKey(null)
  }

  return (
    <PageShell maxWidth="max-w-4xl">
      <PageHeader
        title="API密钥管理"
        description="创建和管理用于外部API调用的密钥"
        backAction={{ onClick: () => navigate(-1) }}
        actions={
          <>
            <ResponsiveActionButton
              variant="outline"
              icon={<BookOpen className="h-4 w-4 md:mr-2" />}
              label="API文档"
              mobileTitle="API文档"
              onClick={() => navigate('/api-documentation')}
            />
            <Button
              variant="outline"
              size="icon"
              onClick={loadData}
              disabled={isLoading}
              title="刷新"
              className="h-9 w-9"
            >
              <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
            </Button>
            <ResponsiveActionButton
              icon={<Plus className="h-4 w-4 md:mr-2" />}
              label="创建密钥"
              mobileTitle="创建API密钥"
              onClick={() => setIsCreateDialogOpen(true)}
            />
          </>
        }
      />

      <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-xs sm:text-sm mb-6">
        <h3 className="font-semibold text-blue-900 mb-2">使用说明</h3>
        <ul className="text-blue-800 space-y-1 list-disc list-inside">
          <li>创建后密钥始终可见（可点击隐藏）</li>
          <li className="hidden sm:list-item">使用"复制"按钮快速复制密钥</li>
          <li>可限制密钥仅访问指定网站</li>
          <li className="hidden sm:list-item">删除密钥后外部系统将无法访问</li>
        </ul>
      </div>

      {error && <InlineError className="mb-6 p-4">{error}</InlineError>}

      {isLoading && (
        <LoadingState
          card={false}
          message={
            <span className="inline-flex flex-col items-center text-muted-foreground">
              <RefreshCw className="h-8 w-8 animate-spin mb-2" />
              加载中...
            </span>
          }
        />
      )}

      {!isLoading && (
        <ApiKeyList
          apiKeys={apiKeys}
          websites={websites}
          onDelete={handleDeleteClick}
        />
      )}

      <CreateApiKeyDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
        onSuccess={handleCreateSuccess}
      />

      <DeleteApiKeyDialog
        open={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
        apiKey={selectedApiKey}
        onSuccess={handleDeleteSuccess}
      />
    </PageShell>
  )
}
