/**
 * 简单的事件总线
 * 用于跨组件通信，避免组件间的直接依赖
 */

type EventCallback = (...args: unknown[]) => void

class EventBus {
  private events: Map<string, EventCallback[]> = new Map()

  /**
   * 订阅事件
   */
  on(event: string, callback: EventCallback): () => void {
    if (!this.events.has(event)) {
      this.events.set(event, [])
    }
    const callbacks = this.events.get(event)
    if (callbacks) {
      callbacks.push(callback)
    }

    // 返回取消订阅函数
    return () => this.off(event, callback)
  }

  /**
   * 取消订阅
   */
  off(event: string, callback: EventCallback): void {
    const callbacks = this.events.get(event)
    if (callbacks) {
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
      }
    }
  }

  /**
   * 触发事件
   */
  emit(event: string, ...args: unknown[]): void {
    const callbacks = this.events.get(event)
    if (callbacks) {
      callbacks.forEach((callback) => callback(...args))
    }
  }

  /**
   * 清除所有事件监听
   */
  clear(): void {
    this.events.clear()
  }
}

// 导出单例
export const eventBus = new EventBus()

// 定义事件类型常量
export const AUTH_EVENTS = {
  UNAUTHORIZED: 'auth:unauthorized',
  LOGOUT: 'auth:logout',
} as const
