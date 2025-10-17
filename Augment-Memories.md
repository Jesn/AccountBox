# AccountBox 项目记忆库

## 项目概述

**项目名称**: AccountBox - 本地账号密码管理系统  
**项目类型**: 前后端分离的Web应用  
**主要目标**: 提供一个本地账号密码管理工具，主要是个人使用

---

## 技术栈

### 后端
- **语言**: C# / .NET 10 (ASP.NET Core)
- **架构**: 分层架构 (Controller → Service → Repository → Data)
- **数据库**: SQLite (开发) / PostgreSQL (生产)
- **ORM**: Entity Framework Core
- **测试**: xUnit, Moq, FluentAssertions
- **API文档**: Swagger

### 前端
- **语言**: TypeScript(严格模式)
- **框架**: React 19
- **构建工具**: Vite 7
- **UI库**: shadcn/ui + Radix UI
- **样式**: Tailwind CSS 4
- **HTTP客户端**: axios 1.12
- **路由**: react-router-dom 7.9.4
- **测试**: Vitest, @testing-library/react, Playwright (E2E)
- **包管理**: pnpm
- **Node版本**: >= 24.10.0

---

## 项目结构

```
AccountBox/
├── backend/
│   ├── src/
│   │   ├── AccountBox.Api/          # Web API项目
│   │   │   ├── Controllers/         # API控制器
│   │   │   ├── Middleware/          # 中间件
│   │   │   └── Program.cs           # 应用入口
│   │   ├── AccountBox.Core/         # 核心业务逻辑
│   │   │   ├── Services/            # 业务服务
│   │   │   ├── Interfaces/          # 服务接口
│   │   │   └── Models/              # DTO模型
│   │   ├── AccountBox.Data/         # 数据访问层
│   │   │   ├── Entities/            # EF Core实体
│   │   │   ├── Repositories/        # 仓储实现
│   │   │   ├── DbContext/           # 数据库上下文
│   │   │   └── Migrations/          # EF迁移
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
│   │   │   ├── accounts/            # 账号管理组件
│   │   │   ├── websites/            # 网站管理组件
│   │   │   ├── api-keys/            # API密钥管理
│   │   │   ├── recycle-bin/         # 回收站
│   │   │   ├── password-generator/  # 密码生成器
│   │   │   ├── vault/               # 保险库组件
│   │   │   ├── ui/                  # shadcn/ui组件
│   │   │   └── common/              # 通用组件
│   │   ├── pages/                   # 页面组件
│   │   ├── services/                # API服务
│   │   ├── hooks/                   # 自定义Hooks
│   │   ├── contexts/                # React Context
│   │   ├── types/                   # TypeScript类型
│   │   ├── stores/                  # 状态管理
│   │   ├── utils/                   # 工具函数
│   │   └── test/                    # 测试配置
│   ├── tests/                       # 测试文件
│   └── package.json
├── specs/                           # 功能规范
│   ├── 001-mvp/                     # MVP功能
│   ├── 002-http-localhost-5173/     # 网站列表表格视图
│   ├── 003-15/                      # 账号列表表格视图
│   ├── 005-api-api-1/               # API功能
│   └── 006-api-management/          # API密钥管理
├── start.sh                         # 启动脚本
├── reset_db.sh                      # 数据库重置脚本
├── CLAUDE.md                        # 开发指南
└── Augment-Memories.md              # 本文件
```

---

## 核心功能模块

### 1. 账号管理系统 (001-mvp)
- **网站管理**: CRUD操作、标签分类
- **账号管理**: 创建、编辑、删除账号，支持用户名、密码、备注、标签
- **软删除与回收站**: 账号支持软删除，可从回收站恢复或永久删除
- **全文搜索**: 快速搜索账号和网站
- **密码生成器**: 生成强密码

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

### 3. API密钥管理 (006-api-management)
- **API密钥生成**: sk_前缀 + 32位随机字符
- **作用域控制**: 支持限制API密钥的访问范围
- **账号状态管理**: 独立于删除操作的启用/禁用状态
- **扩展字段**: JSON键值对存储 (10KB限制)
- **RESTful API**: 账号CRUD、随机获取启用账号

### 4. UI优化
- **表格视图**: 网站列表和账号列表支持表格布局
- **响应式设计**: 支持多种屏幕尺寸
- **shadcn/ui组件**: 统一的UI体验

---

## 关键数据模型

### Website (网站)
- Id, Domain, DisplayName, Tags, CreatedAt, UpdatedAt

### Account (账号)
- Id, WebsiteId, Username, Password (明文), Notes, Tags, IsDeleted, DeletedAt, Status, ExtendedData
- ⚠️ 注意：Password 字段现为明文存储（2025-10-17 架构变更）

### ApiKey (API密钥)
- Id, KeyHash, KeyPlaintext, DisplayName, ScopeType, WebsiteIds, CreatedAt, LastUsedAt
- ⚠️ 注意：移除了 VaultId 字段（2025-10-17 架构变更）

### ~~KeySlot (密钥槽)~~ - 已废弃
- ~~EncryptedVaultKey, Argon2Salt, Argon2Iterations, Argon2MemorySize, Argon2Parallelism~~
- ⚠️ 此表已于 2025-10-17 从数据库中删除

---

## 启动与开发

### 启动服务
```bash
# 使用启动脚本 (推荐)
./start.sh

# 或手动启动
# 终端1: 后端
cd backend/src/AccountBox.Api
dotnet run

# 终端2: 前端
cd frontend
pnpm dev
```

### 访问地址
- 前端应用: http://localhost:5173
- 后端API: http://localhost:5093
- Swagger文档: http://localhost:5093/swagger

### 数据库重置
```bash
./reset_db.sh
```

---

## 开发规范

### 代码风格
- **后端**: C# 命名规范 (PascalCase for classes/methods, camelCase for variables)
- **前端**: 
  - 组件使用PascalCase
  - 工具函数使用camelCase
  - TypeScript严格模式
  - 遵循ESLint规则

### Git提交
- 使用约定式提交格式
- 提交信息用中文
- 示例: `feat: 添加账号搜索功能`

### 测试
- **后端**: xUnit框架，目标覆盖率85%+
- **前端**: Vitest + @testing-library/react
- **E2E**: Playwright

### 依赖管理
- **后端**: NuGet包管理
- **前端**: pnpm包管理 (不使用npm/yarn)

---

## 常见问题与解决方案

### Tailwind CSS问题
- 参考: `tailwind问题处理.md`

### 密码相关问题
- 参考: `密码问题说明.md`

---

## 功能规范文档

所有功能规范存储在 `specs/` 目录:
- **spec.md**: 功能需求规范
- **plan.md**: 实现计划
- **data-model.md**: 数据模型定义
- **quickstart.md**: 快速启动指南

---

## 重要提示

1. **不要修改.git目录**
2. **约定式提交规范 `git add . && git commit -m "<类型>[范围]: <描述>"`,commit 内容用中文**
3. **每完成一个任务就要git commit提交代码**
4. **前端使用pnpm**，不要使用npm或yarn
5. **后端使用.NET 10**，确保环境配置正确
6. **明文存储模式**（2025-10-17起）：密码以明文存储，仅适用于个人自托管环境
7. **修改完成后不需要写README**
8. **创建项目、获取最新SDK时优先调用Context7 MCP**
9. **前端测试调用Playwright**
10. **shadcn UI 库的使用调用shadcn mcp**

---

## 重要架构变更记录

### 2025-10-17: 移除加密系统
- **变更原因**: 项目定位为个人小型项目，加密系统过于复杂，且影响外部API使用
- **变更内容**:
  - 移除 AES-256-GCM 加密
  - 移除 Argon2id 密钥派生
  - 移除 Vault 解锁系统
  - 移除 KeySlot 表和相关组件
  - Account 表从加密字段改为明文 Password 字段
  - ApiKey 移除 VaultId 外键
  - 简化所有 Service 和 Controller（移除 vaultKey 参数）
- **影响范围**:
  - 后端：所有加密相关代码已移除
  - 数据库：迁移 `20251017021759_RemoveEncryption` 已应用
  - 前端：需要移除 Vault 解锁页面和相关服务（待处理）
- **安全建议**: 仅在可信环境使用，启用防火墙、磁盘加密、定期备份
- **相关文档**:
  - `REMOVE_ENCRYPTION_PLAN.md`
  - `REMOVE_ENCRYPTION_PROGRESS.md`

---

## 最后更新

- **日期**: 2025-10-17
- **版本**: 1.0
- **维护者**: Augment Agent

