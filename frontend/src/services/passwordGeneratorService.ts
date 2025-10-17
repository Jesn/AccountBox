import apiClient from './apiClient'
import type { ApiResponse } from '../types/common'

// 请求类型
export interface GeneratePasswordRequest {
  length: number
  includeUppercase: boolean
  includeLowercase: boolean
  includeNumbers: boolean
  includeSymbols: boolean
  excludeAmbiguous: boolean
  uppercasePercentage: number
  lowercasePercentage: number
  numbersPercentage: number
  symbolsPercentage: number
  useCharacterDistribution: boolean
}

export interface CalculateStrengthRequest {
  password: string
}

// 响应类型
export interface PasswordStrength {
  score: number
  level: string
  length: number
  hasUppercase: boolean
  hasLowercase: boolean
  hasNumbers: boolean
  hasSymbols: boolean
  entropy: number
}

export interface GeneratePasswordResponse {
  password: string
  strength: PasswordStrength
}

export interface PasswordStrengthResponse {
  strength: PasswordStrength
}

class PasswordGeneratorService {
  private readonly baseUrl = '/api/password-generator'

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
