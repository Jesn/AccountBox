import { useState, useEffect } from 'react'
import { passwordGeneratorService } from '@/services/passwordGeneratorService'
import type {
  GeneratePasswordRequest,
  PasswordStrength,
} from '@/services/passwordGeneratorService'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Slider } from '@/components/ui/slider'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { PasswordStrengthIndicator } from './PasswordStrengthIndicator'
import { RefreshCw, Copy, Check } from 'lucide-react'

interface PasswordGeneratorDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onAccept: (password: string) => void
}

/**
 * 密码生成器对话框组件
 * 提供密码生成配置和预览功能
 */
export function PasswordGeneratorDialog({
  open,
  onOpenChange,
  onAccept,
}: PasswordGeneratorDialogProps) {
  const [length, setLength] = useState(16)
  const [includeUppercase, setIncludeUppercase] = useState(true)
  const [includeLowercase, setIncludeLowercase] = useState(true)
  const [includeNumbers, setIncludeNumbers] = useState(true)
  const [includeSymbols, setIncludeSymbols] = useState(true)
  const [excludeAmbiguous, setExcludeAmbiguous] = useState(false)

  // 字符比例控制
  const [useCharacterDistribution, setUseCharacterDistribution] =
    useState(false)
  const [uppercasePercentage, setUppercasePercentage] = useState(30)
  const [lowercasePercentage, setLowercasePercentage] = useState(40)
  const [numbersPercentage, setNumbersPercentage] = useState(20)
  const [symbolsPercentage, setSymbolsPercentage] = useState(10)

  const [generatedPassword, setGeneratedPassword] = useState('')
  const [passwordStrength, setPasswordStrength] =
    useState<PasswordStrength | null>(null)
  const [isGenerating, setIsGenerating] = useState(false)
  const [isCopied, setIsCopied] = useState(false)

  // 生成密码
  const generatePassword = async () => {
    setIsGenerating(true)
    setIsCopied(false)

    try {
      const request: GeneratePasswordRequest = {
        length,
        includeUppercase,
        includeLowercase,
        includeNumbers,
        includeSymbols,
        excludeAmbiguous,
        uppercasePercentage,
        lowercasePercentage,
        numbersPercentage,
        symbolsPercentage,
        useCharacterDistribution,
      }

      const response = await passwordGeneratorService.generate(request)

      if (response.success && response.data) {
        setGeneratedPassword(response.data.password)
        setPasswordStrength(response.data.strength)
      }
    } catch (error) {
      console.error('生成密码失败:', error)
    } finally {
      setIsGenerating(false)
    }
  }

  // 复制密码
  const copyPassword = async () => {
    if (generatedPassword) {
      await navigator.clipboard.writeText(generatedPassword)
      setIsCopied(true)
      setTimeout(() => setIsCopied(false), 2000)
    }
  }

  // 接受密码
  const handleAccept = () => {
    if (generatedPassword) {
      onAccept(generatedPassword)
      onOpenChange(false)
    }
  }

  // 对话框打开时自动生成一次密码
  useEffect(() => {
    if (open && !generatedPassword) {
      generatePassword()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open])

  // 配置改变时自动重新生成
  useEffect(() => {
    if (open && generatedPassword) {
      generatePassword()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    length,
    includeUppercase,
    includeLowercase,
    includeNumbers,
    includeSymbols,
    excludeAmbiguous,
    useCharacterDistribution,
    uppercasePercentage,
    lowercasePercentage,
    numbersPercentage,
    symbolsPercentage,
  ])

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>密码生成器</DialogTitle>
          <DialogDescription>配置参数生成安全密码</DialogDescription>
        </DialogHeader>

        <div className="space-y-6 py-4">
          {/* 生成的密码显示 */}
          <div className="space-y-2">
            <Label>生成的密码</Label>
            <div className="flex gap-2">
              <div className="flex-1 rounded-md border px-3 py-2 font-mono text-sm break-all">
                {generatedPassword || '...'}
              </div>
              <Button
                type="button"
                variant="outline"
                size="icon"
                onClick={copyPassword}
                disabled={!generatedPassword}
              >
                {isCopied ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>

          {/* 强度指示器 */}
          {passwordStrength && (
            <PasswordStrengthIndicator strength={passwordStrength} />
          )}

          {/* 长度配置 */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label>密码长度</Label>
              <span className="text-sm font-medium">{length}</span>
            </div>
            <Slider
              value={[length]}
              onValueChange={(values) => setLength(values[0])}
              min={8}
              max={128}
              step={1}
              className="w-full"
            />
            <div className="flex justify-between text-xs text-gray-500">
              <span>8</span>
              <span>128</span>
            </div>
          </div>

          {/* 字符集配置 */}
          <div className="space-y-3">
            <Label>字符类型</Label>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="uppercase"
                checked={includeUppercase}
                onCheckedChange={(checked) =>
                  setIncludeUppercase(checked as boolean)
                }
              />
              <label
                htmlFor="uppercase"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                大写字母 (A-Z)
              </label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="lowercase"
                checked={includeLowercase}
                onCheckedChange={(checked) =>
                  setIncludeLowercase(checked as boolean)
                }
              />
              <label
                htmlFor="lowercase"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                小写字母 (a-z)
              </label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="numbers"
                checked={includeNumbers}
                onCheckedChange={(checked) =>
                  setIncludeNumbers(checked as boolean)
                }
              />
              <label
                htmlFor="numbers"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                数字 (0-9)
              </label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="symbols"
                checked={includeSymbols}
                onCheckedChange={(checked) =>
                  setIncludeSymbols(checked as boolean)
                }
              />
              <label
                htmlFor="symbols"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                符号 (!@#$%^&*...)
              </label>
            </div>

            <div className="flex items-center space-x-2">
              <Checkbox
                id="excludeAmbiguous"
                checked={excludeAmbiguous}
                onCheckedChange={(checked) =>
                  setExcludeAmbiguous(checked as boolean)
                }
              />
              <label
                htmlFor="excludeAmbiguous"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                排除易混淆字符 (0O1lI)
              </label>
            </div>
          </div>

          {/* 字符比例控制 */}
          <div className="space-y-3 border-t pt-3">
            <div className="flex items-center space-x-2">
              <Checkbox
                id="useDistribution"
                checked={useCharacterDistribution}
                onCheckedChange={(checked) =>
                  setUseCharacterDistribution(checked as boolean)
                }
              />
              <label
                htmlFor="useDistribution"
                className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
              >
                启用字符比例控制
              </label>
            </div>

            {useCharacterDistribution && (
              <div className="space-y-3 pl-6">
                {/* 大写字母比例 */}
                {includeUppercase && (
                  <div className="space-y-1">
                    <div className="flex items-center justify-between">
                      <Label className="text-xs">大写字母</Label>
                      <span className="text-xs font-medium">
                        {uppercasePercentage}%
                      </span>
                    </div>
                    <Slider
                      value={[uppercasePercentage]}
                      onValueChange={(values) =>
                        setUppercasePercentage(values[0])
                      }
                      min={0}
                      max={100}
                      step={5}
                      className="w-full"
                    />
                  </div>
                )}

                {/* 小写字母比例 */}
                {includeLowercase && (
                  <div className="space-y-1">
                    <div className="flex items-center justify-between">
                      <Label className="text-xs">小写字母</Label>
                      <span className="text-xs font-medium">
                        {lowercasePercentage}%
                      </span>
                    </div>
                    <Slider
                      value={[lowercasePercentage]}
                      onValueChange={(values) =>
                        setLowercasePercentage(values[0])
                      }
                      min={0}
                      max={100}
                      step={5}
                      className="w-full"
                    />
                  </div>
                )}

                {/* 数字比例 */}
                {includeNumbers && (
                  <div className="space-y-1">
                    <div className="flex items-center justify-between">
                      <Label className="text-xs">数字</Label>
                      <span className="text-xs font-medium">
                        {numbersPercentage}%
                      </span>
                    </div>
                    <Slider
                      value={[numbersPercentage]}
                      onValueChange={(values) =>
                        setNumbersPercentage(values[0])
                      }
                      min={0}
                      max={100}
                      step={5}
                      className="w-full"
                    />
                  </div>
                )}

                {/* 符号比例 */}
                {includeSymbols && (
                  <div className="space-y-1">
                    <div className="flex items-center justify-between">
                      <Label className="text-xs">符号</Label>
                      <span className="text-xs font-medium">
                        {symbolsPercentage}%
                      </span>
                    </div>
                    <Slider
                      value={[symbolsPercentage]}
                      onValueChange={(values) =>
                        setSymbolsPercentage(values[0])
                      }
                      min={0}
                      max={100}
                      step={5}
                      className="w-full"
                    />
                  </div>
                )}

                <div className="text-xs text-muted-foreground">
                  提示：比例会自动归一化，无需总和为 100%
                </div>
              </div>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={generatePassword}
            disabled={isGenerating}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            重新生成
          </Button>
          <Button
            type="button"
            onClick={handleAccept}
            disabled={!generatedPassword}
          >
            使用此密码
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
