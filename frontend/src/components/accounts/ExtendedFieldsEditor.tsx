import { useState, useEffect, useRef } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { X, Plus } from 'lucide-react'

interface ExtendedFieldsEditorProps {
  value?: Record<string, unknown>
  onChange: (value: Record<string, unknown>) => void
  maxSizeKB?: number
}

interface FieldEntry {
  id: string
  key: string
  value: string
}

/**
 * 扩展字段编辑器组件
 * 支持添加、编辑、删除键值对
 * 支持大小限制验证（默认10KB）
 */
export function ExtendedFieldsEditor({
  value = {},
  onChange,
  maxSizeKB = 10,
}: ExtendedFieldsEditorProps) {
  const isInitialMount = useRef(true)
  const previousValueRef = useRef(value)
  const [fields, setFields] = useState<FieldEntry[]>(() => {
    // 初始化时从 value 生成字段列表
    return Object.entries(value).map(([key, val]) => ({
      id: Math.random().toString(36).substring(7),
      key,
      value: typeof val === 'string' ? val : JSON.stringify(val),
    }))
  })
  const [sizeError, setSizeError] = useState<string>('')

  // 只在外部 value 变化时同步（跳过初始挂载）
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false
      previousValueRef.current = value
      return
    }

    // 检查 value 是否真的变化了（深度比较）
    if (JSON.stringify(previousValueRef.current) === JSON.stringify(value)) {
      return // 数据相同，不需要更新
    }

    previousValueRef.current = value

    // 保持现有字段的 ID，避免重新渲染导致失焦
    setFields((currentFields) => {
      const entries = Object.entries(value).map(([key, val]) => {
        // 尝试找到已存在的字段，保持其 ID
        const existingField = currentFields.find((f) => f.key === key)
        return {
          id: existingField?.id || Math.random().toString(36).substring(7),
          key,
          value: typeof val === 'string' ? val : JSON.stringify(val),
        }
      })
      return entries
    })
  }, [value])

  // 计算当前数据大小（字节）
  const calculateSize = (data: Record<string, unknown>): number => {
    const jsonString = JSON.stringify(data)
    return new Blob([jsonString]).size
  }

  // 验证并通知变更
  const notifyChange = (updatedFields: FieldEntry[]) => {
    // 构建新的数据对象
    const newData: Record<string, unknown> = {}
    updatedFields.forEach((field) => {
      if (field.key.trim()) {
        // 尝试解析为 JSON，如果失败则作为字符串
        try {
          newData[field.key.trim()] = JSON.parse(field.value)
        } catch {
          newData[field.key.trim()] = field.value
        }
      }
    })

    // 验证大小
    const sizeInBytes = calculateSize(newData)
    const maxSizeBytes = maxSizeKB * 1024

    if (sizeInBytes > maxSizeBytes) {
      setSizeError(
        `扩展字段大小超过限制 (${(sizeInBytes / 1024).toFixed(2)}KB / ${maxSizeKB}KB)`
      )
      return
    }

    setSizeError('')
    onChange(newData)
  }

  // 添加新字段
  const addField = () => {
    const newFields = [
      ...fields,
      {
        id: Math.random().toString(36).substring(7),
        key: '',
        value: '',
      },
    ]
    setFields(newFields)
  }

  // 更新字段的键
  const updateFieldKey = (id: string, newKey: string) => {
    const updatedFields = fields.map((field) =>
      field.id === id ? { ...field, key: newKey } : field
    )
    setFields(updatedFields)
    notifyChange(updatedFields)
  }

  // 更新字段的值
  const updateFieldValue = (id: string, newValue: string) => {
    const updatedFields = fields.map((field) =>
      field.id === id ? { ...field, value: newValue } : field
    )
    setFields(updatedFields)
    notifyChange(updatedFields)
  }

  // 删除字段
  const removeField = (id: string) => {
    const updatedFields = fields.filter((field) => field.id !== id)
    setFields(updatedFields)
    notifyChange(updatedFields)
  }

  // 计算当前大小
  const currentData: Record<string, unknown> = {}
  fields.forEach((field) => {
    if (field.key.trim()) {
      try {
        currentData[field.key.trim()] = JSON.parse(field.value)
      } catch {
        currentData[field.key.trim()] = field.value
      }
    }
  })
  const currentSizeKB = (calculateSize(currentData) / 1024).toFixed(2)

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <Label className="text-sm font-medium">扩展字段（可选）</Label>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={addField}
          className="h-8"
        >
          <Plus className="mr-1 h-4 w-4" />
          添加字段
        </Button>
      </div>

      {fields.length > 0 && (
        <div className="space-y-2">
          {fields.map((field) => (
            <div key={field.id} className="flex items-start gap-2">
              <div className="flex-1 grid grid-cols-2 gap-2">
                <Input
                  placeholder="字段名"
                  value={field.key}
                  onChange={(e) => updateFieldKey(field.id, e.target.value)}
                  className="h-9"
                />
                <Input
                  placeholder="字段值"
                  value={field.value}
                  onChange={(e) => updateFieldValue(field.id, e.target.value)}
                  className="h-9"
                />
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => removeField(field.id)}
                className="h-9 w-9 p-0 text-gray-500 hover:text-red-600"
              >
                <X className="h-4 w-4" />
              </Button>
            </div>
          ))}
        </div>
      )}

      {fields.length === 0 && (
        <p className="text-sm text-gray-500 py-4 text-center border border-dashed rounded-md">
          暂无扩展字段，点击"添加字段"按钮添加
        </p>
      )}

      {/* 大小提示和错误信息 */}
      <div className="flex items-center justify-between text-xs">
        <span className="text-gray-500">
          当前大小: {currentSizeKB} KB / {maxSizeKB} KB
        </span>
        {sizeError && <span className="text-red-600">{sizeError}</span>}
      </div>
    </div>
  )
}
