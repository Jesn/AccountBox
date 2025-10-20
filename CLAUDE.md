## 项目概述

**项目名称**: AccountBox - 本地账号密码管理系统  
**项目类型**: 前后端分离的Web应用  
**主要目标**: 提供一个本地账号密码管理工具，主要是个人使用

---

## 技术栈

### 后端
- **语言**: C# / .NET 8.0 (ASP.NET Core)
- **架构**: 分层架构 (Controller → Service → Repository → Data)
- **数据库**: SQLite (开发) / PostgreSQL / MySQL (生产)
- **ORM**: Entity Framework Core 9.0
- **数据库驱动**:
  - SQLite: Microsoft.EntityFrameworkCore.Sqlite 9.0.10
  - PostgreSQL: Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4
  - MySQL: Pomelo.EntityFrameworkCore.MySql 9.0.0
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

## 启动与开发

### 方式1: 使用启动脚本

#### SQLite (默认开发环境)
```bash
./start.sh
```

#### PostgreSQL (生产环境)
```bash
./start-postgres.sh  # 启动 PostgreSQL + 后端 + 前端
```

#### MySQL (生产环境)
```bash
./start-mysql.sh  # 启动 MySQL + 后端 + 前端
```

### 方式2: 手动启动
```bash
# 终端1: 后端
cd backend/src/AccountBox.Api
dotnet run

# 终端2: 前端
cd frontend
pnpm install  # 首次需要安装依赖
pnpm dev
```

### 数据库配置

项目支持三种数据库：

#### SQLite (默认)
- **用途**: 本地开发
- **配置**: 无需额外配置，自动创建 `accountbox.db` 文件
- **迁移**: 自动应用

#### PostgreSQL
- **用途**: 生产环境
- **Docker启动**: `docker-compose -f docker-compose.postgres-test.yml up -d`
- **连接信息**:
  - 主机: localhost
  - 端口: 5432
  - 数据库: accountbox
  - 用户: accountbox
  - 密码: accountbox123
- **管理工具**: pgAdmin (http://localhost:5050)
- **环境变量**:
  ```bash
  export DB_PROVIDER=postgresql
  export CONNECTION_STRING="Host=localhost;Port=5432;Database=accountbox;Username=accountbox;Password=accountbox123"
  ```

#### MySQL
- **用途**: 生产环境
- **Docker启动**: `docker-compose -f docker-compose.mysql.yml up -d`
- **连接信息**:
  - 主机: localhost
  - 端口: 3306
  - 数据库: accountbox
  - 用户: accountbox
  - 密码: accountbox123
- **管理工具**: phpMyAdmin (http://localhost:8080)
- **环境变量**:
  ```bash
  export DB_PROVIDER=mysql
  export CONNECTION_STRING="Server=localhost;Port=3306;Database=accountbox;User=accountbox;Password=accountbox123"
  ```

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
