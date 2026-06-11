import { ApiDocumentation } from '@/components/api-keys/ApiDocumentation'
import { PageHeader } from '@/components/layout/PageHeader'
import { PageShell } from '@/components/layout/PageShell'
import { BookOpen } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

/**
 * API 使用文档页面
 */
export function ApiDocumentationPage() {
  const navigate = useNavigate()

  return (
    <PageShell maxWidth="max-w-4xl">
      <PageHeader
        title="外部 API 使用文档"
        description="详细的 API 端点说明和 curl 使用示例"
        icon={<BookOpen className="h-5 w-5 sm:h-6 sm:w-6 text-primary flex-shrink-0" />}
        titleClassName="text-lg sm:text-2xl md:text-3xl"
        backAction={{ onClick: () => navigate(-1) }}
      />

      {/* 快速开始指南 - 移动端简化 */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-xs sm:text-sm mb-6">
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

      <ApiDocumentation />
    </PageShell>
  )
}
