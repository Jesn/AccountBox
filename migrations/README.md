# 数据库迁移脚本

本目录包含所有数据库的 SQL 迁移脚本。

## 📁 目录结构

```
migrations/
├── postgresql/
│   └── V001__Initial_schema.sql    (21 KB)
├── mysql/
│   └── V001__Initial_schema.sql    (31 KB)
└── sqlite/
    └── V001__Initial_schema.sql    (11 KB)
```

## 🔄 生成迁移脚本

### PostgreSQL

```bash
cd backend/src/AccountBox.Data
export DB_PROVIDER=postgresql
dotnet ef migrations script \
  --idempotent \
  --output ../../../migrations/postgresql/V002__Your_migration_name.sql \
  --context AccountBoxDbContext \
  --project . \
  --startup-project ../AccountBox.Api
```

### MySQL

```bash
cd backend/src/AccountBox.Data
export DB_PROVIDER=mysql
dotnet ef migrations script \
  --idempotent \
  --output ../../../migrations/mysql/V002__Your_migration_name.sql \
  --context AccountBoxDbContext \
  --project . \
  --startup-project ../AccountBox.Api
```

### SQLite

**注意**: SQLite 不支持 `--idempotent` 选项

```bash
cd backend/src/AccountBox.Data
export DB_PROVIDER=sqlite
dotnet ef migrations script \
  --output ../../../migrations/sqlite/V002__Your_migration_name.sql \
  --context AccountBoxDbContext \
  --project . \
  --startup-project ../AccountBox.Api
```

## 📝 命名规范

迁移脚本使用以下命名规范：

```
V{版本号}__{描述}.sql
```

示例：
- `V001__Initial_schema.sql` - 初始数据库架构
- `V002__Add_user_table.sql` - 添加用户表
- `V003__Add_indexes.sql` - 添加索引
- `V004__Alter_column_type.sql` - 修改列类型

## 🚀 应用迁移

### Docker 部署（自动）

使用 Docker 部署时，迁移会在容器首次启动时自动应用。

### 手动应用

#### PostgreSQL

```bash
# 方式 1: 使用 psql
psql -U accountbox -d accountbox -f migrations/postgresql/V002__Your_migration.sql

# 方式 2: 使用 Docker
docker exec -i accountbox-postgres-prod psql -U accountbox -d accountbox \
  < migrations/postgresql/V002__Your_migration.sql
```

#### MySQL

```bash
# 方式 1: 使用 mysql 客户端
mysql -u accountbox -p accountbox < migrations/mysql/V002__Your_migration.sql

# 方式 2: 使用 Docker
docker exec -i accountbox-mysql-prod mysql -u accountbox -pYourPassword accountbox \
  < migrations/mysql/V002__Your_migration.sql
```

#### SQLite

```bash
sqlite3 accountbox.db < migrations/sqlite/V002__Your_migration.sql
```

## ⚠️ 重要说明

### PostgreSQL 和 MySQL

- ✅ 使用 `--idempotent` 选项生成
- ✅ 可以安全地重复执行
- ✅ 包含 `IF NOT EXISTS` 检查

### SQLite

- ⚠️ **不支持** `--idempotent` 选项
- ⚠️ 重复执行可能导致错误
- ⚠️ 需要手动添加 `IF NOT EXISTS` 检查（如果需要）

## 📚 相关文档

- [数据库迁移指南](../docs/database-migration-guide.md)
- [Docker 部署指南](../docs/docker-deployment-guide.md)
- [EF Core 迁移文档](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

## 🔍 验证迁移

### 检查迁移历史

#### PostgreSQL

```bash
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox \
  -c "SELECT * FROM \"__EFMigrationsHistory_PostgreSQL\";"
```

#### MySQL

```bash
docker exec accountbox-mysql-prod mysql -u accountbox -pYourPassword accountbox \
  -e "SELECT * FROM __EFMigrationsHistory_MySQL;"
```

#### SQLite

```bash
sqlite3 accountbox.db "SELECT * FROM __EFMigrationsHistory_Sqlite;"
```

### 检查表结构

#### PostgreSQL

```bash
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\dt"
docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\d+ Websites"
```

#### MySQL

```bash
docker exec accountbox-mysql-prod mysql -u accountbox -pYourPassword accountbox \
  -e "SHOW TABLES;"
docker exec accountbox-mysql-prod mysql -u accountbox -pYourPassword accountbox \
  -e "DESCRIBE Websites;"
```

#### SQLite

```bash
sqlite3 accountbox.db ".tables"
sqlite3 accountbox.db ".schema Websites"
```

## 🛠️ 故障排查

### 问题 1: 迁移脚本未生成

**原因**: 没有待应用的迁移

**解决**:
```bash
# 检查迁移状态
dotnet ef migrations list

# 如果需要，添加新迁移
dotnet ef migrations add YourMigrationName
```

### 问题 2: SQLite idempotent 错误

**错误信息**: `Generating idempotent scripts for migrations is not currently supported for SQLite`

**解决**: 移除 `--idempotent` 选项
```bash
dotnet ef migrations script --output migrations/sqlite/V002__Migration.sql
```

### 问题 3: 迁移应用失败

**原因**: 数据库状态与迁移不匹配

**解决**:
```bash
# 1. 检查当前数据库状态
psql -U accountbox -d accountbox -c "\dt"

# 2. 检查迁移历史
psql -U accountbox -d accountbox \
  -c "SELECT * FROM \"__EFMigrationsHistory_PostgreSQL\";"

# 3. 如果需要，手动修复数据库状态
```

## 📊 迁移脚本对比

| 特性 | PostgreSQL | MySQL | SQLite |
|------|-----------|-------|--------|
| 文件大小 | 21 KB | 31 KB | 11 KB |
| Idempotent | ✅ 支持 | ✅ 支持 | ❌ 不支持 |
| 自增主键 | SERIAL | AUTO_INCREMENT | AUTOINCREMENT |
| 时间类型 | timestamp with time zone | datetime | TEXT |
| 布尔类型 | boolean | tinyint(1) | INTEGER |
| JSON 类型 | jsonb | json | TEXT |

## 🔐 安全建议

1. **备份数据库** - 应用迁移前务必备份
2. **测试环境验证** - 先在测试环境验证迁移
3. **版本控制** - 将迁移脚本提交到 Git
4. **代码审查** - 迁移脚本需要代码审查
5. **回滚计划** - 准备回滚脚本

## 📅 迁移历史

| 版本 | 日期 | 描述 | 作者 |
|------|------|------|------|
| V001 | 2025-10-20 | 初始数据库架构 | System |

---

**提示**: 更多详细信息请参考 [数据库迁移指南](../docs/database-migration-guide.md)
