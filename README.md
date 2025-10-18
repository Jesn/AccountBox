# AccountBox - 本地账号密码管理系统

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.2.0-61DAFB)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9.3-3178C6)](https://www.typescriptlang.org/)

一个功能完整的本地账号密码管理系统，采用前后端分离架构。支持 JWT 认证、API 密钥管理、全文搜索、密码生成等功能。

## ✨ 主要特性

- 🔐 **JWT 认证系统** - 主密码登录，Token 有效期 24 小时
- 🌐 **账号管理** - 创建、编辑、删除、搜索账号和网站
- 🗑️ **软删除与回收站** - 支持恢复或永久删除
- 🔑 **API 密钥管理** - 生成、管理、撤销 API 密钥，支持作用域控制
- 🔍 **全文搜索** - 快速搜索账号和网站
- 🎲 **密码生成器** - 生成强密码，支持自定义规则
- 📊 **表格视图** - 网站和账号列表支持表格布局
- 🐳 **Docker 支持** - 单镜像或分离镜像部署

## 🚀 快速开始

### 前置要求

- **后端**: .NET 8.0 SDK
- **前端**: Node.js >= 20.19.0, pnpm
- **数据库**: SQLite (开发) / PostgreSQL (生产)

### 方式 1: 本地使用启动脚本（推荐）

```bash
./start.sh
```

### 方式 2: 手动启动

```bash
# 终端 1: 启动后端
cd backend/src/AccountBox.Api
dotnet run

# 终端 2: 启动前端
cd frontend
pnpm install  # 首次需要
pnpm dev
```

### 方式 3: Docker 启动

```bash
# 单镜像部署（推荐）
docker-compose -f docker-compose.yml up -d

# 或分离镜像部署
docker-compose up -d
```

### 访问应用

- **前端应用**: http://localhost:5173
- **后端 API**: http://localhost:5093
- **Swagger 文档**: http://localhost:5093/swagger

## 📋 技术栈

### 后端
- **框架**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 9.0
- **认证**: JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer)
- **密钥哈希**: BCrypt.Net-Next 4.0.3
- **数据库**: SQLite / PostgreSQL
- **测试**: xUnit, Moq, FluentAssertions

### 前端
- **框架**: React 19.2.0
- **语言**: TypeScript 5.9.3 (严格模式)
- **构建工具**: Vite 7.1.7
- **UI 库**: shadcn/ui + Radix UI
- **样式**: Tailwind CSS 4.1.14
- **HTTP 客户端**: axios 1.12.2
- **路由**: react-router-dom 7.9.4
- **测试**: Vitest 3.2.4, @testing-library/react, Playwright

## 📁 项目结构

```
AccountBox/
├── backend/                    # 后端项目
│   ├── src/
│   │   ├── AccountBox.Api/     # Web API 项目
│   │   ├── AccountBox.Core/    # 核心业务逻辑
│   │   ├── AccountBox.Data/    # 数据访问层
│   │   └── AccountBox.Security/# 安全模块（已废弃）
│   └── tests/                  # 单元测试
├── frontend/                   # 前端项目
│   ├── src/
│   │   ├── components/         # React 组件
│   │   ├── pages/              # 页面组件
│   │   ├── services/           # API 服务
│   │   ├── hooks/              # 自定义 Hooks
│   │   ├── types/              # TypeScript 类型
│   │   └── utils/              # 工具函数
│   └── tests/                  # 测试文件
├── specs/                      # 功能规范
│   ├── 001-mvp/                # MVP 功能
│   ├── 006-api-management/     # API 密钥管理
│   └── 007-accountbox-web-jwt/ # JWT 认证
├── docker-compose.yml          # 分离镜像部署
├── docker-compose.single.yml   # 单镜像部署
├── Dockerfile                  # 单镜像构建配置
└── start.sh                    # 启动脚本
```

## 📚 API 文档

### 内部 API (需要 JWT Token)

所有内部 API 需要在请求头中包含 JWT Token:

```bash
Authorization: Bearer <your-jwt-token>
```

主要端点:
- `GET /api/websites` - 获取网站列表
- `GET /api/accounts` - 获取账号列表
- `GET /api/search` - 全文搜索
- `GET /api/api-keys` - 获取 API 密钥列表

### 外部 API (需要 API Key)

外部 API 需要在请求头中包含 API Key:

```bash
X-API-Key: sk_your_api_key_here
```

主要端点:
- `GET /api/external/websites/{websiteId}/accounts/random` - 获取随机账号
- `GET /api/external/websites` - 获取可访问的网站列表

详细 API 文档请访问: http://localhost:5093/swagger


## 🐳 Docker 部署

### 本地部署

#### 单镜像部署（推荐）

```bash
docker-compose -f docker-compose.yml up -d
```

访问: http://localhost:5093

#### 分离镜像部署

```bash
docker-compose up -d
```

访问:
- 前端: http://localhost:8080
- 后端: http://localhost:5093

### 线上部署

使用公共镜像 `docker.cnb.cool/rich/public/accountbox` 进行部署。

#### SQLite 版本

```bash
docker run -d \
  --name accountbox \
  -p 5093:8080 \
  -v accountbox_data:/app/data \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e Authentication__MasterPassword=your_master_password \
  docker.cnb.cool/rich/public/accountbox:latest
```

#### PostgreSQL 版本

```bash
docker run -d \
  --name accountbox \
  -p 5093:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e Authentication__MasterPassword=your_master_password \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=accountbox;Username=postgres;Password=your_password" \
  --link postgres:postgres \
  docker.cnb.cool/rich/public/accountbox:latest
```

#### Docker Compose 部署（推荐）

**SQLite 版本** (`docker-compose.prod.yml`):

```yaml
version: '3.8'

services:
  accountbox:
    image: docker.cnb.cool/rich/public/accountbox:latest
    container_name: accountbox
    ports:
      - "5093:8080"
    volumes:
      - accountbox_data:/app/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - Authentication__MasterPassword=your_master_password
    restart: unless-stopped

volumes:
  accountbox_data:
```

启动:
```bash
docker-compose -f docker-compose.prod.yml up -d
```

⚠️ **重要**: 请将 `your_master_password` 替换为强密码

**PostgreSQL 版本** (`docker-compose.prod-pg.yml`):

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    container_name: accountbox-postgres
    environment:
      - POSTGRES_DB=accountbox
      - POSTGRES_PASSWORD=your_db_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

  accountbox:
    image: docker.cnb.cool/rich/public/accountbox:latest
    container_name: accountbox
    ports:
      - "5093:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - Authentication__MasterPassword=your_master_password
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=accountbox;Username=postgres;Password=your_db_password
    depends_on:
      - postgres
    restart: unless-stopped

volumes:
  postgres_data:
```

启动:
```bash
docker-compose -f docker-compose.prod-pg.yml up -d
```

⚠️ **重要**: 请将以下参数替换为强密码:
- `your_master_password` - 应用主密码
- `your_db_password` - PostgreSQL 数据库密码

详细 Docker 部署指南请参考 [DOCKER.md](DOCKER.md)

## 📖 开发指南

### 代码风格

- **后端**: C# 标准命名规范，使用 `dotnet format`
- **前端**: TypeScript + React，使用 `pnpm format`

### Git 提交规范

```
<type>[scope]: <description>

类型: feat, fix, docs, style, refactor, test, chore
语言: 中文
示例: feat(auth): 添加 JWT 认证系统
```

### 常用命令

```bash
# 后端
cd backend/src/AccountBox.Api
dotnet run                    # 运行
dotnet format                 # 格式化
dotnet test                   # 测试
dotnet ef migrations add Name # 创建迁移
dotnet ef database update     # 应用迁移

# 前端
cd frontend
pnpm dev                      # 开发
pnpm build                    # 构建
pnpm format                   # 格式化
pnpm lint                     # 检查
pnpm test                     # 测试
```

## 🔐 安全建议

⚠️ **重要**: 本项目采用明文存储模式，仅适用于个人自托管环境。

- 仅在 localhost 或 VPN 环境访问
- 启用防火墙保护
- 使用磁盘加密保护数据库文件
- 定期加密备份数据库
- 主密码应设置为强密码

## 📝 许可证

MIT License - 详见 [LICENSE](LICENSE)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📞 支持

- 📖 [项目记忆库](Augment-Memories.md) - 详细的项目文档
- 🐳 [Docker 部署指南](DOCKER.md) - Docker 部署说明
- 📋 [开发指南](CLAUDE.md) - 开发规范和技术栈

## 🎯 路线图

- [ ] 数据导入/导出功能
- [ ] 账号分享功能
- [ ] 浏览器扩展
- [ ] 移动应用
- [ ] 端到端加密支持

---

**最后更新**: 2025-10-18 | **版本**: 2.0 | **状态**: 生产就绪

