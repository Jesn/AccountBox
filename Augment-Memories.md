# AccountBox 项目记忆库

## 项目概述

**项目名称**: AccountBox - 本地账号密码管理系统  
**项目类型**: 前后端分离的Web应用  
**主要目标**: 提供一个本地账号密码管理工具，主要是个人使用

---

## 技术栈

### 后端
- **语言**: C# / .NET 8.0 (ASP.NET Core)
- **架构**: 分层架构 (Controller → Service → Repository → Data)
- **数据库**: SQLite (开发) / PostgreSQL (生产)
- **ORM**: Entity Framework Core 9.0
- **认证**: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11)
- **密钥哈希**: BCrypt.Net-Next 4.0.3
- **测试**: xUnit, Moq, FluentAssertions
- **API文档**: Swagger

### 前端
- **语言**: TypeScript 5.9.3 (严格模式)
- **框架**: React 19.2.0
- **构建工具**: Vite 7.1.7
- **UI库**: shadcn/ui + Radix UI
- **样式**: Tailwind CSS 4.1.14
- **HTTP客户端**: axios 1.12.2
- **路由**: react-router-dom 7.9.4
- **测试**: Vitest 3.2.4, @testing-library/react 16.3.0, Playwright (E2E)
- **包管理**: pnpm
- **Node版本**: >= 20.19.0

---

## 项目结构

```
AccountBox/
├── backend/
│   ├── src/
│   │   ├── AccountBox.Api/          # Web API项目 (.NET 8.0)
│   │   │   ├── Controllers/         # API控制器
│   │   │   │   ├── WebsiteController.cs
│   │   │   │   ├── AccountController.cs
│   │   │   │   ├── SearchController.cs
│   │   │   │   ├── RecycleBinController.cs
│   │   │   │   ├── PasswordGeneratorController.cs
│   │   │   │   ├── ApiKeysController.cs
│   │   │   │   ├── AuthController.cs
│   │   │   │   └── ExternalApiController.cs
│   │   │   ├── DTOs/                # 数据传输对象
│   │   │   ├── Middleware/          # 中间件
│   │   │   │   ├── ApiKeyAuthMiddleware.cs
│   │   │   │   └── ExceptionHandlingMiddleware.cs
│   │   │   ├── Services/            # 业务服务
│   │   │   │   ├── JwtService.cs
│   │   │   │   ├── ApiKeysManagementService.cs
│   │   │   │   └── RandomAccountService.cs
│   │   │   ├── Validation/          # 验证规则
│   │   │   ├── Program.cs           # 应用入口
│   │   │   └── appsettings.json     # 配置文件
│   │   ├── AccountBox.Core/         # 核心业务逻辑
│   │   │   ├── Services/            # 业务服务
│   │   │   │   ├── WebsiteService.cs
│   │   │   │   ├── AccountService.cs
│   │   │   │   ├── SearchService.cs
│   │   │   │   ├── RecycleBinService.cs
│   │   │   │   ├── PasswordGeneratorService.cs
│   │   │   │   └── ApiKeyService.cs
│   │   │   ├── Interfaces/          # 服务接口
│   │   │   ├── Models/              # DTO模型
│   │   │   ├── Enums/               # 枚举类型
│   │   │   │   ├── AccountStatus.cs
│   │   │   │   └── ApiKeyScopeType.cs
│   │   │   └── Exceptions/          # 自定义异常
│   │   ├── AccountBox.Data/         # 数据访问层 (EF Core 9.0)
│   │   │   ├── Entities/            # EF Core实体
│   │   │   │   ├── Website.cs
│   │   │   │   ├── Account.cs
│   │   │   │   ├── ApiKey.cs
│   │   │   │   ├── ApiKeyWebsiteScope.cs
│   │   │   │   └── LoginAttempt.cs
│   │   │   ├── Repositories/        # 仓储实现
│   │   │   ├── DbContext/           # 数据库上下文
│   │   │   ├── Configurations/      # EF配置
│   │   │   ├── Migrations/          # EF迁移
│   │   │   └── Scripts/             # 数据库脚本
│   │   └── AccountBox.Security/     # (已废弃 2025-10-17) 之前的加密模块
│   │       ├── Encryption/          # AES-256-GCM (已移除)
│   │       ├── KeyDerivation/       # Argon2id (已移除)
│   │       └── VaultManager/        # 密钥管理 (已移除)
│   └── tests/
│       ├── AccountBox.Api.Tests/
│       ├── AccountBox.Core.Tests/
│       └── AccountBox.Security.Tests/
├── frontend/
│   ├── src/
│   │   ├── components/              # React组件
│   │   │   ├── auth/                # 认证相关组件
│   │   │   │   └── LoginForm.tsx
│   │   │   ├── accounts/            # 账号管理组件
│   │   │   │   ├── AccountForm.tsx
│   │   │   │   ├── AccountList.tsx
│   │   │   │   ├── AccountStatusBadge.tsx
│   │   │   │   └── ExtendedFieldsEditor.tsx
│   │   │   ├── websites/            # 网站管理组件
│   │   │   ├── api-keys/            # API密钥管理
│   │   │   ├── recycle-bin/         # 回收站
│   │   │   ├── password-generator/  # 密码生成器
│   │   │   ├── ui/                  # shadcn/ui组件
│   │   │   └── common/              # 通用组件
│   │   ├── pages/                   # 页面组件
│   │   │   ├── LoginPage.tsx
│   │   │   ├── WebsitesPage.tsx
│   │   │   ├── AccountsPage.tsx
│   │   │   ├── ApiKeysPage.tsx
│   │   │   ├── RecycleBinPage.tsx
│   │   │   ├── SearchPage.tsx
│   │   │   └── ApiDocumentationPage.tsx
│   │   ├── services/                # API服务
│   │   │   ├── apiClient.ts
│   │   │   ├── authService.ts
│   │   │   ├── websiteService.ts
│   │   │   ├── accountService.ts
│   │   │   ├── apiKeyService.ts
│   │   │   ├── searchService.ts
│   │   │   ├── recycleBinService.ts
│   │   │   └── passwordGeneratorService.ts
│   │   ├── hooks/                   # 自定义Hooks
│   │   ├── contexts/                # React Context
│   │   ├── types/                   # TypeScript类型
│   │   ├── stores/                  # 状态管理
│   │   ├── utils/                   # 工具函数
│   │   ├── lib/                     # 库函数
│   │   ├── test/                    # 测试配置
│   │   ├── App.tsx                  # 应用入口
│   │   └── main.tsx                 # 主入口
│   ├── tests/                       # 测试文件
│   ├── public/                      # 静态资源
│   ├── package.json
│   ├── vite.config.ts
│   ├── vitest.config.ts
│   ├── tsconfig.json
│   └── tailwind.config.ts
├── specs/                           # 功能规范
│   ├── 001-mvp/                     # MVP功能
│   ├── 002-http-localhost-5173/     # 网站列表表格视图
│   ├── 003-15/                      # 账号列表表格视图
│   ├── 006-api-management/          # API密钥管理
│   ├── 007-accountbox-web-jwt/      # Web前端JWT认证
│   └── ENCRYPTION_REMOVAL_SUMMARY.md
├── docker-compose.yml               # Docker编排
├── docker-compose.single.yml        # 单容器编排
├── Dockerfile                       # Docker镜像
├── start.sh                         # 启动脚本
├── reset_db.sh                      # 数据库重置脚本
├── build-docker.sh                  # Docker构建脚本
├── CLAUDE.md                        # 开发指南
├── DOCKER.md                        # Docker文档
└── Augment-Memories.md              # 本文件
```

---

## 核心功能模块

### 1. 账号管理系统 (001-mvp)
- **网站管理**: CRUD操作、标签分类、分页查询
- **账号管理**: 创建、编辑、删除账号，支持用户名、密码、备注、标签
- **软删除与回收站**: 账号支持软删除，可从回收站恢复或永久删除
- **全文搜索**: 快速搜索账号和网站
- **密码生成器**: 生成强密码（可配置长度、字符类型）

### 2. 存储模式（明文存储 - 2025-10-17 架构变更）
- **⚠️ 重要**: 从加密模式切换为明文存储模式
- **存储方式**: 密码直接以明文形式存储在数据库中
- **适用场景**: 个人自托管环境，依赖其他安全措施保护
- **安全建议**:
  - 仅在 localhost 或 VPN 环境访问
  - 启用防火墙保护
  - 使用磁盘加密保护数据库文件
  - 定期加密备份数据库
- **历史记录**:
  - 之前版本使用 AES-256-GCM 加密 + Argon2id 密钥派生
  - 2025-10-17 移除加密以简化架构，适配个人项目需求

### 3. Web前端JWT认证系统 (007-accountbox-web-jwt)
- **主密码登录**: 用户使用主密码登录，获取JWT Token
- **Token管理**: Token有效期24小时，存储在localStorage
- **路由保护**: 未登录用户自动重定向到登录页面
- **自动Token刷新**: 请求拦截器自动附加Bearer Token
- **登录失败记录**: SQLite存储登录失败记录（用于防暴力破解）

### 4. API密钥管理 (006-api-management)
- **API密钥生成**: sk_前缀 + 32位随机字符，使用BCrypt哈希存储
- **作用域控制**: 支持All（全部网站）或Specific（特定网站）两种作用域
- **账号状态管理**: 独立于删除操作的启用/禁用状态
- **扩展字段**: JSON键值对存储 (10KB限制)
- **外部API**:
  - 获取随机启用账号: `GET /api/external/websites/{websiteId}/accounts/random`
  - 获取可访问网站列表: `GET /api/external/websites`
  - 需要X-API-Key请求头认证

### 5. UI优化
- **表格视图**: 网站列表和账号列表支持表格布局 (002-http-localhost-5173, 003-15)
- **响应式设计**: 支持多种屏幕尺寸
- **shadcn/ui组件**: 统一的UI体验
- **登录页面**: 美观的主密码输入界面

---

## 关键数据模型

### Website (网站)
```
- Id: int (主键)
- Domain: string (域名)
- DisplayName: string (显示名称)
- Tags: string (标签，逗号分隔)
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### Account (账号)
```
- Id: int (主键)
- WebsiteId: int (外键)
- Username: string (用户名)
- Password: string (明文存储 - 2025-10-17 架构变更)
- Notes: string (备注)
- Tags: string (标签，逗号分隔)
- Status: AccountStatus (启用/禁用)
- ExtendedData: string (JSON格式，最大10KB)
- IsDeleted: bool (软删除标记)
- DeletedAt: DateTime? (删除时间)
- CreatedAt: DateTime
- UpdatedAt: DateTime
```

### ApiKey (API密钥)
```
- Id: int (主键)
- KeyHash: string (BCrypt哈希)
- DisplayName: string (显示名称)
- ScopeType: ApiKeyScopeType (All 或 Specific)
- CreatedAt: DateTime
- LastUsedAt: DateTime?
- ApiKeyWebsiteScopes: List<ApiKeyWebsiteScope> (作用域关联)
```

### ApiKeyWebsiteScope (API密钥作用域)
```
- Id: int (主键)
- ApiKeyId: int (外键)
- WebsiteId: int (外键)
```

### LoginAttempt (登录尝试记录)
```
- Id: int (主键)
- Timestamp: DateTime (尝试时间)
- IsSuccessful: bool (是否成功)
- IpAddress: string (IP地址)
```

### ~~KeySlot (密钥槽)~~ - 已废弃
- ~~EncryptedVaultKey, Argon2Salt, Argon2Iterations, Argon2MemorySize, Argon2Parallelism~~
- ⚠️ 此表已于 2025-10-17 从数据库中删除

### 枚举类型

**AccountStatus**
```
- Enabled = 0 (启用)
- Disabled = 1 (禁用)
```

**ApiKeyScopeType**
```
- All = 0 (所有网站)
- Specific = 1 (特定网站)
```

---

## 启动与开发

### 启动服务

#### 方式1: 使用启动脚本 (推荐)
```bash
./start.sh
```

#### 方式2: 手动启动
```bash
# 终端1: 后端
cd backend/src/AccountBox.Api
dotnet run

# 终端2: 前端
cd frontend
pnpm install  # 首次需要安装依赖
pnpm dev
```

#### 方式3: Docker启动
```bash
# 单容器模式
docker-compose -f docker-compose.single.yml up

# 或多容器模式
docker-compose up
```

### 访问地址
- **前端应用**: http://localhost:5173
- **后端API**: http://localhost:5093
- **Swagger文档**: http://localhost:5093/swagger
- **登录页面**: http://localhost:5173 (需要输入主密码)

### 数据库操作

#### 数据库重置
```bash
./reset_db.sh
```

#### 数据库迁移
```bash
cd backend/src/AccountBox.Api
dotnet ef database update
```

#### 创建新迁移
```bash
cd backend/src/AccountBox.Api
dotnet ef migrations add <MigrationName>
```

### 环境配置

#### 后端配置 (appsettings.json)
```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:5093"]
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "AccountBox",
    "Audience": "AccountBox",
    "ExpirationMinutes": 1440
  }
}
```

#### 前端配置 (vite.config.ts)
- API代理: `/api/*` → `http://localhost:5093`
- 开发服务器端口: 5173

### 主密码设置
- **默认主密码**: 在 `appsettings.json` 中配置
- **登录流程**: 输入主密码 → 获取JWT Token → 访问应用

---

## 开发规范

### 代码风格

#### 后端 (C#)
- **命名规范**:
  - 类/方法: PascalCase
  - 变量/参数: camelCase
  - 常量: UPPER_SNAKE_CASE
  - 私有字段: _camelCase
- **格式化**: `dotnet format`
- **注释**: XML文档注释 (`/// <summary>`)
- **异常处理**: 使用自定义异常类

#### 前端 (TypeScript/React)
- **命名规范**:
  - 组件: PascalCase (如 `AccountForm.tsx`)
  - 工具函数: camelCase (如 `formatDate.ts`)
  - 常量: UPPER_SNAKE_CASE
  - 类型/接口: PascalCase
- **格式化**: `pnpm format` (Prettier)
- **类型检查**: `pnpm type-check`
- **Linting**: `pnpm lint` (ESLint)
- **注释**: JSDoc注释

### Git提交规范
- **格式**: `<type>[scope]: <description>`
- **类型**: feat, fix, docs, style, refactor, test, chore
- **语言**: 中文
- **示例**:
  - `feat(auth): 添加JWT认证系统`
  - `fix(account): 修复账号删除bug`
  - `docs: 更新README文档`

### 测试规范

#### 后端测试
- **框架**: xUnit
- **Mock库**: Moq
- **断言**: FluentAssertions
- **目标覆盖率**: 85%+
- **位置**: `backend/tests/`

#### 前端测试
- **单元测试**: Vitest + @testing-library/react
- **E2E测试**: Playwright
- **位置**: `frontend/tests/`
- **运行**: `pnpm test` 或 `pnpm test:run`

### 依赖管理
- **后端**: NuGet包管理 (使用 `dotnet add package`)
- **前端**: pnpm包管理 (使用 `pnpm add` / `pnpm remove`)
- **禁止**: 不使用npm/yarn，不手动编辑package.json

### API设计规范
- **RESTful**: 遵循REST设计原则
- **响应格式**: 统一的ApiResponse<T>包装
- **错误处理**: 统一的错误码和错误消息
- **认证**:
  - 内部API: JWT Bearer Token
  - 外部API: X-API-Key请求头
- **文档**: Swagger自动生成

---

## 常见问题与解决方案

### Tailwind CSS问题
- 参考: `tailwind问题处理.md`
- 版本: Tailwind CSS 4.1.14
- 配置文件: `frontend/tailwind.config.ts`

### 认证相关问题

#### Web前端认证
- **方式**: JWT Token 登录 (功能007-accountbox-web-jwt)
- **流程**: 输入主密码 → 后端验证 → 返回JWT Token → 存储到localStorage
- **Token有效期**: 24小时
- **自动刷新**: 请求拦截器自动附加Bearer Token
- **登出**: 清除localStorage中的Token

#### 外部API认证
- **方式**: API Key (X-API-Key 请求头)
- **格式**: `sk_` 前缀 + 32位随机字符
- **存储**: BCrypt哈希存储
- **作用域**: All 或 Specific

### 数据库相关问题

#### 数据库重置
```bash
./reset_db.sh
```

#### 数据库路径配置
- **环境变量**: `DATABASE_PATH`
- **默认路径**: `backend/src/AccountBox.Api/accountbox.db`
- **连接字符串**: `appsettings.json` 中的 `DefaultConnection`

#### 迁移问题
```bash
# 查看待应用迁移
cd backend/src/AccountBox.Api
dotnet ef migrations list

# 回滚到上一个迁移
dotnet ef database update <PreviousMigrationName>

# 删除最后一个迁移
dotnet ef migrations remove
```

### 端口配置问题
- **前端端口**: 5173 (Vite开发服务器)
- **后端端口**: 5093 (ASP.NET Core)
- **CORS配置**: `appsettings.json` 中的 `Cors:AllowedOrigins`
- **代理配置**: `frontend/vite.config.ts` 中的 `/api` 代理

### 依赖问题

#### 前端依赖安装失败
```bash
cd frontend
rm -rf node_modules pnpm-lock.yaml
pnpm install
```

#### 后端依赖更新
```bash
cd backend/src/AccountBox.Api
dotnet restore
```

### 性能优化建议
- **前端**: 使用React.memo优化组件渲染
- **后端**: 使用分页查询，避免一次加载大量数据
- **数据库**: 为常用查询字段添加索引

---

## 功能规范文档

所有功能规范存储在 `specs/` 目录，每个功能分支对应一个目录：

### 001-mvp (MVP功能)
- **spec.md**: 账号管理系统需求规范
- **plan.md**: 实现计划
- **data-model.md**: 数据模型定义
- **quickstart.md**: 快速启动指南
- **tasks.md**: 任务分解

### 002-http-localhost-5173 (网站列表表格视图)
- 网站列表支持表格布局
- 分页、排序、搜索功能

### 003-15 (账号列表表格视图)
- 账号列表支持表格布局
- 分页、排序、搜索功能

### 006-api-management (API密钥管理)
- **spec.md**: API密钥管理需求规范
- **plan.md**: 实现计划
- **data-model.md**: 数据模型定义
- **research.md**: 技术研究
- **contracts/**: API契约定义

### 007-accountbox-web-jwt (Web前端JWT认证)
- **spec.md**: JWT认证需求规范
- **plan.md**: 实现计划
- **data-model.md**: 数据模型定义
- **research.md**: JWT最佳实践研究
- **contracts/**: API契约定义

### ENCRYPTION_REMOVAL_SUMMARY.md
- 加密系统移除的详细说明
- 迁移指南

---

## 重要提示

### 版本要求
1. **后端**: .NET 8.0 (不是10)
2. **前端**: Node >= 20.19.0, TypeScript 5.9.3
3. **包管理**: 前端使用pnpm，不要使用npm或yarn

### Git提交规范
1. **不要修改.git目录**
2. **约定式提交**: `<type>[scope]: <description>`
3. **语言**: 中文
4. **频率**: 每完成一个任务就要git commit提交代码
5. **示例**: `feat(auth): 添加JWT认证系统`

### 开发规范
1. **明文存储模式**（2025-10-17起）：密码以明文存储，仅适用于个人自托管环境
2. **修改完成后不需要写README**
3. **创建项目、获取最新SDK时优先调用Context7 MCP**
4. **前端测试调用Playwright**
5. **shadcn UI 库的使用调用shadcn MCP**
6. **不需要编写测试，除非用户主动要求**
7. **不需要运行程序，除非用户主动要求**

### 部署配置
1. **CORS配置**: 从 `appsettings.json` 读取 `Cors:AllowedOrigins`
2. **端口映射**: 支持可配置的主机端口 (如 5094, 15001等)
3. **数据库路径**: 支持 `DATABASE_PATH` 环境变量配置
4. **JWT配置**: 从 `appsettings.json` 读取 `JwtSettings`

---

## 重要架构变更记录

### 2025-10-17: 移除加密系统 + 实现JWT认证
- **变更原因**: 项目定位为个人小型项目，加密系统过于复杂，且影响外部API使用；同时实现Web前端JWT认证
- **变更内容**:
  - ✅ 移除 AES-256-GCM 加密
  - ✅ 移除 Argon2id 密钥派生
  - ✅ 移除 Vault 解锁系统
  - ✅ 移除 KeySlot 表和相关组件
  - ✅ Account 表从加密字段改为明文 Password 字段
  - ✅ ApiKey 移除 VaultId 外键
  - ✅ 简化所有 Service 和 Controller（移除 vaultKey 参数）
  - ✅ 实现JWT认证系统 (JwtService, AuthController)
  - ✅ 添加LoginAttempt表用于记录登录失败
  - ✅ 前端实现登录页面和路由保护
  - ✅ 实现API密钥管理 (BCrypt哈希, 作用域控制)
  - ✅ 实现外部API端点 (ExternalApiController)

- **影响范围**:
  - 后端：
    - 所有加密相关代码已移除
    - 新增JWT认证中间件和服务
    - 新增API密钥认证中间件
    - 数据库迁移 `20251017021759_RemoveEncryption` 已应用
  - 数据库：
    - 移除 KeySlot 表
    - 新增 LoginAttempt 表
    - Account 表: Password 字段改为明文
    - ApiKey 表: 移除 VaultId 字段
  - 前端：
    - ✅ 新增登录页面 (LoginPage.tsx)
    - ✅ 新增认证服务 (authService.ts)
    - ✅ 新增路由保护
    - ✅ 新增API密钥管理页面 (ApiKeysPage.tsx)
    - ✅ 移除 Vault 解锁页面和相关服务

- **安全建议**:
  - 仅在可信环境使用（localhost 或 VPN）
  - 启用防火墙保护
  - 使用磁盘加密保护数据库文件
  - 定期加密备份数据库
  - 主密码应设置为强密码

- **相关文档**: 详见 `specs/ENCRYPTION_REMOVAL_SUMMARY.md`

### 技术栈更新
- **后端**: .NET 8.0 (之前文档误写为10)
- **前端**: Node >= 20.19.0 (之前为24.10.0)
- **新增依赖**:
  - BCrypt.Net-Next 4.0.3
  - Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11
  - System.IdentityModel.Tokens.Jwt 8.14.0
  - Entity Framework Core 9.0.10

---

## 最后更新

- **日期**: 2025-10-17
- **版本**: 2.0 (完整的认证和API管理系统)
- **维护者**: Augment Agent
- **状态**: 生产就绪 (Production Ready)

