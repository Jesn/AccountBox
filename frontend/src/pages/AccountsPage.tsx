import { useNavigate, useParams } from 'react-router-dom'
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
import { Plus, Search } from 'lucide-react'
import Pagination from '@/components/common/Pagination'
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { LoadingState } from '@/components/common/LoadingState'
import { ResponsiveActionButton } from '@/components/common/ResponsiveActionButton'
import { useAccountsPageState } from '@/hooks/useAccountsPageState'

/**
 * 账号详情页面
 * 显示指定网站的所有账号
 */
export function AccountsPage() {
  const { websiteId } = useParams<{ websiteId: string }>()
  const navigate = useNavigate()
  const {
    website,
    accounts,
    currentPage,
    setCurrentPage,
    totalPages,
    totalCount,
    pageSize,
    pageSizeOptions,
    searchInput,
    statusFilter,
    isInitialLoading,
    isRefreshingAccounts,
    showCreateAccountDialog,
    setShowCreateAccountDialog,
    showEditAccountDialog,
    setShowEditAccountDialog,
    showDeleteAccountDialog,
    setShowDeleteAccountDialog,
    selectedAccount,
    handleSearchChange,
    handleStatusFilterChange,
    handlePageSizeChange,
    openCreateDialog,
    openEditDialog,
    openDeleteDialog,
    reloadAccounts,
    handleDeleteAccountSuccess,
    handleEnableAccount,
    handleDisableAccount,
  } = useAccountsPageState(websiteId)

  return (
    <PageShell maxWidth="max-w-[1400px]">
      <PageHeader
        title={website?.displayName || website?.domain || '账号管理'}
        description={
          website && website.domain && website.displayName
            ? website.domain
            : undefined
        }
        backAction={{ onClick: () => navigate('/websites') }}
        meta={
          <span className="text-sm text-gray-600 flex-shrink-0">
            共 <span className="font-semibold text-gray-900">{totalCount}</span>{' '}
            个账号
          </span>
        }
        actions={
          <ResponsiveActionButton
            icon={<Plus className="h-4 w-4 md:mr-2" />}
            label="添加账号"
            mobileTitle="添加账号"
            onClick={openCreateDialog}
          />
        }
      />

      <div className="mb-6 flex flex-col sm:flex-row gap-2 sm:gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-500" />
          <Input
            type="text"
            placeholder="搜索用户名、标签或备注..."
            value={searchInput}
            onChange={(event) => handleSearchChange(event.target.value)}
            className="pl-10"
          />
        </div>
        <Select value={statusFilter} onValueChange={handleStatusFilterChange}>
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

      {isInitialLoading ? (
        <LoadingState />
      ) : (
        <div className="relative">
          <div
            className={isRefreshingAccounts ? 'pointer-events-none' : undefined}
            aria-busy={isRefreshingAccounts}
          >
            <div className="hidden md:block">
              <AccountList
                accounts={accounts}
                onEdit={openEditDialog}
                onDelete={openDeleteDialog}
                onEnable={handleEnableAccount}
                onDisable={handleDisableAccount}
              />
            </div>

            <div className="block md:hidden">
              <AccountCardView
                accounts={accounts}
                onEdit={openEditDialog}
                onDelete={openDeleteDialog}
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
                  pageSizeOptions={pageSizeOptions}
                  onPageSizeChange={handlePageSizeChange}
                  onPageChange={setCurrentPage}
                  disabled={isRefreshingAccounts}
                />
              </div>
            )}
          </div>
        </div>
      )}

      {websiteId && (
        <>
          <CreateAccountDialog
            open={showCreateAccountDialog}
            onOpenChange={setShowCreateAccountDialog}
            onSuccess={reloadAccounts}
            websiteId={Number(websiteId)}
          />

          <EditAccountDialog
            open={showEditAccountDialog}
            onOpenChange={setShowEditAccountDialog}
            onSuccess={reloadAccounts}
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
    </PageShell>
  )
}