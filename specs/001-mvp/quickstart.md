# Quickstart Guide: 账号管理系统 MVP

**Feature**: 001-mvp | **Date**: 2025-10-14 | **Plan**: [plan.md](./plan.md)

## Overview

本快速入门指南帮助开发人员快速搭建账号管理系统的本地开发环境，并运行第一个端到端流程。

---

## Prerequisites

### 后端环境

- **.NET SDK**: 版本 10 或更高
  ```bash
  dotnet --version  # 确认已安装
  ```

- **数据库**: SQLite（.NET内置支持，无需额外安装）

- **推荐IDE**: Visual Studio 2022, Rider, 或 VS Code + C# Extension

### 前端环境

- **Node.js**: 版本 18 或更高
  ```bash
  node --version
  npm --version
  ```

- **推荐IDE**: VS Code, WebStorm

---

## Project Setup

### Step 1: 克隆仓库

```bash
# 假设项目仓库地址为：
git clone https://github.com/your-org/AccountBox.git
cd AccountBox
```

### Step 2: 后端设置

#### 2.1 安装依赖

```bash
cd backend
dotnet restore
```

#### 2.2 配置数据库连接

编辑 `backend/src/AccountBox.Api/appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=~/.accountbox/dev.db"
  },
  "Security": {
    "Argon2": {
      "MemorySize": 65536,
      "Iterations": 3,
      "Parallelism": 4
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

#### 2.3 创建数据库并应用迁移

```bash
cd src/AccountBox.Data
dotnet ef migrations add Initial
dotnet ef database update
```

#### 2.4 运行后端

```bash
cd ../AccountBox.Api
dotnet run
```

后端应在 `http://localhost:5000` 启动。验证：

```bash
curl http://localhost:5000/api/vault/status
# 预期响应: {"isInitialized":false,"isUnlocked":false}
```

### Step 3: 前端设置

#### 3.1 安装依赖

```bash
cd ../../frontend
npm install
```

#### 3.2 配置API地址

编辑 `frontend/.env.development`：

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

#### 3.3 运行前端

```bash
npm run dev
```

前端应在 `http://localhost:5173` 启动。浏览器打开该地址，应看到初始化向导。

---

## First-Time Initialization

### Step 4: 初始化应用（设置主密码）

1. 打开浏览器访问 `http://localhost:5173`
2. 系统检测到未初始化，显示"设置主密码"页面
3. 输入主密码（至少8字符），例如：`Test1234!`
4. 点击"初始化"按钮

**后台流程**：

- 前端调用 `POST /api/vault/initialize`
- 后端生成256-bit VaultKey
- Argon2id派生KEK加密VaultKey
- 持久化KeySlot到数据库
- 返回会话令牌

### Step 5: 创建第一个网站

1. 解锁后进入主界面
2. 点击"添加网站"按钮
3. 填写信息：
   - 显示名：`GitHub`
   - 域名：`github.com`
   - 标签：`开发,代码托管`
4. 点击"保存"

**验证**：网站列表中应显示"GitHub"。

### Step 6: 创建第一个账号

1. 点击"GitHub"网站，进入账号列表
2. 点击"添加账号"按钮
3. 填写信息：
   - 用户名：`john.doe@example.com`
   - 密码：点击"生成密码"按钮，选择长度16，包含所有字符类型，点击"接受"
   - 备注：`工作账号`
   - 标签：`主账号`
4. 点击"保存"

**后台流程**：

- 前端调用 `POST /api/accounts`
- 后端使用VaultKey加密密码和备注
- 存储到数据库（PasswordEncrypted, PasswordIV, PasswordTag）
- 返回账号详情（密码已解密）

### Step 7: 测试搜索

1. 在顶部搜索框输入：`john`
2. 按回车键
3. 应显示刚创建的账号

### Step 8: 测试软删除与回收站

1. 在账号列表中，点击账号右侧的"删除"按钮
2. 确认删除
3. 账号从列表消失
4. 点击左侧菜单"系统设置" → "回收站"
5. 应看到刚删除的账号
6. 点击"恢复"按钮
7. 返回网站账号列表，账号重新出现

---

## Development Workflow

### 后端开发

#### 运行单元测试

```bash
cd backend/tests/AccountBox.Core.Tests
dotnet test
```

#### 运行集成测试

```bash
cd ../AccountBox.Api.Tests
dotnet test
```

#### 添加新的数据库迁移

```bash
cd backend/src/AccountBox.Data
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

#### 调试后端

- **Visual Studio**: 按F5启动调试
- **VS Code**: 配置 `.vscode/launch.json`，选择".NET Core Launch (web)"

### 前端开发

#### 运行单元测试

```bash
cd frontend
npm run test
```

#### 运行E2E测试

```bash
npm run test:e2e
```

#### 添加新的shadcn/ui组件

```bash
npx shadcn-ui@latest add <component-name>
# 例如: npx shadcn-ui@latest add dialog
```

#### 调试前端

- **VS Code**: 安装Debugger for Chrome扩展，按F5启动调试
- **浏览器DevTools**: 按F12打开开发者工具

---

## API Testing

### 使用Postman/Insomnia

导入API契约（OpenAPI规范）：

1. 启动Postman
2. 选择"Import" → "File"
3. 导入 `specs/001-mvp/contracts/*.yaml`
4. 所有端点和请求示例将自动加载

### 使用curl测试

#### 初始化应用

```bash
curl -X POST http://localhost:5000/api/vault/initialize \
  -H "Content-Type: application/json" \
  -d '{"masterPassword":"Test1234!"}'

# 响应示例:
# {
#   "sessionToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "expiresAt": "2025-10-14T18:00:00Z"
# }
```

#### 创建网站

```bash
TOKEN="your-session-token-here"

curl -X POST http://localhost:5000/api/websites \
  -H "Content-Type: application/json" \
  -H "X-Vault-Session: $TOKEN" \
  -d '{
    "displayName": "GitHub",
    "domain": "github.com",
    "tags": "开发,代码托管"
  }'
```

#### 获取网站列表

```bash
curl -X GET "http://localhost:5000/api/websites?pageNumber=1&pageSize=10" \
  -H "X-Vault-Session: $TOKEN"
```

#### 创建账号

```bash
curl -X POST http://localhost:5000/api/accounts \
  -H "Content-Type: application/json" \
  -H "X-Vault-Session: $TOKEN" \
  -d '{
    "websiteId": 1,
    "username": "john.doe@example.com",
    "password": "MyP@ssw0rd!",
    "notes": "工作账号",
    "tags": "主账号"
  }'
```

---

## Troubleshooting

### 后端常见问题

#### 问题1: EF Core迁移失败

**错误信息**: `Build failed. Use dotnet build to see the errors.`

**解决方案**:

```bash
# 确保项目可以成功编译
cd backend/src/AccountBox.Api
dotnet build

# 如果编译成功，重新尝试迁移
cd ../AccountBox.Data
dotnet ef database update
```

#### 问题2: 端口5000被占用

**错误信息**: `Unable to bind to http://localhost:5000`

**解决方案**:

编辑 `backend/src/AccountBox.Api/Properties/launchSettings.json`，修改端口：

```json
{
  "profiles": {
    "AccountBox.Api": {
      "applicationUrl": "http://localhost:5001"
    }
  }
}
```

同时更新前端 `.env.development` 中的 `VITE_API_BASE_URL`。

#### 问题3: SQLite数据库锁定

**错误信息**: `database is locked`

**解决方案**:

```bash
# 关闭所有数据库连接（停止后端应用）
# 删除数据库并重新创建
rm ~/.accountbox/dev.db
cd backend/src/AccountBox.Data
dotnet ef database update
```

### 前端常见问题

#### 问题1: npm install失败

**错误信息**: `ERESOLVE unable to resolve dependency tree`

**解决方案**:

```bash
# 清除npm缓存
npm cache clean --force

# 删除node_modules和package-lock.json
rm -rf node_modules package-lock.json

# 重新安装
npm install --legacy-peer-deps
```

#### 问题2: API请求被CORS阻止

**错误信息**: `Access to XMLHttpRequest at 'http://localhost:5000/api/...' from origin 'http://localhost:5173' has been blocked by CORS policy`

**解决方案**:

检查后端CORS配置（`backend/src/AccountBox.Api/Program.cs`）：

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// 在 app.Build() 之后
app.UseCors("AllowFrontend");
```

#### 问题3: 前端无法连接后端

**错误信息**: `Network Error`

**解决方案**:

1. 确认后端正在运行：`curl http://localhost:5000/api/vault/status`
2. 检查 `.env.development` 中的 `VITE_API_BASE_URL` 是否正确
3. 检查防火墙是否阻止了5000端口

---

## Next Steps

完成快速入门后，您可以：

1. **阅读设计文档**:
   - [data-model.md](./data-model.md) - 数据模型详解
   - [research.md](./research.md) - 技术决策和最佳实践

2. **查看API契约**:
   - `contracts/websites.yaml` - 网站管理API
   - `contracts/accounts.yaml` - 账号管理API
   - `contracts/vault.yaml` - 加密存储API

3. **执行实施任务**:
   ```bash
   /speckit.tasks  # 生成详细的实施任务列表
   ```

4. **开始编码**:
   - 按照 `tasks.md` 中的任务顺序实施功能
   - 遵循TDD（测试驱动开发）原则
   - 每完成一个任务提交一次Git

---

## Useful Commands

### 后端

```bash
# 编译项目
dotnet build

# 运行应用
dotnet run --project backend/src/AccountBox.Api

# 运行所有测试
dotnet test

# 清理构建产物
dotnet clean

# 查看EF Core迁移列表
dotnet ef migrations list --project backend/src/AccountBox.Data

# 回滚迁移
dotnet ef database update <MigrationName> --project backend/src/AccountBox.Data
```

### 前端

```bash
# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 构建生产版本
npm run build

# 预览生产构建
npm run preview

# 运行linter
npm run lint

# 格式化代码
npm run format

# 运行单元测试
npm run test

# 运行E2E测试
npm run test:e2e
```

### Git

```bash
# 查看当前分支
git branch

# 提交更改
git add .
git commit -m "feat: 实现网站CRUD功能"

# 推送到远程
git push origin 001-mvp

# 查看提交历史
git log --oneline --graph
```

---

## Resources

- **.NET Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **React Documentation**: https://react.dev/
- **shadcn/ui**: https://ui.shadcn.com/
- **Tailwind CSS**: https://tailwindcss.com/docs
- **Argon2 (密码散列)**: https://en.wikipedia.org/wiki/Argon2

---

## Support

遇到问题？

1. 查看 [Troubleshooting](#troubleshooting) 章节
2. 搜索项目Issues: https://github.com/your-org/AccountBox/issues
3. 创建新Issue并提供详细错误信息

Happy Coding! 🚀
