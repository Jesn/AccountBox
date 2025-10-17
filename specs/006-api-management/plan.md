# Implementation Plan: API密钥管理与外部API服务

**Branch**: `006-api-management` | **Date**: 2025-10-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/006-api-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

本功能为AccountBox添加完整的外部API服务能力，包括：
1. API密钥管理（可随时查看明文，支持作用域控制）
2. 账号启用/禁用状态管理（独立于删除操作）
3. 账号扩展字段（JSON键值对，10KB限制）
4. RESTful API接口（账号CRUD、随机获取启用账号）
5. **⚠️ 明文存储模式**（2025-10-17架构变更）

技术方案：
- 后端：基于现有ASP.NET Core 8.0架构扩展，新增ApiKey实体、中间件进行密钥验证
- 前端：React 19 + shadcn/ui实现密钥管理UI和账号状态管理UI
- 数据库：EF Core Code First迁移添加新表和字段
- API密钥采用"sk_"前缀+32位随机字符，存储明文和哈希值
- **存储模式**：明文存储密码（2025-10-17架构变更），简化架构，适用于个人自托管场景

## Technical Context

**Language/Version**:
- Backend: .NET 8.0 (ASP.NET Core)
- Frontend: TypeScript 5.9.3 + React 19

**Primary Dependencies**:
- Backend: ASP.NET Core 8.0, Entity Framework Core 9.0, BCrypt.Net (密钥哈希)
- Frontend: React 19, Vite 7, shadcn/ui, Tailwind CSS 4, axios 1.12

**Storage**:
- SQLite (开发环境) / PostgreSQL (生产环境)
- 新增表：ApiKeys, ApiKeyWebsiteScopes
- 扩展表：Accounts (新增Status和ExtendedData字段)

**Testing**:
- Backend: xUnit
- Frontend: Vitest + @testing-library/react

**Target Platform**:
- Backend: Linux server / Docker容器
- Frontend: 现代浏览器（Chrome/Firefox/Edge 最新版）

**Project Type**: Web application (frontend + backend)

**Performance Goals**:
- API响应时间：< 500ms (p95)
- 并发请求：支持100+并发
- 随机获取账号：均匀分布（统计学意义）

**Constraints**:
- 扩展字段大小：≤ 10KB
- API密钥明文可随时查看（个人工具场景）
- 不支持批量操作（初版）
- 不实施速率限制（初版）
- **⚠️ 存储模式**（2025-10-17架构变更）：
  - 明文存储密码，简化架构
  - 适用于个人自托管场景
  - 安全依赖：防火墙、磁盘加密、VPN/localhost访问、定期加密备份

**Scale/Scope**:
- 预计API密钥数量：< 100个/用户
- 预计账号数量：< 10,000个/用户
- 扩展字段：键值对模式，无嵌套限制
- API端点：~10个新端点

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Frontend Standards (Principle I)
- [x] React function components + Hooks
- [x] TypeScript strict mode
- [x] Vite 7+ 作为构建工具
- [x] pnpm 作为包管理器
- [x] PascalCase组件命名
- [x] shadcn/ui + Tailwind CSS
- [x] 标准目录结构 (components/, features/, services/)

### ✅ Backend Standards (Principle II)
- [x] .NET 8.0 (符合LTS要求)
- [x] Clean Architecture分层
- [x] ASP.NET Core Web API
- [x] Entity Framework Core Code First
- [x] 内置DI容器
- [x] Microsoft命名规范
- [x] SOLID原则

### ✅ Code Quality & Testing (Principle III)
- [x] ApiResponse<T>统一返回格式
- [x] System.Text.Json序列化
- [x] xUnit单元测试
- [ ] **TODO**: 目标85%测试覆盖率（需在实现阶段达成）
- [ ] **TODO**: SonarAnalyzer.CSharp静态分析（需配置）

### ✅ Security & Data Protection (Principle IV)
- [x] API输入验证（FluentValidation / Data Annotations）
- [x] 异步数据库操作
- [x] 参数化查询（EF Core LINQ）
- [x] BCrypt哈希API密钥（用于验证）
- [x] 明文存储API密钥（用于UI查看，符合个人工具场景）
- [x] 环境变量管理敏感配置

### ✅ Git Commit Standards (Principle V)
- [x] 中文commit消息（type关键字英文）
- [x] Conventional Commits格式
- [x] 每任务完成后立即提交
- [x] 更新tasks.md标记完成

### 📋 Complexity Tracking

**无章程违规**。本功能完全符合现有架构和标准。

## Project Structure

### Documentation (this feature)

```
specs/006-api-management/
├── plan.md              # 本文件 (/speckit.plan输出)
├── research.md          # 阶段0输出（技术调研）
├── data-model.md        # 阶段1输出（数据模型）
├── quickstart.md        # 阶段1输出（快速开始）
├── contracts/           # 阶段1输出（API契约）
│   ├── api-keys.yaml
│   ├── accounts-extended.yaml
│   └── random-account.yaml
└── tasks.md             # 阶段2输出 (/speckit.tasks - NOT by /speckit.plan)
```

### Source Code (repository root)

```
backend/
├── src/
│   ├── AccountBox.Core/
│   │   ├── Models/
│   │   │   ├── ApiKey.cs                  # 新增API密钥实体
│   │   │   ├── ApiKeyWebsiteScope.cs      # 新增作用域关联
│   │   │   └── Account.cs                 # 扩展：Status + ExtendedData字段
│   │   ├── Enums/
│   │   │   ├── AccountStatus.cs           # 新增：Active/Disabled
│   │   │   └── ApiKeyScopeType.cs         # 新增：All/Specific
│   │   └── Services/
│   │       └── IApiKeyService.cs          # 新增密钥验证服务接口
│   ├── AccountBox.Data/
│   │   ├── Migrations/                     # EF Core迁移文件
│   │   └── ApplicationDbContext.cs        # 添加新DbSet
│   ├── AccountBox.Api/
│   │   ├── Controllers/
│   │   │   ├── ApiKeysController.cs       # 新增：密钥管理
│   │   │   ├── AccountsController.cs      # 扩展：状态管理+扩展字段
│   │   │   └── ExternalApiController.cs   # 新增：外部API端点
│   │   ├── Middleware/
│   │   │   └── ApiKeyAuthMiddleware.cs    # 新增：密钥验证中间件
│   │   ├── DTOs/
│   │   │   ├── ApiKeyDto.cs
│   │   │   ├── CreateApiKeyRequest.cs
│   │   │   └── AccountExtendedDto.cs
│   │   └── Services/
│   │       ├── ApiKeyService.cs           # 密钥生成、验证、哈希
│   │       └── RandomAccountService.cs    # 随机账号选择
└── tests/
    ├── AccountBox.Api.Tests/
    │   ├── ApiKeysControllerTests.cs
    │   ├── ApiKeyAuthMiddlewareTests.cs
    │   └── ExternalApiControllerTests.cs
    └── AccountBox.Core.Tests/
        ├── ApiKeyServiceTests.cs
        └── RandomAccountServiceTests.cs

frontend/
├── src/
│   ├── components/
│   │   ├── api-keys/
│   │   │   ├── CreateApiKeyDialog.tsx     # 新增
│   │   │   ├── ApiKeyList.tsx             # 新增
│   │   │   └── DeleteApiKeyDialog.tsx     # 新增
│   │   ├── accounts/
│   │   │   ├── AccountStatusBadge.tsx     # 新增：状态标识
│   │   │   ├── ExtendedFieldsEditor.tsx   # 新增：键值对编辑器
│   │   │   └── AccountListItem.tsx        # 扩展：显示状态
│   │   └── ui/
│   │       └── badge.tsx                   # shadcn/ui组件
│   ├── services/
│   │   ├── apiKeyService.ts               # 新增
│   │   └── accountService.ts              # 扩展：状态操作
│   ├── types/
│   │   ├── ApiKey.ts                      # 新增类型定义
│   │   └── Account.ts                     # 扩展：status + extendedData
│   └── pages/
│       └── ApiKeysPage.tsx                # 新增页面
└── tests/
    ├── components/
    │   └── api-keys/
    │       └── ApiKeyList.test.tsx
    └── services/
        └── apiKeyService.test.ts
```

**Structure Decision**: 采用Option 2 (Web application)结构。项目已按backend/frontend分离，新功能扩展现有结构：
- Backend: 新增ApiKey相关层级，扩展Account实体和服务
- Frontend: 新增api-keys组件目录和服务文件，扩展账号管理组件

**Integration Points**:
- API密钥验证中间件集成到ASP.NET Core管道
- 扩展字段JSON列通过EF Core的HasColumnType("jsonb")或Column(TypeName = "TEXT")映射
- 前端路由添加`/api-keys`页面
- 账号列表UI集成状态过滤器

**Migration Strategy**:
- 数据库迁移添加ApiKeys、ApiKeyWebsiteScopes表
- 为Accounts表添加Status（默认Active）和ExtendedData（默认{}）列
- 现有账号默认状态为Active，ExtendedData为空JSON对象
