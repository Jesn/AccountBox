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

# Compose 项目名（避免与其他栈冲突，down 时更安全）
COMPOSE_PROJECT="accountbox-mysql"

# 数据库初始化控制（默认不初始化）
INIT_DB=${INIT_DB:-0}   # 1=执行迁移/初始化，0=跳过
RESET_DB=${RESET_DB:-0} # 1=在初始化前重建数据库（危险，仅限本地开发）

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

# 交互式选择是否初始化/重置数据库（5秒无输入默认不初始化）
ask_init_reset() {
    echo -e "${YELLOW}是否初始化数据库？ [y/N]（5秒后默认 N）${NC}"
    read -t 5 -n 1 _ans_init || true
    echo ""
    if [[ "${_ans_init}" == "y" || "${_ans_init}" == "Y" ]]; then
        INIT_DB=1
        echo -e "${YELLOW}是否重置数据库（危险，将清空数据）？ [y/N]（5秒后默认 N）${NC}"
        read -t 5 -n 1 _ans_reset || true
        echo ""
        if [[ "${_ans_reset}" == "y" || "${_ans_reset}" == "Y" ]]; then
            RESET_DB=1
        else
            RESET_DB=0
        fi
    else
        INIT_DB=0
        RESET_DB=0
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

    # 仅启动 MySQL 与 phpMyAdmin，避免拉起 accountbox 应用容器造成端口/重名冲突
    echo -e "${YELLOW}启动 MySQL 和 phpMyAdmin 容器...${NC}"
    if docker-compose -p "$COMPOSE_PROJECT" -f docker-compose.mysql.yml up -d mysql phpmyadmin; then
        echo -e "${GREEN}✓ MySQL 容器创建成功${NC}"

        # 基于 healthcheck 等待，避免固定等待时间导致误判
        echo -e "${YELLOW}等待 MySQL 健康检查通过...${NC}"
        MAX_WAIT=60
        waited=0
        while true; do
            status=$(docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}starting{{end}}' accountbox-mysql 2>/dev/null || echo "missing")
            if [ "$status" = "healthy" ]; then
                echo -e "${GREEN}✓ MySQL 健康检查通过${NC}"
                break
            fi
            if [ "$status" = "exited" ] || [ "$status" = "dead" ] || [ "$status" = "missing" ]; then
                echo -e "${RED}✗ MySQL 容器未正常运行 (状态: $status)${NC}"
                echo -e "${YELLOW}容器日志:${NC}"
                docker logs --tail 100 accountbox-mysql || true
                exit 1
            fi
            if [ $waited -ge $MAX_WAIT ]; then
                echo -e "${RED}✗ 等待 MySQL 健康检查超时${NC}"
                echo -e "${YELLOW}容器状态: $status${NC}"
                docker logs --tail 100 accountbox-mysql || true
                exit 1
            fi
            sleep 2
            waited=$((waited+2))
        done

        # 连接验证（做一次重试）
        echo -e "${YELLOW}验证数据库连接...${NC}"
        if docker exec accountbox-mysql mysqladmin ping -h localhost -u "$DB_USER" -p"$DB_PASSWORD" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ MySQL 连接成功${NC}"
        else
            echo -e "${YELLOW}! 初次验证失败，重试中...${NC}"
            sleep 2
            if docker exec accountbox-mysql mysqladmin ping -h localhost -u "$DB_USER" -p"$DB_PASSWORD" > /dev/null 2>&1; then
                echo -e "${GREEN}✓ MySQL 连接成功${NC}"
            else
                echo -e "${RED}✗ MySQL 连接失败${NC}"
                docker logs --tail 100 accountbox-mysql || true
                exit 1
            fi
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

    # 在构建前导出供 MSBuild 使用的环境变量（用于选择迁移集）
    export DB_PROVIDER=$DB_PROVIDER
    export CONNECTION_STRING=$CONNECTION_STRING

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

    if [ "$INIT_DB" = "1" ]; then
        # 可选：在初始化前重建数据库（仅本地开发，危险）
        if [ "$RESET_DB" = "1" ]; then
            echo -e "${YELLOW}! 将重建 MySQL 数据库 ${DB_NAME} ...${NC}"
            # 使用 root 执行重建，确保权限充足（与 docker-compose.mysql.yml 中的 root 密码一致）
            docker exec accountbox-mysql sh -c "mysql -u\"root\" -p\"root123\" -e 'DROP DATABASE IF EXISTS ${DB_NAME}; CREATE DATABASE ${DB_NAME} CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;'" || {
                echo -e "${RED}✗ 重建数据库失败${NC}"; exit 1;
            }
        fi

        echo -e "${YELLOW}应用数据库迁移...${NC}"
        # 如业务表已存在但迁移历史缺失，则写入基线，避免重复建表冲突
        echo -e "${YELLOW}检查迁移历史表并进行必要的基线处理...${NC}"
        if docker exec accountbox-mysql mysql -u"$DB_USER" -p"$DB_PASSWORD" -N -s -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='${DB_NAME}' AND table_name='__EFMigrationsHistory_MySQL'" | grep -q '^0$'; then
            existing_tables=$(docker exec accountbox-mysql mysql -u"$DB_USER" -p"$DB_PASSWORD" -N -s -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='${DB_NAME}' AND table_name IN ('ApiKeys','Websites','Accounts','ApiKeyWebsiteScopes','LoginAttempts')")
            if [ "${existing_tables}" -gt 0 ]; then
                echo -e "${YELLOW}检测到业务表已存在但迁移历史缺失，正在写入基线记录...${NC}"
                MIGRATION_ID=$(ls ../AccountBox.Data.Migrations.MySQL/Migrations/*_MySQL_InitialCreate.cs 2>/dev/null | xargs -n1 basename | sed 's/\.cs$//' | head -n1)
                PRODUCT_VERSION=$(awk -F '"' '/ProductVersion/ {print $4; exit}' ../AccountBox.Data.Migrations.MySQL/Migrations/AccountBoxDbContextModelSnapshot.cs 2>/dev/null)
                [ -z "$PRODUCT_VERSION" ] && PRODUCT_VERSION="9.0.10"
                if [ -n "$MIGRATION_ID" ]; then
                    docker exec accountbox-mysql sh -c "mysql -u\"$DB_USER\" -p\"$DB_PASSWORD\" ${DB_NAME} -e 'CREATE TABLE IF NOT EXISTS \`__EFMigrationsHistory_MySQL\` (\`MigrationId\` varchar(150) NOT NULL, \`ProductVersion\` varchar(32) NOT NULL, PRIMARY KEY (\`MigrationId\`)) DEFAULT CHARSET=utf8mb4; INSERT IGNORE INTO \`__EFMigrationsHistory_MySQL\` (\`MigrationId\`, \`ProductVersion\`) VALUES (\"$MIGRATION_ID\", \"$PRODUCT_VERSION\");'" || {
                        echo -e "${RED}✗ 写入 MySQL 迁移基线失败${NC}"; exit 1;
                    }
                    echo -e "${GREEN}✓ 迁移基线已写入 (${MIGRATION_ID})${NC}"
                else
                    echo -e "${YELLOW}! 未找到 MySQL 初始迁移文件，跳过基线写入${NC}"
                fi
            fi
        fi

        # 构建阶段已导出环境变量
        # 自定义了 BaseIntermediateOutputPath/MSBuildProjectExtensionsPath，需传入 EF 的扩展路径以避免 GetEFProjectMetadata 错误
        # 指向 MySQL 迁移项目的 obj 目录，确保 msbuild 能导入该项目的 EF 目标
        EF_EXT_PATH="../../build/AccountBox.Data.Migrations.MySQL/obj"
        if dotnet ef database update \
            -p ../AccountBox.Data.Migrations.MySQL/AccountBox.Data.Migrations.MySQL.csproj \
            -s ./AccountBox.Api.csproj \
            --msbuildprojectextensionspath "$EF_EXT_PATH"; then
            echo -e "${GREEN}✓ 数据库迁移成功${NC}"
        else
            echo -e "${RED}✗ 数据库迁移失败${NC}"
            exit 1
        fi
    else
        echo -e "${YELLOW}跳过数据库初始化（INIT_DB=0）${NC}"
    fi

    # 启动后端
    echo -e "${YELLOW}启动后端服务 (端口: ${BACKEND_PORT})...${NC}"
    export MASTER_PASSWORD="admin123"
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
    # 解析命令行参数（可选）
    ARGS_OVERRIDE=0
    for arg in "$@"; do
        case "$arg" in
            --init-db) INIT_DB=1; ARGS_OVERRIDE=1 ;;
            --no-init-db) INIT_DB=0; ARGS_OVERRIDE=1 ;;
            --reset-db) RESET_DB=1; ARGS_OVERRIDE=1 ;;
        esac
    done
    # 若未通过参数明确指定，进行5秒超时的交互式选择
    if [ "$ARGS_OVERRIDE" = "0" ]; then
        ask_init_reset
    fi
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

    # 显示默认主密码
    MASTER_PASSWORD_DEFAULT="admin123"

    echo -e "\n${YELLOW}╔════════════════════════════════════════╗${NC}"
    echo -e "${YELLOW}║     开发环境JWT认证信息                 ║${NC}"
    echo -e "${YELLOW}╠════════════════════════════════════════╣${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║  主密码: ${GREEN}${MASTER_PASSWORD_DEFAULT}${YELLOW}              ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║  使用此密码在前端登录页面进行登录       ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}╚════════════════════════════════════════╝${NC}"

    echo -e "\n${YELLOW}提示: 按 Ctrl+C 可以停止所有服务${NC}"

    # 等待用户中断
    wait
}

# 捕获 Ctrl+C 信号
trap 'echo -e "\n${YELLOW}正在停止所有服务...${NC}"; kill_port $BACKEND_PORT "后端"; kill_port $FRONTEND_PORT "前端"; docker-compose -p "$COMPOSE_PROJECT" -f docker-compose.mysql.yml down; exit 0' INT

# 运行主函数
main "$@"
