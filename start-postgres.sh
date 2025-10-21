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
POSTGRES_PORT=5432
PGADMIN_PORT=5050

# 数据库初始化控制（默认不初始化）
INIT_DB=${INIT_DB:-0}   # 1=执行迁移/初始化，0=跳过
RESET_DB=${RESET_DB:-0} # 1=在初始化前清空数据库（仅本地开发）

# Compose 项目名（隔离不同栈，避免 down 时误删其它容器）
COMPOSE_PROJECT="accountbox-postgres"

# PostgreSQL 配置
DB_PROVIDER="postgresql"
DB_HOST="localhost"
DB_PORT="5432"
DB_USER="accountbox"
DB_PASSWORD="accountbox123"
DB_NAME="accountbox"
CONNECTION_STRING="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AccountBox PostgreSQL 启动脚本${NC}"
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

# 启动 PostgreSQL 容器
start_postgres() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动 PostgreSQL 容器${NC}"
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

    # 启动 PostgreSQL 与 pgAdmin（仅启动所需服务，避免与 accountbox-app 重名冲突）
    echo -e "${YELLOW}启动 PostgreSQL 和 pgAdmin 容器...${NC}"
    if docker-compose -p "$COMPOSE_PROJECT" -f docker-compose.postgres.yml up -d postgres pgadmin; then
        echo -e "${GREEN}✓ PostgreSQL 容器创建成功${NC}"

        # 等待容器健康（基于 docker healthcheck），避免固定等待时间导致的误报
        echo -e "${YELLOW}等待 PostgreSQL 健康检查通过...${NC}"
        MAX_WAIT=60
        waited=0
        while true; do
            status=$(docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}starting{{end}}' accountbox-postgres 2>/dev/null || echo "missing")
            if [ "$status" = "healthy" ]; then
                echo -e "${GREEN}✓ PostgreSQL 健康检查通过${NC}"
                break
            fi
            if [ "$status" = "exited" ] || [ "$status" = "dead" ] || [ "$status" = "missing" ]; then
                echo -e "${RED}✗ PostgreSQL 容器未正常运行 (状态: $status)${NC}"
                echo -e "${YELLOW}容器日志:${NC}"
                docker logs --tail 100 accountbox-postgres || true
                exit 1
            fi
            if [ $waited -ge $MAX_WAIT ]; then
                echo -e "${RED}✗ 等待 PostgreSQL 健康检查超时${NC}"
                echo -e "${YELLOW}容器状态: $status${NC}"
                docker logs --tail 100 accountbox-postgres || true
                exit 1
            fi
            sleep 2
            waited=$((waited+2))
        done

        # 最终验证连接（可能仍需 1~2s，做一次重试）
        echo -e "${YELLOW}验证数据库连接...${NC}"
        if docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -c "SELECT version();" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ PostgreSQL 连接成功${NC}"
        else
            echo -e "${YELLOW}! 初次验证失败，重试中...${NC}"
            sleep 2
            if docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -c "SELECT version();" > /dev/null 2>&1; then
                echo -e "${GREEN}✓ PostgreSQL 连接成功${NC}"
            else
                echo -e "${RED}✗ PostgreSQL 连接失败${NC}"
                exit 1
            fi
        fi
    else
        echo -e "${RED}✗ PostgreSQL 容器启动失败${NC}"
        exit 1
    fi
}

# 启动后端
start_backend() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动后端服务${NC}"
    echo -e "${GREEN}========================================${NC}"

    # 清理所有 MSBuild 节点进程
    echo -e "${YELLOW}清理 MSBuild 节点进程...${NC}"
    pkill -9 -f "MSBuild.dll.*nodemode:1" 2>/dev/null || true
    sleep 1

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
        # 可选：在初始化前清空数据库（仅开发场景）
        if [ "$RESET_DB" = "1" ]; then
            echo -e "${YELLOW}! 将清空 PostgreSQL 当前 schema（public）...${NC}"
            docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -v ON_ERROR_STOP=1 -c "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO $DB_USER; GRANT ALL ON SCHEMA public TO public;" || {
                echo -e "${RED}✗ 清空数据库失败${NC}"; exit 1;
            }
        fi

        echo -e "${YELLOW}应用数据库迁移...${NC}"
        # 如果表已经存在但迁移历史表缺失，则进行 baseline，避免重复创建导致的冲突
        echo -e "${YELLOW}检查迁移历史表并进行必要的基线处理...${NC}"
        if docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -XtAc "SELECT to_regclass('__EFMigrationsHistory_PostgreSQL') IS NOT NULL" | grep -q 'f'; then
            existing_tables=$(docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -XtAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_name IN ('ApiKeys','Websites','Accounts','ApiKeyWebsiteScopes','LoginAttempts')")
            if [ "${existing_tables}" -gt 0 ]; then
                echo -e "${YELLOW}检测到业务表已存在但迁移历史缺失，正在写入基线记录...${NC}"

                # 从迁移文件解析 Postgres 初始迁移 ID 与 ProductVersion
                MIGRATION_ID=$(ls ../AccountBox.Data.Migrations.PostgreSQL/Migrations/*_PostgreSQL_InitialCreate.cs 2>/dev/null | xargs -n1 basename | sed 's/\.cs$//' | head -n1)
                PRODUCT_VERSION=$(awk -F '"' '/ProductVersion/ {print $4; exit}' ../AccountBox.Data.Migrations.PostgreSQL/Migrations/AccountBoxDbContextModelSnapshot.cs 2>/dev/null)
                [ -z "$PRODUCT_VERSION" ] && PRODUCT_VERSION="9.0.10"

                if [ -n "$MIGRATION_ID" ]; then
                    docker exec accountbox-postgres psql -U $DB_USER -d $DB_NAME -v ON_ERROR_STOP=1 -c "\
                    CREATE TABLE IF NOT EXISTS public.\"__EFMigrationsHistory_PostgreSQL\" (\
                        \"MigrationId\" character varying(150) NOT NULL,\
                        \"ProductVersion\" character varying(32) NOT NULL,\
                        CONSTRAINT \"__EFMigrationsHistory_PostgreSQL_pkey\" PRIMARY KEY (\"MigrationId\")\
                    );\
                    INSERT INTO public.\"__EFMigrationsHistory_PostgreSQL\" (\"MigrationId\", \"ProductVersion\")\
                    VALUES ('$MIGRATION_ID', '$PRODUCT_VERSION')\
                    ON CONFLICT (\"MigrationId\") DO NOTHING;" || {
                        echo -e "${RED}✗ 写入迁移基线失败${NC}"; exit 1;
                    }
                    echo -e "${GREEN}✓ 迁移基线已写入 (${MIGRATION_ID})${NC}"
                else
                    echo -e "${YELLOW}! 未找到 PostgreSQL 初始迁移文件，跳过基线写入${NC}"
                fi
            fi
        fi

        # 使用 PostgreSQL 迁移程序集执行更新
        # 自定义了 BaseIntermediateOutputPath/MSBuildProjectExtensionsPath，需传入 EF 的扩展路径以避免 GetEFProjectMetadata 错误
        # 为避免 EF 在不同项目上使用不同的扩展路径，这里将扩展路径指向“迁移项目”的 obj 目录
        EF_EXT_PATH="../../build/AccountBox.Data.Migrations.PostgreSQL/obj"
        if dotnet ef database update \
            -p ../AccountBox.Data.Migrations.PostgreSQL/AccountBox.Data.Migrations.PostgreSQL.csproj \
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

    # 检查 docker-compose.postgres.yml 是否存在
    if [ ! -f "docker-compose.postgres.yml" ]; then
        echo -e "${RED}错误: docker-compose.postgres.yml 文件不存在${NC}"
        exit 1
    fi

    # 启动 PostgreSQL
    start_postgres

    # 等待 PostgreSQL 完全启动
    echo -e "${YELLOW}等待 PostgreSQL 完全启动...${NC}"
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
    echo -e "${GREEN}pgAdmin: http://localhost:${PGADMIN_PORT}${NC}"

    # 显示 PostgreSQL 连接信息
    echo -e "\n${BLUE}╔════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║     PostgreSQL 连接信息                 ║${NC}"
    echo -e "${BLUE}╠════════════════════════════════════════╣${NC}"
    echo -e "${BLUE}║                                        ║${NC}"
    echo -e "${BLUE}║  主机: ${GREEN}${DB_HOST}${BLUE}                      ║${NC}"
    echo -e "${BLUE}║  端口: ${GREEN}${DB_PORT}${BLUE}                        ║${NC}"
    echo -e "${BLUE}║  用户: ${GREEN}${DB_USER}${BLUE}                    ║${NC}"
    echo -e "${BLUE}║  密码: ${GREEN}${DB_PASSWORD}${BLUE}                ║${NC}"
    echo -e "${BLUE}║  数据库: ${GREEN}${DB_NAME}${BLUE}                   ║${NC}"
    echo -e "${BLUE}║                                        ║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════╝${NC}"

    # 读取实际生成的主密码
    SECRETS_DIR="backend/data/.secrets"
    MASTER_PASSWORD_FILE="$SECRETS_DIR/master.key"

    echo -e "\n${YELLOW}╔════════════════════════════════════════╗${NC}"
    echo -e "${YELLOW}║     开发环境JWT认证信息                 ║${NC}"
    echo -e "${YELLOW}╠════════════════════════════════════════╣${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"

    if [ -f "$MASTER_PASSWORD_FILE" ]; then
        MASTER_PASSWORD=$(cat "$MASTER_PASSWORD_FILE")
        echo -e "${YELLOW}║  主密码: ${GREEN}${MASTER_PASSWORD}${YELLOW}        ║${NC}"
        echo -e "${YELLOW}║                                        ║${NC}"
        echo -e "${YELLOW}║  密码已保存到:                         ║${NC}"
        echo -e "${YELLOW}║  ${MASTER_PASSWORD_FILE}  ║${NC}"
    else
        echo -e "${YELLOW}║  主密码: ${GREEN}(首次启动时自动生成)${YELLOW}      ║${NC}"
        echo -e "${YELLOW}║                                        ║${NC}"
        echo -e "${YELLOW}║  请查看后端启动日志获取主密码           ║${NC}"
    fi

    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║  使用此密码在前端登录页面进行登录       ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}╚════════════════════════════════════════╝${NC}"

    echo -e "\n${YELLOW}提示: 按 Ctrl+C 可以停止所有服务${NC}"

    # 等待用户中断
    wait
}

# 捕获 Ctrl+C 信号
trap 'echo -e "\n${YELLOW}正在停止所有服务...${NC}"; kill_port $BACKEND_PORT "后端"; kill_port $FRONTEND_PORT "前端"; docker-compose -p "$COMPOSE_PROJECT" -f docker-compose.postgres.yml down; exit 0' INT

# 运行主函数
main "$@"
