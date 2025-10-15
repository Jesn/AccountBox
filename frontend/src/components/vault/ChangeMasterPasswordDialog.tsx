import { useState } from 'react'
import { vaultService } from '@/services/vaultService'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Alert, AlertDescription } from '@/components/ui/alert'

interface ChangeMasterPasswordDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess?: () => void
}

/**
 * 修改主密码对话框
 */
export function ChangeMasterPasswordDialog({
  open,
  onOpenChange,
  onSuccess,
}: ChangeMasterPasswordDialogProps) {
  const [oldPassword, setOldPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const handleClose = () => {
    setOldPassword('')
    setNewPassword('')
    setConfirmPassword('')
    setError(null)
    setSuccess(false)
    onOpenChange(false)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    // 客户端验证
    if (!oldPassword) {
      setError('请输入旧主密码')
      return
    }

    if (!newPassword) {
      setError('请输入新主密码')
      return
    }

    if (newPassword.length < 8) {
      setError('新主密码长度至少为 8 位')
      return
    }

    if (newPassword !== confirmPassword) {
      setError('两次输入的新密码不一致')
      return
    }

    if (oldPassword === newPassword) {
      setError('新密码不能与旧密码相同')
      return
    }

    setIsLoading(true)

    try {
      const response = await vaultService.changePassword({
        oldMasterPassword: oldPassword,
        newMasterPassword: newPassword,
      })

      if (response.success) {
        setSuccess(true)
        setTimeout(() => {
          handleClose()
          onSuccess?.()
        }, 1500)
      } else {
        const errorCode = response.error?.errorCode
        const errorMessage = response.error?.message

        if (errorCode === 'AUTHENTICATION_FAILED') {
          setError('旧主密码错误')
        } else {
          setError(errorMessage || '修改失败，请重试')
        }
      }
    } catch (err) {
      setError('网络错误，请稍后重试')
      console.error('Change password error:', err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>修改主密码</DialogTitle>
          <DialogDescription>
            修改主密码后，所有会话将被清除，需要重新登录
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <Alert variant="destructive">
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          {success && (
            <Alert>
              <AlertDescription>主密码修改成功！正在关闭...</AlertDescription>
            </Alert>
          )}

          <div className="space-y-2">
            <Label htmlFor="oldPassword">旧主密码</Label>
            <Input
              id="oldPassword"
              type="password"
              placeholder="输入旧主密码"
              value={oldPassword}
              onChange={(e) => setOldPassword(e.target.value)}
              disabled={isLoading || success}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="newPassword">新主密码</Label>
            <Input
              id="newPassword"
              type="password"
              placeholder="至少 8 位字符"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              disabled={isLoading || success}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirmPassword">确认新密码</Label>
            <Input
              id="confirmPassword"
              type="password"
              placeholder="再次输入新主密码"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              disabled={isLoading || success}
              required
            />
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              disabled={isLoading || success}
            >
              取消
            </Button>
            <Button type="submit" disabled={isLoading || success}>
              {isLoading ? '正在修改...' : '确认修改'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
