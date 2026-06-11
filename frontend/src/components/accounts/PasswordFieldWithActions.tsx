import { Eye, EyeOff, Settings2, Zap } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { CopyButton } from '@/components/common/CopyButton'

interface PasswordFieldWithActionsProps {
  id: string
  value: string
  showPassword: boolean
  disabled?: boolean
  isGenerating?: boolean
  onChange: (value: string) => void
  onToggleVisibility: () => void
  onQuickGenerate: () => void
  onOpenGenerator: () => void
}

export function PasswordFieldWithActions({
  id,
  value,
  showPassword,
  disabled = false,
  isGenerating = false,
  onChange,
  onToggleVisibility,
  onQuickGenerate,
  onOpenGenerator,
}: PasswordFieldWithActionsProps) {
  return (
    <div className="grid gap-2">
      <Label htmlFor={id}>
        密码 <span className="text-red-500">*</span>
      </Label>
      <div className="flex gap-2 items-start">
        <div className="flex-1 min-w-0">
          <Input
            id={id}
            type={showPassword ? 'text' : 'password'}
            placeholder="密码"
            value={value}
            onChange={(event) => onChange(event.target.value)}
            disabled={disabled}
          />
        </div>
        <CopyButton
          text={value}
          successMessage="密码已复制到剪贴板"
          size="icon"
          variant="outline"
          title="复制密码"
          className="flex-shrink-0"
          disabled={!value || disabled}
        />
        <Button
          type="button"
          variant="outline"
          size="icon"
          onClick={onQuickGenerate}
          disabled={disabled || isGenerating}
          title="快速生成密码"
          className="flex-shrink-0"
        >
          <Zap className="h-4 w-4" />
        </Button>
        <Button
          type="button"
          variant="outline"
          size="icon"
          onClick={onOpenGenerator}
          disabled={disabled}
          title="高级生成"
          className="flex-shrink-0"
        >
          <Settings2 className="h-4 w-4" />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          onClick={onToggleVisibility}
          disabled={disabled}
          className="flex-shrink-0"
        >
          {showPassword ? (
            <EyeOff className="h-4 w-4" />
          ) : (
            <Eye className="h-4 w-4" />
          )}
        </Button>
      </div>
    </div>
  )
}