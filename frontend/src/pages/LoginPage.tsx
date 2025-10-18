import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { authService } from '@/services/authService'
import type { ErrorResponse } from '@/types/common'

/**
 * 登录页面组件
 */
export default function LoginPage() {
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()

  /**
   * 处理登录表单提交
   */
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    // 清除之前的错误
    setError(null)

    // 验证密码不为空
    if (!password.trim()) {
      setError('请输入主密码')
      return
    }

    setIsLoading(true)

    try {
      // 调用登录服务
      await authService.login(password)

      // 登录成功，跳转到首页（或原始请求的页面）
      const returnUrl = new URLSearchParams(window.location.search).get('returnUrl') || '/'
      navigate(returnUrl)
    } catch (err) {
      // 处理登录错误
      const errorResponse = err as ErrorResponse

      if (errorResponse.errorCode === 'PASSWORD_INCORRECT') {
        setError('主密码错误，请重试')
      } else if (errorResponse.errorCode === 'NETWORK_ERROR') {
        setError('网络连接失败，请检查网络')
      } else if (errorResponse.errorCode === 'UNAUTHORIZED') {
        setError('认证失败，请重新登录')
      } else {
        setError(errorResponse.message || '登录失败，请稍后重试')
      }
    } finally {
      setIsLoading(false)
    }
  }

  /**
   * 处理Enter键提交
   */
  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !isLoading) {
      handleSubmit(e as any)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 sm:px-6 lg:px-8">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold">AccountBox</CardTitle>
          <CardDescription>请输入主密码以访问您的账号库</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Input
                id="password"
                type="password"
                placeholder="请输入主密码"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                onKeyPress={handleKeyPress}
                disabled={isLoading}
                autoFocus
                className={error ? 'border-red-500' : ''}
              />
              {error && (
                <p className="text-sm text-red-500">{error}</p>
              )}
            </div>

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
            >
              {isLoading ? '登录中...' : '登录'}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
