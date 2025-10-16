import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { searchService } from '@/services/searchService'
import type { SearchResultItem } from '@/services/searchService'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { ArrowLeft, Search, Eye, EyeOff, ExternalLink } from 'lucide-react'
import Pagination from '@/components/common/Pagination'

/**
 * 全局搜索页面
 * 支持搜索网站名、域名、账号用户名、标签、备注
 */
export function SearchPage() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [searchResults, setSearchResults] = useState<SearchResultItem[]>([])
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(false)
  const [hasSearched, setHasSearched] = useState(false)
  const [visiblePasswords, setVisiblePasswords] = useState<Set<number>>(
    new Set()
  )
  const pageSize = 10

  const handleSearch = async (pageNumber: number = 1) => {
    const trimmedQuery = query.trim()

    if (!trimmedQuery) {
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
        setSearchResults(response.data.items as SearchResultItem[])
        setTotalPages(response.data.totalPages)
        setTotalCount(response.data.totalCount)
        setCurrentPage(pageNumber)
      } else {
        console.error('搜索失败:', response.error?.message)
        setSearchResults([])
        setTotalPages(1)
        setTotalCount(0)
      }
    } catch (error) {
      console.error('搜索出错:', error)
      setSearchResults([])
      setTotalPages(1)
      setTotalCount(0)
    } finally {
      setIsLoading(false)
    }
  }

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSearch(1)
    }
  }

  const handlePageChange = (page: number) => {
    handleSearch(page)
  }

  const togglePasswordVisibility = (accountId: number) => {
    setVisiblePasswords((prev) => {
      const newSet = new Set(prev)
      if (newSet.has(accountId)) {
        newSet.delete(accountId)
      } else {
        newSet.add(accountId)
      }
      return newSet
    })
  }

  const handleViewWebsite = (websiteId: number) => {
    navigate(`/websites/${websiteId}/accounts`)
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-7xl">
        {/* 头部 */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => navigate('/websites')}
            className="mb-4"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            返回网站列表
          </Button>

          <h1 className="text-3xl font-bold mb-2">全局搜索</h1>
          <p className="text-gray-600">
            搜索范围：网站名、域名、账号用户名、标签、备注
          </p>
        </div>

        {/* 搜索框 */}
        <div className="mb-6">
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                type="text"
                placeholder="输入关键词并按回车搜索..."
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                onKeyPress={handleKeyPress}
                className="pl-10"
              />
            </div>
            <Button onClick={() => handleSearch(1)} disabled={isLoading}>
              {isLoading ? '搜索中...' : '搜索'}
            </Button>
          </div>
        </div>

        {/* 搜索结果 */}
        {isLoading && (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600">搜索中...</p>
            </CardContent>
          </Card>
        )}

        {!isLoading && hasSearched && searchResults.length === 0 && (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-gray-600 mb-4">
                未找到匹配 "{query}" 的账号
              </p>
              <p className="text-sm text-gray-500">
                提示：搜索不区分大小写，会自动去除首尾空格
              </p>
            </CardContent>
          </Card>
        )}

        {!isLoading && hasSearched && searchResults.length > 0 && (
          <>
            {/* 结果统计 */}
            <div className="mb-4 text-sm text-gray-600">
              找到 <span className="font-semibold">{totalCount}</span> 个匹配结果
            </div>

            {/* 结果列表 */}
            <div className="rounded-md border overflow-x-auto">
              <Table className="text-sm">
                <TableHeader>
                  <TableRow>
                    <TableHead className="h-10">网站</TableHead>
                    <TableHead className="h-10">用户名</TableHead>
                    <TableHead className="h-10">密码</TableHead>
                    <TableHead className="h-10 hidden md:table-cell">
                      标签
                    </TableHead>
                    <TableHead className="h-10 hidden lg:table-cell">
                      备注
                    </TableHead>
                    <TableHead className="h-10 hidden xl:table-cell">
                      匹配字段
                    </TableHead>
                    <TableHead className="text-right h-10">操作</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {searchResults.map((result) => (
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
                            {visiblePasswords.has(result.accountId)
                              ? result.password
                              : '••••••••'}
                          </span>
                          <button
                            onClick={() =>
                              togglePasswordVisibility(result.accountId)
                            }
                            className="text-gray-400 hover:text-gray-600"
                          >
                            {visiblePasswords.has(result.accountId) ? (
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
                              {field === 'WebsiteDisplayName' && '网站名'}
                              {field === 'WebsiteDomain' && '域名'}
                              {field === 'Username' && '用户名'}
                              {field === 'Tags' && '标签'}
                              {field === 'Notes' && '备注'}
                            </span>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell className="text-right py-2 px-3">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleViewWebsite(result.websiteId)}
                        >
                          <ExternalLink className="h-3 w-3 mr-1" />
                          查看网站
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>

            {/* 分页 */}
            {totalPages > 1 && (
              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
              />
            )}
          </>
        )}

        {/* 初始提示 */}
        {!hasSearched && (
          <Card>
            <CardContent className="py-12 text-center">
              <Search className="mx-auto h-12 w-12 text-gray-400 mb-4" />
              <p className="text-gray-600 mb-2">开始搜索您的账号</p>
              <p className="text-sm text-gray-500">
                在搜索框中输入关键词，支持搜索网站名、域名、账号用户名、标签、备注
              </p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}
