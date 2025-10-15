import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useVault } from '@/hooks/useVault'
import { websiteService } from '@/services/websiteService'
import { vaultService } from '@/services/vaultService'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { ChangeMasterPasswordDialog } from '@/components/vault/ChangeMasterPasswordDialog'
import { CreateWebsiteDialog } from '@/components/websites/CreateWebsiteDialog'
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

        {isLoading ? (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">加载中...</p>
            </CardContent>
          </Card>
        ) : websites.length === 0 ? (
          <Card>
            <CardHeader>
              <CardTitle>开始使用 AccountBox</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-gray-600 mb-4">还没有添加任何网站</p>
              <Button onClick={() => setShowCreateWebsiteDialog(true)}>
                <Plus className="mr-2 h-4 w-4" />
                添加第一个网站
              </Button>
            </CardContent>
          </Card>
        ) : (
          <>
            <div className="grid gap-4">
              {websites.map((website) => (
                <Card key={website.id}>
                  <CardContent className="p-4">
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <h3 className="text-lg font-semibold">
                          {website.displayName || website.domain}
                        </h3>
                        <p className="text-sm text-gray-600">{website.domain}</p>
                        {website.tags && (
                          <p className="text-sm text-gray-500 mt-1">
                            标签: {website.tags}
                          </p>
                        )}
                        <div className="flex gap-4 mt-2 text-sm text-gray-500">
                          <span>活跃账号: {website.activeAccountCount}</span>
                          {website.deletedAccountCount > 0 && (
                            <span>回收站: {website.deletedAccountCount}</span>
                          )}
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => navigate(`/websites/${website.id}/accounts`)}
                        >
                          查看账号
                        </Button>
                        <Button variant="outline" size="sm">
                          编辑
                        </Button>
                        <Button variant="destructive" size="sm">
                          删除
                        </Button>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>

            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={setCurrentPage}
            />
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
    </div>
  )
}
