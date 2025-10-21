import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

/**
 * 错误测试组件
 *
 * 用于测试 ErrorBoundary 是否正常工作
 * 仅在开发环境使用
 *
 * 使用方法：
 * 1. 在任意页面导入此组件
 * 2. 点击按钮触发不同类型的错误
 * 3. 观察 ErrorBoundary 是否正确捕获并显示错误页面
 */
export function ErrorTest() {
  const [shouldThrow, setShouldThrow] = useState(false)

  // 测试：渲染错误
  if (shouldThrow) {
    throw new Error('这是一个测试错误：组件渲染时抛出')
  }

  // 测试：异步错误（不会被 ErrorBoundary 捕获）
  const handleAsyncError = async () => {
    try {
      await Promise.reject(new Error('这是一个异步错误'))
    } catch (error) {
      console.error('异步错误（不会被 ErrorBoundary 捕获）:', error)
      alert('异步错误不会被 ErrorBoundary 捕获，请查看控制台')
    }
  }

  // 测试：事件处理器错误（不会被 ErrorBoundary 捕获）
  const handleEventError = () => {
    try {
      throw new Error('这是一个事件处理器错误')
    } catch (error) {
      console.error('事件处理器错误（不会被 ErrorBoundary 捕获）:', error)
      alert('事件处理器错误不会被 ErrorBoundary 捕获，请查看控制台')
    }
  }

  return (
    <Card className="max-w-2xl mx-auto mt-8">
      <CardHeader>
        <CardTitle>ErrorBoundary 测试工具</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
          <p className="text-sm text-yellow-800">
            ⚠️ 此组件仅用于开发环境测试 ErrorBoundary 功能
          </p>
        </div>

        <div className="space-y-3">
          <div>
            <h3 className="font-semibold mb-2">
              1. 测试渲染错误（会被 ErrorBoundary 捕获）
            </h3>
            <Button
              variant="destructive"
              onClick={() => setShouldThrow(true)}
            >
              触发渲染错误
            </Button>
            <p className="text-xs text-gray-500 mt-1">
              点击后组件会抛出错误，ErrorBoundary 会捕获并显示错误页面
            </p>
          </div>

          <div>
            <h3 className="font-semibold mb-2">
              2. 测试异步错误（不会被 ErrorBoundary 捕获）
            </h3>
            <Button
              variant="outline"
              onClick={handleAsyncError}
            >
              触发异步错误
            </Button>
            <p className="text-xs text-gray-500 mt-1">
              异步错误需要使用 try-catch 手动捕获
            </p>
          </div>

          <div>
            <h3 className="font-semibold mb-2">
              3. 测试事件处理器错误（不会被 ErrorBoundary 捕获）
            </h3>
            <Button
              variant="outline"
              onClick={handleEventError}
            >
              触发事件错误
            </Button>
            <p className="text-xs text-gray-500 mt-1">
              事件处理器中的错误需要使用 try-catch 手动捕获
            </p>
          </div>
        </div>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mt-4">
          <h4 className="font-semibold text-sm text-blue-900 mb-2">
            ErrorBoundary 能捕获的错误：
          </h4>
          <ul className="text-xs text-blue-800 space-y-1">
            <li>✅ 组件渲染过程中的错误</li>
            <li>✅ 生命周期方法中的错误</li>
            <li>✅ 子组件树中的错误</li>
          </ul>

          <h4 className="font-semibold text-sm text-blue-900 mt-3 mb-2">
            ErrorBoundary 不能捕获的错误：
          </h4>
          <ul className="text-xs text-blue-800 space-y-1">
            <li>❌ 事件处理器中的错误（使用 try-catch）</li>
            <li>❌ 异步代码中的错误（使用 try-catch）</li>
            <li>❌ 服务端渲染的错误</li>
            <li>❌ ErrorBoundary 自身的错误</li>
          </ul>
        </div>
      </CardContent>
    </Card>
  )
}
