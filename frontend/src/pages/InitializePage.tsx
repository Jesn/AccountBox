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
 * 初始化页面
 * 首次设置主密码
 */
export function InitializePage() {
  const [masterPassword, setMasterPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { unlock } = useVault()
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    // 客户端验证
    if (!masterPassword) {
      setError('请输入主密码')
      return
    }

    if (masterPassword.length < 8) {
      setError('主密码长度至少为 8 位')
      return
    }

    if (masterPassword !== confirmPassword) {
      setError('两次输入的密码不一致')
      return
    }

    setIsLoading(true)

    try {
      const response = await vaultService.initialize({ masterPassword })

      if (response.success && response.data) {
        // 初始化成功，设置会话并跳转到主页面
        unlock(response.data.sessionId)
        navigate('/websites')
      } else {
        setError(response.error?.message || '初始化失败，请重试')
      }
    } catch (err) {
      setError('网络错误，请检查后端服务是否启动')
      console.error('Initialize error:', err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-2xl">欢迎使用 AccountBox</CardTitle>
          <CardDescription>
            首次使用，请设置主密码
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
                placeholder="至少 8 位字符"
                value={masterPassword}
                onChange={(e) => setMasterPassword(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="confirmPassword">确认密码</Label>
              <Input
                id="confirmPassword"
                type="password"
                placeholder="再次输入主密码"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>

            <div className="rounded-md bg-yellow-50 p-4 text-sm text-yellow-800">
              <p className="font-medium">重要提示：</p>
              <ul className="mt-2 list-inside list-disc space-y-1">
                <li>主密码用于加密您的所有账号数据</li>
                <li>忘记主密码将无法恢复数据</li>
                <li>请妥善保管您的主密码</li>
              </ul>
            </div>

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
            >
              {isLoading ? '正在初始化...' : '设置主密码'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
