import { useNavigate } from 'react-router-dom'
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
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { ResponsiveActionButton } from '@/components/common/ResponsiveActionButton'
import { useWebsitesPageState } from '@/hooks/useWebsitesPageState'

/**
 * 网站管理页面
 *
 * 注意：系统已从加密存储切换为明文存储（2025-10-17架构变更）
 * - 移除了"锁定"和"修改主密码"功能
 * - 不再需要 Vault Session 管理
 */
export function WebsitesPage() {
  const navigate = useNavigate()
  const {
    showCreateWebsiteDialog,
    setShowCreateWebsiteDialog,
    showEditWebsiteDialog,
    setShowEditWebsiteDialog,
    showDeleteWebsiteDialog,
    setShowDeleteWebsiteDialog,
    selectedWebsite,
    websites,
    currentPage,
    setCurrentPage,
    totalPages,
    isLoading,
    searchQuery,
    setSearchQuery,
    isSearching,
    openCreateDialog,
    openEditDialog,
    openDeleteDialog,
    handleCreateSuccess,
    reloadAfterMutation,
    clearSearch,
  } = useWebsitesPageState()

  const handleLogout = () => {
    authService.logout()
    navigate('/login')
  }

  return (
    <PageShell>
      <PageHeader
        title="网站管理"
        titleClassName="text-2xl sm:text-3xl"
        className="mb-8"
        actions={
          <>
            <ResponsiveActionButton
              icon={<Plus className="h-4 w-4 md:mr-2" />}
              label="添加网站"
              mobileTitle="添加网站"
              onClick={openCreateDialog}
            />
            <ResponsiveActionButton
              variant="outline"
              icon={<Key className="h-4 w-4 md:mr-2" />}
              label="API密钥"
              mobileTitle="API密钥"
              onClick={() => navigate('/api-keys')}
            />
            <ResponsiveActionButton
              variant="outline"
              icon={<Trash2 className="h-4 w-4 md:mr-2" />}
              label="回收站"
              mobileTitle="回收站"
              onClick={() => navigate('/recycle-bin')}
            />
            <ResponsiveActionButton
              variant="destructive"
              icon={<LogOut className="h-4 w-4 md:mr-2" />}
              label="登出"
              mobileTitle="登出"
              onClick={handleLogout}
            />
          </>
        }
      />

      <div className="mb-4 relative">
        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
        <Input
          type="text"
          placeholder="全局搜索（搜索所有网站和账号信息）"
          value={searchQuery}
          onChange={(event) => setSearchQuery(event.target.value)}
          className="pl-10 pr-10"
        />
        {searchQuery && (
          <button
            type="button"
            onClick={clearSearch}
            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>

      {!isLoading && searchQuery && websites.length === 0 && (
        <Card>
          <CardContent className="py-12 text-center">
            <p className="text-gray-600 mb-4">
              未找到匹配 "{searchQuery}" 的网站
            </p>
            <Button variant="outline" onClick={clearSearch}>
              清空搜索
            </Button>
          </CardContent>
        </Card>
      )}

      {(!searchQuery || websites.length > 0) && (
        <>
          <div className="hidden md:block">
            <WebsiteList
              websites={websites}
              isLoading={isLoading}
              onViewAccounts={(websiteId) =>
                navigate(`/websites/${websiteId}/accounts`)
              }
              onEdit={openEditDialog}
              onDelete={openDeleteDialog}
              onCreateNew={openCreateDialog}
            />
          </div>

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
                  <Button onClick={openCreateDialog}>
                    <Plus className="mr-2 h-4 w-4" />
                    添加第一个网站
                  </Button>
                </CardContent>
              </Card>
            ) : (
              <WebsiteCardView
                websites={websites}
                onViewAccounts={(websiteId) =>
                  navigate(`/websites/${websiteId}/accounts`)
                }
                onEdit={openEditDialog}
                onDelete={openDeleteDialog}
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

      <CreateWebsiteDialog
        open={showCreateWebsiteDialog}
        onOpenChange={setShowCreateWebsiteDialog}
        onSuccess={handleCreateSuccess}
      />

      <EditWebsiteDialog
        open={showEditWebsiteDialog}
        onOpenChange={setShowEditWebsiteDialog}
        onSuccess={reloadAfterMutation}
        website={selectedWebsite}
      />

      <DeleteWebsiteDialog
        open={showDeleteWebsiteDialog}
        onOpenChange={setShowDeleteWebsiteDialog}
        onSuccess={reloadAfterMutation}
        website={selectedWebsite}
      />
    </PageShell>
  )
}