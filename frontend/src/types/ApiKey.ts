/**
 * API密钥作用域类型
 */
export type ApiKeyScopeType = 'All' | 'Specific'

/**
 * API密钥接口
 */
export interface ApiKey {
  id: number
  name: string
  keyPlaintext: string
  scopeType: ApiKeyScopeType
  websiteIds: number[]
  createdAt: string
  lastUsedAt: string | null
}

/**
 * 创建API密钥请求
 */
export interface CreateApiKeyRequest {
  name: string
  scopeType: ApiKeyScopeType
  websiteIds?: number[]
}
