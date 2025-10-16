import axios from 'axios'
import type { AxiosInstance, AxiosRequestConfig, AxiosError } from 'axios'
import type { ApiResponse, ErrorResponse } from '@/types/common'

/**
 * API 客户端基类
 * 提供统一的 HTTP 请求封装和错误处理
 */
class ApiClient {
  private client: AxiosInstance
  private vaultSessionId: string | null = null
  private onUnauthorizedCallback: (() => void) | null = null

  constructor(baseURL: string = '') {
    this.client = axios.create({
      baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
      },
      withCredentials: true,
    })

    // 请求拦截器：添加 Vault Session 头
    this.client.interceptors.request.use(
      (config) => {
        if (this.vaultSessionId) {
          config.headers['X-Vault-Session'] = this.vaultSessionId
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
        // 检查是否是 401 错误
        if (error.response?.status === 401) {
          // 获取请求的 URL
          const requestUrl = error.config?.url || ''

          // 认证端点的 401 错误应该由调用方处理，不触发自动跳转
          // 这些端点包括：/api/vault/unlock, /api/vault/initialize
          const isAuthEndpoint =
            requestUrl.includes('/api/vault/unlock') ||
            requestUrl.includes('/api/vault/initialize')

          // 只有非认证端点的 401 才触发未授权回调（跳转到解锁页面）
          if (!isAuthEndpoint && this.onUnauthorizedCallback) {
            this.onUnauthorizedCallback()
          }
        }
        return this.handleError(error)
      }
    )
  }

  /**
   * 设置 Vault Session ID
   */
  setVaultSession(sessionId: string | null) {
    this.vaultSessionId = sessionId
  }

  /**
   * 设置未授权回调（当收到 401 错误时调用）
   */
  setOnUnauthorized(callback: (() => void) | null) {
    this.onUnauthorizedCallback = callback
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
export const apiClient = new ApiClient()

export default apiClient
