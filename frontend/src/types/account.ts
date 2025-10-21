/**
 * 账号相关类型定义
 */

/**
 * 账号状态
 */
export type AccountStatus = 'Active' | 'Disabled'

/**
 * 账号实体（基础）
 */
export interface Account {
  id: number
  websiteId: number
  username: string
  password: string // 明文密码（2025-10-17架构变更：从加密存储切换为明文存储）
  notes?: string // 明文备注
  tags?: string
  status: AccountStatus // 账号状态
  extendedData?: Record<string, unknown> // 扩展字段（JSON 键值对）
  isDeleted: boolean
  deletedAt?: string
  createdAt: string
  updatedAt: string
}

/**
 * 账号响应（包含网站信息）
 */
export interface AccountResponse extends Account {
  websiteDomain: string
  websiteDisplayName?: string
}

/**
 * 创建账号请求
 */
export interface CreateAccountRequest {
  websiteId: number
  username: string
  password: string
  notes?: string
  tags?: string
  extendedData?: Record<string, unknown>
}

/**
 * 更新账号请求
 */
export interface UpdateAccountRequest {
  username: string
  password: string
  notes?: string
  tags?: string
  extendedData?: Record<string, unknown>
}

/**
 * 已删除账号响应（回收站）
 */
export interface DeletedAccountResponse extends AccountResponse {
  deletedAt: string
}
