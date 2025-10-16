import type { PasswordStrength } from '@/services/passwordGeneratorService'
import { Progress } from '@/components/ui/progress'

interface PasswordStrengthIndicatorProps {
  strength: PasswordStrength | null
}

/**
 * 密码强度指示器组件
 * 显示密码强度的进度条和文字描述
 */
export function PasswordStrengthIndicator({
  strength,
}: PasswordStrengthIndicatorProps) {
  if (!strength) {
    return null
  }

  // 根据等级确定颜色
  const getColor = (level: string) => {
    switch (level) {
      case 'Weak':
        return 'text-red-600'
      case 'Medium':
        return 'text-yellow-600'
      case 'Strong':
        return 'text-blue-600'
      case 'VeryStrong':
        return 'text-green-600'
      default:
        return 'text-gray-600'
    }
  }

  // 根据等级确定进度条颜色类
  const getProgressColor = (level: string) => {
    switch (level) {
      case 'Weak':
        return 'bg-red-600'
      case 'Medium':
        return 'bg-yellow-600'
      case 'Strong':
        return 'bg-blue-600'
      case 'VeryStrong':
        return 'bg-green-600'
      default:
        return 'bg-gray-600'
    }
  }

  // 中文等级名称
  const getLevelText = (level: string) => {
    switch (level) {
      case 'Weak':
        return '弱'
      case 'Medium':
        return '中'
      case 'Strong':
        return '强'
      case 'VeryStrong':
        return '非常强'
      default:
        return '未知'
    }
  }

  const colorClass = getColor(strength.level)
  const progressColorClass = getProgressColor(strength.level)
  const levelText = getLevelText(strength.level)

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium">密码强度:</span>
        <span className={`text-sm font-bold ${colorClass}`}>{levelText}</span>
      </div>

      {/* 进度条 */}
      <div className="relative">
        <Progress value={strength.score} className="h-2" />
        <div
          className={`absolute top-0 left-0 h-2 rounded-full transition-all ${progressColorClass}`}
          style={{ width: `${strength.score}%` }}
        />
      </div>

      {/* 详细信息 */}
      <div className="grid grid-cols-2 gap-2 text-xs text-gray-600">
        <div>
          <span className="font-medium">分数:</span> {strength.score}/100
        </div>
        <div>
          <span className="font-medium">熵:</span> {strength.entropy.toFixed(1)}{' '}
          bits
        </div>
        <div className="col-span-2">
          <span className="font-medium">特征:</span>{' '}
          {[
            strength.hasUppercase && '大写',
            strength.hasLowercase && '小写',
            strength.hasNumbers && '数字',
            strength.hasSymbols && '符号',
          ]
            .filter(Boolean)
            .join(', ')}
        </div>
      </div>
    </div>
  )
}
