const DEFAULT_DATE_TIME_FORMAT: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
}

export function formatDateTime(value?: string | Date | null, fallback = '未知') {
  if (!value) {
    return fallback
  }

  const date = value instanceof Date ? value : new Date(value)

  if (Number.isNaN(date.getTime())) {
    return fallback
  }

  return date.toLocaleString('zh-CN', DEFAULT_DATE_TIME_FORMAT)
}