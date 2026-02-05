import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { AlertTriangle } from 'lucide-react'

interface ConfirmGeneratePasswordDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: () => void
}

/**
 * 确认生成密码对话框
 * 在编辑账号时，如果已有密码，生成新密码前需要确认
 */
export function ConfirmGeneratePasswordDialog({
  open,
  onOpenChange,
  onConfirm,
}: ConfirmGeneratePasswordDialogProps) {
  const handleConfirm = () => {
    onConfirm()
    onOpenChange(false)
  }

  const handleCancel = () => {
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[450px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-orange-600" />
            确认生成新密码
          </DialogTitle>
          <DialogDescription>
            生成新密码将覆盖当前密码
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <div className="rounded-md bg-orange-50 p-4 border border-orange-200">
            <p className="text-sm text-orange-800">
              ⚠️ 当前密码将被新生成的密码替换。请确认是否继续？
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
          >
            取消
          </Button>
          <Button
            type="button"
            onClick={handleConfirm}
          >
            确认生成
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
