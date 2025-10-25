import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosError } from 'axios'
import type { ApiResponse, ErrorResponse } from '@/types/common'
import { STORAGE_KEYS, API_ENDPOINTS } from '@/lib/constants'
import { eventBus, AUTH_EVENTS } from '@/lib/eventBus'

/**
 * API 客户端基类
 * 提供统一的 HTTP 请求封装和错误处理
 *
 * 注意：系统已从加密存储切换为明文存储（2025-10-17架构变更）
 * - 移除了 Vault Session 管理
 * - 移除了 X-Vault-Session 请求头
 * - 添加了 JWT Token 认证支持（2025-10-17）
 */
class ApiClient {
  private client: AxiosInstance

  constructor(baseURL: string = '') {
    this.client = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
      withCredentials: true,
    })

    // 请求拦截器：自动附加JWT Token
    this.client.interceptors.request.use(
      (config) => {
        // 跳过登录请求（避免循环依赖）
        if (config.url === API_ENDPOINTS.AUTH.LOGIN) {
          return config
        }

        // 直接从localStorage获取Token（避免循环依赖）
        const token = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN)
        const expiry = localStorage.getItem(STORAGE_KEYS.AUTH_EXPIRES)
        
        // 检查Token是否存在且未过期
        if (token && expiry) {
          const expiryDate = new Date(expiry)
          if (expiryDate > new Date()) {
            config.headers.Authorization = `Bearer ${token}`
          } else {
            // Token已过期，清除
            localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN)
            localStorage.removeItem(STORAGE_KEYS.AUTH_EXPIRES)
          }
        }

        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )

    // 响应拦截器：统一处理响应和错误
    this.client.interceptors.response.use(
      (response) => {
        return response
      },
      (error: AxiosError<ApiResponse<unknown>>) => {
        return this.handleError(error)
      }
    )
  }

  /**
   * GET 请求
   */
  async get<T>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.get<ApiResponse<T>>(url, config)
    return response.data
  }

  /**
   * POST 请求
   */
  async post<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config)
    return response.data
  }

  /**
   * PUT 请求
   */
  async put<T>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config)
    return response.data
  }

  /**
   * DELETE 请求
   */
  async delete<T>(
    url: string,
    config?: AxiosRequestConfig
  ): Promise<ApiResponse<T>> {
    const response = await this.client.delete<ApiResponse<T>>(url, config)
    return response.data
  }

  /**
   * 错误处理
   */
  private handleError(error: AxiosError<ApiResponse<unknown>>): Promise<never> {
    if (error.response) {
      // 401 未授权：Token过期或无效，清除Token并触发登出事件
      if (error.response.status === 401) {
        // 清除Token（避免循环依赖）
        localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN)
        localStorage.removeItem(STORAGE_KEYS.AUTH_EXPIRES)

        // 触发未授权事件，由 App 组件统一处理导航和提示
        // 避免在 service 层直接操作 window.location
        eventBus.emit(AUTH_EVENTS.UNAUTHORIZED)

        return Promise.reject({
          errorCode: 'UNAUTHORIZED',
          message: '登录已过期，请重新登录',
        } as ErrorResponse)
      }

      // 服务器返回错误响应
      const apiResponse = error.response.data
      if (apiResponse && apiResponse.error) {
        return Promise.reject(apiResponse.error)
      }
      // 未知错误格式
      return Promise.reject({
        errorCode: 'UNKNOWN_ERROR',
        message: error.message || 'An unexpected error occurred',
      } as ErrorResponse)
    } else if (error.request) {
      // 请求发送但没有收到响应
      return Promise.reject({
        errorCode: 'NETWORK_ERROR',
        message: 'Network error. Please check your connection.',
      } as ErrorResponse)
    } else {
      // 其他错误
      return Promise.reject({
        errorCode: 'CLIENT_ERROR',
        message: error.message || 'An error occurred',
      } as ErrorResponse)
    }
  }
}

// 导出单例
// baseURL 为空，各服务使用完整路径（如 '/api/websites'）
// Vite 开发服务器会自动将 /api/* 请求代理到后端 (http://localhost:5093)
export const apiClient = new ApiClient()

export default apiClient
