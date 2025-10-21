/**
 * 通用类型定义
 */

/**
 * API 错误响应
 */
export interface ErrorResponse {
  errorCode: string
  message: string
  code?: string // 添加 code 字段以支持某些错误场景
  details?: unknown
}

/**
 * API 统一响应格式
 */
export interface ApiResponse<T> {
  success: boolean
  data?: T
  error?: ErrorResponse
  timestamp: string
}

/**
 * 分页响应
 */
export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

/**
 * PagedResult 是 PagedResponse 的别名（兼容后端命名）
 */
export type PagedResult<T> = PagedResponse<T>
