import { apiClient } from './apiClient'
import type { ApiResponse } from '@/types/common'

/**
 * Vault 状态响应
 */
export interface VaultStatusResponse {
  isInitialized: boolean
  isUnlocked: boolean
}

/**
 * 初始化 Vault 请求
 */
export interface InitializeVaultRequest {
  masterPassword: string
}

/**
 * 解锁 Vault 请求
 */
export interface UnlockVaultRequest {
  masterPassword: string
}

/**
 * 修改主密码请求
 */
export interface ChangeMasterPasswordRequest {
  oldMasterPassword: string
  newMasterPassword: string
}

/**
 * Vault 会话响应
 */
export interface VaultSessionResponse {
  sessionId: string
  expiresAt: string
}

/**
 * Vault 服务 API 客户端
 */
class VaultService {
  private readonly baseUrl = '/api/vault'

  /**
   * 获取 Vault 状态
   */
  async getStatus(): Promise<ApiResponse<VaultStatusResponse>> {
    return await apiClient.get<VaultStatusResponse>(`${this.baseUrl}/status`)
  }

  /**
   * 初始化 Vault（首次设置主密码）
   */
  async initialize(
    request: InitializeVaultRequest
  ): Promise<ApiResponse<VaultSessionResponse>> {
    return await apiClient.post<VaultSessionResponse>(
      `${this.baseUrl}/initialize`,
      request
    )
  }

  /**
   * 解锁 Vault
   */
  async unlock(
    request: UnlockVaultRequest
  ): Promise<ApiResponse<VaultSessionResponse>> {
    return await apiClient.post<VaultSessionResponse>(
      `${this.baseUrl}/unlock`,
      request
    )
  }

  /**
   * 锁定 Vault
   */
  async lock(): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.post<{ message: string }>(`${this.baseUrl}/lock`)
  }

  /**
   * 修改主密码
   */
  async changePassword(
    request: ChangeMasterPasswordRequest
  ): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.post<{ message: string }>(
      `${this.baseUrl}/change-password`,
      request
    )
  }
}

export const vaultService = new VaultService()
