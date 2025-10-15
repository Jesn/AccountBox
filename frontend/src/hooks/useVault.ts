import { useContext } from 'react'
import { VaultContext } from '@/contexts/VaultContext'
import type { VaultContextType } from '@/contexts/VaultContext'

/**
 * useVault Hook
 * 封装对 VaultContext 的访问
 */
export const useVault = (): VaultContextType => {
  const context = useContext(VaultContext)
  if (!context) {
    throw new Error('useVault must be used within a VaultProvider')
  }
  return context
}

export default useVault
