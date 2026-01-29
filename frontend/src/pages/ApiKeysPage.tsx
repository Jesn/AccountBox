import { useEffect, useState } from 'react'
import { apiKeyService } from '@/services/apiKeyService'
import { websiteService, type WebsiteResponse } from '@/services/websiteService'
import type { ApiKey } from '@/types/ApiKey'
import { Button } from '@/components/ui/button'
import { CreateApiKeyDialog } from '@/components/api-keys/CreateApiKeyDialog'
import { DeleteApiKeyDialog } from '@/components/api-keys/DeleteApiKeyDialog'
import { ApiKeyList } from '@/components/api-keys/ApiKeyList'
import { Plus, RefreshCw, ArrowLeft, BookOpen } from 'lucide-react'
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
        websiteService.getAll(1, 100), // 获取所有网站
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
    <div className="min-h-screen bg-gray-50 p-4 md:p-6 lg:p-8">
      <div className="mx-auto max-w-4xl">
        {/* 页头 */}
        <div className="mb-6">
          {/* 返回按钮和标题 */}
          <div className="flex items-start justify-between gap-3 mb-4">
            <div className="flex items-center gap-2 flex-1 min-w-0">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate(-1)}
                title="返回"
                className="-ml-2"
              >
                <ArrowLeft className="mr-2 h-4 w-4" />
                返回
              </Button>
              <div className="flex-1 min-w-0">
                <h1 className="text-xl sm:text-2xl md:text-3xl font-bold truncate">API密钥管理</h1>
                <p className="text-xs sm:text-sm text-muted-foreground mt-1 hidden sm:block">
                  创建和管理用于外部API调用的密钥
                </p>
              </div>
            </div>

            {/* 操作按钮 */}
            <div className="flex gap-1.5 flex-shrink-0">
              {/* 移动端：图标按钮 */}
              <Button
                variant="outline"
                size="icon"
                onClick={() => navigate('/api-documentation')}
                title="API文档"
                className="md:hidden h-9 w-9"
              >
                <BookOpen className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="icon"
                onClick={loadData}
                disabled={isLoading}
                title="刷新"
                className="md:hidden h-9 w-9"
              >
                <RefreshCw
                  className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`}
                />
              </Button>
              <Button
                size="icon"
                onClick={() => setIsCreateDialogOpen(true)}
                title="创建API密钥"
                className="md:hidden h-9 w-9"
              >
                <Plus className="h-4 w-4" />
              </Button>

              {/* 桌面端：完整按钮 */}
              <Button
                variant="outline"
                size="sm"
                onClick={() => navigate('/api-documentation')}
                className="hidden md:inline-flex"
              >
                <BookOpen className="h-4 w-4 mr-2" />
                API文档
              </Button>
              <Button
                variant="outline"
                size="icon"
                onClick={loadData}
                disabled={isLoading}
                title="刷新"
                className="hidden md:inline-flex"
              >
                <RefreshCw
                  className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`}
                />
              </Button>
              <Button
                size="sm"
                onClick={() => setIsCreateDialogOpen(true)}
                className="hidden md:inline-flex"
              >
                <Plus className="h-4 w-4 mr-2" />
                创建密钥
              </Button>
            </div>
          </div>

          {/* 说明卡片 - 移动端简化 */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-xs sm:text-sm">
            <h3 className="font-semibold text-blue-900 mb-2">使用说明</h3>
            <ul className="text-blue-800 space-y-1 list-disc list-inside">
              <li>创建后密钥始终可见（可点击隐藏）</li>
              <li className="hidden sm:list-item">使用"复制"按钮快速复制密钥</li>
              <li>可限制密钥仅访问指定网站</li>
              <li className="hidden sm:list-item">删除密钥后外部系统将无法访问</li>
            </ul>
          </div>
        </div>

        {/* 错误提示 */}
        {error && (
          <div className="rounded-md bg-red-50 p-4 mb-6">
            <div className="flex">
              <div className="flex-shrink-0">
                <svg
                  className="h-5 w-5 text-red-400"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clipRule="evenodd"
                  />
                </svg>
              </div>
              <div className="ml-3">
                <p className="text-sm text-red-800">{error}</p>
              </div>
            </div>
          </div>
        )}

        {/* 加载状态 */}
        {isLoading && (
          <div className="text-center py-12">
            <RefreshCw className="h-8 w-8 animate-spin mx-auto text-muted-foreground" />
            <p className="mt-2 text-muted-foreground">加载中...</p>
          </div>
        )}

        {/* API密钥列表 */}
        {!isLoading && (
          <ApiKeyList
            apiKeys={apiKeys}
            websites={websites}
            onDelete={handleDeleteClick}
          />
        )}

        {/* 对话框 */}
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
      </div>
    </div>
  )
}
