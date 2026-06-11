import { Label } from '@/components/ui/label'
import { CopyButton } from '@/components/common/CopyButton'

interface GeneratedPasswordPreviewProps {
  password: string
}

export function GeneratedPasswordPreview({
  password,
}: GeneratedPasswordPreviewProps) {
  return (
    <div className="space-y-2">
      <Label>生成的密码</Label>
      <div className="flex gap-2">
        <div className="flex-1 rounded-md border px-3 py-2 font-mono text-sm break-all">
          {password || '...'}
        </div>
        <CopyButton
          text={password}
          variant="outline"
          size="icon"
          successMessage="密码已复制到剪贴板"
          title="复制密码"
        />
      </div>
    </div>
  )
}