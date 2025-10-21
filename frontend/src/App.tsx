import { lazy, Suspense, useEffect } from 'react'
import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Toaster } from '@/components/ui/sonner'
import { toast } from 'sonner'
import { eventBus, AUTH_EVENTS } from '@/lib/eventBus'
import { ErrorBoundary } from '@/components/error/ErrorBoundary'
import { ErrorPage } from '@/components/error/ErrorPage'

// 懒加载页面组件
const WebsitesPage = lazy(() => import('@/pages/WebsitesPage').then(m => ({ default: m.WebsitesPage })))
const AccountsPage = lazy(() => import('@/pages/AccountsPage').then(m => ({ default: m.AccountsPage })))
const RecycleBinPage = lazy(() => import('@/pages/RecycleBinPage').then(m => ({ default: m.RecycleBinPage })))
const SearchPage = lazy(() => import('@/pages/SearchPage').then(m => ({ default: m.SearchPage })))
const ApiKeysPage = lazy(() => import('@/pages/ApiKeysPage').then(m => ({ default: m.ApiKeysPage })))
const ApiDocumentationPage = lazy(() => import('@/pages/ApiDocumentationPage').then(m => ({ default: m.ApiDocumentationPage })))
const LoginPage = lazy(() => import('@/pages/LoginPage'))

/**
 * 应用根组件
 *
 * 注意：系统已添加JWT身份认证（2025-10-17）
 * - 未登录用户将被重定向到 /login
 * - 所有业务路由都受ProtectedRoute保护
 * - Token存储在localStorage，支持持久化登录
 */
/**
 * 加载中组件
 */
function LoadingFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]" />
        <p className="mt-4 text-gray-600">加载中...</p>
      </div>
    </div>
  )
}

/**
 * 路由内部组件 - 用于访问 useNavigate
 */
function AppRoutes() {
  const navigate = useNavigate()

  // 监听未授权事件
  useEffect(() => {
    const unsubscribe = eventBus.on(AUTH_EVENTS.UNAUTHORIZED, () => {
      // 显示提示
      toast.error('登录已过期，请重新登录')

      // 导航到登录页
      navigate('/login', { replace: true })
    })

    // 清理订阅
    return unsubscribe
  }, [navigate])

  return (
    <Suspense fallback={<LoadingFallback />}>
      <Routes>
          {/* 公开路由 - 登录页面 */}
          <Route path="/login" element={<LoginPage />} />

        {/* 受保护路由 - 需要登录 */}
        <Route
          path="/websites"
          element={
            <ProtectedRoute>
              <WebsitesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/websites/:websiteId/accounts"
          element={
            <ProtectedRoute>
              <AccountsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/recycle-bin"
          element={
            <ProtectedRoute>
              <RecycleBinPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/search"
          element={
            <ProtectedRoute>
              <SearchPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/api-keys"
          element={
            <ProtectedRoute>
              <ApiKeysPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/api-documentation"
          element={
            <ProtectedRoute>
              <ApiDocumentationPage />
            </ProtectedRoute>
          }
        />

        {/* 默认路由 */}
        <Route path="/" element={<Navigate to="/websites" replace />} />
        <Route path="*" element={<Navigate to="/websites" replace />} />
      </Routes>
    </Suspense>
  )
}

function App() {
  return (
    <ErrorBoundary
      fallback={<ErrorPage />}
      onError={(error, errorInfo) => {
        // 这里可以添加错误上报逻辑
        // 例如：发送到 Sentry、LogRocket 等错误监控服务
        console.error('应用错误:', error)
        console.error('错误信息:', errorInfo)
      }}
    >
      <BrowserRouter>
        <AppRoutes />
        <Toaster />
      </BrowserRouter>
    </ErrorBoundary>
  )
}

export default App
