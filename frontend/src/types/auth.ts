/**
 * 认证相关类型定义
 */

/**
 * 登录请求
 */
export interface LoginRequest {
  masterPassword: string
}

/**
 * 登录响应
 */
export interface LoginResponse {
  token: string
  expiresAt: string
}
