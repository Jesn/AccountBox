import { useState, useEffect, useCallback, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { useVault } from '@/hooks/useVault'
import { websiteService } from '@/services/websiteService'
import { vaultService } from '@/services/vaultService'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
import { ChangeMasterPasswordDialog } from '@/components/vault/ChangeMasterPasswordDialog'
import { CreateWebsiteDialog } from '@/components/websites/CreateWebsiteDialog'
import { EditWebsiteDialog } from '@/components/websites/EditWebsiteDialog'
import { DeleteWebsiteDialog } from '@/components/websites/DeleteWebsiteDialog'
import { WebsiteList } from '@/components/websites/WebsiteList'
import { Lock, Plus, Trash2, Search, X } from 'lucide-react'
import Pagination from '@/components/common/Pagination'
import type { WebsiteResponse } from '@/services/websiteService'

/**
 * 网站管理页面
 */
export function WebsitesPage() {
  const { lock } = useVault()
  const navigate = useNavigate()
  const [showChangePasswordDialog, setShowChangePasswordDialog] =
    useState(false)
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

  const filteredWebsites = useMemo(() => {
    if (!searchQuery.trim()) return websites
    const query = searchQuery.toLowerCase()
    return websites.filter(
      (website) =>
        website.displayName?.toLowerCase().includes(query) ||
        website.domain.toLowerCase().includes(query)
    )
  }, [websites, searchQuery])

  useEffect(() => {
    loadWebsites()
  }, [currentPage, loadWebsites])

  const handleLock = async () => {
    try {
      await vaultService.lock()
      lock()
      navigate('/unlock')
    } catch (err) {
      console.error('Lock error:', err)
    }
  }

  const handleChangePasswordSuccess = () => {
    // 修改密码成功后，会话被清除，需要重新解锁
    lock()
    navigate('/unlock')
  }

  const handleCreateWebsiteSuccess = () => {
    // 创建成功后重新加载列表
    loadWebsites()
  }

  const handleEditWebsite = (website: WebsiteResponse) => {
    setSelectedWebsite(website)
    setShowEditWebsiteDialog(true)
  }

  const handleEditWebsiteSuccess = () => {
    // 编辑成功后重新加载列表
    loadWebsites()
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
    loadWebsites()
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-7xl">
        <div className="mb-8 flex items-center justify-between">
          <h1 className="text-3xl font-bold">网站管理</h1>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => navigate('/recycle-bin')}>
              <Trash2 className="mr-2 h-4 w-4" />
              回收站
            </Button>
            <Button
              variant="outline"
              onClick={() => setShowChangePasswordDialog(true)}
            >
              修改主密码
            </Button>
            <Button variant="outline" onClick={handleLock}>
              <Lock className="mr-2 h-4 w-4" />
              锁定
            </Button>
            <Button onClick={() => setShowCreateWebsiteDialog(true)}>
              <Plus className="mr-2 h-4 w-4" />
              添加网站
            </Button>
          </div>
        </div>

        {/* 搜索框 */}
        <div className="mb-4 relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
          <Input
            type="text"
            placeholder="搜索网站（域名或显示名称）"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-10 pr-10"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        {/* 搜索无结果提示 */}
        {!isLoading &&
          searchQuery &&
          filteredWebsites.length === 0 &&
          websites.length > 0 && (
            <Card>
              <CardContent className="py-12 text-center">
                <p className="text-gray-600 mb-4">
                  未找到匹配 "{searchQuery}" 的网站
                </p>
                <Button variant="outline" onClick={() => setSearchQuery('')}>
                  清空搜索
                </Button>
              </CardContent>
            </Card>
          )}

        {/* 网站列表和分页 */}
        {(!searchQuery || filteredWebsites.length > 0) && (
          <>
            <WebsiteList
              websites={filteredWebsites}
              isLoading={isLoading}
              onViewAccounts={handleViewAccounts}
              onEdit={handleEditWebsite}
              onDelete={handleDeleteWebsite}
              onCreateNew={() => setShowCreateWebsiteDialog(true)}
            />

            {!isLoading && filteredWebsites.length > 0 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
              />
            )}
          </>
        )}
      </div>

      <ChangeMasterPasswordDialog
        open={showChangePasswordDialog}
        onOpenChange={setShowChangePasswordDialog}
        onSuccess={handleChangePasswordSuccess}
      />

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
