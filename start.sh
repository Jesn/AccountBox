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

    # 进入后端目录
    cd $BACKEND_DIR

    # 检查是否需要重置数据库
    if [ -f "accountbox.db" ]; then
        echo -e "${YELLOW}检测到已存在数据库${NC}"
        echo -e "${YELLOW}是否需要重置数据库并使用新密码 Test1234! ？ (y/N)${NC}"
        read -t 10 -n 1 reset_db
        echo ""
        if [[ $reset_db =~ ^[Yy]$ ]]; then
            echo -e "${YELLOW}删除旧数据库...${NC}"
            rm -f accountbox.db*
            echo -e "${GREEN}✓ 数据库已删除${NC}"
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

    # 检查是否需要初始化数据库
    echo -e "\n${YELLOW}检查数据库初始化状态...${NC}"
    INIT_STATUS=$(curl -s http://localhost:${BACKEND_PORT}/api/vault/status | grep -o '"isInitialized":[^,}]*' | cut -d':' -f2)

    if [ "$INIT_STATUS" = "false" ]; then
        echo -e "${YELLOW}数据库尚未初始化，正在自动初始化...${NC}"
        INIT_RESPONSE=$(curl -s -X POST http://localhost:${BACKEND_PORT}/api/vault/initialize \
            -H 'Content-Type: application/json' \
            -d '{"masterPassword":"Test1234!"}')

        if echo "$INIT_RESPONSE" | grep -q '"success":true'; then
            echo -e "${GREEN}✓ 数据库初始化成功${NC}"
        else
            echo -e "${RED}✗ 数据库初始化失败${NC}"
            echo -e "${RED}${INIT_RESPONSE}${NC}"
        fi
    else
        echo -e "${GREEN}✓ 数据库已初始化${NC}"
    fi

    # 完成
    echo -e "\n${GREEN}========================================${NC}"
    echo -e "${GREEN}所有服务已启动完成！${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}后端服务: http://localhost:${BACKEND_PORT}${NC}"
    echo -e "${GREEN}前端服务: http://localhost:${FRONTEND_PORT}${NC}"
    echo -e "${GREEN}Swagger API 文档: http://localhost:${BACKEND_PORT}/swagger${NC}"

    # 显示开发环境主密码
    echo -e "\n${YELLOW}╔════════════════════════════════════════╗${NC}"
    echo -e "${YELLOW}║     开发环境测试主密码                 ║${NC}"
    echo -e "${YELLOW}╠════════════════════════════════════════╣${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║        主密码: ${GREEN}Test1234!${YELLOW}              ║${NC}"
    echo -e "${YELLOW}║                                        ║${NC}"
    echo -e "${YELLOW}║  首次初始化时请使用此密码进行测试      ║${NC}"
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
