#!/bin/bash

# AccountBox Docker 构建脚本

set -e

BUILD_TYPE=${1:-single}

echo "======================================"
echo "AccountBox Docker 构建"
echo "版本: $VERSION"
echo "构建类型: $BUILD_TYPE"
echo "======================================"

case $BUILD_TYPE in
  single)
    echo "构建单镜像部署..."
    docker compose -f docker-compose.yml build
    echo ""
    echo "✅ 单镜像构建完成!"
    echo ""
    echo "启动服务:"
    echo "  docker compose -f docker-compose.yml up -d"
    echo ""
    echo "访问应用:"
    echo "  http://localhost:5093"
    ;;

  separated)
    echo "构建分离镜像部署..."
    docker compose build
    echo ""
    echo "✅ 分离镜像构建完成!"
    echo ""
    echo "启动服务:"
    echo "  docker-compose up -d"
    echo ""
    echo "访问应用:"
    echo "  前端: http://localhost:8080"
    echo "  后端: http://localhost:5093"
    ;;

  all)
    echo "构建所有镜像..."
    echo ""
    echo "1/2 构建单镜像..."
    docker compose -f docker-compose.yml build
    echo ""
    echo "2/2 构建分离镜像..."
    docker-compose build
    echo ""
    echo "✅ 所有镜像构建完成!"
    ;;

  *)
    echo "错误: 未知的构建类型 '$BUILD_TYPE'"
    echo ""
    echo "用法: $0 [single|separated|all]"
    echo ""
    echo "  single    - 构建单镜像部署 (默认)"
    echo "  separated - 构建分离镜像部署"
    echo "  all       - 构建所有类型"
    echo ""
    echo "示例:"
    echo "  $0 single"
    exit 1
    ;;
esac

echo ""
echo "查看镜像大小:"
docker images | grep -E "REPOSITORY|accountbox"
