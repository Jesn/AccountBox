#!/bin/bash

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}清理 .NET 构建进程${NC}"
echo -e "${YELLOW}========================================${NC}"

# 清理 dotnet build 进程
echo -e "${YELLOW}清理 dotnet build 进程...${NC}"
pkill -9 -f "dotnet build" 2>/dev/null || true

# 清理 dotnet clean 进程
echo -e "${YELLOW}清理 dotnet clean 进程...${NC}"
pkill -9 -f "dotnet clean" 2>/dev/null || true

# 清理 dotnet run 进程
echo -e "${YELLOW}清理 dotnet run 进程...${NC}"
pkill -9 -f "dotnet run" 2>/dev/null || true

# 清理 MSBuild 节点进程
echo -e "${YELLOW}清理 MSBuild 节点进程...${NC}"
pkill -9 -f "MSBuild.dll.*nodemode:1" 2>/dev/null || true

# 清理编译器进程
echo -e "${YELLOW}清理编译器进程...${NC}"
pkill -9 -f "VBCSCompiler.dll" 2>/dev/null || true

# 等待进程清理完成
sleep 2

# 验证清理结果
echo -e "\n${GREEN}========================================${NC}"
echo -e "${GREEN}清理完成！${NC}"
echo -e "${GREEN}========================================${NC}"

# 显示剩余进程数量
MSBUILD_COUNT=$(ps aux | grep "MSBuild.dll" | grep -v grep | wc -l | xargs)
BUILD_COUNT=$(ps aux | grep "dotnet build" | grep -v grep | wc -l | xargs)

echo -e "${GREEN}剩余 MSBuild 进程: ${MSBUILD_COUNT}${NC}"
echo -e "${GREEN}剩余 dotnet build 进程: ${BUILD_COUNT}${NC}"

if [ "$MSBUILD_COUNT" -eq "0" ] && [ "$BUILD_COUNT" -eq "0" ]; then
    echo -e "\n${GREEN}✓ 所有构建进程已清理完成！${NC}"
else
    echo -e "\n${YELLOW}! 仍有部分进程在运行（可能是 IDE 相关进程）${NC}"
fi
