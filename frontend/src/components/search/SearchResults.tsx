import { Button } from '@/components/ui/button'
import { EmptyState } from '@/components/common/EmptyState'
import { LoadingState } from '@/components/common/LoadingState'
import Pagination from '@/components/common/Pagination'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Search, Eye, EyeOff, ExternalLink } from 'lucide-react'
import type { SearchResultItem } from '@/types'

interface SearchResultsProps {
  query: string
  results: SearchResultItem[]
  visiblePasswords: Set<number>
  currentPage: number
  totalPages: number
  totalCount: number
  pageSize: number
  isLoading: boolean
  hasSearched: boolean
  onPageChange: (page: number) => void
  onTogglePasswordVisibility: (accountId: number) => void
  onViewWebsite: (websiteId: number) => void
}

const matchedFieldLabels: Record<string, string> = {
  WebsiteDisplayName: '网站名',
  WebsiteDomain: '域名',
  Username: '用户名',
  Tags: '标签',
  Notes: '备注',
}

export function SearchResults({
  query,
  results,
  visiblePasswords,
  currentPage,
  totalPages,
  totalCount,
  pageSize,
  isLoading,
  hasSearched,
  onPageChange,
  onTogglePasswordVisibility,
  onViewWebsite,
}: SearchResultsProps) {
  if (isLoading) {
    return <LoadingState message="搜索中..." />
  }

  if (hasSearched && results.length === 0) {
    return (
      <EmptyState
        title={`未找到匹配 "${query}" 的账号`}
        description="提示：搜索不区分大小写，会自动去除首尾空格"
      />
    )
  }

  if (!hasSearched) {
    return (
      <EmptyState
        title="开始搜索您的账号"
        description="在搜索框中输入关键词，支持搜索网站名、域名、账号用户名、标签、备注"
        icon={<Search className="h-12 w-12" />}
      />
    )
  }

  return (
    <>
      <div className="mb-4 text-sm text-gray-600">
        找到 <span className="font-semibold">{totalCount}</span> 个匹配结果
      </div>

      <div className="rounded-md border overflow-x-auto">
        <Table className="text-sm">
          <TableHeader>
            <TableRow>
              <TableHead className="h-10">网站</TableHead>
              <TableHead className="h-10">用户名</TableHead>
              <TableHead className="h-10">密码</TableHead>
              <TableHead className="h-10 hidden md:table-cell">标签</TableHead>
              <TableHead className="h-10 hidden lg:table-cell">备注</TableHead>
              <TableHead className="h-10 hidden xl:table-cell">
                匹配字段
              </TableHead>
              <TableHead className="text-right h-10">操作</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {results.map((result) => {
              const isPasswordVisible = visiblePasswords.has(result.accountId)

              return (
                <TableRow key={result.accountId}>
                  <TableCell className="py-2 px-3">
                    <div>
                      <div className="font-medium">
                        {result.websiteDisplayName || result.websiteDomain}
                      </div>
                      {result.websiteDisplayName && (
                        <div className="text-xs text-gray-500">
                          {result.websiteDomain}
                        </div>
                      )}
                    </div>
                  </TableCell>
                  <TableCell className="py-2 px-3 font-mono text-xs">
                    {result.username}
                  </TableCell>
                  <TableCell className="py-2 px-3">
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-xs">
                        {isPasswordVisible ? result.password : '••••••••'}
                      </span>
                      <button
                        type="button"
                        onClick={() =>
                          onTogglePasswordVisibility(result.accountId)
                        }
                        className="text-gray-400 hover:text-gray-600"
                      >
                        {isPasswordVisible ? (
                          <EyeOff className="h-4 w-4" />
                        ) : (
                          <Eye className="h-4 w-4" />
                        )}
                      </button>
                    </div>
                  </TableCell>
                  <TableCell className="py-2 px-3 hidden md:table-cell">
                    {result.tags || '-'}
                  </TableCell>
                  <TableCell className="py-2 px-3 hidden lg:table-cell max-w-xs truncate">
                    {result.notes || '-'}
                  </TableCell>
                  <TableCell className="py-2 px-3 hidden xl:table-cell">
                    <div className="flex flex-wrap gap-1">
                      {result.matchedFields.map((field) => (
                        <span
                          key={field}
                          className="inline-block px-2 py-0.5 text-xs bg-blue-100 text-blue-800 rounded"
                        >
                          {matchedFieldLabels[field] ?? field}
                        </span>
                      ))}
                    </div>
                  </TableCell>
                  <TableCell className="text-right py-2 px-3">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => onViewWebsite(result.websiteId)}
                    >
                      <ExternalLink className="h-3 w-3 mr-1" />
                      查看网站
                    </Button>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </div>

      {totalPages > 1 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          totalCount={totalCount}
          pageSize={pageSize}
          onPageChange={onPageChange}
        />
      )}
    </>
  )
}