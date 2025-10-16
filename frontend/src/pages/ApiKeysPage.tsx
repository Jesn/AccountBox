import { useEffect, useState } from 'react';
import { apiKeyService } from '@/services/apiKeyService';
import type { ApiKey } from '@/types/ApiKey';
import { Button } from '@/components/ui/button';
import { CreateApiKeyDialog } from '@/components/api-keys/CreateApiKeyDialog';
import { DeleteApiKeyDialog } from '@/components/api-keys/DeleteApiKeyDialog';
import { ApiKeyList } from '@/components/api-keys/ApiKeyList';
import { Plus, RefreshCw } from 'lucide-react';

/**
 * API密钥管理页面
 */
export function ApiKeysPage() {
  const [apiKeys, setApiKeys] = useState<ApiKey[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedApiKey, setSelectedApiKey] = useState<ApiKey | null>(null);

  useEffect(() => {
    loadApiKeys();
  }, []);

  const loadApiKeys = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const keys = await apiKeyService.getAll();
      setApiKeys(keys);
    } catch (err: any) {
      console.error('加载API密钥失败:', err);
      setError(err.response?.data?.error?.message || '加载API密钥失败，请重试');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateSuccess = () => {
    loadApiKeys();
  };

  const handleDeleteClick = (apiKey: ApiKey) => {
    setSelectedApiKey(apiKey);
    setIsDeleteDialogOpen(true);
  };

  const handleDeleteSuccess = () => {
    loadApiKeys();
    setSelectedApiKey(null);
  };

  return (
    <div className="container mx-auto py-6 px-4 max-w-4xl">
      {/* 页头 */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">API密钥管理</h1>
          <p className="text-muted-foreground mt-1">
            创建和管理用于外部API调用的密钥
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="icon"
            onClick={loadApiKeys}
            disabled={isLoading}
            title="刷新"
          >
            <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
          </Button>
          <Button onClick={() => setIsCreateDialogOpen(true)}>
            <Plus className="h-4 w-4 mr-2" />
            创建API密钥
          </Button>
        </div>
      </div>

      {/* 说明卡片 */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <h3 className="font-semibold text-blue-900 mb-2">使用说明</h3>
        <ul className="text-sm text-blue-800 space-y-1 list-disc list-inside">
          <li>创建API密钥后，明文密钥将始终可见（可点击隐藏）</li>
          <li>使用"复制"按钮快速复制密钥到剪贴板</li>
          <li>作用域控制：可限制密钥仅访问指定网站的账号</li>
          <li>删除密钥后，使用该密钥的外部系统将无法访问API</li>
        </ul>
      </div>

      {/* 错误提示 */}
      {error && (
        <div className="rounded-md bg-red-50 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-red-400"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-red-800">{error}</p>
            </div>
          </div>
        </div>
      )}

      {/* 加载状态 */}
      {isLoading && (
        <div className="text-center py-12">
          <RefreshCw className="h-8 w-8 animate-spin mx-auto text-muted-foreground" />
          <p className="mt-2 text-muted-foreground">加载中...</p>
        </div>
      )}

      {/* API密钥列表 */}
      {!isLoading && <ApiKeyList apiKeys={apiKeys} onDelete={handleDeleteClick} />}

      {/* 对话框 */}
      <CreateApiKeyDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
        onSuccess={handleCreateSuccess}
      />

      <DeleteApiKeyDialog
        open={isDeleteDialogOpen}
        onOpenChange={setIsDeleteDialogOpen}
        apiKey={selectedApiKey}
        onSuccess={handleDeleteSuccess}
      />
    </div>
  );
}
