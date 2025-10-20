# ============================================
# AccountBox 生产环境 Dockerfile（Alpine 基础）
# ============================================

# ============================================
# 阶段 1: 构建前端
# ============================================
FROM node:20-alpine AS frontend-builder

WORKDIR /app/frontend

# 复制前端依赖文件
COPY frontend/package.json frontend/pnpm-lock.yaml ./

# 安装 pnpm 并安装依赖
RUN npm install -g pnpm@latest && \
    pnpm install --frozen-lockfile

# 复制前端源代码
COPY frontend/ ./

# 构建前端
RUN pnpm build

# ============================================
# 阶段 2: 构建后端
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS backend-builder

WORKDIR /app/backend

# 复制后端项目文件（包含迁移项目）
COPY backend/src/AccountBox.Core/*.csproj ./AccountBox.Core/
COPY backend/src/AccountBox.Security/*.csproj ./AccountBox.Security/
COPY backend/src/AccountBox.Data/*.csproj ./AccountBox.Data/
COPY backend/src/AccountBox.Api/*.csproj ./AccountBox.Api/
COPY backend/src/AccountBox.Data.Migrations.PostgreSQL/*.csproj ./AccountBox.Data.Migrations.PostgreSQL/
COPY backend/src/AccountBox.Data.Migrations.MySQL/*.csproj ./AccountBox.Data.Migrations.MySQL/
COPY backend/src/AccountBox.Data.Migrations.Sqlite/*.csproj ./AccountBox.Data.Migrations.Sqlite/

# 还原依赖
RUN dotnet restore ./AccountBox.Api/AccountBox.Api.csproj

# 复制所有源代码
COPY backend/src/ ./

# 构建并发布
RUN dotnet publish ./AccountBox.Api/AccountBox.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================
# 阶段 3: 运行时镜像
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# 安装 curl（用于健康检查）
RUN apk add --no-cache curl

WORKDIR /app

# 创建非 root 用户
RUN addgroup -g 1000 accountbox && \
    adduser -D -u 1000 -G accountbox accountbox

# 从构建阶段复制后端文件
COPY --chown=accountbox:accountbox --from=backend-builder /app/publish ./

# 从构建阶段复制前端文件到 wwwroot
COPY --chown=accountbox:accountbox --from=frontend-builder /app/frontend/dist ./wwwroot

# 为 SQLite 数据库准备数据目录（首次挂载命名卷时会复制权限）
RUN mkdir -p /app/data && chown -R accountbox:accountbox /app/data

# 切换到非 root 用户
USER accountbox

# 暴露端口
EXPOSE 5093

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5093/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "AccountBox.Api.dll"]
