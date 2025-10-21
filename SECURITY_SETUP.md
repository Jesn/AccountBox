# AccountBox 安全配置说明

## 概述

AccountBox 现在使用**自动密钥管理系统**，无需手动配置敏感信息。首次启动时会自动生成所有必需的密钥和密码。

---

## 🔐 密钥管理机制

### 自动生成的密钥

1. **JWT 密钥** (`jwt.key`)
   - 用于签发和验证 JWT Token
   - 512位随机密钥（Base64编码）
   - 保存位置：`data/.secrets/jwt.key`

2. **主密码** (`master.key`)
   - 用于登录系统
   - 16位强随机密码（包含大小写字母、数字和特殊字符）
   - 保存位置：`data/.secrets/master.key`

### 密钥优先级

系统按以下优先级读取配置：

```
环境变量 > 持久化文件 > 自动生成
```

---

## 🚀 快速开始

### 方式1：零配置启动（推荐）

```bash
# 启动容器
docker-compose up -d

# 查看日志获取自动生成的主密码
docker-compose logs accountbox | grep "主密码"
```

**首次启动时会看到类似输出：**

```
================================================================================
首次启动 - 已生成随机主密码
主密码: Xy9#mK2$pL5@qR8!
请妥善保存此密码！密码已保存到: /app/data/.secrets/master.key
================================================================================
```

### 方式2：自定义密钥（高级用户）

#### 使用环境变量

```bash
# 创建 .env 文件
cp .env.example .env

# 编辑 .env 文件
nano .env
```

**设置自定义密钥：**

```bash
# JWT 密钥（至少64字符）
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-64-characters-long-please

# 主密码
MASTER_PASSWORD=your-secure-master-password
```

**启动容器：**

```bash
docker-compose up -d
```

---

## 📁 文件结构

```
AccountBox/
├── data/                          # 数据目录（Docker Volume）
│   ├── accountbox.db             # SQLite 数据库
│   └── .secrets/                 # 密钥目录（自动创建）
│       ├── jwt.key               # JWT 密钥
│       └── master.key            # 主密码
├── .env                          # 环境变量（可选，不提交到Git）
└── .env.example                  # 环境变量模板
```

---

## 🔒 安全最佳实践

### 1. 备份密钥

```bash
# 备份整个数据目录
docker run --rm -v accountbox-data:/data -v $(pwd):/backup \
  alpine tar czf /backup/accountbox-backup-$(date +%Y%m%d).tar.gz /data
```

### 2. 修改主密码

**方法A：通过环境变量**

```bash
# 编辑 .env 文件
MASTER_PASSWORD=new-secure-password

# 重启容器
docker-compose restart
```

**方法B：直接修改文件**

```bash
# 进入容器
docker-compose exec accountbox sh

# 修改主密码
echo "new-secure-password" > /app/data/.secrets/master.key

# 退出并重启
exit
docker-compose restart
```

### 3. 轮换 JWT 密钥

```bash
# 删除旧密钥（会自动生成新密钥）
docker-compose exec accountbox rm /app/data/.secrets/jwt.key

# 重启容器
docker-compose restart
```

⚠️ **注意：轮换 JWT 密钥后，所有现有的 Token 将失效，用户需要重新登录。**

### 4. 查看密钥信息

```bash
# 查看 JWT 密钥
docker-compose exec accountbox cat /app/data/.secrets/jwt.key

# 查看主密码
docker-compose exec accountbox cat /app/data/.secrets/master.key
```

---

## 🐳 Docker 部署配置

### 单镜像部署（SQLite）

```bash
# 使用 docker-compose.yml
docker-compose up -d
```

**特点：**
- ✅ 零配置
- ✅ 自动生成密钥
- ✅ 数据持久化到 Volume

### 生产环境部署（PostgreSQL/MySQL）

```bash
# 使用 docker-compose.prod.yml
docker-compose -f docker-compose.prod.yml up -d
```

**推荐配置 `.env` 文件：**

```bash
# 数据库配置
DB_PROVIDER=postgresql
CONNECTION_STRING=Host=postgres;Port=5432;Database=accountbox;Username=accountbox;Password=your-db-password

# 安全配置（强烈推荐设置）
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-64-characters-long
MASTER_PASSWORD=your-secure-master-password
```

---

## 🔍 故障排查

### 问题1：忘记主密码

**解决方案：**

```bash
# 查看保存的主密码
docker-compose exec accountbox cat /app/data/.secrets/master.key
```

### 问题2：密钥文件丢失

**解决方案：**

```bash
# 删除容器和 Volume（会丢失所有数据）
docker-compose down -v

# 重新启动（会生成新密钥）
docker-compose up -d
```

### 问题3：无法启动容器

**检查日志：**

```bash
docker-compose logs accountbox
```

**常见原因：**
- 端口被占用
- Volume 权限问题
- 环境变量格式错误

---

## 📝 环境变量参考

| 变量名 | 说明 | 默认值 | 必需 |
|--------|------|--------|------|
| `JWT_SECRET_KEY` | JWT 密钥 | 自动生成 | ❌ |
| `MASTER_PASSWORD` | 主密码 | 自动生成 | ❌ |
| `DATA_PATH` | 数据目录 | `/app/data` | ❌ |
| `DB_PROVIDER` | 数据库类型 | `sqlite` | ❌ |
| `DATABASE_PATH` | SQLite 路径 | `/app/data/accountbox.db` | ❌ |
| `CONNECTION_STRING` | 数据库连接字符串 | - | ⚠️ (非SQLite时) |
| `ASPNETCORE_ENVIRONMENT` | 运行环境 | `Production` | ❌ |

---

## 🛡️ 安全注意事项

### ✅ 推荐做法

1. **定期备份** `data/.secrets/` 目录
2. **使用强密码**（如果手动设置）
3. **限制容器网络访问**
4. **定期更新镜像**
5. **监控登录日志**

### ❌ 避免做法

1. ❌ 不要将 `.env` 文件提交到 Git
2. ❌ 不要在公网暴露容器端口（除非使用 HTTPS）
3. ❌ 不要使用弱密码（如 `admin123`）
4. ❌ 不要在日志中记录敏感信息
5. ❌ 不要共享密钥文件

---

## 📚 相关文档

- [Docker 部署指南](./DOCKER.md)
- [开发环境配置](./README.md)
- [API 文档](http://localhost:5095/swagger)

---

## 🆘 获取帮助

如有问题，请查看：

1. 容器日志：`docker-compose logs accountbox`
2. 健康检查：`curl http://localhost:5095/health`
3. 密钥信息：查看 `data/.secrets/` 目录

---

**最后更新：** 2025-10-21
