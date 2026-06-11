import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { InlineError } from '@/components/common/InlineError'

interface WebsiteFormProps {
  idPrefix?: string
  domain: string
  displayName: string
  tags: string
  error?: string | null
  disabled?: boolean
  autoFocus?: boolean
  onDomainChange: (value: string) => void
  onDisplayNameChange: (value: string) => void
  onTagsChange: (value: string) => void
}

export function WebsiteForm({
  idPrefix = 'website',
  domain,
  displayName,
  tags,
  error,
  disabled = false,
  autoFocus = false,
  onDomainChange,
  onDisplayNameChange,
  onTagsChange,
}: WebsiteFormProps) {
  const domainId = `${idPrefix}-domain`
  const displayNameId = `${idPrefix}-displayName`
  const tagsId = `${idPrefix}-tags`

  return (
    <div className="grid gap-4 py-4">
      <div className="grid gap-2">
        <Label htmlFor={domainId}>
          域名 <span className="text-red-500">*</span>
        </Label>
        <Input
          id={domainId}
          placeholder="example.com"
          value={domain}
          onChange={(event) => onDomainChange(event.target.value)}
          disabled={disabled}
          autoFocus={autoFocus}
        />
      </div>

      <div className="grid gap-2">
        <Label htmlFor={displayNameId}>显示名称（可选）</Label>
        <Input
          id={displayNameId}
          placeholder="示例网站"
          value={displayName}
          onChange={(event) => onDisplayNameChange(event.target.value)}
          disabled={disabled}
        />
      </div>

      <div className="grid gap-2">
        <Label htmlFor={tagsId}>标签（可选）</Label>
        <Textarea
          id={tagsId}
          placeholder="工作, 重要"
          value={tags}
          onChange={(event) => onTagsChange(event.target.value)}
          disabled={disabled}
          rows={3}
        />
      </div>

      <InlineError>{error}</InlineError>
    </div>
  )
}