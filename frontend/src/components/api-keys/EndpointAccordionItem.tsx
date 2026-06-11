import {
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion'
import { CopyableCodeBlock } from '@/components/api-keys/CopyableCodeBlock'
import { MethodBadge } from '@/components/api-keys/MethodBadge'
import type { ApiEndpointDoc } from '@/components/api-keys/apiDocumentationData'

interface EndpointAccordionItemProps {
  endpoint: ApiEndpointDoc
  copiedId: string | null
  formatExample: (value: string) => string
  onCopy: (value: string, copyId: string) => void
}

export function EndpointAccordionItem({
  endpoint,
  copiedId,
  formatExample,
  onCopy,
}: EndpointAccordionItemProps) {
  const curlExample = formatExample(endpoint.curlExample)
  const requestBody = endpoint.requestBody
    ? formatExample(endpoint.requestBody)
    : undefined
  const successResponse = formatExample(endpoint.successResponse)
  const errorResponse = endpoint.errorResponse
    ? formatExample(endpoint.errorResponse)
    : undefined

  return (
    <AccordionItem value={endpoint.id}>
      <AccordionTrigger className="hover:no-underline">
        <div className="flex items-start gap-2 sm:gap-3 text-left w-full pr-2">
          <MethodBadge method={endpoint.method} />
          <div className="flex-1 min-w-0">
            <div className="font-semibold text-sm sm:text-base">
              {endpoint.title}
            </div>
            <div className="text-[10px] sm:text-sm text-muted-foreground font-mono break-all">
              {endpoint.path}
            </div>
          </div>
        </div>
      </AccordionTrigger>
      <AccordionContent>
        <div className="space-y-3 sm:space-y-4 pt-2 min-w-0">
          <p className="text-xs sm:text-sm text-muted-foreground">
            {endpoint.description}
          </p>

          <CopyableCodeBlock
            title="curl 示例"
            value={curlExample}
            copyId={`curl-${endpoint.id}`}
            copiedId={copiedId}
            onCopy={onCopy}
          />

          {requestBody && (
            <CopyableCodeBlock
              title="请求体"
              value={requestBody}
              copyId={`request-${endpoint.id}`}
              copiedId={copiedId}
              onCopy={onCopy}
            />
          )}

          <CopyableCodeBlock
            title="成功响应"
            value={successResponse}
            copyId={`success-${endpoint.id}`}
            copiedId={copiedId}
            onCopy={onCopy}
          />

          {errorResponse && (
            <CopyableCodeBlock
              title="错误响应示例"
              value={errorResponse}
              copyId={`error-${endpoint.id}`}
              copiedId={copiedId}
              onCopy={onCopy}
            />
          )}
        </div>
      </AccordionContent>
    </AccordionItem>
  )
}