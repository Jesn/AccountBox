import type { ComponentProps, ReactNode } from 'react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

type ButtonProps = ComponentProps<typeof Button>

interface ResponsiveActionButtonProps extends Omit<ButtonProps, 'children' | 'size'> {
  icon: ReactNode
  label: ReactNode
  mobileTitle?: string
  desktopSize?: ButtonProps['size']
  mobileClassName?: string
  desktopClassName?: string
}

export function ResponsiveActionButton({
  icon,
  label,
  mobileTitle,
  desktopSize = 'sm',
  mobileClassName,
  desktopClassName,
  className,
  ...buttonProps
}: ResponsiveActionButtonProps) {
  return (
    <>
      <Button
        {...buttonProps}
        size="icon"
        title={mobileTitle ?? (typeof label === 'string' ? label : undefined)}
        className={cn('md:hidden h-9 w-9', className, mobileClassName)}
      >
        {icon}
      </Button>
      <Button
        {...buttonProps}
        size={desktopSize}
        className={cn('hidden md:inline-flex', className, desktopClassName)}
      >
        {icon}
        {label}
      </Button>
    </>
  )
}