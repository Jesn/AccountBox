import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { accountService } from '@/services/accountService'
import { websiteService } from '@/services/websiteService'
import type { AccountResponse } from '@/services/accountService'
import type { WebsiteResponse } from '@/services/websiteService'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { AccountList } from '@/components/accounts/AccountList'
import { CreateAccountDialog } from '@/components/accounts/CreateAccountDialog'
import { EditAccountDialog } from '@/components/accounts/EditAccountDialog'
import { DeleteAccountDialog } from '@/components/accounts/DeleteAccountDialog'
import { ArrowLeft, Plus, Search } from 'lucide-react'
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
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'Active' | 'Disabled'>('all')
  const [showCreateAccountDialog, setShowCreateAccountDialog] = useState(false)
  const [showEditAccountDialog, setShowEditAccountDialog] = useState(false)
  const [showDeleteAccountDialog, setShowDeleteAccountDialog] = useState(false)
  const [selectedAccount, setSelectedAccount] =
    useState<AccountResponse | null>(null)
  const pageSize = 15

  // 只在websiteId变化时加载网站信息
  useEffect(() => {
    if (!websiteId) return

    let cancelled = false

    const fetchWebsite = async () => {
      try {
        const response = await websiteService.getById(parseInt(websiteId))
        if (!cancelled && response.success && response.data) {
          setWebsite(response.data)
        }
      } catch (error) {
        if (!cancelled) {
          console.error('加载网站信息失败:', error)
        }
      }
    }

    fetchWebsite()

    return () => {
      cancelled = true
    }
  }, [websiteId])

  // 加载账号列表
  const loadAccounts = async () => {
    if (!websiteId) return

    setIsLoading(true)
    try {
      const response = await accountService.getAll(
        currentPage,
        pageSize,
        parseInt(websiteId),
        searchTerm,
        statusFilter !== 'all' ? statusFilter : undefined
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

  // 在页面、搜索、状态筛选变化时加载账号列表
  useEffect(() => {
    loadAccounts()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [websiteId, currentPage, searchTerm, statusFilter])

  const handleSearchChange = (value: string) => {
    setSearchTerm(value)
    setCurrentPage(1) // 搜索时重置到第一页
  }

  const handleStatusFilterChange = (value: string) => {
    setStatusFilter(value as 'all' | 'Active' | 'Disabled')
    setCurrentPage(1) // 状态筛选时重置到第一页
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
    setSelectedAccount(account)
    setShowDeleteAccountDialog(true)
  }

  const handleDeleteAccountSuccess = () => {
    // 删除成功后重新加载列表
    loadAccounts()
  }

  const handleEnableAccount = async (account: AccountResponse) => {
    try {
      const response = await accountService.enable(account.id)
      if (response.success) {
        loadAccounts()
      }
    } catch (error) {
      console.error('启用账号失败:', error)
    }
  }

  const handleDisableAccount = async (account: AccountResponse) => {
    try {
      const response = await accountService.disable(account.id)
      if (response.success) {
        loadAccounts()
      }
    } catch (error) {
      console.error('禁用账号失败:', error)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-[1400px]">
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate('/websites')}
            className="mb-4"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            返回网站列表
          </Button>

          <div className="flex items-center justify-between mb-4">
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

          {/* 搜索和筛选区域 */}
          <div className="flex gap-4 mb-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-500" />
              <Input
                type="text"
                placeholder="搜索用户名、标签或备注..."
                value={searchTerm}
                onChange={(e) => handleSearchChange(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              value={statusFilter}
              onValueChange={handleStatusFilterChange}
            >
              <SelectTrigger className="w-[160px]">
                <SelectValue placeholder="筛选状态" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">全部状态</SelectItem>
                <SelectItem value="Active">活跃</SelectItem>
                <SelectItem value="Disabled">已禁用</SelectItem>
              </SelectContent>
            </Select>
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
              onEnable={handleEnableAccount}
              onDisable={handleDisableAccount}
            />

            {!isLoading && accounts.length > 0 && (
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

          <DeleteAccountDialog
            open={showDeleteAccountDialog}
            onOpenChange={setShowDeleteAccountDialog}
            onSuccess={handleDeleteAccountSuccess}
            account={selectedAccount}
          />
        </>
      )}
    </div>
  )
}
