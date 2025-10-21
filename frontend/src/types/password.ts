/**
 * 密码生成器相关类型定义
 */

/**
 * 生成密码请求
 */
export interface GeneratePasswordRequest {
  length: number
  includeUppercase: boolean
  includeLowercase: boolean
  includeNumbers: boolean
  includeSymbols: boolean
  excludeAmbiguous: boolean
  useCharacterDistribution?: boolean
  uppercasePercentage?: number
  lowercasePercentage?: number
  numbersPercentage?: number
  symbolsPercentage?: number
}

/**
 * 计算密码强度请求
 */
export interface CalculateStrengthRequest {
  password: string
}

/**
 * 密码强度等级
 */
export type PasswordStrengthLevel = 'Weak' | 'Fair' | 'Good' | 'Strong' | 'VeryStrong'

/**
 * 密码强度信息
 */
export interface PasswordStrength {
  score: number // 0-100
  level: PasswordStrengthLevel
  feedback: string[]
  estimatedCrackTime: string
}

/**
 * 生成密码响应
 */
export interface GeneratePasswordResponse {
  password: string
  strength: PasswordStrength
}

/**
 * 密码强度响应
 */
export interface PasswordStrengthResponse {
  strength: PasswordStrength
}
