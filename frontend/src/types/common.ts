/**
 * 通用类型定义
 */

// API 响应格式
export interface ErrorResponse {
  errorCode: string
  message: string
  code?: string // 添加 code 字段以支持某些错误场景
  details?: unknown
}

export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: ErrorResponse
  timestamp: string
}

// 分页响应
export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

// PagedResult 是 PagedResponse 的别名（兼容后端命名）
export type PagedResult<T> = PagedResponse<T>

// Website 实体
export interface Website {
  id: number
  domain: string
  displayName: string
  tags?: string
  createdAt: string
  updatedAt: string
}

// Account 实体
export interface Account {
  id: number
  websiteId: number
  username: string
  password: string // 前端显示的解密后密码
  notes?: string // 前端显示的解密后备注
  tags?: string
  isDeleted: boolean
  deletedAt?: string
  createdAt: string
  updatedAt: string
}

// Vault 相关
export interface VaultStatus {
  isInitialized: boolean
  isUnlocked: boolean
}

export interface VaultSession {
  sessionId: string
  expiresAt: string
}
