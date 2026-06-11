import { useState, useEffect, useRef } from 'react'
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
import { AccountCardView } from '@/components/accounts/AccountCardView'
import { CreateAccountDialog } from '@/components/accounts/CreateAccountDialog'
import { EditAccountDialog } from '@/components/accounts/EditAccountDialog'
import { DeleteAccountDialog } from '@/components/accounts/DeleteAccountDialog'
import { ArrowLeft, Plus, Search } from 'lucide-react'
import Pagination from '@/components/common/Pagination'

const PAGE_SIZE_OPTIONS = [15, 30, 50, 100]
const DEFAULT_PAGE_NUMBER = 1
const DEFAULT_PAGE_SIZE = 15
const SEARCH_DEBOUNCE_MS = 400

type AccountStatusFilter = 'all' | 'Active' | 'Disabled'

const parsePageNumber = (value: string | null) => {
  const pageNumber = Number(value)
  return Number.isInteger(pageNumber) && pageNumber > 0
    ? pageNumber
    : DEFAULT_PAGE_NUMBER
}

const parsePageSize = (value: string | null) => {
  const pageSize = Number(value)
  return PAGE_SIZE_OPTIONS.includes(pageSize) ? pageSize : DEFAULT_PAGE_SIZE
}

const parseStatusFilter = (value: string | null): AccountStatusFilter => {
  return value === 'Active' || value === 'Disabled' ? value : 'all'
}

const parseSearchTerm = (value: string | null) => value?.trim() ?? ''

/**
 * 账号详情页面
 * 显示指定网站的所有账号
 */
export function AccountsPage() {
  const { websiteId } = useParams<{ websiteId: string }>()
  const navigate = useNavigate()
  const initialSearchParams = useRef(new URLSearchParams(window.location.search))
  const accountRequestId = useRef(0)
  const [website, setWebsite] = useState<WebsiteResponse | null>(null)
  const [accounts, setAccounts] = useState<AccountResponse[]>([])
  const [currentPage, setCurrentPage] = useState(() =>
    parsePageNumber(initialSearchParams.current.get('pageNumber'))
  )
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loadedAccountsWebsiteId, setLoadedAccountsWebsiteId] = useState<string | null>(null)
  const [pageSize, setPageSize] = useState(() =>
    parsePageSize(initialSearchParams.current.get('pageSize'))
  )
  const [isLoading, setIsLoading] = useState(true)
  const [searchInput, setSearchInput] = useState(() =>
    parseSearchTerm(initialSearchParams.current.get('searchTerm'))
  )
  const [searchTerm, setSearchTerm] = useState(() =>
    parseSearchTerm(initialSearchParams.current.get('searchTerm'))
  )
  const [statusFilter, setStatusFilter] = useState<AccountStatusFilter>(() =>
    parseStatusFilter(initialSearchParams.current.get('status'))
  )
  const [showCreateAccountDialog, setShowCreateAccountDialog] = useState(false)
  const [showEditAccountDialog, setShowEditAccountDialog] = useState(false)
  const [showDeleteAccountDialog, setShowDeleteAccountDialog] = useState(false)
  const [selectedAccount, setSelectedAccount] =
    useState<AccountResponse | null>(null)
  const hasLoadedCurrentWebsiteAccounts = loadedAccountsWebsiteId === websiteId
  const isInitialLoading = isLoading && !hasLoadedCurrentWebsiteAccounts
  const isRefreshingAccounts = isLoading && hasLoadedCurrentWebsiteAccounts
  const listMinHeight = pageSize >= 50 ? '960px' : pageSize >= 30 ? '760px' : '620px'

  useEffect(() => {
    const nextSearchParams = new URLSearchParams()

    if (currentPage !== DEFAULT_PAGE_NUMBER) {
      nextSearchParams.set('pageNumber', currentPage.toString())
    }

    if (pageSize !== DEFAULT_PAGE_SIZE) {
      nextSearchParams.set('pageSize', pageSize.toString())
    }

    if (statusFilter !== 'all') {
      nextSearchParams.set('status', statusFilter)
    }

    if (searchTerm) {
      nextSearchParams.set('searchTerm', searchTerm)
    }

    const nextQueryString = nextSearchParams.toString()
    const nextUrl = `${window.location.pathname}${nextQueryString ? `?${nextQueryString}` : ''}${window.location.hash}`
    const currentUrl = `${window.location.pathname}${window.location.search}${window.location.hash}`

    if (nextUrl !== currentUrl) {
      window.history.replaceState(null, '', nextUrl)
    }
  }, [currentPage, pageSize, searchTerm, statusFilter])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      const nextSearchTerm = searchInput.trim()

      setSearchTerm((currentSearchTerm) => {
        if (currentSearchTerm === nextSearchTerm) {
          return currentSearchTerm
        }

        setCurrentPage(1)
        return nextSearchTerm
      })
    }, SEARCH_DEBOUNCE_MS)

    return () => window.clearTimeout(timeoutId)
  }, [searchInput])

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

    const requestId = ++accountRequestId.current
    setIsLoading(true)

    try {
      const response = await accountService.getAll(
        currentPage,
        pageSize,
        parseInt(websiteId),
        searchTerm,
        statusFilter !== 'all' ? statusFilter : undefined
      )

      if (requestId !== accountRequestId.current) {
        return
      }

      if (response.success && response.data) {
        setAccounts(response.data.items as AccountResponse[])
        setTotalPages(response.data.totalPages)
        setTotalCount(response.data.totalCount)
        setLoadedAccountsWebsiteId(websiteId)
      }
    } catch (error) {
      if (requestId === accountRequestId.current) {
        console.error('加载账号列表失败:', error)
      }
    } finally {
      if (requestId === accountRequestId.current) {
        setIsLoading(false)
      }
    }
  }

  // 在页面、搜索、状态筛选变化时加载账号列表
  useEffect(() => {
    loadAccounts()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [websiteId, currentPage, pageSize, searchTerm, statusFilter])

  const handleSearchChange = (value: string) => {
    setSearchInput(value)
  }

  const handleStatusFilterChange = (value: string) => {
    setStatusFilter(value as AccountStatusFilter)
    setCurrentPage(1) // 状态筛选时重置到第一页
  }

  const handlePageSizeChange = (nextPageSize: number) => {
    setPageSize(nextPageSize)
    setCurrentPage(1)
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
    const shouldMoveToPreviousPage = accounts.length === 1 && currentPage > 1

    if (shouldMoveToPreviousPage) {
      setCurrentPage((page) => Math.max(1, page - 1))
      return
    }

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
    <div className="min-h-screen bg-gray-50 p-4 md:p-6 lg:p-8">
      <div className="mx-auto max-w-[1400px]">
        {/* 头部区域 */}
        <div className="mb-6">
          {/* 返回按钮 */}
          <Button
            variant="ghost"
            onClick={() => navigate('/websites')}
            className="mb-3 -ml-2"
            size="sm"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            返回
          </Button>

          {/* 标题和添加按钮 */}
          <div className="flex items-start justify-between gap-3 mb-4">
            <div className="flex-1 min-w-0">
              <h1 className="text-xl sm:text-2xl md:text-3xl font-bold truncate">
                {website?.displayName || website?.domain || '账号管理'}
              </h1>
              {website && website.domain && website.displayName && (
                <p className="text-sm text-gray-600 mt-1 truncate">{website.domain}</p>
              )}
              <p className="text-sm text-gray-600 mt-1">
                共 <span className="font-semibold text-gray-900">{totalCount}</span> 个账号
              </p>
            </div>
            {/* 移动端：图标按钮 */}
            <Button
              onClick={() => setShowCreateAccountDialog(true)}
              size="icon"
              className="md:hidden flex-shrink-0"
              title="添加账号"
            >
              <Plus className="h-4 w-4" />
            </Button>
            {/* 桌面端：完整按钮 */}
            <Button
              onClick={() => setShowCreateAccountDialog(true)}
              className="hidden md:inline-flex"
            >
              <Plus className="mr-2 h-4 w-4" />
              添加账号
            </Button>
          </div>

          {/* 搜索和筛选区域 */}
          <div className="flex flex-col sm:flex-row gap-2 sm:gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-500" />
              <Input
                type="text"
                placeholder="搜索用户名、标签或备注..."
                value={searchInput}
                onChange={(e) => handleSearchChange(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              value={statusFilter}
              onValueChange={handleStatusFilterChange}
            >
              <SelectTrigger className="w-full sm:w-[160px]">
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

        {isInitialLoading ? (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">加载中...</p>
            </CardContent>
          </Card>
        ) : (
          <div className="relative" style={{ minHeight: listMinHeight }}>
            <div
              className={isRefreshingAccounts ? 'pointer-events-none' : undefined}
              aria-busy={isRefreshingAccounts}
            >
              {/* 桌面端：表格视图 */}
              <div className="hidden md:block">
                <AccountList
                  accounts={accounts}
                  onEdit={handleEditAccount}
                  onDelete={handleDeleteAccount}
                  onEnable={handleEnableAccount}
                  onDisable={handleDisableAccount}
                />
              </div>

              {/* 移动端：卡片视图 */}
              <div className="block md:hidden">
                <AccountCardView
                  accounts={accounts}
                  onEdit={handleEditAccount}
                  onDelete={handleDeleteAccount}
                  onEnable={handleEnableAccount}
                  onDisable={handleDisableAccount}
                />
              </div>

              {(accounts.length > 0 || totalCount > 0) && (
                <div className="relative">
                  {isRefreshingAccounts && (
                    <div className="absolute inset-x-0 -top-3 flex justify-center">
                      <div className="rounded-full bg-white/95 px-3 py-1 text-xs text-gray-500 shadow-sm ring-1 ring-gray-200">
                        正在切换页面...
                      </div>
                    </div>
                  )}
                  <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalCount={totalCount}
                    pageSize={pageSize}
                    pageSizeOptions={PAGE_SIZE_OPTIONS}
                    onPageSizeChange={handlePageSizeChange}
                    onPageChange={setCurrentPage}
                    disabled={isRefreshingAccounts}
                  />
                </div>
              )}
            </div>
          </div>
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
