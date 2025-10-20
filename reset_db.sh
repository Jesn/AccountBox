#!/bin/bash

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}重置数据库脚本${NC}"
echo -e "${YELLOW}========================================${NC}"

# 数据库路径
DB_PATH="backend/src/AccountBox.Api/accountbox.db"
BACKEND_PORT=5093

# 检查是否在项目根目录
if [ ! -d "backend/src/AccountBox.Api" ]; then
    echo -e "${RED}错误: 请在项目根目录运行此脚本${NC}"
    exit 1
fi

# 检查数据库是否存在
if [ -f "$DB_PATH" ]; then
    echo -e "${YELLOW}发现已存在的数据库${NC}"
    echo -e "${YELLOW}是否要删除并重新初始化？ (y/N)${NC}"
    read -n 1 confirm
    echo ""

    if [[ ! $confirm =~ ^[Yy]$ ]]; then
        echo -e "${YELLOW}操作已取消${NC}"
        exit 0
    fi

    # 停止后端服务
    echo -e "${YELLOW}停止后端服务...${NC}"
    lsof -ti:$BACKEND_PORT | xargs kill -9 2>/dev/null || true
    sleep 2

    # 删除数据库
    echo -e "${YELLOW}删除数据库文件...${NC}"
    rm -f ${DB_PATH}*
    echo -e "${GREEN}✓ 数据库已删除${NC}"
else
    echo -e "${YELLOW}未发现数据库文件，将在首次启动时自动初始化${NC}"
fi

echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}重置完成！${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "${YELLOW}现在可以运行 ./start.sh 启动服务${NC}"
echo -e "${YELLOW}系统将自动使用密码 Test1234! 初始化${NC}"
