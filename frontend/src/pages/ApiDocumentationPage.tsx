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
    <div className="container mx-auto py-4 md:py-6 px-4 max-w-4xl">
      {/* 页头 */}
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => navigate(-1)}
            title="返回"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <BookOpen className="h-8 w-8 text-primary" />
              <h1 className="text-2xl sm:text-3xl font-bold">外部 API 使用文档</h1>
            </div>
            <p className="text-muted-foreground mt-1">
              详细的 API 端点说明和 curl 使用示例
            </p>
          </div>
        </div>
      </div>

      {/* 快速开始指南 */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
        <h3 className="font-semibold text-blue-900 mb-2">快速开始</h3>
        <ol className="text-sm text-blue-800 space-y-2 list-decimal list-inside">
          <li>
            前往{' '}
            <button
              onClick={() => navigate('/api-keys')}
              className="underline font-medium hover:text-blue-600"
            >
              API 密钥管理页面
            </button>{' '}
            创建 API 密钥
          </li>
          <li>复制生成的 API 密钥</li>
          <li>
            选择下方任意端点,将示例中的{' '}
            <code className="bg-blue-100 px-1 py-0.5 rounded">
              YOUR_API_KEY
            </code>{' '}
            替换为你的实际密钥
          </li>
          <li>在终端中执行 curl 命令开始使用</li>
        </ol>
      </div>

      {/* API 文档主体 */}
      <ApiDocumentation />
    </div>
  )
}
