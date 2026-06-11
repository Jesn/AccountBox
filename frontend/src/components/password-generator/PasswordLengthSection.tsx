import { Label } from '@/components/ui/label'
import { Slider } from '@/components/ui/slider'

interface PasswordLengthSectionProps {
  length: number
  onLengthChange: (length: number) => void
}

export function PasswordLengthSection({
  length,
  onLengthChange,
}: PasswordLengthSectionProps) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label>密码长度</Label>
        <span className="text-sm font-medium">{length}</span>
      </div>
      <Slider
        value={[length]}
        onValueChange={(values) => onLengthChange(values[0])}
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
  )
}