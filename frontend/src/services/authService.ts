import { apiClient } from './apiClient';
import { STORAGE_KEYS, API_ENDPOINTS } from '@/lib/constants';
import { ErrorHandler, createError } from '@/lib/errorHandler';

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
    const response = await apiClient.post<LoginResponse>(
      API_ENDPOINTS.AUTH.LOGIN,
      { masterPassword } as LoginRequest
    );

    if (response.success && response.data) {
      const { token, expiresAt } = response.data;
      
      // 将 DateTime 转换为 ISO 字符串存储
      const expiryString = typeof expiresAt === 'string' ? expiresAt : new Date(expiresAt).toISOString();
      
      // 存储Token和过期时间到localStorage
      this.setToken(token, expiryString);
      
      return {
        token,
        expiresAt: expiryString
      };
    } else {
      const error = response.error || createError('LOGIN_FAILED', '登录失败');
      throw createError(
        error.errorCode,
        ErrorHandler.formatError(error)
      );
    }
  }

  /**
   * 用户登出
   */
  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.AUTH_EXPIRES);
  }

  /**
   * 获取当前Token
   * @returns JWT Token字符串或null
   */
  getToken(): string | null {
    const token = localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
    const expiry = localStorage.getItem(STORAGE_KEYS.AUTH_EXPIRES);

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
    const expiry = localStorage.getItem(STORAGE_KEYS.AUTH_EXPIRES);
    return expiry ? new Date(expiry) : null;
  }

  /**
   * 设置Token和过期时间
   * @private
   */
  private setToken(token: string, expiresAt: string): void {
    localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, token);
    localStorage.setItem(STORAGE_KEYS.AUTH_EXPIRES, expiresAt);
  }

  /**
   * 检查Token是否即将过期
   * @returns 是否需要刷新Token
   */
  shouldRefreshToken(): boolean {
    const expiry = this.getTokenExpiry();
    if (!expiry) return false;
    
    const now = new Date();
    const timeUntilExpiry = expiry.getTime() - now.getTime();
    
    // 如果Token在5分钟内过期，建议刷新
    return timeUntilExpiry < 5 * 60 * 1000;
  }
}

// 导出单例实例
export const authService = new AuthService();
export default authService;
