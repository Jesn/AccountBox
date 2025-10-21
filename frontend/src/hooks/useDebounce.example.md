# useDebounce Hook 使用示例

## 密码生成器防抖优化

### 问题场景

用户在密码生成器中拖动滑块调整密码长度：

```
用户操作：拖动滑块从 16 → 32

优化前（无防抖）：
时间 0ms:   length = 16 → API 请求 1
时间 50ms:  length = 17 → API 请求 2
时间 100ms: length = 18 → API 请求 3
时间 150ms: length = 19 → API 请求 4
...
时间 800ms: length = 32 → API 请求 17

结果：发送了 17 次 API 请求！❌
```

### 优化方案

使用 `useDebounce` Hook 添加 500ms 延迟：

```typescript
import { useDebounce } from '@/hooks/useDebounce'

// 原始值（实时变化）
const [length, setLength] = useState(16)

// 防抖值（延迟 500ms 更新）
const debouncedLength = useDebounce(length, 500)

// 使用防抖值触发 API 请求
useEffect(() => {
  generatePassword() // 只在 debouncedLength 变化时调用
}, [debouncedLength])
```

### 优化效果

```
用户操作：拖动滑块从 16 → 32

优化后（500ms 防抖）：
时间 0ms:   length = 16 → 设置定时器
时间 50ms:  length = 17 → 取消上一个定时器，重新设置
时间 100ms: length = 18 → 取消上一个定时器，重新设置
时间 150ms: length = 19 → 取消上一个定时器，重新设置
...
时间 800ms: length = 32 → 设置定时器
时间 1300ms: 用户停止 500ms → API 请求 1

结果：只发送了 1 次 API 请求！✅
节省：16 次无用请求（94% 减少）
```

## 实际代码示例

### PasswordGeneratorDialog.tsx

```typescript
export function PasswordGeneratorDialog() {
  // 原始状态（实时更新）
  const [length, setLength] = useState(16)
  const [uppercasePercentage, setUppercasePercentage] = useState(30)
  const [lowercasePercentage, setLowercasePercentage] = useState(45)
  const [numbersPercentage, setNumbersPercentage] = useState(20)
  const [symbolsPercentage, setSymbolsPercentage] = useState(5)

  // 防抖值（延迟 500ms 更新）
  const debouncedLength = useDebounce(length, 500)
  const debouncedUppercasePercentage = useDebounce(uppercasePercentage, 500)
  const debouncedLowercasePercentage = useDebounce(lowercasePercentage, 500)
  const debouncedNumbersPercentage = useDebounce(numbersPercentage, 500)
  const debouncedSymbolsPercentage = useDebounce(symbolsPercentage, 500)

  // 只在防抖值变化时生成密码
  useEffect(() => {
    if (open && generatedPassword) {
      generatePassword()
    }
  }, [
    debouncedLength,              // ✅ 防抖后的值
    debouncedUppercasePercentage, // ✅ 防抖后的值
    debouncedLowercasePercentage, // ✅ 防抖后的值
    debouncedNumbersPercentage,   // ✅ 防抖后的值
    debouncedSymbolsPercentage,   // ✅ 防抖后的值
    includeUppercase,             // ✅ 布尔值不需要防抖
    includeLowercase,
    includeNumbers,
    includeSymbols,
    excludeAmbiguous,
    useCharacterDistribution,
  ])

  return (
    <Dialog>
      {/* 滑块绑定原始值，实时更新 UI */}
      <Slider
        value={[length]}
        onValueChange={(values) => setLength(values[0])}
        min={8}
        max={128}
      />
      {/* 显示当前值：{length} */}
    </Dialog>
  )
}
```

## 用户体验

### 优化前
- ❌ 滑块拖动时密码频繁闪烁
- ❌ 网络请求过多，可能导致卡顿
- ❌ 服务器负载高

### 优化后
- ✅ 滑块拖动流畅，数字实时更新
- ✅ 停止拖动后才生成新密码
- ✅ 网络请求大幅减少
- ✅ 服务器负载降低

## 性能对比

| 操作 | 优化前 | 优化后 | 节省 |
|------|--------|--------|------|
| 拖动长度滑块（16→32） | 17次请求 | 1次请求 | ↓94% |
| 调整4个百分比滑块 | 40+次请求 | 4次请求 | ↓90% |
| 快速切换多个选项 | 10+次请求 | 1次请求 | ↓90% |

## 其他使用场景

### 1. 搜索框

```typescript
function SearchBox() {
  const [searchTerm, setSearchTerm] = useState('')
  const debouncedSearchTerm = useDebounce(searchTerm, 300)

  useEffect(() => {
    // 只在用户停止输入 300ms 后才搜索
    if (debouncedSearchTerm) {
      search(debouncedSearchTerm)
    }
  }, [debouncedSearchTerm])

  return (
    <input
      value={searchTerm}
      onChange={(e) => setSearchTerm(e.target.value)}
      placeholder="搜索..."
    />
  )
}
```

### 2. 窗口大小调整

```typescript
function ResponsiveComponent() {
  const [windowWidth, setWindowWidth] = useState(window.innerWidth)
  const debouncedWidth = useDebounce(windowWidth, 200)

  useEffect(() => {
    const handleResize = () => setWindowWidth(window.innerWidth)
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  useEffect(() => {
    // 只在停止调整窗口 200ms 后才重新布局
    recalculateLayout(debouncedWidth)
  }, [debouncedWidth])
}
```

### 3. 自动保存

```typescript
function AutoSaveEditor() {
  const [content, setContent] = useState('')
  const debouncedContent = useDebounce(content, 1000)

  useEffect(() => {
    // 用户停止输入 1 秒后自动保存
    if (debouncedContent) {
      saveToServer(debouncedContent)
    }
  }, [debouncedContent])

  return (
    <textarea
      value={content}
      onChange={(e) => setContent(e.target.value)}
    />
  )
}
```

## 注意事项

1. **延迟时间选择**
   - 搜索框：300-500ms
   - 滑块：500-800ms
   - 自动保存：1000-2000ms

2. **用户反馈**
   - 显示"正在生成..."状态
   - 保持 UI 响应（显示实时值）
   - 避免用户困惑

3. **取消机制**
   - useDebounce 自动处理清理
   - 组件卸载时自动取消定时器

4. **不适用场景**
   - 按钮点击（应该立即响应）
   - 表单提交（应该立即处理）
   - 紧急操作（如删除确认）
