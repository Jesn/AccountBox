import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { vaultService } from '@/services/vaultService'
import { useVault } from '@/hooks/useVault'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'

/**
 * 解锁页面
 * 输入主密码解锁应用
 */
export function UnlockPage() {
  const [masterPassword, setMasterPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { unlock } = useVault()
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!masterPassword) {
      setError('请输入主密码')
      return
    }

    setIsLoading(true)

    try {
      const response = await vaultService.unlock({ masterPassword })

      if (response.success && response.data) {
        // 解锁成功，设置会话并跳转到主页面
        unlock(response.data.sessionId)
        navigate('/websites')
      } else {
        // 解锁失败
        const errorCode = response.error?.errorCode
        const errorMessage = response.error?.message

        if (errorCode === 'AUTHENTICATION_FAILED') {
          setError('主密码错误，请重试')
        } else {
          setError(errorMessage || '解锁失败，请重试')
        }
      }
    } catch (err) {
      setError('网络错误，请检查后端服务是否启动')
      console.error('Unlock error:', err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">AccountBox 已锁定</CardTitle>
          <CardDescription>
            请输入主密码以解锁
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
              <Alert variant="destructive">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            <div className="space-y-2">
              <Label htmlFor="masterPassword">主密码</Label>
              <Input
                id="masterPassword"
                type="password"
                placeholder="输入您的主密码"
                value={masterPassword}
                onChange={(e) => setMasterPassword(e.target.value)}
                disabled={isLoading}
                autoFocus
                required
              />
            </div>

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
            >
              {isLoading ? '正在解锁...' : '解锁'}
            </Button>

            <div className="text-center text-sm text-gray-600">
              <p>忘记密码？数据无法恢复</p>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
