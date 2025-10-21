# Error Boundary 使用文档

## 概述

Error Boundary（错误边界）是 React 提供的一种错误处理机制，用于捕获子组件树中的 JavaScript 错误，记录错误并显示备用 UI，防止整个应用崩溃（白屏）。

## 已实现的功能

### 1. ErrorBoundary 组件

**位置：** `src/components/error/ErrorBoundary.tsx`

**功能：**
- ✅ 捕获子组件渲染错误
- ✅ 显示友好的错误页面
- ✅ 提供重试和返回首页功能
- ✅ 开发环境显示详细错误信息
- ✅ 支持自定义错误处理回调
- ✅ 支持自定义 fallback UI

### 2. ErrorPage 组件

**位置：** `src/components/error/ErrorPage.tsx`

**功能：**
- ✅ 美观的错误页面设计
- ✅ 使用 shadcn/ui 组件
- ✅ 响应式布局
- ✅ 开发环境显示错误详情（可折叠）
- ✅ 提供操作按钮（重试、返回首页）
- ✅ 显示帮助信息

### 3. ErrorTest 组件

**位置：** `src/components/error/ErrorTest.tsx`

**功能：**
- ✅ 测试 ErrorBoundary 功能
- ✅ 演示不同类型的错误
- ✅ 仅用于开发环境

## 使用方法

### 基本使用（已在 App.tsx 中配置）

```typescript
import { ErrorBoundary } from '@/components/error/ErrorBoundary'
import { ErrorPage } from '@/components/error/ErrorPage'

function App() {
  return (
    <ErrorBoundary
      fallback={<ErrorPage />}
      onError={(error, errorInfo) => {
        // 可选：添加错误上报逻辑
        console.error('应用错误:', error)
      }}
    >
      <YourApp />
    </ErrorBoundary>
  )
}
```

### 局部使用（保护特定组件）

```typescript
// 只保护某个复杂组件
<ErrorBoundary>
  <ComplexComponent />
</ErrorBoundary>

// 保护整个页面
<ErrorBoundary>
  <AccountsPage />
</ErrorBoundary>
```

### 自定义错误页面

```typescript
<ErrorBoundary
  fallback={
    <div>
      <h1>出错了</h1>
      <button onClick={() => window.location.reload()}>刷新</button>
    </div>
  }
>
  <App />
</ErrorBoundary>
```

### 添加错误上报

```typescript
<ErrorBoundary
  onError={(error, errorInfo) => {
    // 发送到错误监控服务
    // Sentry.captureException(error, { extra: errorInfo })
    // LogRocket.captureException(error)

    // 或发送到自己的服务器
    fetch('/api/log-error', {
      method: 'POST',
      body: JSON.stringify({
        error: error.toString(),
        stack: errorInfo.componentStack,
        timestamp: new Date().toISOString(),
      })
    })
  }}
>
  <App />
</ErrorBoundary>
```

## 测试 ErrorBoundary

### 方法 1：使用 ErrorTest 组件

```typescript
// 在任意页面导入
import { ErrorTest } from '@/components/error/ErrorTest'

function TestPage() {
  return (
    <div>
      <h1>测试页面</h1>
      {import.meta.env.DEV && <ErrorTest />}
    </div>
  )
}
```

### 方法 2：手动触发错误

```typescript
function BuggyComponent() {
  const [shouldThrow, setShouldThrow] = useState(false)

  if (shouldThrow) {
    throw new Error('测试错误')
  }

  return (
    <button onClick={() => setShouldThrow(true)}>
      触发错误
    </button>
  )
}
```

### 方法 3：在浏览器控制台测试

```javascript
// 打开浏览器控制台，输入：
throw new Error('测试 ErrorBoundary')
```

## ErrorBoundary 的限制

### ✅ 能捕获的错误

1. **组件渲染过程中的错误**
   ```typescript
   function Component() {
     throw new Error('渲染错误') // ✅ 会被捕获
   }
   ```

2. **生命周期方法中的错误**
   ```typescript
   useEffect(() => {
     throw new Error('Effect 错误') // ✅ 会被捕获
   }, [])
   ```

3. **子组件树中的错误**
   ```typescript
   <ErrorBoundary>
     <ChildComponent /> {/* 子组件的错误会被捕获 */}
   </ErrorBoundary>
   ```

### ❌ 不能捕获的错误

1. **事件处理器中的错误**
   ```typescript
   function Component() {
     const handleClick = () => {
       throw new Error('事件错误') // ❌ 不会被捕获
     }

     // 需要手动 try-catch
     const handleClickSafe = () => {
       try {
         throw new Error('事件错误')
       } catch (error) {
         console.error(error)
       }
     }
   }
   ```

2. **异步代码中的错误**
   ```typescript
   function Component() {
     useEffect(() => {
       // ❌ 不会被捕获
       setTimeout(() => {
         throw new Error('异步错误')
       }, 1000)

       // 需要手动 try-catch
       setTimeout(() => {
         try {
           throw new Error('异步错误')
         } catch (error) {
           console.error(error)
         }
       }, 1000)
     }, [])
   }
   ```

3. **Promise 中的错误**
   ```typescript
   function Component() {
     useEffect(() => {
       // ❌ 不会被捕获
       Promise.reject(new Error('Promise 错误'))

       // 需要手动 catch
       Promise.reject(new Error('Promise 错误'))
         .catch(error => console.error(error))
     }, [])
   }
   ```

4. **服务端渲染的错误**
   - ErrorBoundary 只在客户端有效

5. **ErrorBoundary 自身的错误**
   - 需要在外层再包一个 ErrorBoundary

## 最佳实践

### 1. 分层保护

```typescript
// 全局保护
<ErrorBoundary fallback={<GlobalErrorPage />}>
  <App>
    {/* 页面级保护 */}
    <ErrorBoundary fallback={<PageErrorFallback />}>
      <ComplexPage />
    </ErrorBoundary>
  </App>
</ErrorBoundary>
```

### 2. 结合 try-catch

```typescript
function Component() {
  const handleSubmit = async () => {
    try {
      await api.submit(data)
    } catch (error) {
      // 手动处理异步错误
      toast.error('提交失败')
      console.error(error)
    }
  }
}
```

### 3. 错误上报

```typescript
<ErrorBoundary
  onError={(error, errorInfo) => {
    // 只在生产环境上报
    if (import.meta.env.PROD) {
      reportError(error, errorInfo)
    }
  }}
>
  <App />
</ErrorBoundary>
```

### 4. 用户友好的错误信息

```typescript
// ❌ 不好
throw new Error('undefined is not a function')

// ✅ 好
throw new Error('加载用户数据失败，请刷新页面重试')
```

## 常见问题

### Q: 为什么我的错误没有被捕获？

A: 检查是否是以下情况：
- 事件处理器中的错误（需要 try-catch）
- 异步代码中的错误（需要 try-catch）
- Promise 中的错误（需要 .catch()）

### Q: 如何在生产环境隐藏错误详情？

A: ErrorPage 组件已经处理了，只在开发环境显示详情：
```typescript
{import.meta.env.DEV && <ErrorDetails />}
```

### Q: 如何重置 ErrorBoundary 状态？

A: 有两种方式：
1. 使用内置的"重试"按钮
2. 刷新页面

### Q: 可以嵌套使用 ErrorBoundary 吗？

A: 可以！内层的 ErrorBoundary 会先捕获错误，如果内层也出错，外层会捕获。

## 效果对比

### 优化前
```
组件错误 → 白屏 → 用户困惑 → 关闭页面 ❌
```

### 优化后
```
组件错误 → 友好提示 → 点击重试 → 继续使用 ✅
```

## 相关资源

- [React 官方文档 - Error Boundaries](https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary)
- [错误边界最佳实践](https://kentcdodds.com/blog/use-react-error-boundary-to-handle-errors-in-react)
