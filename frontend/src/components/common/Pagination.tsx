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
  pageSizeOptions?: number[]
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
  const showBottomControls = showPageControls || canChangePageSize
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
    let end = Math.min(safeTotalPages - 1, start + middleSize - 1)

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
    <div className="mt-4 flex flex-col gap-3 rounded-lg border bg-white px-3 py-3 sm:px-4">
      {showSummary && hasTotal && (
        <div className="text-center text-xs text-gray-600 sm:text-sm">
          共 <span className="font-semibold text-gray-900">{totalCount}</span> 条
          {hasPageSize && totalCount > 0 && (
            <>
              ，当前显示第{' '}
              <span className="font-semibold text-gray-900">{rangeStart}</span>
              {' - '}
              <span className="font-semibold text-gray-900">{rangeEnd}</span> 条
            </>
          )}
        </div>
      )}

      {showBottomControls && (
        <div className="flex flex-col items-center justify-between gap-3 lg:flex-row">
          {showPageControls && (
            <div className="flex items-center justify-center gap-1 sm:gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => changePage(1)}
                disabled={safeCurrentPage === 1}
                className="hidden h-8 flex-shrink-0 sm:inline-flex sm:h-9"
              >
                首页
              </Button>

              <Button
                variant="outline"
                size="sm"
                onClick={() => changePage(safeCurrentPage - 1)}
                disabled={safeCurrentPage === 1}
                className="h-8 flex-shrink-0 sm:h-9"
              >
                <ChevronLeft className="h-4 w-4" />
                <span className="hidden sm:inline ml-1">上一页</span>
              </Button>

              <div className="flex min-w-0 gap-1">
                {getPageNumbers().map((page) =>
                  typeof page === 'number' ? (
                    <Button
                      key={page}
                      variant={page === safeCurrentPage ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => changePage(page)}
                      className="h-8 min-w-8 px-2 text-xs sm:h-9 sm:min-w-10 sm:px-3 sm:text-sm"
                    >
                      {page}
                    </Button>
                  ) : (
                    <span
                      key={page}
                      className="flex h-8 min-w-6 items-center justify-center text-sm text-gray-500 sm:h-9 sm:min-w-8"
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
                disabled={safeCurrentPage === safeTotalPages}
                className="h-8 flex-shrink-0 sm:h-9"
              >
                <span className="hidden sm:inline mr-1">下一页</span>
                <ChevronRight className="h-4 w-4" />
              </Button>

              <Button
                variant="outline"
                size="sm"
                onClick={() => changePage(safeTotalPages)}
                disabled={safeCurrentPage === safeTotalPages}
                className="hidden h-8 flex-shrink-0 sm:inline-flex sm:h-9"
              >
                末页
              </Button>
            </div>
          )}

          <div className="flex flex-col items-center justify-center gap-2 text-xs text-gray-600 sm:flex-row sm:text-sm lg:ml-auto">
            {canChangePageSize && (
              <div className="flex items-center justify-center gap-2">
                <span>每页</span>
                <Select
                  value={pageSize.toString()}
                  onValueChange={(value) => onPageSizeChange(Number(value))}
                >
                  <SelectTrigger className="h-8 w-[86px] sm:h-9">
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

            {showJump && showPageControls && (
              <div className="flex items-center justify-center gap-2">
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
                  className="h-8 w-16 text-center sm:h-9"
                />
                <span>页</span>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleJump}
                  className="h-8 sm:h-9"
                >
                  跳转
                </Button>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}
