import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { accountService } from '@/services/accountService'
import { websiteService } from '@/services/websiteService'
import type { AccountResponse } from '@/services/accountService'
import type { WebsiteResponse } from '@/services/websiteService'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { AccountList } from '@/components/accounts/AccountList'
import { CreateAccountDialog } from '@/components/accounts/CreateAccountDialog'
import { EditAccountDialog } from '@/components/accounts/EditAccountDialog'
import { ArrowLeft, Plus } from 'lucide-react'
import Pagination from '@/components/common/Pagination'

/**
 * 账号详情页面
 * 显示指定网站的所有账号
 */
export function AccountsPage() {
  const { websiteId } = useParams<{ websiteId: string }>()
  const navigate = useNavigate()
  const [website, setWebsite] = useState<WebsiteResponse | null>(null)
  const [accounts, setAccounts] = useState<AccountResponse[]>([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [showCreateAccountDialog, setShowCreateAccountDialog] = useState(false)
  const [showEditAccountDialog, setShowEditAccountDialog] = useState(false)
  const [selectedAccount, setSelectedAccount] = useState<AccountResponse | null>(null)
  const pageSize = 10

  useEffect(() => {
    if (websiteId) {
      loadWebsite()
      loadAccounts()
    }
  }, [websiteId, currentPage])

  const loadWebsite = async () => {
    if (!websiteId) return

    try {
      const response = await websiteService.getById(parseInt(websiteId))
      if (response.success && response.data) {
        setWebsite(response.data)
      }
    } catch (error) {
      console.error('加载网站信息失败:', error)
    }
  }

  const loadAccounts = async () => {
    if (!websiteId) return

    setIsLoading(true)
    try {
      const response = await accountService.getAll(
        currentPage,
        pageSize,
        parseInt(websiteId)
      )
      if (response.success && response.data) {
        setAccounts(response.data.items as AccountResponse[])
        setTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('加载账号列表失败:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const handleCreateAccountSuccess = () => {
    // 创建成功后重新加载列表
    loadAccounts()
  }

  const handleEditAccount = (account: AccountResponse) => {
    setSelectedAccount(account)
    setShowEditAccountDialog(true)
  }

  const handleEditAccountSuccess = () => {
    // 编辑成功后重新加载列表
    loadAccounts()
  }

  const handleDeleteAccount = (account: AccountResponse) => {
    // TODO: 实现删除账号功能
    console.log('删除账号:', account)
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate('/websites')}
            className="mb-4"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            返回网站列表
          </Button>

          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold">
                {website?.displayName || website?.domain || '账号管理'}
              </h1>
              {website && website.domain && website.displayName && (
                <p className="text-gray-600 mt-1">{website.domain}</p>
              )}
            </div>
            <Button onClick={() => setShowCreateAccountDialog(true)}>
              <Plus className="mr-2 h-4 w-4" />
              添加账号
            </Button>
          </div>
        </div>

        {isLoading ? (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">加载中...</p>
            </CardContent>
          </Card>
        ) : (
          <>
            <AccountList
              accounts={accounts}
              onEdit={handleEditAccount}
              onDelete={handleDeleteAccount}
            />

            {totalPages > 1 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
              />
            )}
          </>
        )}
      </div>

      {websiteId && (
        <>
          <CreateAccountDialog
            open={showCreateAccountDialog}
            onOpenChange={setShowCreateAccountDialog}
            onSuccess={handleCreateAccountSuccess}
            websiteId={parseInt(websiteId)}
          />

          <EditAccountDialog
            open={showEditAccountDialog}
            onOpenChange={setShowEditAccountDialog}
            onSuccess={handleEditAccountSuccess}
            account={selectedAccount}
          />
        </>
      )}
    </div>
  )
}
