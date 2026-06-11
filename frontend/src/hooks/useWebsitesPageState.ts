import { useCallback, useEffect, useState } from 'react'
import { websiteService } from '@/services/websiteService'
import { searchService } from '@/services/searchService'
import { useDebounce } from '@/hooks/useDebounce'
import { APP_CONFIG } from '@/lib/constants'
import type { WebsiteResponse } from '@/types'

export function useWebsitesPageState() {
  const [showCreateWebsiteDialog, setShowCreateWebsiteDialog] = useState(false)
  const [showEditWebsiteDialog, setShowEditWebsiteDialog] = useState(false)
  const [showDeleteWebsiteDialog, setShowDeleteWebsiteDialog] = useState(false)
  const [selectedWebsite, setSelectedWebsite] =
    useState<WebsiteResponse | null>(null)
  const [websites, setWebsites] = useState<WebsiteResponse[]>([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [searchQuery, setSearchQuery] = useState('')
  const [isSearching, setIsSearching] = useState(false)
  const debouncedSearchQuery = useDebounce(
    searchQuery,
    APP_CONFIG.SEARCH_DEBOUNCE_MS
  )
  const pageSize = APP_CONFIG.DEFAULT_PAGE_SIZE

  const loadWebsites = useCallback(async () => {
    setIsLoading(true)
    try {
      const response = await websiteService.getAll(currentPage, pageSize)
      if (response.success && response.data) {
        setWebsites(response.data.items)
        setTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('加载网站列表失败:', error)
    } finally {
      setIsLoading(false)
    }
  }, [currentPage, pageSize])

  const searchWebsites = useCallback(
    async (query: string) => {
      const trimmedQuery = query.trim()

      if (!trimmedQuery) {
        setIsSearching(false)
        return
      }

      setIsLoading(true)
      setIsSearching(true)
      try {
        const response = await searchService.search(trimmedQuery, 1, pageSize)
        if (response.success && response.data) {
          const websiteMap = new Map<number, WebsiteResponse>()
          response.data.items.forEach((item) => {
            if (!websiteMap.has(item.websiteId)) {
              websiteMap.set(item.websiteId, {
                id: item.websiteId,
                domain: item.websiteDomain,
                displayName: item.websiteDisplayName || item.websiteDomain,
                activeAccountCount: 0,
                disabledAccountCount: 0,
                deletedAccountCount: 0,
                createdAt: item.createdAt,
                updatedAt: item.updatedAt,
              })
            }
          })
          setWebsites(Array.from(websiteMap.values()))
          setTotalPages(1)
        }
      } catch (error) {
        console.error('搜索失败:', error)
      } finally {
        setIsLoading(false)
      }
    },
    [pageSize]
  )

  useEffect(() => {
    if (!isSearching) {
      loadWebsites()
    }
  }, [loadWebsites, isSearching])

  useEffect(() => {
    searchWebsites(debouncedSearchQuery)
  }, [debouncedSearchQuery, searchWebsites])

  const openCreateDialog = () => {
    setShowCreateWebsiteDialog(true)
  }

  const openEditDialog = (website: WebsiteResponse) => {
    setSelectedWebsite(website)
    setShowEditWebsiteDialog(true)
  }

  const openDeleteDialog = (website: WebsiteResponse) => {
    setSelectedWebsite(website)
    setShowDeleteWebsiteDialog(true)
  }

  const handleCreateSuccess = () => {
    setSearchQuery('')
    setIsSearching(false)
    loadWebsites()
  }

  const reloadAfterMutation = () => {
    if (searchQuery) {
      searchWebsites(searchQuery)
      return
    }

    loadWebsites()
  }

  const clearSearch = () => {
    setSearchQuery('')
    setIsSearching(false)
    setCurrentPage(1)
  }

  return {
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
  }
}