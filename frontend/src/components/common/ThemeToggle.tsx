import { useState } from 'react'
import { setTheme } from '@/lib/theme'
import { Button } from '@/components/ui/button'

export function ThemeToggle() {
  const [isDark, setIsDark] = useState<boolean>(() =>
    document.documentElement.classList.contains('dark')
  )

  return (
    <Button
      variant="outline"
      size="sm"
      onClick={() => {
        const next = !isDark
        setIsDark(next)
        setTheme(next ? 'dark' : 'light')
      }}
      className="h-8 px-2"
    >
      {isDark ? 'Light' : 'Dark'}
    </Button>
  )
}
