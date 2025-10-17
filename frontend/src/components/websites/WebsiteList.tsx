import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
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
 * 以表格形式显示网站列表,支持查看账号、编辑和删除操作
 */
export function WebsiteList({
  websites,
  isLoading,
  onViewAccounts,
  onEdit,
  onDelete,
  onCreateNew,
}: WebsiteListProps) {
  // 加载状态或空状态
  if (isLoading || websites.length === 0) {
    return (
      <Card>
        {isLoading ? (
          <CardContent className="py-12 text-center">
            <p className="text-gray-600">加载中...</p>
          </CardContent>
        ) : (
          <>
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
          </>
        )}
      </Card>
    )
  }

  // 网站列表 - 表格视图
  return (
    <div className="rounded-md border overflow-x-auto">
      <Table className="text-sm">
        <TableHeader>
          <TableRow>
            <TableHead className="h-10 w-16">ID</TableHead>
            <TableHead className="h-10">显示名</TableHead>
            <TableHead className="h-10">域名</TableHead>
            <TableHead className="h-10">活跃账号</TableHead>
            <TableHead className="h-10">禁用账号</TableHead>
            <TableHead className="h-10">回收站</TableHead>
            <TableHead className="hidden md:table-cell h-10">标签</TableHead>
            <TableHead className="text-right h-10">操作</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {websites.map((website) => (
            <TableRow key={website.id}>
              <TableCell className="py-2 px-3 text-muted-foreground">
                {website.id}
              </TableCell>
              <TableCell
                className="font-medium max-w-xs truncate py-2 px-3"
                title={website.displayName || website.domain}
              >
                {website.displayName || website.domain}
              </TableCell>
              <TableCell
                className="max-w-xs truncate py-2 px-3"
                title={website.domain}
              >
                {website.domain}
              </TableCell>
              <TableCell className="py-2 px-3">
                {website.activeAccountCount}
              </TableCell>
              <TableCell className="py-2 px-3">
                {website.disabledAccountCount}
              </TableCell>
              <TableCell className="py-2 px-3">
                {website.deletedAccountCount}
              </TableCell>
              <TableCell className="hidden md:table-cell py-2 px-3">
                {website.tags || '-'}
              </TableCell>
              <TableCell className="text-right py-2 px-3">
                <div className="flex gap-2 justify-end">
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
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
