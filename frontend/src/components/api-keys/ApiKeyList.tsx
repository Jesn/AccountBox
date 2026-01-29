import { useState } from 'react'
import type { ApiKey } from '@/types/ApiKey'
import type { WebsiteResponse } from '@/services/websiteService'
import { Button } from '@/components/ui/button'
import { CopyButton } from '@/components/common/CopyButton'
import { Badge } from '@/components/ui/badge'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Eye, EyeOff, Trash2 } from 'lucide-react'

interface ApiKeyListProps {
  apiKeys: ApiKey[]
  websites: WebsiteResponse[]
  onDelete: (apiKey: ApiKey) => void
}

/**
 * API密钥列表组件
 */
export function ApiKeyList({ apiKeys, websites, onDelete }: ApiKeyListProps) {
  const [visibleKeys, setVisibleKeys] = useState<Set<number>>(new Set())

  // 根据网站ID获取网站名称（优先 displayName，否则使用 domain）
  const getWebsiteName = (websiteId: number): string => {
    const website = websites.find((w) => w.id === websiteId)
    if (!website) return `网站#${websiteId}`
    return website.displayName || website.domain
  }

  const toggleKeyVisibility = (keyId: number) => {
    setVisibleKeys((prev) => {
      const newSet = new Set(prev)
      if (newSet.has(keyId)) {
        newSet.delete(keyId)
      } else {
        newSet.add(keyId)
      }
      return newSet
    })
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  const maskKey = (key: string) => {
    // 显示前缀"sk_"和最后4个字符
    return `${key.substring(0, 3)}${'*'.repeat(28)}${key.substring(key.length - 4)}`
  }

  if (apiKeys.length === 0) {
    return (
      <div className="text-center py-12 text-muted-foreground">
        <p>暂无API密钥</p>
        <p className="text-sm mt-2">点击上方"创建API密钥"按钮开始创建</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {apiKeys.map((apiKey) => {
        const isVisible = visibleKeys.has(apiKey.id)

        return (
          <Card key={apiKey.id}>
            <CardHeader className="p-3 sm:p-6">
              <div className="flex items-start justify-between gap-2">
                <div className="space-y-1 flex-1 min-w-0">
                  <CardTitle className="text-base sm:text-lg truncate">{apiKey.name}</CardTitle>
                  <CardDescription className="text-xs sm:text-sm">
                    创建于 {formatDate(apiKey.createdAt)}
                    {apiKey.lastUsedAt && (
                      <span className="ml-2 hidden sm:inline">
                        · 最后使用: {formatDate(apiKey.lastUsedAt)}
                      </span>
                    )}
                  </CardDescription>
                </div>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => onDelete(apiKey)}
                  title="删除密钥"
                  className="h-8 w-8 flex-shrink-0"
                >
                  <Trash2 className="h-3.5 w-3.5 text-red-500" />
                </Button>
              </div>
            </CardHeader>
            <CardContent className="space-y-3 p-3 sm:px-6 sm:pb-6 pt-0">
              {/* 密钥显示 */}
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs sm:text-sm font-medium">密钥</span>
                  <div className="flex gap-1.5">
                    {/* 移动端：图标按钮 */}
                    <Button
                      variant="outline"
                      size="icon"
                      onClick={() => toggleKeyVisibility(apiKey.id)}
                      className="md:hidden h-7 w-7"
                      title={isVisible ? "隐藏" : "显示"}
                    >
                      {isVisible ? (
                        <EyeOff className="h-3.5 w-3.5" />
                      ) : (
                        <Eye className="h-3.5 w-3.5" />
                      )}
                    </Button>
                    <CopyButton
                      text={apiKey.keyPlaintext}
                      successMessage="密钥已复制"
                      variant="outline"
                      className="md:hidden h-7 w-7 p-0"
                    />

                    {/* 桌面端：完整按钮 */}
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => toggleKeyVisibility(apiKey.id)}
                      className="hidden md:inline-flex"
                    >
                      {isVisible ? (
                        <>
                          <EyeOff className="h-4 w-4 mr-1" />
                          隐藏
                        </>
                      ) : (
                        <>
                          <Eye className="h-4 w-4 mr-1" />
                          显示
                        </>
                      )}
                    </Button>
                    <CopyButton
                      text={apiKey.keyPlaintext}
                      successMessage="API密钥已复制"
                      variant="outline"
                      showText
                      className="hidden md:inline-flex"
                    />
                  </div>
                </div>
                <div className="font-mono text-xs sm:text-sm bg-muted p-2 sm:p-3 rounded-md break-all">
                  {isVisible
                    ? apiKey.keyPlaintext
                    : maskKey(apiKey.keyPlaintext)}
                </div>
              </div>

              {/* 作用域信息 */}
              <div className="space-y-2">
                <span className="text-xs sm:text-sm font-medium">作用域</span>
                <div className="text-sm">
                  {apiKey.scopeType === 'All' ? (
                    <Badge variant="default" className="bg-blue-500 text-xs">
                      所有网站
                    </Badge>
                  ) : (
                    <div className="space-y-2">
                      <Badge variant="secondary" className="text-xs">
                        指定网站 ({apiKey.websiteIds.length}个)
                      </Badge>
                      {apiKey.websiteIds.length > 0 && (
                        <div className="flex flex-wrap gap-1.5">
                          {apiKey.websiteIds.map((websiteId) => (
                            <Badge
                              key={websiteId}
                              variant="outline"
                              className="text-xs"
                            >
                              {getWebsiteName(websiteId)}
                            </Badge>
                          ))}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
