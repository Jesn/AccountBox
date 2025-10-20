import axios, { type AxiosError } from 'axios';
import type { ErrorResponse } from '@/types/common';

// 在生产环境（Docker容器内）使用相对路径
// 在开发环境使用完整URL（通过Vite代理）
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';
const TOKEN_KEY = 'accountbox_token';
const TOKEN_EXPIRY_KEY = 'accountbox_token_expiry';

export interface LoginRequest {
  masterPassword: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

/**
 * 认证服务 - 处理登录、登出和Token管理
 */
class AuthService {
  /**
   * 用户登录
   * @param masterPassword 主密码
   * @returns 登录响应（包含Token和过期时间）
   */
  async login(masterPassword: string): Promise<LoginResponse> {
    try {
      const response = await axios.post<LoginResponse>(
        `${API_BASE_URL}/api/auth/login`,
        { masterPassword } as LoginRequest
      );

      const { token, expiresAt } = response.data;

      // 存储Token和过期时间到localStorage
      localStorage.setItem(TOKEN_KEY, token);
      localStorage.setItem(TOKEN_EXPIRY_KEY, expiresAt);

      return response.data;
    } catch (error) {
      // 处理axios错误
      const axiosError = error as AxiosError<{ error?: { code?: string; message?: string } }>;

      if (axiosError.response) {
        // 服务器返回错误响应
        const errorData = axiosError.response.data?.error;

        if (errorData) {
          // 后端返回了结构化错误信息
          throw {
            errorCode: errorData.code || 'UNKNOWN_ERROR',
            message: errorData.message || '登录失败，请稍后重试',
          } as ErrorResponse;
        }

        // 根据HTTP状态码返回友好的错误消息
        if (axiosError.response.status === 401) {
          throw {
            errorCode: 'PASSWORD_INCORRECT',
            message: '主密码错误，请重试',
          } as ErrorResponse;
        }

        throw {
          errorCode: 'SERVER_ERROR',
          message: '服务器错误，请稍后重试',
        } as ErrorResponse;
      } else if (axiosError.request) {
        // 请求发送但没有收到响应
        throw {
          errorCode: 'NETWORK_ERROR',
          message: '网络连接失败，请检查网络',
        } as ErrorResponse;
      } else {
        // 其他错误
        throw {
          errorCode: 'CLIENT_ERROR',
          message: '登录过程中发生错误，请稍后重试',
        } as ErrorResponse;
      }
    }
  }

  /**
   * 用户登出
   */
  logout(): void {
    // 清除localStorage中的Token
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(TOKEN_EXPIRY_KEY);
  }

  /**
   * 获取当前Token
   * @returns JWT Token字符串或null
   */
  getToken(): string | null {
    const token = localStorage.getItem(TOKEN_KEY);
    const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY);

    // 如果Token不存在，返回null
    if (!token || !expiry) {
      return null;
    }

    // 检查Token是否过期
    const expiryDate = new Date(expiry);
    if (expiryDate <= new Date()) {
      // Token已过期，清除并返回null
      this.logout();
      return null;
    }

    return token;
  }

  /**
   * 检查用户是否已认证
   * @returns 是否已认证
   */
  isAuthenticated(): boolean {
    return this.getToken() !== null;
  }

  /**
   * 获取Token过期时间
   * @returns Token过期时间或null
   */
  getTokenExpiry(): Date | null {
    const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY);
    return expiry ? new Date(expiry) : null;
  }
}

// 导出单例实例
export const authService = new AuthService();
export default authService;
