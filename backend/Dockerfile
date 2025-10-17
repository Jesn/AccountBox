# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["src/AccountBox.Api/AccountBox.Api.csproj", "AccountBox.Api/"]
COPY ["src/AccountBox.Core/AccountBox.Core.csproj", "AccountBox.Core/"]
COPY ["src/AccountBox.Data/AccountBox.Data.csproj", "AccountBox.Data/"]
COPY ["src/AccountBox.Security/AccountBox.Security.csproj", "AccountBox.Security/"]

# 还原所有项目的依赖（按依赖顺序）
RUN dotnet restore "AccountBox.Core/AccountBox.Core.csproj" && \
    dotnet restore "AccountBox.Data/AccountBox.Data.csproj" && \
    dotnet restore "AccountBox.Security/AccountBox.Security.csproj" && \
    dotnet restore "AccountBox.Api/AccountBox.Api.csproj"

# 复制所有源代码
COPY src/ .

# 构建和发布应用
WORKDIR /src/AccountBox.Api
RUN dotnet publish "AccountBox.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# 创建非root用户
RUN adduser --disabled-password --gecos '' --uid 1000 appuser && \
    chown -R appuser:appuser /app

# 从构建阶段复制发布文件
COPY --from=build --chown=appuser:appuser /app/publish .

# 创建数据目录
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

# 切换到非root用户
USER appuser

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:5093 \
    ASPNETCORE_ENVIRONMENT=Production \
    DATABASE_PATH=/app/data/accountbox.db

# 暴露端口
EXPOSE 5093

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5093/health || exit 1

# 启动应用
ENTRYPOINT ["dotnet", "AccountBox.Api.dll"]
