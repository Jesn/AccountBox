# AccountBox Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-17

## Active Technologies
- (001-mvp)
- TypeScript 5.9.3 (已确认自 frontend/package.json) + React 19, Vite 7, shadcn/ui, Tailwind CSS 4, axios 1.12 (002-http-localhost-5173)
- N/A (前端展示层功能，数据来自现有 websiteService API) (002-http-localhost-5173)
- TypeScript 5.9.3（已确认自 frontend/package.json） + React 19, Vite 7, shadcn/ui, Tailwind CSS 4, axios 1.12（已存在） (003-15)
- N/A（前端展示层优化，数据来自现有 API） (003-15)
- BCrypt.Net-Next (API密钥哈希), ASP.NET Core 9.0 (外部API), Entity Framework Core 9.0 (006-api-management)
- SQLite（通过Entity Framework Core,用于存储登录失败记录） (007-accountbox-web-jwt)

## Project Structure
```
backend/
  src/
    AccountBox.Api/          # Web API项目
      Controllers/           # API控制器
        ExternalApiController.cs  # 外部API端点
      Services/             # 业务服务
        RandomAccountService.cs   # 随机账号服务
        ApiKeysManagementService.cs  # API密钥管理
      Middleware/           # 中间件
        ApiKeyAuthMiddleware.cs   # API密钥认证
    AccountBox.Core/         # 核心业务逻辑
      Enums/                # 枚举类型
        AccountStatus.cs    # 账号状态
        ApiKeyScopeType.cs  # API密钥作用域
    AccountBox.Data/         # 数据访问层
      Entities/             # 实体模型
        ApiKey.cs           # API密钥实体
        ApiKeyWebsiteScope.cs  # 密钥作用域实体
frontend/
  src/
    components/
      api-keys/             # API密钥管理组件
      accounts/             # 账号管理组件
        AccountStatusBadge.tsx  # 状态标识
        ExtendedFieldsEditor.tsx  # 扩展字段编辑器
    pages/
      ApiKeysPage.tsx       # API密钥管理页面
    services/
      apiKeyService.ts      # API密钥服务
```

## Commands

### Backend
```bash
# 启动后端API服务器
cd backend/src/AccountBox.Api
dotnet run

# 运行数据库迁移
dotnet ef database update

# 代码格式化
dotnet format
```

### Frontend
```bash
# 安装依赖
pnpm install

# 启动开发服务器
pnpm dev

# 代码格式化
pnpm prettier --write "src/**/*.{ts,tsx,js,jsx,json,css}"

# 类型检查
pnpm type-check
```

### API测试
```bash
# 创建API密钥后，使用curl测试外部API
curl -X GET 'http://localhost:5093/api/external/websites/1/accounts/random' \
  -H 'X-API-Key: sk_your_api_key_here'
```

## Code Style
- 后端: C# 标准命名规范，使用 dotnet format
- 前端: TypeScript + React，使用 Prettier 格式化
- 注释: 使用 XML 文档注释（C#）和 JSDoc（TypeScript）

## Recent Changes
- 007-accountbox-web-jwt: Added SQLite（通过Entity Framework Core,用于存储登录失败记录）
- 006-api-management: Added BCrypt.Net-Next, 外部API服务, API密钥管理, 账号状态管理, 扩展字段支持, 随机账号获取
- 005-api-api-1: Added [if applicable, e.g., PostgreSQL, CoreData, files or N/A]

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
