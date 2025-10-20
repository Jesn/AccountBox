#!/bin/bash
set -e

# PostgreSQL 数据库初始化脚本
# 此脚本会在容器首次启动时自动执行

echo "========================================="
echo "AccountBox PostgreSQL 初始化"
echo "========================================="

# 应用迁移脚本
echo "应用数据库迁移..."

# 检查迁移脚本是否存在
if [ -f /docker-entrypoint-initdb.d/migrations/V001__Initial_schema.sql ]; then
    echo "执行 V001__Initial_schema.sql..."
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f /docker-entrypoint-initdb.d/migrations/V001__Initial_schema.sql
    echo "✓ 迁移完成"
else
    echo "⚠ 未找到迁移脚本，跳过迁移"
fi

echo "========================================="
echo "PostgreSQL 初始化完成"
echo "========================================="
