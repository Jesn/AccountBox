import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { WebsitesPage } from '@/pages/WebsitesPage'
import { AccountsPage } from '@/pages/AccountsPage'
import { RecycleBinPage } from '@/pages/RecycleBinPage'
import { SearchPage } from '@/pages/SearchPage'
import { ApiKeysPage } from '@/pages/ApiKeysPage'
import { ApiDocumentationPage } from '@/pages/ApiDocumentationPage'
import LoginPage from '@/pages/LoginPage'
import ProtectedRoute from '@/components/auth/ProtectedRoute'
import { Toaster } from '@/components/ui/sonner'

/**
 * 应用根组件
 *
 * 注意：系统已添加JWT身份认证（2025-10-17）
 * - 未登录用户将被重定向到 /login
 * - 所有业务路由都受ProtectedRoute保护
 * - Token存储在localStorage，支持持久化登录
 */
function App() {
  return (
    <BrowserRouter>
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
      <Toaster />
    </BrowserRouter>
  )
}

export default App
