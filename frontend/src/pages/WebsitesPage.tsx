import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { websiteService } from '@/services/websiteService'
import { searchService } from '@/services/searchService'
import { authService } from '@/services/authService'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
import { CreateWebsiteDialog } from '@/components/websites/CreateWebsiteDialog'
import { EditWebsiteDialog } from '@/components/websites/EditWebsiteDialog'
import { DeleteWebsiteDialog } from '@/components/websites/DeleteWebsiteDialog'
import { WebsiteList } from '@/components/websites/WebsiteList'
import { WebsiteCardView } from '@/components/websites/WebsiteCardView'
import { Plus, Trash2, Search as SearchIcon, X, Key, LogOut } from 'lucide-react'
import Pagination from '@/components/common/Pagination'
import type { WebsiteResponse } from '@/services/websiteService'

/**
 * 网站管理页面
 *
 * 注意：系统已从加密存储切换为明文存储（2025-10-17架构变更）
 * - 移除了"锁定"和"修改主密码"功能
 * - 不再需要 Vault Session 管理
 */
export function WebsitesPage() {
  const navigate = useNavigate()
  const [showCreateWebsiteDialog, setShowCreateWebsiteDialog] = useState(false)
  const [showEditWebsiteDialog, setShowEditWebsiteDialog] = useState(false)
  const [showDeleteWebsiteDialog, setShowDeleteWebsiteDialog] = useState(false)
  const [selectedWebsite, setSelectedWebsite] =
    useState<WebsiteResponse | null>(null)
  const [websites, setWebsites] = useState<WebsiteResponse[]>([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [isSearching, setIsSearching] = useState(false)
  const pageSize = 15

  const loadWebsites = useCallback(async () => {
    setIsLoading(true)
    try {
      const response = await websiteService.getAll(currentPage, pageSize)
      if (response.success && response.data) {
        setWebsites(response.data.items as WebsiteResponse[])
        setTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('加载网站列表失败:', error)
    } finally {
      setIsLoading(false)
    }
  }, [currentPage, pageSize])

  // 全局搜索功能
  const handleSearch = useCallback(async () => {
    if (!searchQuery.trim()) {
      // 清空搜索时重置搜索状态，让 useEffect 触发 loadWebsites
      setIsSearching(false)
      return
    }

    setIsLoading(true)
    setIsSearching(true)
    try {
      const response = await searchService.search(searchQuery, 1, 15) // 每页15条
      if (response.success && response.data) {
        // 从搜索结果中提取唯一的网站信息
        const websiteMap = new Map<number, WebsiteResponse>()
        response.data.items.forEach((item) => {
          if (!websiteMap.has(item.websiteId)) {
            websiteMap.set(item.websiteId, {
              id: item.websiteId,
              domain: item.websiteDomain,
              displayName: item.websiteDisplayName || item.websiteDomain,
              activeAccountCount: 0, // 搜索结果不包含账号数量
              disabledAccountCount: 0,
              deletedAccountCount: 0,
              createdAt: item.createdAt,
              updatedAt: item.updatedAt,
            })
          }
        })
        setWebsites(Array.from(websiteMap.values()))
        setTotalPages(1) // 搜索结果不分页
      }
    } catch (error) {
      console.error('搜索失败:', error)
    } finally {
      setIsLoading(false)
    }
  }, [searchQuery])

  useEffect(() => {
    if (!isSearching) {
      loadWebsites()
    }
  }, [currentPage, loadWebsites, isSearching])

  // 处理搜索输入变化
  useEffect(() => {
    const timer = setTimeout(() => {
      handleSearch()
    }, 500) // 500ms 防抖

    return () => clearTimeout(timer)
  }, [searchQuery, handleSearch])

  const handleCreateWebsiteSuccess = () => {
    // 创建成功后重新加载列表
    setSearchQuery('')
    setIsSearching(false)
    loadWebsites()
  }

  const handleEditWebsite = (website: WebsiteResponse) => {
    setSelectedWebsite(website)
    setShowEditWebsiteDialog(true)
  }

  const handleEditWebsiteSuccess = () => {
    // 编辑成功后重新加载列表
    if (searchQuery) {
      handleSearch()
    } else {
      loadWebsites()
    }
  }

  const handleViewAccounts = (websiteId: number) => {
    navigate(`/websites/${websiteId}/accounts`)
  }

  const handleDeleteWebsite = (website: WebsiteResponse) => {
    setSelectedWebsite(website)
    setShowDeleteWebsiteDialog(true)
  }

  const handleDeleteWebsiteSuccess = () => {
    // 删除成功后重新加载列表
    if (searchQuery) {
      handleSearch()
    } else {
      loadWebsites()
    }
  }

  const handleClearSearch = () => {
    setSearchQuery('')
    setIsSearching(false)
    setCurrentPage(1)
  }

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-6 lg:p-8">
      <div className="mx-auto max-w-7xl px-4 md:px-0">
        <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <h1 className="text-2xl sm:text-3xl font-bold">网站管理</h1>
          <div className="flex flex-col sm:flex-row gap-2">
            <Button variant="outline" onClick={() => navigate('/api-keys')} className="w-full sm:w-auto">
              <Key className="mr-2 h-4 w-4" />
              API密钥
            </Button>
            <Button variant="outline" onClick={() => navigate('/recycle-bin')} className="w-full sm:w-auto">
              <Trash2 className="mr-2 h-4 w-4" />
              回收站
            </Button>
            <Button onClick={() => setShowCreateWebsiteDialog(true)} className="w-full sm:w-auto">
              <Plus className="mr-2 h-4 w-4" />
              添加网站
            </Button>
            <Button variant="destructive" onClick={handleLogout} className="w-full sm:w-auto">
              <LogOut className="mr-2 h-4 w-4" />
              登出
            </Button>
          </div>
        </div>

        {/* 全局搜索框 */}
        <div className="mb-4 relative">
          <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
          <Input
            type="text"
            placeholder="全局搜索（搜索所有网站和账号信息）"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10 pr-10"
          />
          {searchQuery && (
            <button
              onClick={handleClearSearch}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        {/* 搜索无结果提示 */}
        {!isLoading && searchQuery && websites.length === 0 && (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600 mb-4">
                未找到匹配 "{searchQuery}" 的网站
              </p>
              <Button variant="outline" onClick={handleClearSearch}>
                清空搜索
              </Button>
            </CardContent>
          </Card>
        )}

        {/* 网站列表和分页 */}
        {(!searchQuery || websites.length > 0) && (
          <>
            {/* 桌面端：表格视图 */}
            <div className="hidden md:block">
              <WebsiteList
                websites={websites}
                isLoading={isLoading}
                onViewAccounts={handleViewAccounts}
                onEdit={handleEditWebsite}
                onDelete={handleDeleteWebsite}
                onCreateNew={() => setShowCreateWebsiteDialog(true)}
              />
            </div>

            {/* 移动端：卡片视图 */}
            <div className="block md:hidden">
              {isLoading ? (
                <Card>
                  <CardContent className="py-12 text-center">
                    <p className="text-gray-600">加载中...</p>
                  </CardContent>
                </Card>
              ) : websites.length === 0 ? (
                <Card>
                  <CardContent className="py-12 text-center">
                    <p className="text-gray-600 mb-4">还没有添加任何网站</p>
                    <Button onClick={() => setShowCreateWebsiteDialog(true)}>
                      <Plus className="mr-2 h-4 w-4" />
                      添加第一个网站
                    </Button>
                  </CardContent>
                </Card>
              ) : (
                <WebsiteCardView
                  websites={websites}
                  onViewAccounts={handleViewAccounts}
                  onEdit={handleEditWebsite}
                  onDelete={handleDeleteWebsite}
                />
              )}
            </div>

            {!isLoading && websites.length > 0 && !isSearching && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
              />
            )}
          </>
        )}
      </div>

      <CreateWebsiteDialog
        open={showCreateWebsiteDialog}
        onOpenChange={setShowCreateWebsiteDialog}
        onSuccess={handleCreateWebsiteSuccess}
      />

      <EditWebsiteDialog
        open={showEditWebsiteDialog}
        onOpenChange={setShowEditWebsiteDialog}
        onSuccess={handleEditWebsiteSuccess}
        website={selectedWebsite}
      />

      <DeleteWebsiteDialog
        open={showDeleteWebsiteDialog}
        onOpenChange={setShowDeleteWebsiteDialog}
        onSuccess={handleDeleteWebsiteSuccess}
        website={selectedWebsite}
      />
    </div>
  )
}
