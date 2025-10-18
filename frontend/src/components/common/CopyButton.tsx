import { Button } from '@/components/ui/button'
import { Copy, Check } from 'lucide-react'
import { useState } from 'react'
import { toast } from 'sonner'
import { copyToClipboard } from '@/lib/clipboard'

interface CopyButtonProps {
  /**
   * 要复制的文本内容
   */
  text: string

  /**
   * 复制成功后的提示消息，默认为 "已复制到剪贴板"
   */
  successMessage?: string

  /**
   * 按钮变体样式
   */
  variant?: 'default' | 'outline' | 'ghost' | 'link' | 'destructive' | 'secondary'

  /**
   * 按钮尺寸
   */
  size?: 'default' | 'sm' | 'lg' | 'icon'

  /**
   * 是否显示文本，true 显示"复制"/"已复制"文字，false 只显示图标
   */
  showText?: boolean

  /**
   * 按钮的额外 CSS 类名
   */
  className?: string

  /**
   * 悬停提示文本
   */
  title?: string

  /**
   * 复制成功后图标恢复的延迟时间（毫秒），默认 2000ms
   */
  resetDelay?: number
}

/**
 * 通用复制按钮组件
 *
 * 功能：
 * - 点击复制文本到剪贴板
 * - 复制成功后图标切换为勾号（绿色）
 * - 显示 Toast 提示
 * - 自动恢复按钮状态
 *
 * @example
 * // 只显示图标
 * <CopyButton text="要复制的内容" />
 *
 * @example
 * // 显示文字
 * <CopyButton text="API密钥" showText successMessage="密钥已复制" />
 */
export function CopyButton({
  text,
  successMessage = '已复制到剪贴板',
  variant = 'ghost',
  size = 'sm',
  showText = false,
  className = '',
  title = '复制',
  resetDelay = 2000,
}: CopyButtonProps) {
  const [isCopied, setIsCopied] = useState(false)

  const handleCopy = async () => {
    try {
      const success = await copyToClipboard(text)
      if (success) {
        setIsCopied(true)
        toast.success(successMessage)

        // 延迟后恢复按钮状态
        setTimeout(() => {
          setIsCopied(false)
        }, resetDelay)
      } else {
        toast.error('复制失败，请重试')
      }
    } catch (err) {
      console.error('复制失败:', err)
      toast.error('复制失败，请重试')
    }
  }

  return (
    <Button
      variant={variant}
      size={size}
      onClick={handleCopy}
      className={className}
      title={isCopied ? '已复制' : title}
    >
      {isCopied ? (
        <>
          <Check className="h-4 w-4 text-green-600" />
          {showText && <span className="ml-1">已复制</span>}
        </>
      ) : (
        <>
          <Copy className="h-4 w-4" />
          {showText && <span className="ml-1">复制</span>}
        </>
      )}
    </Button>
  )
}
