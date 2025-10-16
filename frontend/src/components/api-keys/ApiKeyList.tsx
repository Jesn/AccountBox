import { useState } from 'react';
import type { ApiKey } from '@/types/ApiKey';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Eye, EyeOff, Copy, Trash2 } from 'lucide-react';

interface ApiKeyListProps {
  apiKeys: ApiKey[];
  onDelete: (apiKey: ApiKey) => void;
}

/**
 * API密钥列表组件
 */
export function ApiKeyList({ apiKeys, onDelete }: ApiKeyListProps) {
  const [visibleKeys, setVisibleKeys] = useState<Set<number>>(new Set());
  const [copiedKeyId, setCopiedKeyId] = useState<number | null>(null);

  const toggleKeyVisibility = (keyId: number) => {
    setVisibleKeys((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(keyId)) {
        newSet.delete(keyId);
      } else {
        newSet.add(keyId);
      }
      return newSet;
    });
  };

  const copyToClipboard = async (apiKey: ApiKey) => {
    try {
      await navigator.clipboard.writeText(apiKey.keyPlaintext);
      setCopiedKeyId(apiKey.id);
      setTimeout(() => setCopiedKeyId(null), 2000);
    } catch (err) {
      console.error('复制失败:', err);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const maskKey = (key: string) => {
    // 显示前缀"sk_"和最后4个字符
    return `${key.substring(0, 3)}${'*'.repeat(28)}${key.substring(key.length - 4)}`;
  };

  if (apiKeys.length === 0) {
    return (
      <div className="text-center py-12 text-muted-foreground">
        <p>暂无API密钥</p>
        <p className="text-sm mt-2">点击上方"创建API密钥"按钮开始创建</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {apiKeys.map((apiKey) => {
        const isVisible = visibleKeys.has(apiKey.id);
        const isCopied = copiedKeyId === apiKey.id;

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
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => copyToClipboard(apiKey)}
                    >
                      <Copy className="h-4 w-4 mr-1" />
                      {isCopied ? '已复制' : '复制'}
                    </Button>
                  </div>
                </div>
                <div className="font-mono text-sm bg-muted p-3 rounded-md break-all">
                  {isVisible ? apiKey.keyPlaintext : maskKey(apiKey.keyPlaintext)}
                </div>
              </div>

              {/* 作用域信息 */}
              <div className="space-y-1">
                <span className="text-sm font-medium">作用域</span>
                <div className="text-sm text-muted-foreground">
                  {apiKey.scopeType === 'All' ? (
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      所有网站
                    </span>
                  ) : (
                    <div>
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                        指定网站 ({apiKey.websiteIds.length}个)
                      </span>
                      {apiKey.websiteIds.length > 0 && (
                        <div className="mt-2 text-xs">
                          网站ID: {apiKey.websiteIds.join(', ')}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
