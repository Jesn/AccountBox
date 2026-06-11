import { useNavigate } from 'react-router-dom'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { RecycleBinList } from '@/components/recycle-bin/RecycleBinList'
import { EmptyRecycleBinDialog } from '@/components/recycle-bin/EmptyRecycleBinDialog'
import { PermanentDeleteDialog } from '@/components/recycle-bin/PermanentDeleteDialog'
import { RestoreAccountDialog } from '@/components/recycle-bin/RestoreAccountDialog'
import Pagination from '@/components/common/Pagination'
import { Trash2, Search, X } from 'lucide-react'
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { ResponsiveActionButton } from '@/components/common/ResponsiveActionButton'
import { useRecycleBinPageState } from '@/hooks/useRecycleBinPageState'

/**
 * 回收站页面
 * 显示已删除账号列表,支持恢复、永久删除和清空回收站
 */
export function RecycleBinPage() {
  const navigate = useNavigate()
  const {
    showEmptyDialog,
    setShowEmptyDialog,
    showDeleteDialog,
    setShowDeleteDialog,
    showRestoreDialog,
    setShowRestoreDialog,
    selectedAccount,
    deletedAccounts,
    currentPage,
    setCurrentPage,
    totalPages,
    totalCount,
    isLoading,
    websites,
    selectedWebsiteId,
    setSelectedWebsiteId,
    searchText,
    setSearchText,
    clearSearch,
    pageSize,
    openRestoreDialog,
    openDeleteDialog,
    confirmRestore,
    confirmDelete,
    reloadDeletedAccounts,
  } = useRecycleBinPageState()

  return (
    <PageShell maxWidth="max-w-6xl">
      <PageHeader
        title="回收站"
        backAction={{ onClick: () => navigate('/websites'), variant: 'outline' }}
        meta={
          totalCount > 0 ? (
            <span className="text-xs sm:text-sm text-gray-600 flex-shrink-0">
              ({totalCount})
            </span>
          ) : undefined
        }
        actions={
          totalCount > 0 ? (
            <ResponsiveActionButton
              variant="destructive"
              icon={<Trash2 className="h-4 w-4 md:mr-2" />}
              label="清空回收站"
              mobileTitle="清空回收站"
              onClick={() => setShowEmptyDialog(true)}
            />
          ) : undefined
        }
      />

      <div className="mb-6 flex flex-col sm:flex-row gap-2">
        <div className="w-full sm:w-48">
          <Select value={selectedWebsiteId} onValueChange={setSelectedWebsiteId}>
            <SelectTrigger>
              <SelectValue placeholder="选择网站" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">所有网站</SelectItem>
              {websites.map((website) => (
                <SelectItem key={website.id} value={website.id.toString()}>
                  {website.displayName || website.domain}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <Input
            type="text"
            placeholder="搜索账号..."
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            className="pl-10 pr-10"
          />
          {searchText && (
            <button
              type="button"
              onClick={clearSearch}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
      </div>

      <RecycleBinList
        deletedAccounts={deletedAccounts}
        isLoading={isLoading}
        onRestore={openRestoreDialog}
        onPermanentlyDelete={openDeleteDialog}
      />

      {!isLoading && deletedAccounts.length > 0 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={setCurrentPage}
        />
      )}

      <EmptyRecycleBinDialog
        open={showEmptyDialog}
        onOpenChange={setShowEmptyDialog}
        onSuccess={reloadDeletedAccounts}
        totalCount={totalCount}
      />

      <RestoreAccountDialog
        open={showRestoreDialog}
        onOpenChange={setShowRestoreDialog}
        account={selectedAccount}
        onConfirm={confirmRestore}
      />

      <PermanentDeleteDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        account={selectedAccount}
        onConfirm={confirmDelete}
      />
    </PageShell>
  )
}