import { useEffect, useState } from 'react'
import { searchService } from '@/services/searchService'
import { APP_CONFIG } from '@/lib/constants'
import type { SearchResultItem } from '@/types'

const INITIAL_PAGE = 1
const INITIAL_TOTAL_PAGES = 1

export function useSearchPageState() {
  const [query, setQuery] = useState('')
  const [searchResults, setSearchResults] = useState<SearchResultItem[]>([])
  const [currentPage, setCurrentPage] = useState(INITIAL_PAGE)
  const [totalPages, setTotalPages] = useState(INITIAL_TOTAL_PAGES)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const [hasSearched, setHasSearched] = useState(false)
  const [visiblePasswords, setVisiblePasswords] = useState<Set<number>>(
    new Set()
  )
  const pageSize = APP_CONFIG.SEARCH_PAGE_SIZE

  const resetSearch = () => {
    setHasSearched(false)
    setSearchResults([])
    setTotalPages(INITIAL_TOTAL_PAGES)
    setTotalCount(0)
    setCurrentPage(INITIAL_PAGE)
  }

  useEffect(() => {
    if (query.trim() === '' && hasSearched) {
      resetSearch()
    }
  }, [query, hasSearched])

  const search = async (pageNumber: number = INITIAL_PAGE) => {
    const trimmedQuery = query.trim()

    if (!trimmedQuery) {
      resetSearch()
      return
    }

    setIsLoading(true)
    setHasSearched(true)

    try {
      const response = await searchService.search(
        trimmedQuery,
        pageNumber,
        pageSize
      )

      if (response.success && response.data) {
        setSearchResults(response.data.items)
        setTotalPages(response.data.totalPages)
        setTotalCount(response.data.totalCount)
        setCurrentPage(pageNumber)
        return
      }

      console.error('搜索失败:', response.error?.message)
      setSearchResults([])
      setTotalPages(INITIAL_TOTAL_PAGES)
      setTotalCount(0)
    } catch (error) {
      console.error('搜索出错:', error)
      setSearchResults([])
      setTotalPages(INITIAL_TOTAL_PAGES)
      setTotalCount(0)
    } finally {
      setIsLoading(false)
    }
  }

  const togglePasswordVisibility = (accountId: number) => {
    setVisiblePasswords((prev) => {
      const next = new Set(prev)
      if (next.has(accountId)) {
        next.delete(accountId)
      } else {
        next.add(accountId)
      }
      return next
    })
  }

  return {
    query,
    setQuery,
    searchResults,
    currentPage,
    totalPages,
    totalCount,
    isLoading,
    hasSearched,
    visiblePasswords,
    pageSize,
    search,
    togglePasswordVisibility,
  }
}