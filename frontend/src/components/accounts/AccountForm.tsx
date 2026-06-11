import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { ExtendedFieldsEditor } from '@/components/accounts/ExtendedFieldsEditor'
import { PasswordFieldWithActions } from '@/components/accounts/PasswordFieldWithActions'
import { InlineError } from '@/components/common/InlineError'

interface AccountFormProps {
  idPrefix?: string
  username: string
  password: string
  showPassword: boolean
  notes: string
  tags: string
  extendedData: Record<string, unknown>
  error?: string | null
  disabled?: boolean
  isGeneratingPassword?: boolean
  autoFocus?: boolean
  onUsernameChange: (value: string) => void
  onPasswordChange: (value: string) => void
  onShowPasswordChange: (value: boolean) => void
  onNotesChange: (value: string) => void
  onTagsChange: (value: string) => void
  onExtendedDataChange: (value: Record<string, unknown>) => void
  onQuickGeneratePassword: () => void
  onOpenPasswordGenerator: () => void
}

export function AccountForm({
  idPrefix = 'account',
  username,
  password,
  showPassword,
  notes,
  tags,
  extendedData,
  error,
  disabled = false,
  isGeneratingPassword = false,
  autoFocus = false,
  onUsernameChange,
  onPasswordChange,
  onShowPasswordChange,
  onNotesChange,
  onTagsChange,
  onExtendedDataChange,
  onQuickGeneratePassword,
  onOpenPasswordGenerator,
}: AccountFormProps) {
  const usernameId = `${idPrefix}-username`
  const passwordId = `${idPrefix}-password`
  const notesId = `${idPrefix}-notes`
  const tagsId = `${idPrefix}-tags`

  return (
    <div className="grid gap-4 py-4">
      <div className="grid gap-2">
        <Label htmlFor={usernameId}>
          用户名 <span className="text-red-500">*</span>
        </Label>
        <Input
          id={usernameId}
          placeholder="用户名或邮箱"
          value={username}
          onChange={(event) => onUsernameChange(event.target.value)}
          disabled={disabled}
          autoFocus={autoFocus}
        />
      </div>

      <PasswordFieldWithActions
        id={passwordId}
        value={password}
        showPassword={showPassword}
        disabled={disabled}
        isGenerating={isGeneratingPassword}
        onChange={onPasswordChange}
        onToggleVisibility={() => onShowPasswordChange(!showPassword)}
        onQuickGenerate={onQuickGeneratePassword}
        onOpenGenerator={onOpenPasswordGenerator}
      />

      <div className="grid gap-2">
        <Label htmlFor={notesId}>备注（可选）</Label>
        <Textarea
          id={notesId}
          placeholder="添加备注信息..."
          value={notes}
          onChange={(event) => onNotesChange(event.target.value)}
          disabled={disabled}
          rows={3}
        />
      </div>

      <div className="grid gap-2">
        <Label htmlFor={tagsId}>标签（可选）</Label>
        <Input
          id={tagsId}
          placeholder="重要, 工作"
          value={tags}
          onChange={(event) => onTagsChange(event.target.value)}
          disabled={disabled}
        />
      </div>

      <div className="border-t pt-4">
        <ExtendedFieldsEditor
          value={extendedData}
          onChange={onExtendedDataChange}
          maxSizeKB={10}
        />
      </div>

      <InlineError>{error}</InlineError>
    </div>
  )
}