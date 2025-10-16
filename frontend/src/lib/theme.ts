// Simple theme management for Tailwind v4 + shadcn/ui.
// Applies `.dark` class to the <html> element and persists preference.

export type Theme = 'light' | 'dark' | 'system'

const STORAGE_KEY = 'theme'

function resolveTheme(pref: Theme): 'light' | 'dark' {
  if (pref === 'dark') return 'dark'
  if (pref === 'light') return 'light'
  // system
  return window.matchMedia &&
    window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light'
}

export function applyTheme(pref: Theme) {
  const resolved = resolveTheme(pref)
  const root = document.documentElement
  if (resolved === 'dark') root.classList.add('dark')
  else root.classList.remove('dark')
}

export function setTheme(pref: Theme) {
  try {
    localStorage.setItem(STORAGE_KEY, pref)
  } catch {}
  applyTheme(pref)
}

export function initTheme() {
  let pref: Theme = 'system'
  try {
    const stored = localStorage.getItem(STORAGE_KEY) as Theme | null
    if (stored === 'light' || stored === 'dark' || stored === 'system') {
      pref = stored
    }
  } catch {}

  applyTheme(pref)

  // react to system changes when using system preference
  if (pref === 'system' && window.matchMedia) {
    const mql = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = () => applyTheme('system')
    try {
      mql.addEventListener('change', handler)
    } catch {
      // Safari
      // @ts-ignore
      mql.addListener(handler)
    }
  }
}
