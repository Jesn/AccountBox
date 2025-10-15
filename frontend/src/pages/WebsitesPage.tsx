import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useVault } from '@/hooks/useVault'
import { vaultService } from '@/services/vaultService'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { ChangeMasterPasswordDialog } from '@/components/vault/ChangeMasterPasswordDialog'

/**
 * 网站管理页面（临时占位，将在 US1 中实现）
 */
export function WebsitesPage() {
  const { lock } = useVault()
  const navigate = useNavigate()
  const [showChangePasswordDialog, setShowChangePasswordDialog] = useState(false)

  const handleLock = async () => {
    try {
      await vaultService.lock()
      lock()
      navigate('/unlock')
    } catch (err) {
      console.error('Lock error:', err)
    }
  }

  const handleChangePasswordSuccess = () => {
    // 修改密码成功后，会话被清除，需要重新解锁
    lock()
    navigate('/unlock')
  }

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-8 flex items-center justify-between">
          <h1 className="text-3xl font-bold">网站管理</h1>
          <div className="flex gap-2">
            <Button
              variant="outline"
              onClick={() => setShowChangePasswordDialog(true)}
            >
              修改主密码
            </Button>
            <Button variant="outline" onClick={handleLock}>
              锁定
            </Button>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>欢迎使用 AccountBox</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-gray-600">
              Vault 已解锁！网站和账号管理功能将在 User Story 1 中实现。
            </p>
            <p className="mt-2 text-sm text-gray-500">
              当前您可以测试锁定和修改主密码功能。
            </p>
          </CardContent>
        </Card>
      </div>

      <ChangeMasterPasswordDialog
        open={showChangePasswordDialog}
        onOpenChange={setShowChangePasswordDialog}
        onSuccess={handleChangePasswordSuccess}
      />
    </div>
  )
}
