import { useState, useEffect } from 'react'
import { accountService } from '@/services/accountService'
import type {
  UpdateAccountRequest,
  AccountResponse,
} from '@/services/accountService'
import { passwordGeneratorService } from '@/services/passwordGeneratorService'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { PasswordGeneratorDialog } from '@/components/password-generator/PasswordGeneratorDialog'
import { ExtendedFieldsEditor } from '@/components/accounts/ExtendedFieldsEditor'
import { Eye, EyeOff, Zap, Settings2 } from 'lucide-react'

interface EditAccountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  account: AccountResponse | null
}

/**
 * 编辑账号对话框组件
 * 允许用户修改账号信息
 */
export function EditAccountDialog({
  open,
  onOpenChange,
  onSuccess,
  account,
}: EditAccountDialogProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [notes, setNotes] = useState('')
  const [tags, setTags] = useState('')
  const [extendedData, setExtendedData] = useState<Record<string, unknown>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showPasswordGenerator, setShowPasswordGenerator] = useState(false)
  const [isGeneratingPassword, setIsGeneratingPassword] = useState(false)

  // 当对话框打开或账号数据变化时，更新表单
  useEffect(() => {
    if (account) {
      setUsername(account.username)
      setPassword(account.password)
      setNotes(account.notes || '')
      setTags(account.tags || '')
      setExtendedData(account.extendedData || {})
    }
  }, [account])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!account) {
      setError('账号数据不存在')
      return
    }

    // 基础验证
    if (!username.trim()) {
      setError('用户名不能为空')
      return
    }

    if (!password.trim()) {
      setError('密码不能为空')
      return
    }

    setIsSubmitting(true)
    try {
      const request: UpdateAccountRequest = {
        username: username.trim(),
        password: password.trim(),
        notes: notes.trim() || undefined,
        tags: tags.trim() || undefined,
        extendedData:
          Object.keys(extendedData).length > 0 ? extendedData : undefined,
      }

      const response = await accountService.update(account.id, request)

      if (response.success) {
        setShowPassword(false)
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '更新失败')
      }
    } catch (err) {
      console.error('更新账号失败:', err)
      setError('更新账号时发生错误，请重试')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setShowPassword(false)
    setError(null)
    onOpenChange(false)
  }

  // 快速生成密码（使用默认配置）
  const handleQuickGenerate = async () => {
    setIsGeneratingPassword(true)
    try {
      const response = await passwordGeneratorService.generateQuick()
      if (response.success && response.data) {
        setPassword(response.data.password)
        setShowPassword(true) // 自动显示生成的密码
      }
    } catch (error) {
      console.error('生成密码失败:', error)
      setError('生成密码失败，请重试')
    } finally {
      setIsGeneratingPassword(false)
    }
  }

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="w-[95vw] sm:w-full sm:max-w-[600px]">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>编辑账号</DialogTitle>
              <DialogDescription>
                修改账号的用户名、密码和其他信息
              </DialogDescription>
            </DialogHeader>

            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label htmlFor="edit-username">
                  用户名 <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="edit-username"
                  placeholder="用户名或邮箱"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  disabled={isSubmitting}
                  autoFocus
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="edit-password">
                  密码 <span className="text-red-500">*</span>
                </Label>
                <div className="flex gap-2">
                  <Input
                    id="edit-password"
                    type={showPassword ? 'text' : 'password'}
                    placeholder="密码"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={isSubmitting}
                    className="flex-1"
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    onClick={handleQuickGenerate}
                    disabled={isSubmitting || isGeneratingPassword}
                    title="快速生成密码"
                  >
                    <Zap className="h-4 w-4" />
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    onClick={() => setShowPasswordGenerator(true)}
                    disabled={isSubmitting}
                    title="高级生成"
                  >
                    <Settings2 className="h-4 w-4" />
                  </Button>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    onClick={() => setShowPassword(!showPassword)}
                    disabled={isSubmitting}
                  >
                    {showPassword ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="edit-notes">备注（可选）</Label>
                <Textarea
                  id="edit-notes"
                  placeholder="添加备注信息..."
                  value={notes}
                  onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                    setNotes(e.target.value)
                  }
                  disabled={isSubmitting}
                  rows={3}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="edit-tags">标签（可选）</Label>
                <Input
                  id="edit-tags"
                  placeholder="重要, 工作"
                  value={tags}
                  onChange={(e) => setTags(e.target.value)}
                  disabled={isSubmitting}
                />
              </div>

              <div className="border-t pt-4">
                <ExtendedFieldsEditor
                  value={extendedData}
                  onChange={setExtendedData}
                  maxSizeKB={10}
                />
              </div>

              {error && (
                <div className="rounded-md bg-red-50 p-3 text-sm text-red-800">
                  {error}
                </div>
              )}
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={handleCancel}
                disabled={isSubmitting}
              >
                取消
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? '保存中...' : '保存'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <PasswordGeneratorDialog
        open={showPasswordGenerator}
        onOpenChange={setShowPasswordGenerator}
        onAccept={(generatedPassword) => {
          setPassword(generatedPassword)
          setShowPassword(true) // 自动显示生成的密码
        }}
      />
    </>
  )
}
