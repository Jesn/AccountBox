import { Button } from '@/components/ui/button'
import { ApiDocumentation } from '@/components/api-keys/ApiDocumentation'
import { ArrowLeft, BookOpen } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

/**
 * API 使用文档页面
 */
export function ApiDocumentationPage() {
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50 p-4 md:p-6 lg:p-8">
      <div className="mx-auto max-w-4xl">
        {/* 页头 */}
        <div className="mb-6">
          <div className="flex items-start justify-between gap-3 mb-4">
            <div className="flex items-center gap-2 flex-1 min-w-0">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate(-1)}
                title="返回"
                className="-ml-2"
              >
                <ArrowLeft className="mr-2 h-4 w-4" />
                返回
              </Button>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <BookOpen className="h-5 w-5 sm:h-6 sm:w-6 text-primary flex-shrink-0" />
                  <h1 className="text-lg sm:text-2xl md:text-3xl font-bold truncate">
                    外部 API 使用文档
                  </h1>
                </div>
                <p className="text-xs sm:text-sm text-muted-foreground mt-1 hidden sm:block">
                  详细的 API 端点说明和 curl 使用示例
                </p>
              </div>
            </div>
          </div>

          {/* 快速开始指南 - 移动端简化 */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-xs sm:text-sm">
            <h3 className="font-semibold text-blue-900 mb-2">快速开始</h3>
            <ol className="text-blue-800 space-y-1.5 list-decimal list-inside">
              <li>
                前往{' '}
                <button
                  onClick={() => navigate('/api-keys')}
                  className="underline font-medium hover:text-blue-600"
                >
                  API 密钥管理页面
                </button>{' '}
                创建密钥
              </li>
              <li>复制生成的 API 密钥</li>
              <li>
                将示例中的{' '}
                <code className="bg-blue-100 px-1 py-0.5 rounded text-[10px] sm:text-xs">
                  YOUR_API_KEY
                </code>{' '}
                替换为实际密钥
              </li>
              <li className="hidden sm:list-item">在终端中执行 curl 命令开始使用</li>
            </ol>
          </div>
        </div>

        {/* API 文档主体 */}
        <ApiDocumentation />
      </div>
    </div>
  )
}
