import { useCallback, useEffect, useRef, useState } from 'react'
import { accountService } from '@/services/accountService'
import { websiteService } from '@/services/websiteService'
import { useDebounce } from '@/hooks/useDebounce'
import { APP_CONFIG } from '@/lib/constants'
import type { AccountResponse, WebsiteResponse } from '@/types'

export type AccountStatusFilter = 'all' | 'Active' | 'Disabled'

const PAGE_SIZE_OPTIONS: readonly number[] = APP_CONFIG.PAGE_SIZE_OPTIONS
const DEFAULT_PAGE_NUMBER = APP_CONFIG.DEFAULT_PAGE_NUMBER
const DEFAULT_PAGE_SIZE = APP_CONFIG.DEFAULT_PAGE_SIZE
const SEARCH_DEBOUNCE_MS = APP_CONFIG.ACCOUNT_SEARCH_DEBOUNCE_MS

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

export function useAccountsPageState(websiteId?: string) {
  const initialSearchParams = useRef(new URLSearchParams(window.location.search))
  const accountRequestId = useRef(0)
  const [website, setWebsite] = useState<WebsiteResponse | null>(null)
  const [accounts, setAccounts] = useState<AccountResponse[]>([])
  const [currentPage, setCurrentPage] = useState(() =>
    parsePageNumber(initialSearchParams.current.get('pageNumber'))
  )
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loadedAccountsWebsiteId, setLoadedAccountsWebsiteId] = useState<
    string | null
  >(null)
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
  const debouncedSearchInput = useDebounce(searchInput, SEARCH_DEBOUNCE_MS)
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
    const nextSearchTerm = debouncedSearchInput.trim()
    if (searchTerm === nextSearchTerm) {
      return
    }

    setCurrentPage(1)
    setSearchTerm(nextSearchTerm)
  }, [debouncedSearchInput, searchTerm])

  useEffect(() => {
    if (!websiteId) return

    let cancelled = false

    const fetchWebsite = async () => {
      try {
        const response = await websiteService.getById(Number(websiteId))
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

  const loadAccounts = useCallback(async () => {
    if (!websiteId) return

    const requestId = ++accountRequestId.current
    setIsLoading(true)

    try {
      const response = await accountService.getAll(
        currentPage,
        pageSize,
        Number(websiteId),
        searchTerm,
        statusFilter !== 'all' ? statusFilter : undefined
      )

      if (requestId !== accountRequestId.current) {
        return
      }

      if (response.success && response.data) {
        setAccounts(response.data.items)
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
  }, [currentPage, pageSize, searchTerm, statusFilter, websiteId])

  useEffect(() => {
    loadAccounts()
  }, [loadAccounts])

  const handleSearchChange = (value: string) => {
    setSearchInput(value)
  }

  const handleStatusFilterChange = (value: string) => {
    setStatusFilter(value as AccountStatusFilter)
    setCurrentPage(1)
  }

  const handlePageSizeChange = (nextPageSize: number) => {
    setPageSize(nextPageSize)
    setCurrentPage(1)
  }

  const openCreateDialog = () => {
    setShowCreateAccountDialog(true)
  }

  const openEditDialog = (account: AccountResponse) => {
    setSelectedAccount(account)
    setShowEditAccountDialog(true)
  }

  const openDeleteDialog = (account: AccountResponse) => {
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

  return {
    website,
    accounts,
    currentPage,
    setCurrentPage,
    totalPages,
    totalCount,
    pageSize,
    pageSizeOptions: PAGE_SIZE_OPTIONS,
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
    reloadAccounts: loadAccounts,
    handleDeleteAccountSuccess,
    handleEnableAccount,
    handleDisableAccount,
  }
}