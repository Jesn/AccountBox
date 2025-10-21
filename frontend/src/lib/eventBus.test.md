# EventBus 使用说明

## 概述

EventBus 是一个简单的事件总线，用于跨组件通信，避免组件间的直接依赖。

## 当前使用场景

### 1. 401 未授权处理

**流程：**
```
API 请求 → 401 错误 → apiClient 触发事件 → App 组件监听 → 显示提示 + 跳转登录
```

**优势：**
- ✅ 解耦：apiClient 不需要知道如何导航
- ✅ 集中处理：所有 401 错误在 App.tsx 统一处理
- ✅ 优雅提示：使用 toast 而不是硬跳转
- ✅ 保留状态：使用 React Router 导航，不丢失应用状态

## 代码示例

### 触发事件（apiClient.ts）

```typescript
import { eventBus, AUTH_EVENTS } from '@/lib/eventBus'

// 当检测到 401 错误时
if (error.response.status === 401) {
  authService.logout()

  // 触发事件
  eventBus.emit(AUTH_EVENTS.UNAUTHORIZED)
}
```

### 监听事件（App.tsx）

```typescript
import { eventBus, AUTH_EVENTS } from '@/lib/eventBus'
import { toast } from 'sonner'

function AppRoutes() {
  const navigate = useNavigate()

  useEffect(() => {
    // 订阅事件
    const unsubscribe = eventBus.on(AUTH_EVENTS.UNAUTHORIZED, () => {
      toast.error('登录已过期，请重新登录')
      navigate('/login', { replace: true })
    })

    // 清理订阅
    return unsubscribe
  }, [navigate])

  return <Routes>...</Routes>
}
```

## 扩展使用

### 添加新事件

```typescript
// 1. 在 eventBus.ts 中定义事件常量
export const AUTH_EVENTS = {
  UNAUTHORIZED: 'auth:unauthorized',
  LOGOUT: 'auth:logout',
  TOKEN_REFRESH: 'auth:token_refresh', // 新增
} as const

// 2. 触发事件
eventBus.emit(AUTH_EVENTS.TOKEN_REFRESH, { newToken: 'xxx' })

// 3. 监听事件
eventBus.on(AUTH_EVENTS.TOKEN_REFRESH, (data) => {
  console.log('Token refreshed:', data)
})
```

### 其他可能的使用场景

1. **全局通知**
   ```typescript
   eventBus.emit('notification:show', {
     type: 'success',
     message: '操作成功'
   })
   ```

2. **数据刷新**
   ```typescript
   eventBus.emit('data:refresh', { entity: 'websites' })
   ```

3. **主题切换**
   ```typescript
   eventBus.emit('theme:change', { theme: 'dark' })
   ```

## 注意事项

1. **避免滥用**：只在真正需要跨组件通信时使用
2. **类型安全**：考虑使用 TypeScript 泛型增强类型安全
3. **内存泄漏**：务必在组件卸载时取消订阅
4. **调试困难**：事件流不如直接调用直观，需要良好的命名和文档

## 对比其他方案

| 方案 | 优点 | 缺点 | 适用场景 |
|------|------|------|---------|
| EventBus | 解耦、简单 | 类型安全弱 | 跨层级通信 |
| Props | 类型安全 | 层级深时繁琐 | 父子组件 |
| Context | React 原生 | 性能问题 | 全局状态 |
| Zustand | 功能强大 | 额外依赖 | 复杂状态管理 |
