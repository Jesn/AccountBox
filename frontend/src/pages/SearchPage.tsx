import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { Search } from 'lucide-react'
import { SearchResults } from '@/components/search/SearchResults'
import { useSearchPageState } from '@/hooks/useSearchPageState'

/**
 * 全局搜索页面
 * 支持搜索网站名、域名、账号用户名、标签、备注
 */
export function SearchPage() {
  const navigate = useNavigate()
  const {
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
  } = useSearchPageState()

  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      search(1)
    }
  }

  return (
    <PageShell>
      <PageHeader
        title="全局搜索"
        description="搜索范围：网站名、域名、账号用户名、标签、备注"
        titleClassName="text-2xl sm:text-3xl"
        className="mb-8"
        backAction={{ label: '返回网站列表', onClick: () => navigate('/websites') }}
      />

      <div className="mb-6">
        <div className="flex flex-col sm:flex-row gap-2">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              type="text"
              placeholder="输入关键词并按回车搜索..."
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              onKeyDown={handleKeyDown}
              className="pl-10"
            />
          </div>
          <Button
            onClick={() => search(1)}
            disabled={isLoading}
            className="w-full sm:w-auto"
          >
            {isLoading ? '搜索中...' : '搜索'}
          </Button>
        </div>
      </div>

      <SearchResults
        query={query}
        results={searchResults}
        visiblePasswords={visiblePasswords}
        currentPage={currentPage}
        totalPages={totalPages}
        totalCount={totalCount}
        pageSize={pageSize}
        isLoading={isLoading}
        hasSearched={hasSearched}
        onPageChange={search}
        onTogglePasswordVisibility={togglePasswordVisibility}
        onViewWebsite={(websiteId) => navigate(`/websites/${websiteId}/accounts`)}
      />
    </PageShell>
  )
}