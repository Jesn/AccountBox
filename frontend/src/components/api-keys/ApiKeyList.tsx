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
    <div className="space-y-4">
      {apiKeys.map((apiKey) => {
        const isVisible = visibleKeys.has(apiKey.id)

        return (
          <Card key={apiKey.id}>
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="space-y-1">
                  <CardTitle className="text-lg">{apiKey.name}</CardTitle>
                  <CardDescription>
                    创建于 {formatDate(apiKey.createdAt)}
                    {apiKey.lastUsedAt && (
                      <span className="ml-2">
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
                >
                  <Trash2 className="h-4 w-4 text-red-500" />
                </Button>
              </div>
            </CardHeader>
            <CardContent className="space-y-3">
              {/* 密钥显示 */}
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-medium">密钥</span>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => toggleKeyVisibility(apiKey.id)}
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
                      successMessage="API密钥已复制到剪贴板"
                      variant="outline"
                      showText
                    />
                  </div>
                </div>
                <div className="font-mono text-sm bg-muted p-3 rounded-md break-all">
                  {isVisible
                    ? apiKey.keyPlaintext
                    : maskKey(apiKey.keyPlaintext)}
                </div>
              </div>

              {/* 作用域信息 */}
              <div className="space-y-2">
                <span className="text-sm font-medium">作用域</span>
                <div className="text-sm">
                  {apiKey.scopeType === 'All' ? (
                    <Badge variant="default" className="bg-blue-500">
                      所有网站
                    </Badge>
                  ) : (
                    <div className="space-y-2">
                      <Badge variant="secondary">
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
