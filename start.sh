#!/bin/bash

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 项目路径
BACKEND_DIR="backend/src/AccountBox.Api"
FRONTEND_DIR="frontend"

# 端口配置
BACKEND_PORT=5093
FRONTEND_PORT=5173

# 数据库初始化控制（默认不初始化）
INIT_DB=${INIT_DB:-0}   # 1=执行迁移/初始化，0=跳过
RESET_DB=${RESET_DB:-0} # 1=初始化前删除本地 SQLite 文件（仅本地开发）

# 交互式选择是否初始化/重置数据库（5秒无输入默认不初始化）
ask_init_reset() {
    echo -e "${YELLOW}是否初始化数据库？ [y/N]（5秒后默认 N）${NC}"
    read -t 5 -n 1 _ans_init || true
    echo ""
    if [[ "${_ans_init}" == "y" || "${_ans_init}" == "Y" ]]; then
        INIT_DB=1
        echo -e "${YELLOW}是否重置数据库（危险，将删除本地 SQLite 文件）？ [y/N]（5秒后默认 N）${NC}"
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

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}AccountBox 项目启动脚本${NC}"
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

# 启动后端
start_backend() {
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}启动后端服务${NC}"
    echo -e "${GREEN}========================================${NC}"

    # 检查并清理端口
    kill_port $BACKEND_PORT "后端"

    # 统一的 SQLite 数据文件路径（放在 backend/data/accountbox.db）
    # 在进入后端目录之前计算绝对路径
    DATA_DIR_FULL="$(pwd)/backend/data"
    mkdir -p "$DATA_DIR_FULL"

    # 进入后端目录
    cd $BACKEND_DIR

    DATA_DIR="../../data"
    DATA_DB_PATH="$DATA_DIR/accountbox.db"

    # 可选：在初始化前删除本地 SQLite DB（非交互）
    if [ "$INIT_DB" = "1" ] && [ "$RESET_DB" = "1" ]; then
        if [ -f "$DATA_DB_PATH" ]; then
            echo -e "${YELLOW}! 删除本地 SQLite 数据库文件...${NC}"
            rm -f "$DATA_DB_PATH"*
            echo -e "${GREEN}✓ 数据库文件已删除${NC}"
        fi
    fi

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

    # 应用数据库迁移（默认跳过）
    if [ "$INIT_DB" = "1" ]; then
        echo -e "${YELLOW}应用数据库迁移...${NC}"
        # 说明：项目在 backend/Directory.Build.props 中自定义了 BaseIntermediateOutputPath/MSBuildProjectExtensionsPath
        #       这会改变默认的 obj 目录位置，EF CLI 需要通过 --msbuildprojectextensionspath 显式告知该路径
        #       否则会出现 “该项目中不存在目标 GetEFProjectMetadata” 的错误。
        #       为避免跨项目目标导入路径不一致，这里使用“迁移项目”的 obj 目录
        EF_EXT_PATH="../../build/AccountBox.Data.Migrations.Sqlite/obj"
        # 使用 design-time 工厂时，通过环境变量告知 SQLite 的连接字符串（与运行时一致）
        export DB_PROVIDER=sqlite
        export DATABASE_PATH="$(cd "$DATA_DIR" && pwd)/accountbox.db"
        export CONNECTION_STRING="Data Source=${DATABASE_PATH}"

        if dotnet ef database update \
            -p ../AccountBox.Data.Migrations.Sqlite/AccountBox.Data.Migrations.Sqlite.csproj \
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
    # 运行时通过 DATABASE_PATH 覆盖 appsettings.json 中的相对路径，避免工作目录差异导致的找不到数据库文件
    export DB_PROVIDER=sqlite
    export DATA_PATH="$DATA_DIR_FULL"
    export DATABASE_PATH="$DATA_DIR_FULL/accountbox.db"
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
    # 交互式选择（5秒无输入默认不初始化）
    ask_init_reset
    # 确保在项目根目录
    if [ ! -d "$BACKEND_DIR" ] || [ ! -d "$FRONTEND_DIR" ]; then
        echo -e "${RED}错误: 请在项目根目录运行此脚本${NC}"
        exit 1
    fi

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

    # JWT认证系统已自动配置，无需额外初始化
    echo -e "\n${GREEN}✓ JWT认证系统已配置${NC}"

    # 完成
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}所有服务已启动完成！${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}后端服务: http://localhost:${BACKEND_PORT}${NC}"
    echo -e "${GREEN}前端服务: http://localhost:${FRONTEND_PORT}${NC}"
    echo -e "${GREEN}Swagger API 文档: http://localhost:${BACKEND_PORT}/swagger${NC}"

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
trap 'echo -e "\n${YELLOW}正在停止所有服务...${NC}"; kill_port $BACKEND_PORT "后端"; kill_port $FRONTEND_PORT "前端"; exit 0' INT

# 运行主函数
main
