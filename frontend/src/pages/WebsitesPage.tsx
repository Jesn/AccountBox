import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useVault } from '@/hooks/useVault'
import { websiteService } from '@/services/websiteService'
import { vaultService } from '@/services/vaultService'
import { Button } from '@/components/ui/button'
import { ChangeMasterPasswordDialog } from '@/components/vault/ChangeMasterPasswordDialog'
import { CreateWebsiteDialog } from '@/components/websites/CreateWebsiteDialog'
import { EditWebsiteDialog } from '@/components/websites/EditWebsiteDialog'
import { WebsiteList } from '@/components/websites/WebsiteList'
import { Lock, Plus } from 'lucide-react'
import Pagination from '@/components/common/Pagination'
import type { WebsiteResponse } from '@/services/websiteService'

/**
 * 网站管理页面
 */
export function WebsitesPage() {
  const { lock } = useVault()
  const navigate = useNavigate()
  const [showChangePasswordDialog, setShowChangePasswordDialog] = useState(false)
  const [showCreateWebsiteDialog, setShowCreateWebsiteDialog] = useState(false)
  const [showEditWebsiteDialog, setShowEditWebsiteDialog] = useState(false)
  const [selectedWebsite, setSelectedWebsite] = useState<WebsiteResponse | null>(null)
  const [websites, setWebsites] = useState<WebsiteResponse[]>([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const pageSize = 10

  useEffect(() => {
    loadWebsites()
  }, [currentPage])

  const loadWebsites = async () => {
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
  }

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
    // TODO: 实现删除网站功能
    console.log('删除网站:', website)
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8 flex items-center justify-between">
          <h1 className="text-3xl font-bold">网站管理</h1>
          <div className="flex gap-2">
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

        <WebsiteList
          websites={websites}
          isLoading={isLoading}
          onViewAccounts={handleViewAccounts}
          onEdit={handleEditWebsite}
          onDelete={handleDeleteWebsite}
          onCreateNew={() => setShowCreateWebsiteDialog(true)}
        />

        {!isLoading && websites.length > 0 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
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
    </div>
  )
}
