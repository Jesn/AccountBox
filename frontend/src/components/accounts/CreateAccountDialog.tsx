import { useState } from 'react'
import { accountService } from '@/services/accountService'
import type { CreateAccountRequest } from '@/services/accountService'
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
import { Eye, EyeOff, KeyRound } from 'lucide-react'

interface CreateAccountDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: () => void
  websiteId: number
}

/**
 * 创建账号对话框组件
 * 允许用户为指定网站添加新账号
 */
export function CreateAccountDialog({
  open,
  onOpenChange,
  onSuccess,
  websiteId,
}: CreateAccountDialogProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [notes, setNotes] = useState('')
  const [tags, setTags] = useState('')
  const [extendedData, setExtendedData] = useState<Record<string, unknown>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showPasswordGenerator, setShowPasswordGenerator] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

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
      const request: CreateAccountRequest = {
        websiteId,
        username: username.trim(),
        password: password.trim(),
        notes: notes.trim() || undefined,
        tags: tags.trim() || undefined,
        extendedData:
          Object.keys(extendedData).length > 0 ? extendedData : undefined,
      }

      const response = await accountService.create(request)

      if (response.success) {
        // 重置表单
        setUsername('')
        setPassword('')
        setShowPassword(false)
        setNotes('')
        setTags('')
        setExtendedData({})
        onOpenChange(false)
        onSuccess()
      } else {
        setError(response.error?.message || '创建失败')
      }
    } catch (err) {
      console.error('创建账号失败:', err)
      setError('创建账号时发生错误，请重试')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setUsername('')
    setPassword('')
    setShowPassword(false)
    setNotes('')
    setTags('')
    setExtendedData({})
    setError(null)
    onOpenChange(false)
  }

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="sm:max-w-[425px]">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>添加账号</DialogTitle>
              <DialogDescription>为该网站添加新的账号信息</DialogDescription>
            </DialogHeader>

            <div className="grid gap-4 py-4">
              <div className="grid gap-2">
                <Label htmlFor="username">
                  用户名 <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="username"
                  placeholder="用户名或邮箱"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  disabled={isSubmitting}
                  autoFocus
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="password">
                  密码 <span className="text-red-500">*</span>
                </Label>
                <div className="flex gap-2">
                  <Input
                    id="password"
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
                    onClick={() => setShowPasswordGenerator(true)}
                    disabled={isSubmitting}
                    title="生成密码"
                  >
                    <KeyRound className="h-4 w-4" />
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
                <Label htmlFor="notes">备注（可选）</Label>
                <Textarea
                  id="notes"
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
                <Label htmlFor="tags">标签（可选）</Label>
                <Input
                  id="tags"
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
                {isSubmitting ? '创建中...' : '创建'}
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
