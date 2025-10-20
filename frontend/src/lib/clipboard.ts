/**
 * 剪贴板工具函数
 * 支持 HTTPS 和 HTTP 环境下的复制功能
 */

/**
 * 复制文本到剪贴板
 * 优先使用现代的 Clipboard API，如果不可用则回退到 document.execCommand
 *
 * @param text 要复制的文本内容
 * @returns Promise<boolean> 复制是否成功
 */
export async function copyToClipboard(text: string): Promise<boolean> {
  // 方法1: 尝试使用现代 Clipboard API (需要 HTTPS 或 localhost)
  if (navigator.clipboard && navigator.clipboard.writeText) {
    try {
      await navigator.clipboard.writeText(text)
      return true
    } catch (err) {
      console.warn('Clipboard API 失败，尝试回退方法:', err)
      // 继续尝试回退方法
    }
  }

  // 方法2: 回退到 document.execCommand (支持 HTTP)
  try {
    // 创建临时 textarea 元素
    const textarea = document.createElement('textarea')
    textarea.value = text
    textarea.style.position = 'fixed'
    textarea.style.top = '0'
    textarea.style.left = '0'
    textarea.style.width = '1px'
    textarea.style.height = '1px'
    textarea.style.padding = '0'
    textarea.style.border = 'none'
    textarea.style.outline = 'none'
    textarea.style.boxShadow = 'none'
    textarea.style.background = 'transparent'
    textarea.setAttribute('readonly', '')

    document.body.appendChild(textarea)

    // 选中文本
    textarea.select()
    textarea.setSelectionRange(0, text.length)

    // 执行复制命令
    const successful = document.execCommand('copy')
    document.body.removeChild(textarea)

    if (successful) {
      return true
    }

    throw new Error('execCommand 复制失败')
  } catch (err) {
    console.error('所有复制方法都失败了:', err)
    return false
  }
}
