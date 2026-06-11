import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { ChevronLeft, ChevronRight } from 'lucide-react'

interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
  totalCount?: number
  pageSize?: number
  pageSizeOptions?: readonly number[]
  onPageSizeChange?: (pageSize: number) => void
  showSummary?: boolean
  showJump?: boolean
  disabled?: boolean
}

type PageItem = number | 'ellipsis-left' | 'ellipsis-right'

export default function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  totalCount,
  pageSize,
  pageSizeOptions,
  onPageSizeChange,
  showSummary = true,
  showJump = true,
  disabled = false,
}: PaginationProps) {
  const [maxVisiblePages, setMaxVisiblePages] = useState(5)
  const [jumpPage, setJumpPage] = useState(currentPage.toString())

  useEffect(() => {
    const calculateMaxPages = () => {
      const width = window.innerWidth

      if (width >= 1024) {
        setMaxVisiblePages(7)
        return
      }

      if (width >= 640) {
        setMaxVisiblePages(5)
        return
      }

      setMaxVisiblePages(3)
    }

    calculateMaxPages()
    window.addEventListener('resize', calculateMaxPages)

    return () => window.removeEventListener('resize', calculateMaxPages)
  }, [])

  useEffect(() => {
    setJumpPage(currentPage.toString())
  }, [currentPage])

  const safeTotalPages = Math.max(1, totalPages)
  const safeCurrentPage = Math.min(Math.max(currentPage, 1), safeTotalPages)
  const hasTotal = typeof totalCount === 'number'
  const hasPageSize = typeof pageSize === 'number' && pageSize > 0
  const canChangePageSize =
    hasPageSize &&
    Array.isArray(pageSizeOptions) &&
    pageSizeOptions.length > 0 &&
    typeof onPageSizeChange === 'function'
  const showPageControls = safeTotalPages > 1
  const rangeStart = hasTotal && hasPageSize && totalCount > 0
    ? (safeCurrentPage - 1) * pageSize + 1
    : 0
  const rangeEnd = hasTotal && hasPageSize
    ? Math.min(safeCurrentPage * pageSize, totalCount)
    : 0

  const changePage = (page: number) => {
    if (disabled) {
      return
    }

    const nextPage = Math.min(Math.max(page, 1), safeTotalPages)
    if (nextPage !== safeCurrentPage) {
      onPageChange(nextPage)
    }
  }

  const handleJump = () => {
    if (disabled) {
      return
    }

    const parsedPage = Number(jumpPage)
    if (!Number.isInteger(parsedPage)) {
      setJumpPage(safeCurrentPage.toString())
      return
    }

    changePage(parsedPage)
  }

  const getPageNumbers = (): PageItem[] => {
    if (safeTotalPages <= maxVisiblePages + 2) {
      return Array.from({ length: safeTotalPages }, (_, index) => index + 1)
    }

    const middleSize = Math.max(1, maxVisiblePages - 2)
    const halfVisible = Math.floor(middleSize / 2)
    let start = Math.max(2, safeCurrentPage - halfVisible)
    const end = Math.min(safeTotalPages - 1, start + middleSize - 1)

    if (end - start + 1 < middleSize) {
      start = Math.max(2, end - middleSize + 1)
    }

    const pages: PageItem[] = [1]

    if (start > 2) {
      pages.push('ellipsis-left')
    }

    for (let page = start; page <= end; page++) {
      pages.push(page)
    }

    if (end < safeTotalPages - 1) {
      pages.push('ellipsis-right')
    }

    pages.push(safeTotalPages)
    return pages
  }

  if (safeTotalPages <= 1 && !hasTotal) {
    return null
  }

  return (
    <div className="mt-4 flex flex-col items-center gap-2 rounded-lg border bg-white px-3 py-2 sm:px-4 sm:py-3">
      {(showSummary && hasTotal) || canChangePageSize ? (
        <div className="flex w-full flex-wrap items-center justify-center gap-x-3 gap-y-1 text-xs text-gray-600 sm:text-sm">
          {showSummary && hasTotal && (
            <span className="text-center">
              共 <span className="font-semibold text-gray-900">{totalCount}</span> 条
              {hasPageSize && totalCount > 0 && (
                <span className="hidden text-gray-500 sm:inline">
                  {' '}· {rangeStart}-{rangeEnd}
                </span>
              )}
            </span>
          )}

          {canChangePageSize && (
            <div className="flex items-center justify-center gap-1.5">
              <span>每页</span>
              <Select
                value={pageSize.toString()}
                onValueChange={(value) => onPageSizeChange(Number(value))}
                disabled={disabled}
              >
                <SelectTrigger className="h-7 w-[78px] px-2 text-xs sm:h-8 sm:w-[86px] sm:text-sm">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {pageSizeOptions.map((option) => (
                    <SelectItem key={option} value={option.toString()}>
                      {option} 条
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}
        </div>
      ) : null}

      {showPageControls && (
        <div className="flex w-full items-center justify-center gap-2 sm:w-auto">
          <Button
            variant="outline"
            size="sm"
            onClick={() => changePage(1)}
            disabled={disabled || safeCurrentPage === 1}
            className="hidden h-8 flex-shrink-0 lg:inline-flex"
          >
            首页
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => changePage(safeCurrentPage - 1)}
            disabled={disabled || safeCurrentPage === 1}
            className="h-8 min-w-9 flex-shrink-0 px-2 sm:h-9 sm:px-3"
          >
            <ChevronLeft className="h-4 w-4" />
            <span className="hidden sm:ml-1 sm:inline">上一页</span>
          </Button>

          <div className="flex h-8 min-w-[88px] items-center justify-center rounded-md border bg-gray-50 px-3 text-xs font-medium text-gray-700 sm:hidden">
            {safeCurrentPage} / {safeTotalPages}
          </div>

          <div className="hidden min-w-0 gap-1 sm:flex">
            {getPageNumbers().map((page) =>
              typeof page === 'number' ? (
                <Button
                  key={page}
                  variant={page === safeCurrentPage ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => changePage(page)}
                  disabled={disabled}
                  className="h-9 min-w-10 px-3 text-sm"
                >
                  {page}
                </Button>
              ) : (
                <span
                  key={page}
                  className="flex h-9 min-w-8 items-center justify-center text-sm text-gray-500"
                >
                  ...
                </span>
              )
            )}
          </div>

          <Button
            variant="outline"
            size="sm"
            onClick={() => changePage(safeCurrentPage + 1)}
            disabled={disabled || safeCurrentPage === safeTotalPages}
            className="h-8 min-w-9 flex-shrink-0 px-2 sm:h-9 sm:px-3"
          >
            <span className="hidden sm:mr-1 sm:inline">下一页</span>
            <ChevronRight className="h-4 w-4" />
          </Button>

          <Button
            variant="outline"
            size="sm"
            onClick={() => changePage(safeTotalPages)}
            disabled={disabled || safeCurrentPage === safeTotalPages}
            className="hidden h-8 flex-shrink-0 lg:inline-flex"
          >
            末页
          </Button>
        </div>
      )}

      {showJump && showPageControls && (
        <div className="hidden items-center justify-center gap-2 text-sm text-gray-600 sm:flex">
          <span>跳至</span>
          <Input
            value={jumpPage}
            onChange={(event) => setJumpPage(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === 'Enter') {
                handleJump()
              }
            }}
            inputMode="numeric"
            disabled={disabled}
            className="h-8 w-16 text-center sm:h-9"
          />
          <span>页</span>
          <Button
            variant="outline"
            size="sm"
            onClick={handleJump}
            disabled={disabled}
            className="h-8 sm:h-9"
          >
            跳转
          </Button>
        </div>
      )}
    </div>
  )
}
