import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { Slider } from '@/components/ui/slider'

interface CharacterDistributionSectionProps {
  enabled: boolean
  includeUppercase: boolean
  includeLowercase: boolean
  includeNumbers: boolean
  includeSymbols: boolean
  uppercasePercentage: number
  lowercasePercentage: number
  numbersPercentage: number
  symbolsPercentage: number
  onEnabledChange: (value: boolean) => void
  onUppercasePercentageChange: (value: number) => void
  onLowercasePercentageChange: (value: number) => void
  onNumbersPercentageChange: (value: number) => void
  onSymbolsPercentageChange: (value: number) => void
}

interface PercentageSliderProps {
  label: string
  value: number
  onChange: (value: number) => void
}

function PercentageSlider({ label, value, onChange }: PercentageSliderProps) {
  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between">
        <Label className="text-xs">{label}</Label>
        <span className="text-xs font-medium">{value}%</span>
      </div>
      <Slider
        value={[value]}
        onValueChange={(values) => onChange(values[0])}
        min={0}
        max={100}
        step={5}
        className="w-full"
      />
    </div>
  )
}

export function CharacterDistributionSection({
  enabled,
  includeUppercase,
  includeLowercase,
  includeNumbers,
  includeSymbols,
  uppercasePercentage,
  lowercasePercentage,
  numbersPercentage,
  symbolsPercentage,
  onEnabledChange,
  onUppercasePercentageChange,
  onLowercasePercentageChange,
  onNumbersPercentageChange,
  onSymbolsPercentageChange,
}: CharacterDistributionSectionProps) {
  return (
    <div className="space-y-2 border-t pt-2">
      <div className="flex items-center space-x-2">
        <Checkbox
          id="useDistribution"
          checked={enabled}
          onCheckedChange={(checked) => onEnabledChange(checked as boolean)}
        />
        <label
          htmlFor="useDistribution"
          className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
        >
          启用字符比例控制
        </label>
      </div>

      {enabled && (
        <div className="space-y-2 pl-6">
          {includeUppercase && (
            <PercentageSlider
              label="大写字母"
              value={uppercasePercentage}
              onChange={onUppercasePercentageChange}
            />
          )}
          {includeLowercase && (
            <PercentageSlider
              label="小写字母"
              value={lowercasePercentage}
              onChange={onLowercasePercentageChange}
            />
          )}
          {includeNumbers && (
            <PercentageSlider
              label="数字"
              value={numbersPercentage}
              onChange={onNumbersPercentageChange}
            />
          )}
          {includeSymbols && (
            <PercentageSlider
              label="符号"
              value={symbolsPercentage}
              onChange={onSymbolsPercentageChange}
            />
          )}

          <div className="text-xs text-muted-foreground">
            提示：比例会自动归一化，无需总和为 100%
          </div>
        </div>
      )}
    </div>
  )
}