import apiClient from './apiClient'
import { API_ENDPOINTS } from '@/lib/constants'
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
      useCharacterDistribution: false,
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
