import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import type { WebsiteResponse } from '@/services/websiteService'
import { Eye, Edit, Trash2 } from 'lucide-react'
import { toast } from 'sonner'

interface WebsiteCardViewProps {
  websites: WebsiteResponse[]
  onViewAccounts: (websiteId: number) => void
  onEdit: (website: WebsiteResponse) => void
  onDelete: (website: WebsiteResponse) => void
}

/**
 * 网站卡片视图组件（移动端）
 * 以卡片形式显示网站列表，适合小屏幕设备
 */
export function WebsiteCardView({
  websites,
  onViewAccounts,
  onEdit,
  onDelete,
}: WebsiteCardViewProps) {
  if (websites.length === 0) {
    return null
  }

  const handleCopyId = (id: number, e: React.MouseEvent) => {
    e.stopPropagation()
    navigator.clipboard.writeText(id.toString())
    toast.success('ID 已复制到剪贴板')
  }

  return (
    <div className="space-y-4">
      {websites.map((website) => (
        <Card
          key={website.id}
          className="overflow-hidden cursor-pointer hover:shadow-md transition-shadow"
          onClick={() => onViewAccounts(website.id)}
        >
          <CardContent className="p-4">
            {/* 标题和ID */}
            <div className="flex items-start justify-between mb-3">
              <div className="flex-1 min-w-0">
                <h3 className="font-medium text-base truncate mb-1">
                  {website.displayName || website.domain}
                </h3>
                {website.displayName && website.domain && (
                  <p className="text-sm text-gray-600 truncate">
                    {website.domain}
                  </p>
                )}
              </div>
              <span
                className="text-xs text-gray-500 ml-2 flex-shrink-0 cursor-pointer hover:text-gray-700 hover:underline"
                onClick={(e) => handleCopyId(website.id, e)}
                title="点击复制 ID"
              >
                ID: {website.id}
              </span>
            </div>

            {/* 账号统计 - 紧凑显示 */}
            <div className="flex items-center gap-4 mb-3 text-sm">
              <div className="flex items-center gap-1">
                <span className="font-semibold text-green-700">
                  {website.activeAccountCount}
                </span>
                <span className="text-gray-600">活跃</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="font-semibold text-gray-700">
                  {website.disabledAccountCount}
                </span>
                <span className="text-gray-600">禁用</span>
              </div>
              <div className="flex items-center gap-1">
                <span className="font-semibold text-red-700">
                  {website.deletedAccountCount}
                </span>
                <span className="text-gray-600">回收</span>
              </div>
            </div>

            {/* 操作按钮 */}
            <div className="flex gap-2 pt-3 border-t">
              <Button
                variant="default"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  onViewAccounts(website.id)
                }}
                className="flex-1"
              >
                <Eye className="h-4 w-4 mr-1" />
                查看账号
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  onEdit(website)
                }}
              >
                <Edit className="h-4 w-4" />
              </Button>
              <Button
                variant="destructive"
                size="sm"
                onClick={(e) => {
                  e.stopPropagation()
                  onDelete(website)
                }}
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
