import { useState, useEffect } from 'react'
import { accountService } from '@/services/accountService'
import type {
  UpdateAccountRequest,
  AccountResponse,
} from '@/types'
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
import { ConfirmGeneratePasswordDialog } from '@/components/accounts/ConfirmGeneratePasswordDialog'

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
  const [showConfirmGenerate, setShowConfirmGenerate] = useState(false)

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
    // 如果已有密码，需要二次确认
    if (password.trim()) {
      setShowConfirmGenerate(true)
      return
    }

    // 密码为空，直接生成
    await generatePassword()
  }

  // 执行密码生成
  const generatePassword = async () => {
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

            <AccountForm
              idPrefix="edit-account"
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

      <ConfirmGeneratePasswordDialog
        open={showConfirmGenerate}
        onOpenChange={setShowConfirmGenerate}
        onConfirm={generatePassword}
      />
    </>
  )
}
