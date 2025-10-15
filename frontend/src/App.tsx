import { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { VaultProvider } from '@/contexts/VaultContext'
import { vaultService } from '@/services/vaultService'
import { InitializePage } from '@/pages/InitializePage'
import { UnlockPage } from '@/pages/UnlockPage'
import { WebsitesPage } from '@/pages/WebsitesPage'
import { useVault } from '@/hooks/useVault'

/**
 * 应用启动流程路由守卫
 */
function AppRouter() {
  const [isLoading, setIsLoading] = useState(true)
  const [isInitialized, setIsInitialized] = useState(false)
  const { isUnlocked } = useVault()

  useEffect(() => {
    // 检查 Vault 状态
    const checkStatus = async () => {
      try {
        const response = await vaultService.getStatus()
        if (response.success && response.data) {
          setIsInitialized(response.data.isInitialized)
        }
      } catch (err) {
        console.error('Failed to check vault status:', err)
      } finally {
        setIsLoading(false)
      }
    }

    checkStatus()
  }, [])

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="mb-4 h-12 w-12 animate-spin rounded-full border-4 border-gray-300 border-t-blue-600 mx-auto"></div>
          <p className="text-gray-600">正在加载...</p>
        </div>
      </div>
    )
  }

  return (
    <Routes>
      {/* 未初始化：显示初始化页面 */}
      {!isInitialized && (
        <>
          <Route path="/initialize" element={<InitializePage />} />
          <Route path="*" element={<Navigate to="/initialize" replace />} />
        </>
      )}

      {/* 已初始化但未解锁：显示解锁页面 */}
      {isInitialized && !isUnlocked && (
        <>
          <Route path="/unlock" element={<UnlockPage />} />
          <Route path="*" element={<Navigate to="/unlock" replace />} />
        </>
      )}

      {/* 已解锁：显示主应用 */}
      {isInitialized && isUnlocked && (
        <>
          <Route path="/websites" element={<WebsitesPage />} />
          <Route path="/" element={<Navigate to="/websites" replace />} />
          <Route path="*" element={<Navigate to="/websites" replace />} />
        </>
      )}
    </Routes>
  )
}

/**
 * 应用根组件
 */
function App() {
  return (
    <VaultProvider>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </VaultProvider>
  )
}

export default App
