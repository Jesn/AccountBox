import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      // 只代理以 /api/ 开头的请求（注意末尾的斜杠）
      // 这样 /api-keys 和 /api-documentation 等前端路由就不会被代理
      '^/api/': {
        target: 'http://localhost:5093',
        changeOrigin: true,
      },
    },
  },
})
