import type { ErrorResponse } from '@/types/common'

/**
 * 错误处理工具类
 */
export class ErrorHandler {
  /**
   * 格式化错误消息
   */
  static formatError(error: ErrorResponse): string {
    switch (error.errorCode) {
      case 'NETWORK_ERROR':
        return '网络连接失败，请检查网络设置'
      case 'UNAUTHORIZED':
        return '登录已过期，请重新登录'
      case 'FORBIDDEN':
        return '权限不足，无法执行此操作'
      case 'NOT_FOUND':
        return '请求的资源不存在'
      case 'VALIDATION_ERROR':
        return '输入数据格式错误，请检查后重试'
      case 'SERVER_ERROR':
        return '服务器内部错误，请稍后重试'
      case 'PASSWORD_INCORRECT':
        return '密码错误，请重试'
      default:
        return error.message || '操作失败，请稍后重试'
    }
  }

  /**
   * 判断是否为网络错误
   */
  static isNetworkError(error: ErrorResponse): boolean {
    return error.errorCode === 'NETWORK_ERROR'
  }

  /**
   * 判断是否为认证错误
   */
  static isAuthError(error: ErrorResponse): boolean {
    return ['UNAUTHORIZED', 'PASSWORD_INCORRECT'].includes(error.errorCode)
  }

  /**
   * 判断是否为服务器错误
   */
  static isServerError(error: ErrorResponse): boolean {
    return error.errorCode === 'SERVER_ERROR'
  }

  /**
   * 判断是否为用户输入错误
   */
  static isUserError(error: ErrorResponse): boolean {
    return ['VALIDATION_ERROR', 'INVALID_INPUT'].includes(error.errorCode)
  }
}

/**
 * 创建标准化的错误响应
 */
export function createError(
  errorCode: string,
  message: string,
  details?: any
): ErrorResponse {
  return {
    errorCode,
    message,
    details,
  }
}

/**
 * 错误重试工具
 */
export class RetryHandler {
  /**
   * 带重试的异步操作
   */
  static async withRetry<T>(
    operation: () => Promise<T>,
    maxRetries: number = 3,
    delay: number = 1000
  ): Promise<T> {
    let lastError: Error

    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        return await operation()
      } catch (error) {
        lastError = error as Error
        
        // 如果是最后一次尝试，直接抛出错误
        if (attempt === maxRetries) {
          throw lastError
        }

        // 等待指定时间后重试
        await new Promise(resolve => setTimeout(resolve, delay * attempt))
      }
    }

    throw lastError!
  }

  /**
   * 判断错误是否可以重试
   */
  static isRetryableError(error: ErrorResponse): boolean {
    return ['NETWORK_ERROR', 'SERVER_ERROR'].includes(error.errorCode)
  }
}