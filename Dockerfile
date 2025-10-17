# ============================================
# Stage 1: Build Frontend
# ============================================
FROM node:20-alpine AS frontend-builder

ARG VERSION=1.0.0
WORKDIR /build

# 安装 pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# 复制前端依赖文件
COPY frontend/package.json frontend/pnpm-lock.yaml ./

# 安装依赖
RUN pnpm install --frozen-lockfile

# 复制前端源代码
COPY frontend/ .

# 构建前端应用
RUN VITE_VERSION=${VERSION} pnpm build

# ============================================
# Stage 2: Build Backend
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-builder

WORKDIR /src

# 复制后端项目文件并还原依赖
COPY backend/src/AccountBox.Api/AccountBox.Api.csproj AccountBox.Api/
COPY backend/src/AccountBox.Core/AccountBox.Core.csproj AccountBox.Core/
COPY backend/src/AccountBox.Data/AccountBox.Data.csproj AccountBox.Data/
COPY backend/src/AccountBox.Security/AccountBox.Security.csproj AccountBox.Security/

# 还原所有项目的依赖（按依赖顺序）
RUN dotnet restore "AccountBox.Core/AccountBox.Core.csproj" && \
    dotnet restore "AccountBox.Data/AccountBox.Data.csproj" && \
    dotnet restore "AccountBox.Security/AccountBox.Security.csproj" && \
    dotnet restore "AccountBox.Api/AccountBox.Api.csproj"

# 复制后端源代码
COPY backend/src/ .

# 构建和发布后端应用
WORKDIR /src/AccountBox.Api
RUN dotnet publish "AccountBox.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Stage 3: Final Runtime Image
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

ARG VERSION=1.0.0
ENV APP_VERSION=${VERSION}

WORKDIR /app

# 安装必要的工具
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

# 创建非root用户
RUN adduser --disabled-password --gecos '' --uid 1000 appuser && \
    chown -R appuser:appuser /app

# 从后端构建阶段复制发布文件
COPY --from=backend-builder --chown=appuser:appuser /app/publish .

# 从前端构建阶段复制构建产物到后端的 wwwroot 目录
COPY --from=frontend-builder --chown=appuser:appuser /build/dist ./wwwroot

# 创建数据目录
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

# 切换到非root用户
USER appuser

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5093 \
    ASPNETCORE_ENVIRONMENT=Production \
    DATABASE_PATH=/app/data/accountbox.db

# 暴露端口 (5093 for API, frontend will be served from same port via wwwroot)
EXPOSE 5093

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5093/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "AccountBox.Api.dll"]

# ============================================
# Cleanup: Remove duplicate directories created during build
# ============================================
# Note: This is a build-time cleanup script that runs on the host machine
# after the Docker image is built. It removes the duplicate backend/backend/
# and backend/frontend/ directories that may be created during the build process.
# These directories are added to .gitignore to prevent them from being committed.
