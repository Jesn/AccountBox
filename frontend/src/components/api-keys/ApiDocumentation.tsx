import { useState } from 'react'
import { Accordion } from '@/components/ui/accordion'
import { copyToClipboard } from '@/lib/clipboard'
import { ApiNotice } from '@/components/api-keys/ApiNotice'
import { EndpointAccordionItem } from '@/components/api-keys/EndpointAccordionItem'
import { API_ENDPOINT_DOCS } from '@/components/api-keys/apiDocumentationData'

/**
 * API 使用文档组件
 * 展示外部 API 端点的 curl 使用示例
 */
export function ApiDocumentation() {
  const [copiedId, setCopiedId] = useState<string | null>(null)

  const apiBaseUrl =
    typeof window !== 'undefined' ? window.location.origin : 'http://localhost:5093'

  const handleCopy = async (text: string, id: string) => {
    const success = await copyToClipboard(text)
    if (success) {
      setCopiedId(id)
      setTimeout(() => setCopiedId(null), 2000)
    }
  }

  const replaceBaseUrl = (text: string): string => {
    return text.replace(/http:\/\/localhost:5093/g, apiBaseUrl)
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-lg sm:text-2xl font-bold mb-2">API 端点列表</h2>
        <p className="text-xs sm:text-sm text-muted-foreground">
          以下是外部 API 端点的 curl 使用示例。请将示例中的{' '}
          <code className="bg-muted px-1 py-0.5 rounded text-[10px] sm:text-xs">
            YOUR_API_KEY
          </code>{' '}
          替换为你的实际 API 密钥。
        </p>
      </div>

      <Accordion type="single" collapsible className="w-full">
        {API_ENDPOINT_DOCS.map((endpoint) => (
          <EndpointAccordionItem
            key={endpoint.id}
            endpoint={endpoint}
            copiedId={copiedId}
            formatExample={replaceBaseUrl}
            onCopy={handleCopy}
          />
        ))}
      </Accordion>

      <ApiNotice />
    </div>
  )
}