import { RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { CharacterDistributionSection } from '@/components/password-generator/CharacterDistributionSection'
import { CharacterTypeSection } from '@/components/password-generator/CharacterTypeSection'
import { GeneratedPasswordPreview } from '@/components/password-generator/GeneratedPasswordPreview'
import { PasswordLengthSection } from '@/components/password-generator/PasswordLengthSection'
import { PasswordStrengthIndicator } from '@/components/password-generator/PasswordStrengthIndicator'
import { usePasswordGenerator } from '@/hooks/usePasswordGenerator'

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
  const generator = usePasswordGenerator(open)

  const handleAccept = () => {
    if (generator.generatedPassword) {
      onAccept(generator.generatedPassword)
      onOpenChange(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="w-[95vw] sm:w-full sm:max-w-[500px] max-h-[90vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>密码生成器</DialogTitle>
          <DialogDescription>配置参数生成安全密码</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-3 overflow-y-auto flex-1">
          <GeneratedPasswordPreview password={generator.generatedPassword} />

          {generator.passwordStrength && (
            <PasswordStrengthIndicator strength={generator.passwordStrength} />
          )}

          <PasswordLengthSection
            length={generator.length}
            onLengthChange={generator.setLength}
          />

          <CharacterTypeSection
            includeUppercase={generator.includeUppercase}
            includeLowercase={generator.includeLowercase}
            includeNumbers={generator.includeNumbers}
            includeSymbols={generator.includeSymbols}
            excludeAmbiguous={generator.excludeAmbiguous}
            onIncludeUppercaseChange={generator.setIncludeUppercase}
            onIncludeLowercaseChange={generator.setIncludeLowercase}
            onIncludeNumbersChange={generator.setIncludeNumbers}
            onIncludeSymbolsChange={generator.setIncludeSymbols}
            onExcludeAmbiguousChange={generator.setExcludeAmbiguous}
          />

          <CharacterDistributionSection
            enabled={generator.useCharacterDistribution}
            includeUppercase={generator.includeUppercase}
            includeLowercase={generator.includeLowercase}
            includeNumbers={generator.includeNumbers}
            includeSymbols={generator.includeSymbols}
            uppercasePercentage={generator.uppercasePercentage}
            lowercasePercentage={generator.lowercasePercentage}
            numbersPercentage={generator.numbersPercentage}
            symbolsPercentage={generator.symbolsPercentage}
            onEnabledChange={generator.setUseCharacterDistribution}
            onUppercasePercentageChange={generator.setUppercasePercentage}
            onLowercasePercentageChange={generator.setLowercasePercentage}
            onNumbersPercentageChange={generator.setNumbersPercentage}
            onSymbolsPercentageChange={generator.setSymbolsPercentage}
          />
        </div>

        <DialogFooter className="flex-shrink-0">
          <Button
            type="button"
            variant="outline"
            onClick={generator.generatePassword}
            disabled={generator.isGenerating}
          >
            <RefreshCw className="mr-2 h-4 w-4" />
            重新生成
          </Button>
          <Button
            type="button"
            onClick={handleAccept}
            disabled={!generator.generatedPassword}
          >
            使用此密码
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}