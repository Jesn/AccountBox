# Implementation Plan: 账号管理系统 MVP

**Branch**: `001-mvp` | **Date**: 2025-10-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-mvp/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

本功能实现一个本地加密的账号密码管理系统，支持网站和账号的CRUD操作、软删除与回收站、全文搜索、密码生成器以及基于信封加密的本地存储。系统采用前后端分离架构：后端使用C#/.NET 10 Web API提供RESTful服务，前端使用React + TypeScript构建用户界面，所有敏感数据通过AES-256-GCM加密存储在本地SQLite数据库中。

## Technical Context

**后端技术栈**：
- **Language/Version**: C# / .NET 10
- **架构模式**: 分层架构（Controller → Service → Repository → Data）
- **Primary Dependencies**:
  - ASP.NET Core Web API
  - Entity Framework Core
  - Argon2id KDF库（用于密钥派生）
  - 加密库（System.Security.Cryptography for AES-256-GCM）
- **Storage**: SQLite（默认）；支持PostgreSQL/MySQL
- **Testing**: xUnit, Moq, FluentAssertions
- **Target Platform**: 跨平台（Windows, macOS, Linux）

**前端技术栈**：
- **Language/Version**: TypeScript（启用严格模式）
- **Framework**: React 18+
- **UI Library**: shadcn/ui组件库
- **Styling**: Tailwind CSS
- **Testing**: Jest, React Testing Library, Playwright（E2E）
- **Development Tools**: shadcn/context7工具链
- **命名规范**: 组件使用PascalCase，工具函数使用camelCase
- **Target Platform**: 现代浏览器（Chrome, Firefox, Safari, Edge）

**加密与安全**：
- **加密算法**: AES-256-GCM（业务数据加密）
- **密钥派生**: Argon2id KDF（主密码 → KEK）
- **密钥管理**: 信封加密（VaultKey加密数据，KEK加密VaultKey）

**Project Type**: Web应用（前后端分离）
**Performance Goals**:
- 搜索响应时间 < 0.5秒（1000条记录）
- 分页加载 < 1秒（1000条记录）
- 应用启动解锁 < 3秒
- 支持至少1000个账号流畅管理

**Constraints**:
- 本地存储，无网络依赖
- VaultKey仅驻留内存，不得持久化
- 所有敏感数据必须加密存储
- 单用户单设备场景

**Scale/Scope**:
- 用户规模：单用户
- 数据量：支持1000+账号
- 功能模块：6个核心用户故事，43个功能需求
- 界面：约15-20个React组件

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**状态**: ✅ 通过

**Constitution Version**: 1.0.0 (Ratified: 2025-10-15)

**验证结果**：当前实施计划完全符合AccountBox Constitution的所有核心原则：

✅ **I. Frontend Development Standards**:
- React函数组件 + Hooks模式
- TypeScript严格模式
- Vite 5+构建工具
- pnpm包管理器
- shadcn/ui + Tailwind CSS
- 符合目录结构规范（components/, hooks/, services/, types/等）

✅ **II. Backend Development Standards**:
- .NET 10 LTS版本
- 分层架构（Controller → Service → Repository → Data）
- ASP.NET Core Web API
- Entity Framework Core Code First
- 内置依赖注入
- 接口命名遵循`I`前缀规范

✅ **III. Code Quality & Testing**:
- 统一`ApiResponse<T>`返回格式（需在实施阶段创建）
- System.Text.Json序列化器
- xUnit测试框架
- 目标测试覆盖率：85%+（已在tasks.md中规划）
- 已规划SonarAnalyzer集成

✅ **IV. Security & Data Protection**:
- 密码使用Argon2id（宪法允许,推荐用于密钥派生场景）
- EF Core参数化查询（防SQL注入）
- 异步数据库操作
- 敏感配置使用环境变量（appsettings.Development.json）

✅ **V. Git Commit Standards**:
- 已采用约定式提交格式，git commit 内容用中文
- `.git`目录不修改

**无违规项** - 设计完全符合宪法要求，无需复杂度豁免。

## Project Structure

### Documentation (this feature)

```
specs/001-mvp/
├── spec.md              # 功能规格说明（已完成）
├── plan.md              # 本文件（实施计划）
├── research.md          # 阶段0输出：技术研究与决策
├── data-model.md        # 阶段1输出：数据模型设计
├── quickstart.md        # 阶段1输出：快速入门指南
├── contracts/           # 阶段1输出：API契约定义
│   ├── websites.yaml    # 网站管理API
│   ├── accounts.yaml    # 账号管理API
│   ├── recycle-bin.yaml # 回收站API
│   ├── search.yaml      # 搜索API
│   ├── password-gen.yaml # 密码生成器API
│   └── vault.yaml       # 加密存储API
├── checklists/
│   └── requirements.md  # 规格说明质量检查清单（已完成）
└── tasks.md             # 阶段2输出（/speckit.tasks命令生成）
```

### Source Code (repository root)

```
backend/
├── src/
│   ├── AccountBox.Api/          # Web API项目
│   │   ├── Controllers/         # API控制器
│   │   ├── Middleware/          # 中间件（加密、异常处理）
│   │   └── Program.cs           # 应用入口
│   ├── AccountBox.Core/         # 核心业务逻辑
│   │   ├── Services/            # 业务服务层
│   │   ├── Interfaces/          # 服务接口
│   │   └── Models/              # 业务模型（DTO）
│   ├── AccountBox.Data/         # 数据访问层
│   │   ├── Entities/            # EF Core实体
│   │   ├── Repositories/        # 仓储实现
│   │   ├── DbContext/           # 数据库上下文
│   │   └── Migrations/          # EF迁移文件
│   └── AccountBox.Security/     # 加密模块
│       ├── Encryption/          # 加密服务（AES-256-GCM）
│       ├── KeyDerivation/       # Argon2id KDF
│       └── VaultManager/        # 密钥管理（信封加密）
└── tests/
    ├── AccountBox.Api.Tests/           # API集成测试
    ├── AccountBox.Core.Tests/          # 业务逻辑单元测试
    ├── AccountBox.Data.Tests/          # 数据访问测试
    └── AccountBox.Security.Tests/      # 加密模块测试

frontend/
├── src/
│   ├── components/              # React组件
│   │   ├── websites/            # 网站管理组件
│   │   ├── accounts/            # 账号管理组件
│   │   ├── recycle-bin/         # 回收站组件
│   │   ├── search/              # 搜索组件
│   │   ├── password-generator/  # 密码生成器组件
│   │   └── common/              # 通用组件（分页、对话框等）
│   ├── pages/                   # 页面组件
│   ├── services/                # API服务（axios封装）
│   ├── hooks/                   # 自定义React Hooks
│   ├── utils/                   # 工具函数（加密客户端等）
│   ├── types/                   # TypeScript类型定义
│   └── App.tsx                  # 应用入口
└── tests/
    ├── unit/                    # Jest单元测试
    ├── integration/             # React Testing Library测试
    └── e2e/                     # Playwright端到端测试
```

**Structure Decision**:

选择**Web应用（前后端分离）**结构，理由如下：

1. **后端（backend/）**：
   - 采用分层架构，符合用户指定的Controller → Service → Repository → Data模式
   - 独立的Security项目封装加密逻辑，便于单元测试和复用
   - EF Core + Repository模式便于支持多种数据库（SQLite/PostgreSQL/MySQL）

2. **前端（frontend/）**：
   - React + TypeScript符合用户指定的技术栈
   - 组件按功能模块组织（websites/accounts/recycle-bin等），对应功能需求
   - shadcn/ui + Tailwind CSS提供一致的UI体验

3. **测试策略**：
   - 后端：xUnit单元测试 + 集成测试
   - 前端：Jest单元测试 + React Testing Library + Playwright E2E
   - 覆盖所有关键路径（加密、软删除、搜索等）

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

**无违规项** - 当前设计符合最佳实践，无需复杂度豁免。
