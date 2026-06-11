import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'

interface CharacterTypeSectionProps {
  includeUppercase: boolean
  includeLowercase: boolean
  includeNumbers: boolean
  includeSymbols: boolean
  excludeAmbiguous: boolean
  onIncludeUppercaseChange: (value: boolean) => void
  onIncludeLowercaseChange: (value: boolean) => void
  onIncludeNumbersChange: (value: boolean) => void
  onIncludeSymbolsChange: (value: boolean) => void
  onExcludeAmbiguousChange: (value: boolean) => void
}

const options = [
  {
    id: 'uppercase',
    label: '大写字母 (A-Z)',
    key: 'includeUppercase',
  },
  {
    id: 'lowercase',
    label: '小写字母 (a-z)',
    key: 'includeLowercase',
  },
  {
    id: 'numbers',
    label: '数字 (0-9)',
    key: 'includeNumbers',
  },
  {
    id: 'symbols',
    label: '符号 (!@#$%^&*...)',
    key: 'includeSymbols',
  },
  {
    id: 'excludeAmbiguous',
    label: '排除易混淆字符 (0O1lI)',
    key: 'excludeAmbiguous',
  },
] as const

export function CharacterTypeSection({
  includeUppercase,
  includeLowercase,
  includeNumbers,
  includeSymbols,
  excludeAmbiguous,
  onIncludeUppercaseChange,
  onIncludeLowercaseChange,
  onIncludeNumbersChange,
  onIncludeSymbolsChange,
  onExcludeAmbiguousChange,
}: CharacterTypeSectionProps) {
  const checkedByKey = {
    includeUppercase,
    includeLowercase,
    includeNumbers,
    includeSymbols,
    excludeAmbiguous,
  }

  const changeByKey = {
    includeUppercase: onIncludeUppercaseChange,
    includeLowercase: onIncludeLowercaseChange,
    includeNumbers: onIncludeNumbersChange,
    includeSymbols: onIncludeSymbolsChange,
    excludeAmbiguous: onExcludeAmbiguousChange,
  }

  return (
    <div className="space-y-2">
      <Label>字符类型</Label>

      {options.map((option) => (
        <div key={option.id} className="flex items-center space-x-2">
          <Checkbox
            id={option.id}
            checked={checkedByKey[option.key]}
            onCheckedChange={(checked) =>
              changeByKey[option.key](checked as boolean)
            }
          />
          <label
            htmlFor={option.id}
            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
          >
            {option.label}
          </label>
        </div>
      ))}
    </div>
  )
}