# AccountBox Docker 部署指南

本文档介绍如何使用 Docker 和 Docker Compose 部署 AccountBox 应用。

## 部署方式

AccountBox 提供两种 Docker 部署方式:

### 1. 单镜像部署（推荐）
- 前端和后端打包在一个镜像中
- 使用 ASP.NET Core 的静态文件服务提供前端
- 更简单的部署和管理
- 使用 `docker-compose.single.yml`

### 2. 分离镜像部署
- 前端和后端分别构建独立镜像
- 前端使用 Nginx，后端使用 ASP.NET Core
- 更灵活的扩展和配置
- 使用 `docker-compose.yml`

## 目录结构

```
AccountBox/
├── Dockerfile                 # 单镜像多阶段构建配置（前端+后端）
├── docker-compose.single.yml  # 单镜像部署配置
├── docker-compose.yml         # 分离镜像部署配置
├── .dockerignore             # 构建忽略文件
├── backend/
│   ├── Dockerfile            # 后端独立镜像构建配置
│   └── .dockerignore         # 后端构建忽略文件
└── frontend/
    ├── Dockerfile            # 前端独立镜像构建配置
    ├── nginx.conf            # Nginx 配置
    └── .dockerignore         # 前端构建忽略文件
```

## 快速开始

### 方式一：单镜像部署（推荐）

#### 1. 构建并启动服务

```bash
docker-compose -f docker-compose.single.yml up -d
```

#### 2. 查看服务状态

```bash
docker-compose -f docker-compose.single.yml ps
```

#### 3. 查看日志

```bash
docker-compose -f docker-compose.single.yml logs -f
```

#### 4. 停止服务

```bash
docker-compose -f docker-compose.single.yml down
```

#### 5. 访问应用

- **应用地址**: http://localhost:5093
- **前端和API都通过这个端口访问**

### 方式二：分离镜像部署

#### 1. 构建并启动所有服务

```bash
docker-compose up -d
```

#### 2. 查看服务状态

```bash
docker-compose ps
```

#### 3. 查看日志

```bash
# 查看所有服务日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f backend
docker-compose logs -f frontend
```

#### 4. 停止服务

```bash
docker-compose down
```

#### 5. 访问应用

- **前端应用**: http://localhost:8080
- **后端API**: http://localhost:5093

## 镜像特性

### 单镜像部署 (Dockerfile)

**三阶段构建**:
1. **Frontend Build**: 使用 `node:20-alpine` + pnpm 构建前端
2. **Backend Build**: 使用 `mcr.microsoft.com/dotnet/sdk:9.0` 构建后端
3. **Runtime**: 使用 `mcr.microsoft.com/dotnet/aspnet:9.0` 运行应用，包含前端静态文件

**优化特性**:
- 三阶段构建，最大化缓存利用
- 前端构建产物嵌入后端 wwwroot 目录
- 单一端口访问，简化网络配置
- 非root用户运行 (appuser)
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
