import apiClient from './apiClient'
import { API_ENDPOINTS } from '@/lib/constants'
import type {
  ApiResponse,
  GeneratePasswordRequest,
  CalculateStrengthRequest,
  GeneratePasswordResponse,
  PasswordStrengthResponse,
} from '@/types'

class PasswordGeneratorService {
  private readonly baseUrl = API_ENDPOINTS.PASSWORD_GENERATOR

  /**
   * 生成密码
   */
  async generate(
    request: GeneratePasswordRequest
  ): Promise<ApiResponse<GeneratePasswordResponse>> {
    return await apiClient.post<GeneratePasswordResponse>(
      `${this.baseUrl}/generate`,
      request
    )
  }

  /**
   * 使用默认配置快速生成密码
   *
   * 配置说明：
   * - 长度：16 位
   * - 字符类型：大写字母、小写字母、数字、特殊符号
   * - 排除易混淆字符：0O1lI
   * - 字符分布：大写 30%、小写 45%、数字 20%、符号 5%
   * - 使用字符分布模式确保每种字符类型至少出现一次
   */
  async generateQuick(): Promise<ApiResponse<GeneratePasswordResponse>> {
    const defaultRequest: GeneratePasswordRequest = {
      length: 16,
      includeUppercase: true,
      includeLowercase: true,
      includeNumbers: true,
      includeSymbols: true,
      excludeAmbiguous: true,
      uppercasePercentage: 30,
      lowercasePercentage: 45,
      numbersPercentage: 20,
      symbolsPercentage: 5,
      useCharacterDistribution: true,
    }
    return await this.generate(defaultRequest)
  }

  /**
   * 计算密码强度
   */
  async calculateStrength(
    request: CalculateStrengthRequest
  ): Promise<ApiResponse<PasswordStrengthResponse>> {
    return await apiClient.post<PasswordStrengthResponse>(
      `${this.baseUrl}/strength`,
      request
    )
  }
}

export const passwordGeneratorService = new PasswordGeneratorService()
