/**
 * 应用常量定义
 */

// 本地存储键名
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'accountbox_token',
  AUTH_EXPIRES: 'accountbox_token_expiry',
} as const

// API 端点
export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/api/auth/login',
    LOGOUT: '/api/auth/logout',
    VERIFY: '/api/auth/verify',
  },
  WEBSITES: '/api/websites',
  ACCOUNTS: '/api/accounts',
  SEARCH: '/api/search',
  RECYCLE_BIN: '/api/recycle-bin',
  API_KEYS: '/api/api-keys',
  PASSWORD_GENERATOR: '/api/password-generator',
  EXTERNAL_API: '/api/external',
} as const

// 应用配置
export const APP_CONFIG = {
  DEFAULT_PAGE_SIZE: 15,
  MAX_PAGE_SIZE: 100,
  TOKEN_REFRESH_THRESHOLD: 5 * 60 * 1000, // 5分钟
} as const