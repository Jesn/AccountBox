import { useCallback, useEffect, useState } from 'react'
import { recycleBinService } from '@/services/recycleBinService'
import { websiteService } from '@/services/websiteService'
import { APP_CONFIG } from '@/lib/constants'
import type { DeletedAccountResponse, WebsiteOptionResponse } from '@/types'

const ALL_WEBSITES = 'all'

export function useRecycleBinPageState() {
  const [showEmptyDialog, setShowEmptyDialog] = useState(false)
  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [showRestoreDialog, setShowRestoreDialog] = useState(false)
  const [selectedAccount, setSelectedAccount] =
    useState<DeletedAccountResponse | null>(null)
  const [deletedAccounts, setDeletedAccounts] = useState<
    DeletedAccountResponse[]
  >([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [websites, setWebsites] = useState<WebsiteOptionResponse[]>([])
  const [selectedWebsiteId, setSelectedWebsiteIdState] =
    useState<string>(ALL_WEBSITES)
  const [searchText, setSearchTextState] = useState('')
  const pageSize = APP_CONFIG.RECYCLE_BIN_PAGE_SIZE

  const loadWebsites = useCallback(async () => {
    try {
      const response = await websiteService.getOptions()
      if (response.success && response.data) {
        setWebsites(response.data)
      }
    } catch (error) {
      console.error('加载网站列表失败:', error)
    }
  }, [])

  const loadDeletedAccounts = useCallback(async () => {
    setIsLoading(true)
    try {
      const websiteId =
        selectedWebsiteId === ALL_WEBSITES ? undefined : Number(selectedWebsiteId)
      const search = searchText.trim() || undefined

      const response = await recycleBinService.getDeletedAccounts(
        currentPage,
        pageSize,
        websiteId,
        search
      )

      if (response.success && response.data) {
        setDeletedAccounts(response.data.items)
        setTotalPages(response.data.totalPages)
        setTotalCount(response.data.totalCount)
      }
    } catch (error) {
      console.error('加载回收站失败:', error)
    } finally {
      setIsLoading(false)
    }
  }, [currentPage, pageSize, searchText, selectedWebsiteId])

  useEffect(() => {
    loadWebsites()
  }, [loadWebsites])

  useEffect(() => {
    loadDeletedAccounts()
  }, [loadDeletedAccounts])

  const setSelectedWebsiteId = (value: string) => {
    setCurrentPage(1)
    setSelectedWebsiteIdState(value)
  }

  const setSearchText = (value: string) => {
    setCurrentPage(1)
    setSearchTextState(value)
  }

  const clearSearch = () => {
    setSearchText('')
  }

  const openRestoreDialog = (account: DeletedAccountResponse) => {
    setSelectedAccount(account)
    setShowRestoreDialog(true)
  }

  const openDeleteDialog = (account: DeletedAccountResponse) => {
    setSelectedAccount(account)
    setShowDeleteDialog(true)
  }

  const confirmRestore = async (account: DeletedAccountResponse) => {
    try {
      const response = await recycleBinService.restoreAccount(account.id)
      if (response.success) {
        await loadDeletedAccounts()
        return
      }

      console.error('恢复账号失败:', response.error?.message)
      alert(`恢复账号失败: ${response.error?.message}`)
    } catch (error) {
      console.error('恢复账号失败:', error)
      alert('恢复账号时发生错误,请重试')
    }
  }

  const confirmDelete = async (account: DeletedAccountResponse) => {
    try {
      const response = await recycleBinService.permanentlyDeleteAccount(
        account.id
      )
      if (response.success) {
        await loadDeletedAccounts()
        return
      }

      console.error('永久删除账号失败:', response.error?.message)
      alert(`永久删除账号失败: ${response.error?.message}`)
    } catch (error) {
      console.error('永久删除账号失败:', error)
      alert('永久删除账号时发生错误,请重试')
    }
  }

  return {
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
    reloadDeletedAccounts: loadDeletedAccounts,
  }
}