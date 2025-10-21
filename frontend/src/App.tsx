import { lazy, Suspense } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Toaster } from '@/components/ui/sonner'

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

function App() {
  return (
    <BrowserRouter>
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
      <Toaster />
    </BrowserRouter>
  )
}

export default App
