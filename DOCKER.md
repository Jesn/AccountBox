# AccountBox Docker 部署指南

本文档介绍如何使用 Docker 和 Docker Compose 部署 AccountBox 应用。

## 部署方式

AccountBox 提供两种 Docker 部署方式:

### 1. 单镜像部署（推荐）
- 前端和后端打包在一个镜像中
- 使用 ASP.NET Core 的静态文件服务提供前端
- 更简单的部署和管理
- 使用 `docker-compose.yml`

### 2. 数据库栈部署
- MySQL 或 PostgreSQL 与应用一同启动
- 使用 `docker-compose.mysql.yml` 或 `docker-compose.postgres.yml`

## 目录结构

```
AccountBox/
├── Dockerfile                 # 单镜像多阶段构建配置（前端+后端）
├── docker-compose.yml         # 单镜像（SQLite）部署配置
├── docker-compose.mysql.yml   # MySQL 部署配置
├── docker-compose.postgres.yml# PostgreSQL 部署配置
├── .dockerignore             # 构建忽略文件
├── backend/
│   ├── Dockerfile            # 后端独立镜像构建配置
│   └── .dockerignore         # 后端构建忽略文件
└── frontend/
    ├── Dockerfile            # 前端独立镜像构建配置
    ├── nginx.conf            # Nginx 配置
    └── .dockerignore         # 前端构建忽略文件
```

## 部署总览表（快速对照）

- `docker-compose.yml`（单镜像/SQLite）
  - 端口: 本机 5095 → 容器 5093
  - 数据卷: `accountbox-data:/app/data`
  - 适用: 最简部署（内置 SQLite）

- `docker-compose.mysql.yml`（MySQL 栈）
  - 端口: MySQL 3306、phpMyAdmin 8080、应用 5095
  - 适用: 使用 MySQL，带可视化管理

- `docker-compose.postgres.yml`（PostgreSQL 栈）
  - 端口: PostgreSQL 5432、pgAdmin 5050、应用 5095
  - 适用: 使用 PostgreSQL，带可视化管理

- `docker-compose.prod.yml`（生产一体化编排）
  - 包含数据库与后端服务，使用根 `Dockerfile`
  - 通过环境变量选择 `DB_PROVIDER` 和 `CONNECTION_STRING`

## 快速开始

### 方式一：单镜像部署（推荐）

#### 1. 构建并启动服务

```bash
docker compose -f docker-compose.yml up -d
```

#### 2. 查看服务状态

```bash
docker compose -f docker-compose.yml ps
```

#### 3. 查看日志

```bash
docker compose -f docker-compose.yml logs -f
```

#### 4. 停止服务

```bash
docker compose -f docker-compose.yml down
```

#### 5. 访问应用

- **应用地址**: http://localhost:5095
- **前端和 API 都通过该端口访问**

### 方式二：数据库栈（MySQL/PostgreSQL）

```bash
# MySQL 栈
docker compose -f docker-compose.mysql.yml up -d

# PostgreSQL 栈
docker compose -f docker-compose.postgres.yml up -d
```

#### MySQL 栈示例（docker-compose.mysql.yml）

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: accountbox-mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: root123
      MYSQL_DATABASE: accountbox
      MYSQL_USER: accountbox
      MYSQL_PASSWORD: accountbox123
    ports:
      - "3306:3306"
    volumes:
      - mysql-data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "accountbox", "-paccountbox123"]
      interval: 10s
      timeout: 5s
      retries: 5

  phpmyadmin:
    image: phpmyadmin:latest
    container_name: accountbox-phpmyadmin
    restart: unless-stopped
    environment:
      PMA_HOST: mysql
      PMA_PORT: 3306
      PMA_USER: accountbox
      PMA_PASSWORD: accountbox123
    ports:
      - "8080:80"
    depends_on:
      mysql:
        condition: service_healthy

  accountbox:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: accountbox-app
    restart: unless-stopped
    ports:
      - "5095:5093"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DB_PROVIDER=mysql
      - CONNECTION_STRING=Server=mysql;Port=3306;Database=accountbox;User=accountbox;Password=accountbox123
      - ASPNETCORE_URLS=http://+:5093
      - Authentication__MasterPassword=${MASTER_PASSWORD:-admin123}
    depends_on:
      mysql:
        condition: service_healthy

volumes:
  mysql-data:
```

启动后：
- 应用地址: http://localhost:5095
- phpMyAdmin: http://localhost:8080 （账号: accountbox / 密码: accountbox123）

## 镜像特性

### 单镜像部署 (Dockerfile)

**三阶段构建**:
1. **Frontend Build**: 使用 `node:20-alpine` + pnpm 构建前端
2. **Backend Build**: 使用 `mcr.microsoft.com/dotnet/sdk:8.0-alpine` 构建后端
3. **Runtime**: 使用 `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` 运行应用，包含前端静态文件

**优化特性**:
- 三阶段构建，最大化缓存利用
- 前端构建产物嵌入后端 wwwroot 目录
- 单一端口访问，简化网络配置
- 非root用户运行 (accountbox)
- 健康检查端点
- 数据持久化 (/app/data)

**预期镜像大小**: ~250MB

### 后端镜像 (backend/Dockerfile)

**多阶段构建**:
1. **Build Stage**: 使用 `mcr.microsoft.com/dotnet/sdk:9.0` 构建应用
2. **Runtime Stage**: 使用 `mcr.microsoft.com/dotnet/aspnet:9.0` 运行应用

**优化特性**:
- 分层构建，优化缓存利用
- 非root用户运行 (appuser)
- 健康检查端点
- 数据持久化 (/app/data)

**镜像大小**: ~220MB (相比 SDK 镜像的 ~1GB 大幅减小)

### 前端镜像 (frontend/Dockerfile)

**多阶段构建**:
1. **Build Stage**: 使用 `node:20-alpine` 构建前端资源
2. **Runtime Stage**: 使用 `nginx:1.27-alpine` 服务静态文件

**优化特性**:
- 使用 pnpm 优化依赖安装
- Alpine Linux 基础镜像
- 非root用户运行 (nginx)
- Gzip 压缩
- 安全头设置
- API 代理到后端
- 静态资源缓存优化

**镜像大小**: ~50MB (相比 Node.js 镜像的 ~200MB 大幅减小)

## 环境变量配置

### 后端环境变量

在 `docker-compose.yml` 中可以配置以下环境变量:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - DATABASE_PATH=/app/data/accountbox.db
  - ASPNETCORE_URLS=http://+:5093
```

### 前端配置

前端通过 Nginx 配置文件 (`frontend/nginx.conf`) 进行配置，主要配置项:
- API 代理到后端服务
- SPA 路由支持
- 静态资源缓存策略

## 数据持久化

数据库文件存储在 Docker 卷 `accountbox-data` 中:

```yaml
volumes:
  accountbox-data:
    driver: local
```

### 备份数据库

```bash
# 进入后端容器
docker-compose exec backend sh

# 数据库位置
ls -l /app/data/accountbox.db
```

## 健康检查

两个服务都配置了健康检查:

### 后端健康检查
```bash
curl http://localhost:5093/health
```

### 前端健康检查
```bash
curl http://localhost:8080/health
```

## 开发模式

如果需要在开发模式下运行，可以创建 `docker-compose.dev.yml`:

```yaml
version: '3.8'

services:
  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    volumes:
      - ./backend/src:/src
    ports:
      - "5093:5093"

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    volumes:
      - ./frontend/src:/app/src
    ports:
      - "8080:8080"
```

然后使用:
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

## 故障排查

### 1. 构建失败

```bash
# 清理所有构建缓存
docker-compose build --no-cache

# 查看详细构建日志
docker-compose build --progress=plain
```

### 2. 容器无法启动

```bash
# 查看容器日志
docker-compose logs backend
docker-compose logs frontend

# 进入容器调试
docker-compose exec backend sh
docker-compose exec frontend sh
```

### 3. 数据库问题

```bash
# 查看数据库文件
docker-compose exec backend ls -l /app/data/

# 重置数据库（警告：会删除所有数据）
docker-compose down -v
docker-compose up -d
```

## 生产部署建议

1. **使用环境变量文件**:
   ```bash
   # 创建 .env 文件
   echo "ASPNETCORE_ENVIRONMENT=Production" > .env
   ```

2. **配置 HTTPS**:
   - 使用 Nginx 或 Traefik 作为反向代理
   - 配置 SSL/TLS 证书

3. **监控和日志**:
   - 集成 Prometheus + Grafana
   - 使用 ELK Stack 或 Loki 收集日志

4. **备份策略**:
   - 定期备份 `accountbox-data` 卷
   - 使用 cron 任务自动备份

5. **资源限制**:
   ```yaml
   services:
     backend:
       deploy:
         resources:
           limits:
             cpus: '1'
             memory: 512M
           reservations:
             cpus: '0.5'
             memory: 256M
   ```

## 镜像优化总结

### 后端镜像优化
- ✅ 多阶段构建 (SDK -> Runtime)
- ✅ 使用 .dockerignore 排除不必要文件
- ✅ 分层缓存优化
- ✅ 非root用户运行
- ✅ 仅包含运行时依赖

### 前端镜像优化
- ✅ 多阶段构建 (Node -> Nginx)
- ✅ Alpine Linux 基础镜像
- ✅ 使用 pnpm 优化依赖
- ✅ 静态资源压缩
- ✅ 非root用户运行
- ✅ 仅包含构建产物

### 预期镜像大小
- **后端**: ~220MB (相比未优化的 ~1GB)
- **前端**: ~50MB (相比未优化的 ~200MB)
- **总计**: ~270MB (优化率 ~78%)
