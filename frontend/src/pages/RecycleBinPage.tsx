import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { recycleBinService } from '@/services/recycleBinService'
import { Button } from '@/components/ui/button'
import { RecycleBinList } from '@/components/recycle-bin/RecycleBinList'
import { EmptyRecycleBinDialog } from '@/components/recycle-bin/EmptyRecycleBinDialog'
import { PermanentDeleteDialog } from '@/components/recycle-bin/PermanentDeleteDialog'
import Pagination from '@/components/common/Pagination'
import { ArrowLeft, Trash2 } from 'lucide-react'
import type { DeletedAccountResponse } from '@/services/recycleBinService'

/**
 * 回收站页面
 * 显示已删除账号列表,支持恢复、永久删除和清空回收站
 */
export function RecycleBinPage() {
  const navigate = useNavigate()
  const [showEmptyDialog, setShowEmptyDialog] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [selectedAccount, setSelectedAccount] = useState<DeletedAccountResponse | null>(null)
  const [deletedAccounts, setDeletedAccounts] = useState<
    DeletedAccountResponse[]
  >([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const pageSize = 10

  useEffect(() => {
    loadDeletedAccounts()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentPage])

  const loadDeletedAccounts = async () => {
    setIsLoading(true)
    try {
      const response = await recycleBinService.getDeletedAccounts(
        currentPage,
        pageSize
      )
      if (response.success && response.data) {
        setDeletedAccounts(response.data.items as DeletedAccountResponse[])
        setTotalPages(response.data.totalPages)
        setTotalCount(response.data.totalCount)
      }
    } catch (error) {
      console.error('加载回收站失败:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const handleRestore = async (account: DeletedAccountResponse) => {
    try {
      const response = await recycleBinService.restoreAccount(account.id)
      if (response.success) {
        // 恢复成功后重新加载列表
        await loadDeletedAccounts()
      } else {
        console.error('恢复账号失败:', response.error?.message)
        alert(`恢复账号失败: ${response.error?.message}`)
      }
    } catch (error) {
      console.error('恢复账号失败:', error)
      alert('恢复账号时发生错误,请重试')
    }
  }

  const handlePermanentlyDelete = async (account: DeletedAccountResponse) => {
    // 打开确认对话框
    setSelectedAccount(account)
    setShowDeleteDialog(true)
  }

  const handleConfirmDelete = async (account: DeletedAccountResponse) => {
    try {
      const response = await recycleBinService.permanentlyDeleteAccount(
        account.id
      )
      if (response.success) {
        // 删除成功后重新加载列表
        await loadDeletedAccounts()
      } else {
        console.error('永久删除账号失败:', response.error?.message)
        alert(`永久删除账号失败: ${response.error?.message}`)
      }
    } catch (error) {
      console.error('永久删除账号失败:', error)
      alert('永久删除账号时发生错误,请重试')
    }
  }

  const handleEmptySuccess = () => {
    // 清空成功后重新加载列表
    loadDeletedAccounts()
  }

  const handleBack = () => {
    navigate('/websites')
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Button variant="outline" size="sm" onClick={handleBack}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              返回
            </Button>
            <h1 className="text-3xl font-bold">回收站</h1>
            {totalCount > 0 && (
              <span className="text-sm text-gray-600">
                共 {totalCount} 个已删除账号
              </span>
            )}
          </div>
          {totalCount > 0 && (
            <Button
              variant="destructive"
              onClick={() => setShowEmptyDialog(true)}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              清空回收站
            </Button>
          )}
        </div>

        <RecycleBinList
          deletedAccounts={deletedAccounts}
          isLoading={isLoading}
          onRestore={handleRestore}
          onPermanentlyDelete={handlePermanentlyDelete}
        />

        {!isLoading && deletedAccounts.length > 0 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={setCurrentPage}
          />
        )}
      </div>

      <EmptyRecycleBinDialog
        open={showEmptyDialog}
        onOpenChange={setShowEmptyDialog}
        onSuccess={handleEmptySuccess}
        totalCount={totalCount}
      />

      <PermanentDeleteDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        account={selectedAccount}
        onConfirm={handleConfirmDelete}
      />
    </div>
  )
}
