import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import type { WebsiteResponse } from '@/services/websiteService'
import { Plus } from 'lucide-react'

interface WebsiteListProps {
  websites: WebsiteResponse[]
  isLoading: boolean
  onViewAccounts: (websiteId: number) => void
  onEdit: (website: WebsiteResponse) => void
  onDelete: (website: WebsiteResponse) => void
  onCreateNew: () => void
}

/**
 * 网站列表组件
 * 显示网站列表卡片,支持查看账号、编辑和删除操作
 */
export function WebsiteList({
  websites,
  isLoading,
  onViewAccounts,
  onEdit,
  onDelete,
  onCreateNew,
}: WebsiteListProps) {
  // 加载状态
  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600">加载中...</p>
        </CardContent>
      </Card>
    )
  }

  // 空状态
  if (websites.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>开始使用 AccountBox</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-gray-600 mb-4">还没有添加任何网站</p>
          <Button onClick={onCreateNew}>
            <Plus className="mr-2 h-4 w-4" />
            添加第一个网站
          </Button>
        </CardContent>
      </Card>
    )
  }

  // 网站列表
  return (
    <div className="grid gap-4">
      {websites.map((website) => (
        <Card key={website.id}>
          <CardContent className="p-4">
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <h3 className="text-lg font-semibold">
                  {website.displayName || website.domain}
                </h3>
                <p className="text-sm text-gray-600">{website.domain}</p>
                {website.tags && (
                  <p className="text-sm text-gray-500 mt-1">
                    标签: {website.tags}
                  </p>
                )}
                <div className="flex gap-4 mt-2 text-sm text-gray-500">
                  <span>活跃账号: {website.activeAccountCount}</span>
                  {website.deletedAccountCount > 0 && (
                    <span>回收站: {website.deletedAccountCount}</span>
                  )}
                </div>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => onViewAccounts(website.id)}
                >
                  查看账号
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => onEdit(website)}
                >
                  编辑
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={() => onDelete(website)}
                >
                  删除
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
