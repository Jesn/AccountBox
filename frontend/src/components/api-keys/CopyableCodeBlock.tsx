import { Check, Copy } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface CopyableCodeBlockProps {
  title: string
  value: string
  copyId: string
  copiedId: string | null
  onCopy: (value: string, copyId: string) => void
}

export function CopyableCodeBlock({
  title,
  value,
  copyId,
  copiedId,
  onCopy,
}: CopyableCodeBlockProps) {
  const copied = copiedId === copyId

  return (
    <div className="min-w-0">
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs sm:text-sm font-medium">{title}</span>
        <Button
          variant="outline"
          size="sm"
          onClick={() => onCopy(value, copyId)}
          className="h-7 sm:h-8 text-xs flex-shrink-0"
        >
          {copied ? (
            <>
              <Check className="h-3 w-3 sm:h-4 sm:w-4 sm:mr-1" />
              <span className="hidden sm:inline">已复制</span>
            </>
          ) : (
            <>
              <Copy className="h-3 w-3 sm:h-4 sm:w-4 sm:mr-1" />
              <span className="hidden sm:inline">复制</span>
            </>
          )}
        </Button>
      </div>
      <pre className="bg-muted p-2 sm:p-3 rounded-md overflow-x-auto text-[10px] sm:text-sm max-w-full">
        <code className="block">{value}</code>
      </pre>
    </div>
  )
}