import { createContext, useState, useCallback } from 'react'
import type { ReactNode } from 'react'

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
export const VaultContext = createContext<VaultContextType | undefined>(undefined)

export interface VaultProviderProps {
  children: ReactNode
}

/**
 * Vault Provider 组件
 */
export const VaultProvider = ({ children }: VaultProviderProps) => {
  const [isUnlocked, setIsUnlocked] = useState(false)
  const [vaultSessionId, setVaultSessionId] = useState<string | null>(null)

  const unlock = useCallback((sessionId: string) => {
    setVaultSessionId(sessionId)
    setIsUnlocked(true)
  }, [])

  const lock = useCallback(() => {
    setVaultSessionId(null)
    setIsUnlocked(false)
  }, [])

  const value: VaultContextType = {
    isUnlocked,
    vaultSessionId,
    unlock,
    lock,
  }

  return <VaultContext.Provider value={value}>{children}</VaultContext.Provider>
}
