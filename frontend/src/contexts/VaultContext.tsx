import { createContext, useState, useCallback, useEffect } from 'react'
import type { ReactNode } from 'react'
import { apiClient } from '@/services/apiClient'

/**
 * Vault Context 接口
 */
export interface VaultContextType {
  isUnlocked: boolean
  vaultSessionId: string | null
  unlock: (sessionId: string) => void
  lock: () => void
}

/**
 * Vault Context
 * 管理应用的加密状态和会话
 */
export const VaultContext = createContext<VaultContextType | undefined>(
  undefined
)

export interface VaultProviderProps {
  children: ReactNode
}

/**
 * Vault Provider 组件
 */
export const VaultProvider = ({ children }: VaultProviderProps) => {
  // 从 localStorage 恢复 session
  const [isUnlocked, setIsUnlocked] = useState(() => {
    const savedSession = localStorage.getItem('vaultSessionId')
    return !!savedSession
  })
  const [vaultSessionId, setVaultSessionId] = useState<string | null>(() => {
    return localStorage.getItem('vaultSessionId')
  })

  const lock = useCallback(() => {
    setVaultSessionId(null)
    setIsUnlocked(false)
    // 清除 API 客户端的会话 ID
    apiClient.setVaultSession(null)
    // 清除 localStorage
    localStorage.removeItem('vaultSessionId')
  }, [])

  // 组件挂载时恢复 session 到 apiClient 并设置 401 回调
  useEffect(() => {
    const savedSession = localStorage.getItem('vaultSessionId')
    if (savedSession) {
      apiClient.setVaultSession(savedSession)
    }

    // 设置 401 未授权回调
    apiClient.setOnUnauthorized(() => {
      console.log('Session expired or invalid, redirecting to unlock page...')
      // 清空缓存
      lock()
      // 跳转到解锁页面
      window.location.href = '/unlock'
    })

    // 清理函数
    return () => {
      apiClient.setOnUnauthorized(null)
    }
  }, [lock])

  const unlock = useCallback((sessionId: string) => {
    setVaultSessionId(sessionId)
    setIsUnlocked(true)
    // 设置 API 客户端的会话 ID
    apiClient.setVaultSession(sessionId)
    // 持久化到 localStorage
    localStorage.setItem('vaultSessionId', sessionId)
  }, [])

  const value: VaultContextType = {
    isUnlocked,
    vaultSessionId,
    unlock,
    lock,
  }

  return <VaultContext.Provider value={value}>{children}</VaultContext.Provider>
}
