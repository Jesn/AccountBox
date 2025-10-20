# 数据库迁移指南

## 概述

AccountBox 支持三种数据库：SQLite、PostgreSQL 和 MySQL。本文档介绍生产环境推荐的迁移方案。

---

## 🥇 方案1：独立 SQL 迁移脚本（最推荐）

### 优点
- ✅ 完全可控，可人工审查每个 SQL 语句
- ✅ 支持复杂的数据迁移逻辑
- ✅ 可集成到 CI/CD 流程
- ✅ 回滚方便
- ✅ 适合团队协作

### 生成迁移脚本

```bash
# 进入 Data 项目目录
cd backend/src/AccountBox.Data

# 为 PostgreSQL 生成迁移脚本
DB_PROVIDER=postgresql dotnet ef migrations script \
  --idempotent \
  --output ../../migrations/postgresql/V001__Initial_schema.sql \
  --context AccountBoxDbContext \
  --project ../AccountBox.Data \
  --startup-project ../AccountBox.Api

# 为 MySQL 生成迁移脚本
DB_PROVIDER=mysql dotnet ef migrations script \
  --idempotent \
  --output ../../migrations/mysql/V001__Initial_schema.sql \
  --context AccountBoxDbContext \
  --project ../AccountBox.Data \
  --startup-project ../AccountBox.Api

# 为 SQLite 生成迁移脚本
DB_PROVIDER=sqlite dotnet ef migrations script \
  --idempotent \
  --output ../../migrations/sqlite/V001__Initial_schema.sql \
  --context AccountBoxDbContext \
  --project ../AccountBox.Data \
  --startup-project ../AccountBox.Api
```

### 应用迁移

#### PostgreSQL
```bash
# 方式1：使用 psql
psql -U accountbox -d accountbox -f migrations/postgresql/V001__Initial_schema.sql

# 方式2：使用 Docker
docker exec -i accountbox-postgres psql -U accountbox -d accountbox < migrations/postgresql/V001__Initial_schema.sql
```

#### MySQL
```bash
# 方式1：使用 mysql 客户端
mysql -u accountbox -p accountbox < migrations/mysql/V001__Initial_schema.sql

# 方式2：使用 Docker
docker exec -i accountbox-mysql mysql -u accountbox -paccountbox123 accountbox < migrations/mysql/V001__Initial_schema.sql
```

#### SQLite
```bash
sqlite3 accountbox.db < migrations/sqlite/V001__Initial_schema.sql
```

### 目录结构

```
AccountBox/
├── migrations/
│   ├── postgresql/
│   │   ├── V001__Initial_schema.sql
│   │   ├── V002__Add_indexes.sql
│   │   └── V003__Add_api_keys.sql
│   ├── mysql/
│   │   ├── V001__Initial_schema.sql
│   │   ├── V002__Add_indexes.sql
│   │   └── V003__Add_api_keys.sql
│   └── sqlite/
│       ├── V001__Initial_schema.sql
│       ├── V002__Add_indexes.sql
│       └── V003__Add_api_keys.sql
└── backend/
```

---

## 🥈 方案2：使用 Flyway（企业级）

### 安装 Flyway

```bash
# macOS
brew install flyway

# Linux
wget -qO- https://repo1.maven.org/maven2/org/flywaydb/flyway-commandline/9.22.3/flyway-commandline-9.22.3-linux-x64.tar.gz | tar xvz && sudo ln -s `pwd`/flyway-9.22.3/flyway /usr/local/bin

# Windows
choco install flyway-commandline
```

### 配置 Flyway

创建 `flyway.conf`：

```properties
# PostgreSQL
flyway.url=jdbc:postgresql://localhost:5432/accountbox
flyway.user=accountbox
flyway.password=accountbox123
flyway.locations=filesystem:./migrations/postgresql
flyway.table=flyway_schema_history
```

### 应用迁移

```bash
# 查看迁移状态
flyway info

# 应用迁移
flyway migrate

# 验证迁移
flyway validate

# 回滚（需要 Flyway Teams 版本）
flyway undo
```

### 目录结构

```
migrations/
├── postgresql/
│   ├── V1__Initial_schema.sql
│   ├── V2__Add_indexes.sql
│   └── V3__Add_api_keys.sql
├── mysql/
│   ├── V1__Initial_schema.sql
│   ├── V2__Add_indexes.sql
│   └── V3__Add_api_keys.sql
└── sqlite/
    ├── V1__Initial_schema.sql
    ├── V2__Add_indexes.sql
    └── V3__Add_api_keys.sql
```

---

## 🥉 方案3：Docker 容器启动时自动迁移

### 创建迁移脚本

创建 `docker-entrypoint-initdb.d/` 目录：

```bash
mkdir -p docker/postgres/initdb.d
mkdir -p docker/mysql/initdb.d
```

### PostgreSQL Dockerfile

```dockerfile
FROM postgres:16-alpine

# 复制迁移脚本
COPY migrations/postgresql/*.sql /docker-entrypoint-initdb.d/

# 设置权限
RUN chmod +x /docker-entrypoint-initdb.d/*.sql
```

### MySQL Dockerfile

```dockerfile
FROM mysql:8.0

# 复制迁移脚本
COPY migrations/mysql/*.sql /docker-entrypoint-initdb.d/

# 设置权限
RUN chmod +x /docker-entrypoint-initdb.d/*.sql
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  postgres:
    build:
      context: .
      dockerfile: docker/postgres/Dockerfile
    environment:
      POSTGRES_DB: accountbox
      POSTGRES_USER: accountbox
      POSTGRES_PASSWORD: accountbox123
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  mysql:
    build:
      context: .
      dockerfile: docker/mysql/Dockerfile
    environment:
      MYSQL_DATABASE: accountbox
      MYSQL_USER: accountbox
      MYSQL_PASSWORD: accountbox123
      MYSQL_ROOT_PASSWORD: root123
    volumes:
      - mysql_data:/var/lib/mysql
    ports:
      - "3306:3306"

volumes:
  postgres_data:
  mysql_data:
```

---

## 🔧 方案4：改进当前的 EnsureCreated 方案

### 修改 Program.cs

将当前的 `EnsureCreated()` 改为更智能的迁移逻辑：

```csharp
// 应用数据库迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccountBoxDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")?.ToLower() ?? "sqlite";

    try
    {
        // 检查数据库是否可以连接
        if (!db.Database.CanConnect())
        {
            logger.LogInformation("数据库不存在，正在创建...");
            db.Database.EnsureCreated();
            logger.LogInformation("数据库创建成功");
        }
        else
        {
            // 数据库存在，检查是否有待应用的迁移
            var pendingMigrations = db.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                logger.LogWarning("检测到 {Count} 个待应用的迁移，但当前使用 EnsureCreated 模式",
                    pendingMigrations.Count);
                logger.LogWarning("生产环境建议使用独立的 SQL 迁移脚本");
                logger.LogInformation("待应用的迁移: {Migrations}",
                    string.Join(", ", pendingMigrations));
            }
            else
            {
                logger.LogInformation("数据库架构已是最新");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "数据库迁移失败");
        throw;
    }
}
```

---

## 📋 迁移最佳实践

### 1. 版本命名规范

```
V{版本号}__{描述}.sql

示例：
V001__Initial_schema.sql
V002__Add_user_table.sql
V003__Add_indexes_to_user_table.sql
V004__Alter_user_email_column.sql
```

### 2. 迁移脚本编写规范

```sql
-- V001__Initial_schema.sql
-- 描述：创建初始数据库架构
-- 作者：张三
-- 日期：2025-01-20

-- 创建表
CREATE TABLE IF NOT EXISTS "Websites" (
    "Id" SERIAL PRIMARY KEY,
    "Domain" VARCHAR(255) NOT NULL UNIQUE,
    "DisplayName" VARCHAR(255) NOT NULL,
    "Tags" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 创建索引
CREATE INDEX IF NOT EXISTS "IX_Websites_Domain" ON "Websites" ("Domain");
CREATE INDEX IF NOT EXISTS "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");

-- 插入初始数据（可选）
-- INSERT INTO "Websites" ("Domain", "DisplayName") VALUES ('example.com', 'Example');
```

### 3. 回滚脚本

为每个迁移创建对应的回滚脚本：

```sql
-- U001__Initial_schema.sql (回滚脚本)
-- 描述：回滚初始数据库架构
-- 作者：张三
-- 日期：2025-01-20

-- 删除索引
DROP INDEX IF EXISTS "IX_Websites_CreatedAt";
DROP INDEX IF EXISTS "IX_Websites_Domain";

-- 删除表
DROP TABLE IF EXISTS "Websites";
```

### 4. 测试迁移

在应用到生产环境之前，务必在测试环境验证：

```bash
# 1. 在测试数据库应用迁移
psql -U accountbox -d accountbox_test -f migrations/postgresql/V001__Initial_schema.sql

# 2. 验证表结构
psql -U accountbox -d accountbox_test -c "\d+ Websites"

# 3. 测试回滚
psql -U accountbox -d accountbox_test -f migrations/postgresql/U001__Initial_schema.sql

# 4. 验证回滚成功
psql -U accountbox -d accountbox_test -c "\dt"
```

---

## 🚀 CI/CD 集成

### GitHub Actions 示例

```yaml
name: Database Migration

on:
  push:
    branches: [ main ]
    paths:
      - 'migrations/**'

jobs:
  migrate:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup PostgreSQL
      uses: ikalnytskyi/action-setup-postgres@v4
      with:
        username: accountbox
        password: accountbox123
        database: accountbox

    - name: Apply Migrations
      run: |
        for file in migrations/postgresql/*.sql; do
          echo "Applying $file..."
          psql -U accountbox -d accountbox -f "$file"
        done

    - name: Verify Migration
      run: |
        psql -U accountbox -d accountbox -c "\dt"
```

---

## 📊 迁移状态跟踪

### 创建迁移历史表

```sql
CREATE TABLE IF NOT EXISTS "MigrationHistory" (
    "Id" SERIAL PRIMARY KEY,
    "Version" VARCHAR(50) NOT NULL UNIQUE,
    "Description" VARCHAR(255) NOT NULL,
    "AppliedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "AppliedBy" VARCHAR(100),
    "ExecutionTime" INTEGER, -- 毫秒
    "Success" BOOLEAN NOT NULL DEFAULT TRUE,
    "ErrorMessage" TEXT
);
```

### 迁移脚本模板

```sql
-- 开始迁移
DO $$
DECLARE
    v_start_time TIMESTAMP;
    v_version VARCHAR(50) := 'V001';
    v_description VARCHAR(255) := 'Initial schema';
BEGIN
    v_start_time := clock_timestamp();

    -- 检查是否已应用
    IF EXISTS (SELECT 1 FROM "MigrationHistory" WHERE "Version" = v_version) THEN
        RAISE NOTICE 'Migration % already applied', v_version;
        RETURN;
    END IF;

    -- 执行迁移
    CREATE TABLE IF NOT EXISTS "Websites" (
        "Id" SERIAL PRIMARY KEY,
        "Domain" VARCHAR(255) NOT NULL UNIQUE,
        "DisplayName" VARCHAR(255) NOT NULL,
        "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

    -- 记录迁移历史
    INSERT INTO "MigrationHistory" ("Version", "Description", "ExecutionTime")
    VALUES (
        v_version,
        v_description,
        EXTRACT(MILLISECONDS FROM (clock_timestamp() - v_start_time))
    );

    RAISE NOTICE 'Migration % applied successfully', v_version;

EXCEPTION WHEN OTHERS THEN
    -- 记录失败
    INSERT INTO "MigrationHistory" ("Version", "Description", "Success", "ErrorMessage")
    VALUES (v_version, v_description, FALSE, SQLERRM);

    RAISE;
END $$;
```

---

## 🎯 推荐方案总结

| 场景 | 推荐方案 | 理由 |
|------|---------|------|
| 小型项目/个人项目 | 方案4（EnsureCreated） | 简单快速 |
| 中型项目 | 方案1（SQL脚本） | 可控性好，易于审查 |
| 大型项目/企业 | 方案2（Flyway） | 专业工具，功能完善 |
| 容器化部署 | 方案3（Docker初始化） | 自动化程度高 |

**对于 AccountBox 项目，建议使用方案1（SQL脚本）**，因为：
1. 项目规模适中
2. 支持多数据库
3. 便于版本控制
4. 团队协作友好
5. 可以逐步过渡到 Flyway
