import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5093';
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
    const response = await axios.post<LoginResponse>(
      `${API_BASE_URL}/api/auth/login`,
      { masterPassword } as LoginRequest
    );

    const { token, expiresAt } = response.data;

    // 存储Token和过期时间到localStorage
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(TOKEN_EXPIRY_KEY, expiresAt);

    return response.data;
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
