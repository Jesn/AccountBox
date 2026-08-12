const DEFAULT_DATE_TIME_FORMAT: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
}

/**
 * 将后端返回的时间格式化为展示文本。
 *
 * 后端业务时间统一为配置时区（如 Asia/Shanghai）的墙钟时间，
 * 无时区后缀的 ISO 字符串按「字面时间」展示，避免浏览器本地时区二次换算。
 */
export function formatDateTime(value?: string | Date | null, fallback = '未知') {
  if (!value) {
    return fallback
  }

  if (typeof value === 'string') {
    const wallClock = formatIsoWallClock(value)
    if (wallClock) {
      return wallClock
    }
  }

  const date = value instanceof Date ? value : new Date(value)

  if (Number.isNaN(date.getTime())) {
    return fallback
  }

  return date.toLocaleString('zh-CN', DEFAULT_DATE_TIME_FORMAT)
}

/**
 * 解析 ISO 日期时间的墙钟部分（YYYY-MM-DDTHH:mm），按固定格式输出。
 * 带 Z / 偏移量的时间仍走 Date 解析路径。
 */
function formatIsoWallClock(value: string): string | null {
  const trimmed = value.trim()
  // 仅处理无时区后缀的墙钟时间，避免对 UTC(Z)/偏移量时间做错误截断
  if (/[zZ]|[+-]\d{2}:?\d{2}$/.test(trimmed)) {
    return null
  }

  const match = trimmed.match(
    /^(\d{4})-(\d{2})-(\d{2})[T ](\d{2}):(\d{2})(?::\d{2}(?:\.\d+)?)?$/,
  )
  if (!match) {
    return null
  }

  const [, year, month, day, hour, minute] = match
  return `${year}/${month}/${day} ${hour}:${minute}`
}
