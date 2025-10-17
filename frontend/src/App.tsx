import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { WebsitesPage } from '@/pages/WebsitesPage'
import { AccountsPage } from '@/pages/AccountsPage'
import { RecycleBinPage } from '@/pages/RecycleBinPage'
import { SearchPage } from '@/pages/SearchPage'
import { ApiKeysPage } from '@/pages/ApiKeysPage'
import { ApiDocumentationPage } from '@/pages/ApiDocumentationPage'
import { Toaster } from '@/components/ui/sonner'

/**
 * 应用根组件
 *
 * 注意：系统已从加密存储切换为明文存储（2025-10-17架构变更）
 * - 不再需要主密码初始化和解锁流程
 * - 移除了 VaultContext、InitializePage、UnlockPage
 * - 应用启动后直接进入主界面
 */
function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/websites" element={<WebsitesPage />} />
        <Route
          path="/websites/:websiteId/accounts"
          element={<AccountsPage />}
        />
        <Route path="/recycle-bin" element={<RecycleBinPage />} />
        <Route path="/search" element={<SearchPage />} />
        <Route path="/api-keys" element={<ApiKeysPage />} />
        <Route path="/api-documentation" element={<ApiDocumentationPage />} />
        <Route path="/" element={<Navigate to="/websites" replace />} />
        <Route path="*" element={<Navigate to="/websites" replace />} />
      </Routes>
      <Toaster />
    </BrowserRouter>
  )
}

export default App
