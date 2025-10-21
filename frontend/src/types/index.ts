/**
 * 类型定义统一导出
 *
 * 使用方式：
 * import { Account, Website, ApiResponse } from '@/types'
 */

// 通用类型
export type { ApiResponse, ErrorResponse, PagedResponse, PagedResult } from './common'

// 账号相关
export type {
  Account,
  AccountResponse,
  AccountStatus,
  CreateAccountRequest,
  UpdateAccountRequest,
  DeletedAccountResponse,
} from './account'

// 网站相关
export type {
  Website,
  WebsiteResponse,
  CreateWebsiteRequest,
  UpdateWebsiteRequest,
  AccountCountResponse,
} from './website'

// API密钥相关
export type {
  ApiKey,
  ApiKeyScopeType,
  CreateApiKeyRequest,
} from './ApiKey'

// 认证相关
export type {
  LoginRequest,
  LoginResponse,
} from './auth'

// 密码生成器相关
export type {
  GeneratePasswordRequest,
  CalculateStrengthRequest,
  PasswordStrength,
  PasswordStrengthLevel,
  GeneratePasswordResponse,
  PasswordStrengthResponse,
} from './password'

// 搜索相关
export type {
  SearchResultItem,
} from './search'
