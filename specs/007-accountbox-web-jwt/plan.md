# Implementation Plan: Web前端JWT身份认证系统

**Branch**: `007-accountbox-web-jwt` | **Date**: 2025-10-17 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-accountbox-web-jwt/spec.md`

## Summary

实现基于JWT Token的Web前端身份认证系统,使用主密码作为登录凭证。后端使用ASP.NET Core JWT Bearer认证保护所有内部API端点,前端使用React实现登录页面、Token管理和路由保护。Token有效期24小时,存储在localStorage中实现持久化登录。

## Technical Context

**Language/Version**:
- Backend: .NET 9.0
- Frontend: TypeScript 5.9.3 + React 19

**Primary Dependencies**:
- Backend: ASP.NET Core 9.0, Microsoft.AspNetCore.Authentication.JwtBearer, System.IdentityModel.Tokens.Jwt
- Frontend: React 19, react-router-dom 7.9.4, axios 1.12.2, shadcn/ui

**Storage**: SQLite（通过Entity Framework Core,用于存储登录失败记录）

**Testing**:
- Backend: xUnit
- Frontend: Vitest + React Testing Library

**Target Platform**: Web Application（跨平台,浏览器访问）

**Project Type**: Web（frontend + backend分离架构）

**Performance Goals**:
- 登录响应时间 < 500ms
- Token验证开销 < 10ms
- 前端登录流程 < 10秒

**Constraints**:
- Token必须在24小时后过期
- 登录失败5次后实施1分钟冷却期
- 所有Token相关操作必须线程安全

**Scale/Scope**:
- 单用户系统（个人使用）
- 支持多设备同时登录
- 无并发登录数量限制

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Frontend Development Standards ✅

- **React Function Components**: 将使用function components和Hooks（登录页面、路由守卫）
- **TypeScript Strict Mode**: 已启用
- **Vite**: 已使用Vite 7.1.7
- **pnpm**: 已使用pnpm包管理器
- **shadcn/ui**: 将使用Button、Input、Card等组件构建登录界面
- **Code Conventions**: 遵循PascalCase组件命名、TypeScript接口定义
- **Directory Structure**: 遵循现有结构（components/、services/、pages/）

### Principle II: Backend Development Standards ✅

- **NET 9.0**: 满足要求（使用.NET 9.0）
- **Clean Architecture**: 遵循现有分层结构（Api、Core、Data、Security）
- **ASP.NET Core**: 已使用
- **Entity Framework Core**: 已使用（用于存储登录失败记录）
- **DI Container**: 将使用内置DI注册JWT服务和认证服务
- **Naming Guidelines**: 遵循Microsoft命名规范
- **SOLID Principles**: Controller不超过3个参数,使用DTO传递登录请求

### Principle III: Code Quality & Testing ✅

- **API Response Format**: 将使用统一的`ApiResponse<T>`包装器
- **System.Text.Json**: 已使用作为默认序列化器
- **xUnit**: 将编写JwtService和AuthController的单元测试
- **Test Coverage**: 目标覆盖率≥85%
- **SonarAnalyzer**: 将在CI/CD中集成静态分析

### Principle IV: Security & Data Protection ✅

- **Input Validation**: 将使用Data Annotations验证登录请求模型
- **Async Programming**: 所有数据库操作使用async/await
- **Password Hashing**: 复用现有的Argon2id主密码验证逻辑（来自IVaultManager.Unlock）
- **Secret Management**: JWT签名密钥存储在appsettings.json中,生产环境使用环境变量
- **SQL Injection Prevention**: 使用EF Core LINQ查询

### Principle V: Git Commit Standards ✅

- **Commit Language**: 所有commit消息将使用中文
- **Conventional Commits**: 遵循`feat[auth]:`、`fix[auth]:`等格式
- **Per-Task Commit**: 每完成一个task立即commit

### Constitution Compliance Summary

**Status**: ✅ **PASS** - All principles complied

**Justification**: 本功能完全符合项目章程要求,无需任何例外或豁免。JWT认证是标准Web安全实践,符合Clean Architecture原则。

## Project Structure

### Documentation (this feature)

```
specs/007-accountbox-web-jwt/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── auth-api.yaml    # OpenAPI specification for auth endpoints
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
backend/
├── src/
│   ├── AccountBox.Api/
│   │   ├── Controllers/
│   │   │   └── AuthController.cs              # 新增：登录、登出端点
│   │   ├── Services/
│   │   │   └── JwtService.cs                  # 新增：JWT Token生成和验证
│   │   ├── Middleware/
│   │   │   └── LoginThrottleMiddleware.cs     # 新增：登录失败限制中间件
│   │   └── Program.cs                          # 修改：添加JWT认证配置
│   ├── AccountBox.Core/
│   │   ├── Models/
│   │   │   └── Auth/
│   │   │       ├── LoginRequest.cs            # 新增：登录请求DTO
│   │   │       ├── LoginResponse.cs           # 新增：登录响应DTO
│   │   │       └── JwtSettings.cs             # 新增：JWT配置模型
│   │   └── Interfaces/
│   │       └── IJwtService.cs                 # 新增：JWT服务接口
│   └── AccountBox.Data/
│       ├── Entities/
│       │   └── LoginAttempt.cs                # 新增：登录失败记录实体
│       └── DbContext/
│           └── AccountBoxDbContext.cs         # 修改：添加LoginAttempts DbSet
└── tests/
    └── AccountBox.Api.Tests/
        ├── AuthControllerTests.cs             # 新增：认证控制器测试
        └── JwtServiceTests.cs                 # 新增：JWT服务测试

frontend/
├── src/
│   ├── components/
│   │   └── auth/
│   │       └── ProtectedRoute.tsx             # 新增：路由守卫组件
│   ├── pages/
│   │   └── LoginPage.tsx                      # 新增：登录页面
│   ├── services/
│   │   └── authService.ts                     # 新增：认证服务（登录、登出、Token管理）
│   ├── lib/
│   │   └── axios.ts                           # 修改：添加Token拦截器
│   ├── App.tsx                                # 修改：添加路由保护逻辑
│   └── main.tsx                               # 修改：添加认证状态检查
└── tests/
    └── auth/
        ├── LoginPage.test.tsx                 # 新增：登录页面测试
        └── authService.test.ts                # 新增：认证服务测试
```

**Structure Decision**: 采用现有的Web应用架构（frontend + backend分离）。后端遵循Clean Architecture分层,前端遵循React最佳实践目录结构。新增代码集中在auth相关模块,最小化对现有代码的侵入性。

## Complexity Tracking

*No constitutional violations - this section is not required.*
