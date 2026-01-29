import { Button } from '@/components/ui/button'
import { ChevronLeft, ChevronRight } from 'lucide-react'

interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
}

export default function Pagination({
  currentPage,
  totalPages,
  onPageChange,
}: PaginationProps) {
  const handlePrevious = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1)
    }
  }

  const handleNext = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1)
    }
  }

  // 生成页码按钮（移动端最多显示3个，桌面端显示5个）
  const getPageNumbers = (isMobile: boolean) => {
    const pages: number[] = []
    const maxVisible = isMobile ? 3 : 5

    if (totalPages <= maxVisible) {
      // 如果总页数小于等于最大可见页数，显示所有页码
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i)
      }
    } else {
      // 否则，显示当前页附近的页码
      const halfVisible = Math.floor(maxVisible / 2)
      let start = Math.max(1, currentPage - halfVisible)
      const end = Math.min(totalPages, start + maxVisible - 1)

      if (end - start + 1 < maxVisible) {
        start = Math.max(1, end - maxVisible + 1)
      }

      for (let i = start; i <= end; i++) {
        pages.push(i)
      }
    }

    return pages
  }

  if (totalPages <= 1) {
    return null
  }

  return (
    <div className="flex items-center justify-center gap-1 sm:gap-2 mt-4 px-2">
      {/* 上一页按钮 - 移动端只显示图标 */}
      <Button
        variant="outline"
        size="sm"
        onClick={handlePrevious}
        disabled={currentPage === 1}
        className="h-8 sm:h-9"
      >
        <ChevronLeft className="h-4 w-4" />
        <span className="hidden sm:inline ml-1">上一页</span>
      </Button>

      {/* 页码按钮 - 移动端显示3个，桌面端显示5个 */}
      <div className="flex gap-1">
        {getPageNumbers(true).map((page) => (
          <Button
            key={page}
            variant={page === currentPage ? 'default' : 'outline'}
            size="sm"
            onClick={() => onPageChange(page)}
            className="min-w-8 sm:min-w-10 h-8 sm:h-9 px-2 sm:px-3 text-xs sm:text-sm sm:hidden"
          >
            {page}
          </Button>
        ))}
        {getPageNumbers(false).map((page) => (
          <Button
            key={page}
            variant={page === currentPage ? 'default' : 'outline'}
            size="sm"
            onClick={() => onPageChange(page)}
            className="hidden sm:inline-flex min-w-10 h-9"
          >
            {page}
          </Button>
        ))}
      </div>

      {/* 下一页按钮 - 移动端只显示图标 */}
      <Button
        variant="outline"
        size="sm"
        onClick={handleNext}
        disabled={currentPage === totalPages}
        className="h-8 sm:h-9"
      >
        <span className="hidden sm:inline mr-1">下一页</span>
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  )
}
