#!/bin/bash

# ============================================
# AccountBox Docker 部署脚本
# ============================================

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 配置
ENV_FILE=".env.prod"
COMPOSE_FILE="docker-compose.prod.yml"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AccountBox Docker 部署${NC}"
echo -e "${GREEN}========================================${NC}"

# 检查环境变量文件
if [ ! -f "$ENV_FILE" ]; then
    echo -e "${RED}✗ 未找到环境变量文件: $ENV_FILE${NC}"
    echo -e "${YELLOW}请复制 .env.prod.example 并修改配置：${NC}"
    echo -e "${YELLOW}  cp .env.prod.example .env.prod${NC}"
    echo -e "${YELLOW}  vim .env.prod${NC}"
    exit 1
fi

echo -e "${GREEN}✓ 找到环境变量文件${NC}"

# 检查 Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker 未安装${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker 已安装${NC}"

# 检查 Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}✗ Docker Compose 未安装${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Docker Compose 已安装${NC}"

# 读取数据库类型
DB_PROVIDER=$(grep "^DB_PROVIDER=" "$ENV_FILE" | cut -d '=' -f2)
echo -e "${BLUE}数据库类型: $DB_PROVIDER${NC}"

# 构建镜像
echo -e "\n${YELLOW}构建 Docker 镜像...${NC}"
docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build

echo -e "${GREEN}✓ 镜像构建完成${NC}"

# 启动服务
echo -e "\n${YELLOW}启动服务...${NC}"

if [ "$DB_PROVIDER" = "postgresql" ] || [ "$DB_PROVIDER" = "postgres" ]; then
    echo -e "${BLUE}使用 PostgreSQL 数据库${NC}"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d postgres backend
elif [ "$DB_PROVIDER" = "mysql" ]; then
    echo -e "${BLUE}使用 MySQL 数据库${NC}"
    docker-compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d mysql backend
else
    echo -e "${RED}✗ 不支持的数据库类型: $DB_PROVIDER${NC}"
    exit 1
fi

echo -e "${GREEN}✓ 服务启动完成${NC}"

# 等待服务就绪
echo -e "\n${YELLOW}等待服务就绪...${NC}"
sleep 10

# 检查服务状态
echo -e "\n${YELLOW}检查服务状态...${NC}"
docker-compose -f "$COMPOSE_FILE" ps

# 检查健康状态
echo -e "\n${YELLOW}检查应用健康状态...${NC}"
if curl -f http://localhost:5093/health > /dev/null 2>&1; then
    echo -e "${GREEN}✓ 应用健康检查通过${NC}"
else
    echo -e "${RED}✗ 应用健康检查失败${NC}"
    echo -e "${YELLOW}查看日志：${NC}"
    docker-compose -f "$COMPOSE_FILE" logs --tail=50 backend
    exit 1
fi

# 验证数据库
echo -e "\n${YELLOW}验证数据库...${NC}"
if [ "$DB_PROVIDER" = "postgresql" ] || [ "$DB_PROVIDER" = "postgres" ]; then
    docker exec accountbox-postgres-prod psql -U accountbox -d accountbox -c "\dt" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ PostgreSQL 数据库验证通过${NC}"
    else
        echo -e "${RED}✗ PostgreSQL 数据库验证失败${NC}"
    fi
elif [ "$DB_PROVIDER" = "mysql" ]; then
    docker exec accountbox-mysql-prod mysql -u accountbox -p$(grep "^MYSQL_PASSWORD=" "$ENV_FILE" | cut -d '=' -f2) accountbox -e "SHOW TABLES;" > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ MySQL 数据库验证通过${NC}"
    else
        echo -e "${RED}✗ MySQL 数据库验证失败${NC}"
    fi
fi

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}部署完成！${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${BLUE}应用地址: http://localhost:5093${NC}"
echo -e "${BLUE}健康检查: http://localhost:5093/health${NC}"
echo -e "${BLUE}API 文档: http://localhost:5093/swagger${NC}"
echo -e "\n${YELLOW}常用命令：${NC}"
echo -e "  查看日志: docker-compose -f $COMPOSE_FILE logs -f backend"
echo -e "  停止服务: docker-compose -f $COMPOSE_FILE down"
echo -e "  重启服务: docker-compose -f $COMPOSE_FILE restart backend"
echo -e "${GREEN}========================================${NC}"
