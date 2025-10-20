#!/bin/bash

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 项目路径
BACKEND_DIR="backend/src/AccountBox.Api"
FRONTEND_DIR="frontend"

# 端口配置
BACKEND_PORT=5093
FRONTEND_PORT=5173
MYSQL_PORT=3306
PHPMYADMIN_PORT=8080

# MySQL 配置
DB_PROVIDER="mysql"
DB_HOST="localhost"
DB_PORT="3306"
DB_USER="accountbox"
DB_PASSWORD="accountbox123"
DB_NAME="accountbox"
CONNECTION_STRING="Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};User=${DB_USER};Password=${DB_PASSWORD}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AccountBox MySQL 启动脚本${NC}"
echo -e "${GREEN}========================================${NC}"

# 检查并杀掉占用端口的进程
kill_port() {
    local port=$1
    local service_name=$2

    echo -e "${YELLOW}检查端口 ${port} 是否被占用...${NC}"

    # 查找占用端口的进程
    local pid=$(lsof -ti:$port)

    if [ ! -z "$pid" ]; then
        echo -e "${RED}端口 ${port} 被进程 ${pid} 占用${NC}"
        echo -e "${YELLOW}正在终止进程...${NC}"
        kill -9 $pid
        sleep 1
        echo -e "${GREEN}✓ 进程已终止${NC}"
    else
        echo -e "${GREEN}✓ 端口 ${port} 未被占用${NC}"
    fi
}

# 启动 MySQL 容器
start_mysql() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动 MySQL 容器${NC}"
    echo -e "${GREEN}========================================${NC}"

    # 检查 Docker 是否安装
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}✗ Docker 未安装${NC}"
        exit 1
    fi

    # 检查 Docker Compose 是否安装
    if ! command -v docker-compose &> /dev/null; then
        echo -e "${RED}✗ Docker Compose 未安装${NC}"
        exit 1
    fi

    # 启动 MySQL 容器
    echo -e "${YELLOW}启动 MySQL 和 phpMyAdmin 容器...${NC}"
    if docker-compose -f docker-compose.mysql.yml up -d; then
        echo -e "${GREEN}✓ MySQL 容器已启动${NC}"

        # 等待 MySQL 启动
        echo -e "${YELLOW}等待 MySQL 启动...${NC}"
        sleep 5

        # 验证连接
        echo -e "${YELLOW}验证数据库连接...${NC}"
        if docker exec accountbox-mysql-test mysqladmin ping -h localhost -u $DB_USER -p$DB_PASSWORD > /dev/null 2>&1; then
            echo -e "${GREEN}✓ MySQL 连接成功${NC}"
        else
            echo -e "${RED}✗ MySQL 连接失败${NC}"
            exit 1
        fi
    else
        echo -e "${RED}✗ MySQL 容器启动失败${NC}"
        exit 1
    fi
}

# 启动后端
start_backend() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动后端服务${NC}"
    echo -e "${GREEN}========================================${NC}"

    # 检查并清理端口
    kill_port $BACKEND_PORT "后端"

    # 进入后端目录
    cd $BACKEND_DIR

    # 清理之前的构建
    echo -e "${YELLOW}清理之前的构建...${NC}"
    dotnet clean > /dev/null 2>&1

    # 构建后端
    echo -e "${YELLOW}构建后端项目...${NC}"
    if dotnet build; then
        echo -e "${GREEN}✓ 后端构建成功${NC}"
    else
        echo -e "${RED}✗ 后端构建失败${NC}"
        exit 1
    fi

    # 应用数据库迁移
    echo -e "${YELLOW}应用数据库迁移...${NC}"
    export DB_PROVIDER=$DB_PROVIDER
    export CONNECTION_STRING=$CONNECTION_STRING

    if dotnet ef database update; then
        echo -e "${GREEN}✓ 数据库迁移成功${NC}"
    else
        echo -e "${RED}✗ 数据库迁移失败${NC}"
        exit 1
    fi

    # 启动后端
    echo -e "${YELLOW}启动后端服务 (端口: ${BACKEND_PORT})...${NC}"
    dotnet run &
    BACKEND_PID=$!

    echo -e "${GREEN}✓ 后端服务已启动 (PID: ${BACKEND_PID})${NC}"

    # 返回根目录
    cd ../../..
}

# 启动前端
start_frontend() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动前端服务${NC}"
    echo -e "${GREEN}========================================${NC}"

    # 检查并清理端口
    kill_port $FRONTEND_PORT "前端"

    # 进入前端目录
    cd $FRONTEND_DIR

    # 检查包管理器
    if [ -f "pnpm-lock.yaml" ]; then
        PKG_MANAGER="pnpm"
    elif [ -f "package-lock.json" ]; then
        PKG_MANAGER="npm"
    elif [ -f "yarn.lock" ]; then
        PKG_MANAGER="yarn"
    else
        PKG_MANAGER="npm"
    fi

    echo -e "${YELLOW}使用包管理器: ${PKG_MANAGER}${NC}"

    # 安装依赖（如果需要）
    if [ ! -d "node_modules" ]; then
        echo -e "${YELLOW}安装依赖...${NC}"
        $PKG_MANAGER install
    fi

    # 构建前端
    echo -e "${YELLOW}构建前端项目...${NC}"
    if $PKG_MANAGER run build; then
        echo -e "${GREEN}✓ 前端构建成功${NC}"
    else
        echo -e "${RED}✗ 前端构建失败${NC}"
        exit 1
    fi

    # 启动前端开发服务器
    echo -e "${YELLOW}启动前端开发服务器 (端口: ${FRONTEND_PORT})...${NC}"
    $PKG_MANAGER run dev &
    FRONTEND_PID=$!

    echo -e "${GREEN}✓ 前端服务已启动 (PID: ${FRONTEND_PID})${NC}"

    # 返回根目录
    cd ..
}

# 主函数
main() {
    # 确保在项目根目录
    if [ ! -d "$BACKEND_DIR" ] || [ ! -d "$FRONTEND_DIR" ]; then
        echo -e "${RED}错误: 请在项目根目录运行此脚本${NC}"
        exit 1
    fi

    # 检查 docker-compose.mysql.yml 是否存在
    if [ ! -f "docker-compose.mysql.yml" ]; then
        echo -e "${RED}错误: docker-compose.mysql.yml 文件不存在${NC}"
        exit 1
    fi

    # 启动 MySQL
    start_mysql

    # 等待 MySQL 完全启动
    echo -e "${YELLOW}等待 MySQL 完全启动...${NC}"
    sleep 3

    # 启动后端
    start_backend

    # 等待后端启动
    echo -e "${YELLOW}等待后端服务启动...${NC}"
    sleep 3

    # 启动前端
    start_frontend

    # 等待服务完全启动
    echo -e "${YELLOW}等待服务完全启动...${NC}"
    sleep 5

    # 完成
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}所有服务已启动完成！${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}后端服务: http://localhost:${BACKEND_PORT}${NC}"
    echo -e "${GREEN}前端服务: http://localhost:${FRONTEND_PORT}${NC}"
    echo -e "${GREEN}Swagger API 文档: http://localhost:${BACKEND_PORT}/swagger${NC}"
    echo -e "${GREEN}phpMyAdmin: http://localhost:${PHPMYADMIN_PORT}${NC}"

    # 显示 MySQL 连接信息
    echo -e "\n${BLUE}╔════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║     MySQL 连接信息                      ║${NC}"
    echo -e "${BLUE}╠════════════════════════════════════════╣${NC}"
    echo -e "${BLUE}║                                        ║${NC}"
    echo -e "${BLUE}║  主机: ${GREEN}${DB_HOST}${BLUE}                      ║${NC}"
    echo -e "${BLUE}║  端口: ${GREEN}${DB_PORT}${BLUE}                        ║${NC}"
    echo -e "${BLUE}║  用户: ${GREEN}${DB_USER}${BLUE}                    ║${NC}"
    echo -e "${BLUE}║  密码: ${GREEN}${DB_PASSWORD}${BLUE}                ║${NC}"
    echo -e "${BLUE}║  数据库: ${GREEN}${DB_NAME}${BLUE}                   ║${NC}"
    echo -e "${BLUE}║                                        ║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"

    # 显示开发环境主密码
    echo -e "\n${YELLOW}╔════════════════════════════════════════╗${NC}"
    echo -e "${YELLOW}║     开发环境JWT认证信息                 ║${NC}"
    echo -e "${YELLOW}╠════════════════════════════════════════╣${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║        主密码: ${GREEN}admin123${YELLOW}              ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║  使用此密码在前端登录页面进行登录       ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}╚════════════════════════════════════════╝${NC}"

    echo -e "\n${YELLOW}提示: 按 Ctrl+C 可以停止所有服务${NC}"

    # 等待用户中断
    wait
}

# 捕获 Ctrl+C 信号
trap 'echo -e "\n${YELLOW}正在停止所有服务...${NC}"; kill_port $BACKEND_PORT "后端"; kill_port $FRONTEND_PORT "前端"; docker-compose -f docker-compose.mysql.yml down; exit 0' INT

# 运行主函数
main
