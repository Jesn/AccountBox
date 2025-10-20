# Docker 部署指南

## 概述

本指南介绍如何使用 Docker 和 Docker Compose 部署 AccountBox 到生产环境。

---

## 🎯 推荐的 Docker 部署方案

### 方案特点

1. **自动迁移** - 数据库容器首次启动时自动应用 SQL 迁移脚本
2. **多数据库支持** - 支持 PostgreSQL 和 MySQL
3. **单镜像部署** - 前后端打包在一个镜像中
4. **健康检查** - 自动监控服务健康状态
5. **持久化存储** - 数据库数据持久化到 Docker volumes

---

## 📁 目录结构

```
AccountBox/
├── docker/
│   ├── postgres/
│   │   └── initdb.d/
│   │       └── 01-init.sh          # PostgreSQL 初始化脚本
│   └── mysql/
│       └── initdb.d/
│           └── 01-init.sh          # MySQL 初始化脚本
├── migrations/
│   ├── postgresql/
│   │   └── V001__Initial_schema.sql
│   ├── mysql/
│   │   └── V001__Initial_schema.sql
│   └── sqlite/
│       └── V001__Initial_schema.sql
├── Dockerfile                       # 生产环境 Dockerfile
├── docker-compose.prod.yml          # 生产环境 Docker Compose
└── .env.prod.example                # 环境变量示例
```

---

## 🚀 快速开始

### 1. 准备环境变量

```bash
# 复制环境变量示例文件
cp .env.prod.example .env.prod

# 编辑配置（重要：修改所有密码和密钥！）
vim .env.prod
```

**必须修改的配置**：
- `POSTGRES_PASSWORD` - PostgreSQL 密码
- `MYSQL_PASSWORD` - MySQL 密码
- `MYSQL_ROOT_PASSWORD` - MySQL root 密码
- `JWT_SECRET_KEY` - JWT 密钥（至少 32 个字符）
- `CORS_ORIGIN` - 允许的前端域名

### 2. 构建并启动服务

#### 使用 PostgreSQL

```bash
# 构建镜像
docker-compose -f docker-compose.prod.yml --env-file .env.prod build

# 启动服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d postgres backend

# 查看日志
docker-compose -f docker-compose.prod.yml logs -f
```

#### 使用 MySQL

```bash
# 修改 .env.prod 中的 DB_PROVIDER
DB_PROVIDER=mysql
CONNECTION_STRING=Server=mysql;Port=3306;Database=accountbox;User=accountbox;Password=your-password

# 启动服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d mysql backend
```

### 3. 验证部署

```bash
# 检查服务状态
docker-compose -f docker-compose.prod.yml ps

# 检查健康状态
curl http://localhost:5093/health

# 查看数据库表
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\dt"
```

---

## 📋 详细步骤

### 步骤 1：生成迁移脚本（首次部署）

如果还没有生成迁移脚本，需要先生成：

```bash
# 进入 Data 项目目录
cd backend/src/AccountBox.Data

# 生成 PostgreSQL 迁移脚本
export DB_PROVIDER=postgresql
dotnet ef migrations script \
  --idempotent \
  --output ../../../migrations/postgresql/V001__Initial_schema.sql \
  --context AccountBoxDbContext \
  --project . \
  --startup-project ../AccountBox.Api

# 生成 MySQL 迁移脚本
export DB_PROVIDER=mysql
dotnet ef migrations script \
  --idempotent \
  --output ../../../migrations/mysql/V001__Initial_schema.sql \
  --context AccountBoxDbContext \
  --project . \
  --startup-project ../AccountBox.Api

# 返回项目根目录
cd ../../..
```

### 步骤 2：配置环境变量

创建 `.env.prod` 文件：

```bash
# 数据库配置
DB_PROVIDER=postgresql
POSTGRES_DB=accountbox
POSTGRES_USER=accountbox
POSTGRES_PASSWORD=StrongPassword123!
CONNECTION_STRING=Host=postgres;Port=5432;Database=accountbox;Username=accountbox;Password=StrongPassword123!

# JWT 配置
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long-change-this
JWT_ISSUER=AccountBox
JWT_AUDIENCE=AccountBox-Web
JWT_EXPIRATION=1440

# CORS 配置
CORS_ORIGIN=https://your-domain.com

# 应用配置
ASPNETCORE_ENVIRONMENT=Production
BACKEND_PORT=5093
```

### 步骤 3：构建 Docker 镜像

```bash
# 构建生产镜像
docker-compose -f docker-compose.prod.yml --env-file .env.prod build

# 查看镜像
docker images | grep accountbox
```

### 步骤 4：启动服务

```bash
# 启动所有服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# 或者只启动需要的服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d postgres backend
```

### 步骤 5：验证部署

```bash
# 1. 检查容器状态
docker-compose -f docker-compose.prod.yml ps

# 2. 查看日志
docker-compose -f docker-compose.prod.yml logs -f backend

# 3. 检查健康状态
curl http://localhost:5093/health

# 4. 验证数据库
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\dt"

# 5. 测试 API
curl http://localhost:5093/api/health
```

---

## 🔄 更新部署

### 更新应用代码

```bash
# 1. 拉取最新代码
git pull

# 2. 重新构建镜像
docker-compose -f docker-compose.prod.yml --env-file .env.prod build backend

# 3. 重启服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d backend

# 4. 查看日志确认启动成功
docker-compose -f docker-compose.prod.yml logs -f backend
```

### 应用数据库迁移

**方式 1：手动应用（推荐）**

```bash
# PostgreSQL
docker exec -i accountbox-postgres-prod psql -U accountbox -d accountbox < migrations/postgresql/V002__Add_new_feature.sql

# MySQL
docker exec -i accountbox-mysql-prod mysql -u accountbox -pYourPassword accountbox < migrations/mysql/V002__Add_new_feature.sql
```

**方式 2：重新创建数据库容器（仅开发/测试环境）**

```bash
# 停止并删除数据库容器
docker-compose -f docker-compose.prod.yml down postgres

# 删除数据卷（警告：会丢失所有数据！）
docker volume rm accountbox_postgres_data

# 重新启动（会自动应用所有迁移）
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d postgres
```

---

## 🛠️ 常用命令

### 服务管理

```bash
# 启动服务
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d

# 停止服务
docker-compose -f docker-compose.prod.yml down

# 重启服务
docker-compose -f docker-compose.prod.yml restart backend

# 查看服务状态
docker-compose -f docker-compose.prod.yml ps

# 查看日志
docker-compose -f docker-compose.prod.yml logs -f backend
docker-compose -f docker-compose.prod.yml logs -f postgres
```

### 数据库管理

```bash
# 进入 PostgreSQL 容器
docker exec -it accountbox-postgres-prod psql -U accountbox -d accountbox

# 进入 MySQL 容器
docker exec -it accountbox-mysql-prod mysql -u accountbox -p accountbox

# 备份 PostgreSQL 数据库
docker exec accountbox-postgres-prod pg_dump -U accountbox accountbox > backup.sql

# 恢复 PostgreSQL 数据库
docker exec -i accountbox-postgres-prod psql -U accountbox -d accountbox < backup.sql

# 备份 MySQL 数据库
docker exec accountbox-mysql-prod mysqldump -u accountbox -p accountbox > backup.sql

# 恢复 MySQL 数据库
docker exec -i accountbox-mysql-prod mysql -u accountbox -p accountbox < backup.sql
```

### 容器管理

```bash
# 查看容器资源使用
docker stats accountbox-backend-prod

# 进入容器
docker exec -it accountbox-backend-prod sh

# 查看容器日志
docker logs -f accountbox-backend-prod

# 清理未使用的资源
docker system prune -a
```

---

## 🔒 安全建议

### 1. 密码和密钥

- ✅ 使用强密码（至少 16 个字符）
- ✅ JWT_SECRET_KEY 至少 32 个字符
- ✅ 定期更换密码和密钥
- ✅ 不要将 `.env.prod` 提交到版本控制

### 2. 网络安全

- ✅ 使用 HTTPS（配置反向代理如 Nginx）
- ✅ 限制数据库端口只在内部网络访问
- ✅ 配置防火墙规则
- ✅ 使用 Docker 网络隔离

### 3. 容器安全

- ✅ 使用非 root 用户运行容器
- ✅ 定期更新基础镜像
- ✅ 扫描镜像漏洞
- ✅ 限制容器资源使用

### 4. 数据安全

- ✅ 定期备份数据库
- ✅ 加密敏感数据
- ✅ 使用持久化存储
- ✅ 测试恢复流程

---

## 🌐 使用 Nginx 反向代理

### Nginx 配置示例

```nginx
server {
    listen 80;
    server_name your-domain.com;

    # 重定向到 HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    # SSL 证书
    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    # SSL 配置
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # 代理到后端
    location /api/ {
        proxy_pass http://localhost:5093;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # 前端静态文件
    location / {
        proxy_pass http://localhost:5093;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 📊 监控和日志

### 查看应用日志

```bash
# 实时查看日志
docker-compose -f docker-compose.prod.yml logs -f backend

# 查看最近 100 行日志
docker-compose -f docker-compose.prod.yml logs --tail=100 backend

# 查看特定时间的日志
docker-compose -f docker-compose.prod.yml logs --since 2024-01-01T00:00:00 backend
```

### 监控容器资源

```bash
# 查看资源使用
docker stats accountbox-backend-prod accountbox-postgres-prod

# 查看容器详细信息
docker inspect accountbox-backend-prod
```

---

## 🐛 故障排查

### 问题 1：容器无法启动

```bash
# 查看容器日志
docker-compose -f docker-compose.prod.yml logs backend

# 检查容器状态
docker-compose -f docker-compose.prod.yml ps

# 检查配置
docker-compose -f docker-compose.prod.yml config
```

### 问题 2：数据库连接失败

```bash
# 检查数据库容器状态
docker-compose -f docker-compose.prod.yml ps postgres

# 测试数据库连接
docker exec accountbox-postgres-prod pg_isready -U accountbox

# 查看数据库日志
docker-compose -f docker-compose.prod.yml logs postgres
```

### 问题 3：迁移脚本未执行

```bash
# 检查迁移脚本是否存在
ls -la migrations/postgresql/

# 手动执行迁移
docker exec -i accountbox-postgres-prod psql -U accountbox -d accountbox < migrations/postgresql/V001__Initial_schema.sql

# 查看数据库表
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\dt"
```

---

## 📚 相关文档

- [数据库迁移指南](./database-migration-guide.md)
- [Docker 官方文档](https://docs.docker.com/)
- [Docker Compose 文档](https://docs.docker.com/compose/)
- [PostgreSQL Docker 镜像](https://hub.docker.com/_/postgres)
- [MySQL Docker 镜像](https://hub.docker.com/_/mysql)
