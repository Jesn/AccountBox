import { useState } from 'react'
import { accountService } from '@/services/accountService'
import type { CreateAccountRequest } from '@/types'
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
import { PasswordGeneratorDialog } from '@/components/password-generator/PasswordGeneratorDialog'
import { AccountForm } from '@/components/accounts/AccountForm'

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
  const [isGeneratingPassword, setIsGeneratingPassword] = useState(false)

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
              <DialogTitle>添加账号</DialogTitle>
              <DialogDescription>为该网站添加新的账号信息</DialogDescription>
            </DialogHeader>

            <AccountForm
              username={username}
              password={password}
              showPassword={showPassword}
              notes={notes}
              tags={tags}
              extendedData={extendedData}
              error={error}
              disabled={isSubmitting}
              isGeneratingPassword={isGeneratingPassword}
              autoFocus
              onUsernameChange={setUsername}
              onPasswordChange={setPassword}
              onShowPasswordChange={setShowPassword}
              onNotesChange={setNotes}
              onTagsChange={setTags}
              onExtendedDataChange={setExtendedData}
              onQuickGeneratePassword={handleQuickGenerate}
              onOpenPasswordGenerator={() => setShowPasswordGenerator(true)}
            />

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
