# Quick Start: JWT身份认证系统

**Feature**: 007-accountbox-web-jwt
**Date**: 2025-10-17
**Audience**: 开发者快速上手指南

## Overview

本指南帮助开发者快速理解和实施JWT身份认证系统。涵盖关键概念、实施步骤和常见问题。

## Core Concepts

### 1. 认证流程

```
┌─────────────────┐         ┌──────────────────┐         ┌────────────────┐
│   前端登录页    │         │  AuthController  │         │  VaultManager  │
│                 │         │                  │         │                │
│  1. 输入主密码  │────────>│  2. 验证主密码   │────────>│  3. Unlock()   │
│                 │         │                  │<────────│    成功/失败   │
│                 │         │                  │         │                │
│                 │         │  4. 生成JWT      │         │                │
│                 │<────────│     Token        │         │                │
│  5. 存储Token   │         │                  │         │                │
│     到localStorage       │                  │         │                │
└─────────────────┘         └──────────────────┘         └────────────────┘
```

### 2. Token结构

JWT Token包含三部分(Base64编码):

```
<Header>.<Payload>.<Signature>
```

**Header**:
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (Claims)**:
```json
{
  "sub": "vault-user",           // 主体（固定值）
  "jti": "uuid",                 // Token唯一标识
  "iat": 1734434896,             // 签发时间
  "exp": 1734521296,             // 过期时间（24小时后）
  "iss": "AccountBox",           // 签发者
  "aud": "AccountBox-Web"        // 受众
}
```

**Signature**: HMACSHA256(base64(header) + "." + base64(payload), secret)

### 3. 双轨认证体系

```
┌─────────────────────────────────────────────────────┐
│              AccountBox API认证体系                  │
├─────────────────────────────────────────────────────┤
│                                                     │
│  /api/auth/*         无需认证（公开端点）           │
│                                                     │
│  /api/*              JWT Token认证（内部API）       │
│                      ├─ Middleware: JwtBearer       │
│                      └─ Header: Authorization       │
│                                                     │
│  /api/external/*     API Key认证（外部调用）        │
│                      ├─ Middleware: ApiKeyAuth      │
│                      └─ Header: X-API-Key           │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## Implementation Steps

### Phase 1: 后端基础设施

#### 1.1 添加NuGet包

```bash
cd backend/src/AccountBox.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
```

#### 1.2 配置JWT设置（appsettings.json）

```json
{
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-here-must-be-at-least-32-characters",
    "Issuer": "AccountBox",
    "Audience": "AccountBox-Web",
    "ExpirationHours": 24,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true
  },
  "LoginThrottle": {
    "MaxFailedAttempts": 5,
    "CooldownSeconds": 60,
    "WindowMinutes": 15
  }
}
```

**生成安全密钥**:
```bash
# 生成256位随机密钥（32字节）
openssl rand -base64 32
```

#### 1.3 创建JWT服务（JwtService.cs）

核心方法:
- `GenerateToken(string subject)`: 生成JWT Token
- `ValidateToken(string token)`: 验证Token有效性
- `GetClaimsFromToken(string token)`: 提取Claims

#### 1.4 创建认证控制器（AuthController.cs）

端点:
- `POST /api/auth/login`: 登录
- `POST /api/auth/logout`: 登出

#### 1.5 配置JWT认证中间件（Program.cs）

```csharp
// 1. 添加JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // 配置JWT验证参数
    });

// 2. 添加授权
builder.Services.AddAuthorization();

// 3. 应用中间件
app.UseAuthentication();
app.UseAuthorization();
```

#### 1.6 保护现有API端点

两种方式:
1. **全局策略**（推荐）: 所有Controller默认需要认证
2. **[Authorize]特性**: 在每个Controller/Action上添加

### Phase 2: 前端登录系统

#### 2.1 创建认证服务（authService.ts）

核心功能:
```typescript
export const authService = {
  // 登录
  login: async (masterPassword: string) => {
    const response = await axios.post('/api/auth/login', { masterPassword })
    const { token, expiresAt } = response.data.data
    localStorage.setItem('authToken', token)
    return { token, expiresAt }
  },

  // 登出
  logout: () => {
    localStorage.removeItem('authToken')
    window.location.href = '/login'
  },

  // 获取Token
  getToken: () => localStorage.getItem('authToken'),

  // 检查是否已认证
  isAuthenticated: () => !!authService.getToken()
}
```

#### 2.2 配置Axios拦截器（lib/axios.ts）

```typescript
// 请求拦截器：自动附加Token
axios.interceptors.request.use(config => {
  const token = authService.getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// 响应拦截器：处理401错误
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      authService.logout() // 自动跳转登录页
    }
    return Promise.reject(error)
  }
)
```

#### 2.3 创建登录页面（LoginPage.tsx）

使用shadcn/ui组件:
- `Card`: 登录卡片容器
- `Input`: 密码输入框
- `Button`: 登录按钮
- `Label`: 标签

#### 2.4 实现路由保护（ProtectedRoute.tsx）

```typescript
export const ProtectedRoute = ({ children }: { children: ReactNode }) => {
  const isAuth = authService.isAuthenticated()

  if (!isAuth) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}
```

#### 2.5 更新App路由（App.tsx）

```typescript
<Routes>
  <Route path="/login" element={<LoginPage />} />
  <Route path="/*" element={
    <ProtectedRoute>
      <MainLayout />
    </ProtectedRoute>
  } />
</Routes>
```

### Phase 3: 登录失败限制

#### 3.1 创建LoginAttempt实体

```csharp
public class LoginAttempt
{
    public long Id { get; set; }
    public string IPAddress { get; set; }
    public DateTime AttemptTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? UserAgent { get; set; }
}
```

#### 3.2 添加到DbContext

```csharp
public DbSet<LoginAttempt> LoginAttempts { get; set; }
```

#### 3.3 创建并运行迁移

```bash
cd backend/src/AccountBox.Api
dotnet ef migrations add AddLoginAttempts
dotnet ef database update
```

#### 3.4 实现登录限制中间件

检查逻辑:
1. 从IMemoryCache获取失败次数
2. 如果 >= 5次，检查是否在冷却期
3. 冷却期内拒绝请求（返回429）
4. 登录失败时更新缓存和数据库

## Testing

### 后端测试

#### 单元测试示例（JwtServiceTests.cs）

```csharp
[Fact]
public void GenerateToken_ShouldReturnValidToken()
{
    // Arrange
    var jwtService = new JwtService(/* ... */);

    // Act
    var token = jwtService.GenerateToken("vault-user");

    // Assert
    Assert.NotNull(token);
    Assert.True(jwtService.ValidateToken(token));
}

[Fact]
public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
{
    // 测试Token过期逻辑
}
```

#### 集成测试示例（AuthControllerTests.cs）

```csharp
[Fact]
public async Task Login_WithCorrectPassword_ShouldReturnToken()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new { masterPassword = "correct" };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", request);

    // Assert
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    Assert.NotNull(result.Data.Token);
}
```

### 前端测试

#### 组件测试示例（LoginPage.test.tsx）

```typescript
test('successful login redirects to home', async () => {
  render(<LoginPage />)

  const passwordInput = screen.getByLabelText('主密码')
  const loginButton = screen.getByRole('button', { name: '登录' })

  await userEvent.type(passwordInput, 'correct-password')
  await userEvent.click(loginButton)

  await waitFor(() => {
    expect(window.location.pathname).toBe('/')
  })
})
```

#### 服务测试示例（authService.test.ts）

```typescript
test('authService.login stores token in localStorage', async () => {
  const mockToken = 'mock-jwt-token'
  axios.post.mockResolvedValue({
    data: { data: { token: mockToken, expiresAt: '2025-10-18T00:00:00Z' } }
  })

  await authService.login('password')

  expect(localStorage.getItem('authToken')).toBe(mockToken)
})
```

## Manual Testing

### 测试场景1：成功登录

```bash
# 1. 确保后端运行
cd backend/src/AccountBox.Api
dotnet run

# 2. 使用curl测试登录
curl -X POST http://localhost:5093/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"masterPassword":"your-master-password"}'

# 期望响应：
# {
#   "data": {
#     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#     "expiresAt": "2025-10-18T12:34:56Z"
#   },
#   "error": null
# }
```

### 测试场景2：密码错误

```bash
curl -X POST http://localhost:5093/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"masterPassword":"wrong-password"}'

# 期望响应：
# {
#   "data": null,
#   "error": {
#     "code": "PASSWORD_INCORRECT",
#     "message": "密码错误,请重试"
#   }
# }
```

### 测试场景3：登录失败限制

```bash
# 连续5次输入错误密码
for i in {1..5}; do
  curl -X POST http://localhost:5093/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"masterPassword":"wrong"}'
done

# 第6次请求应返回429
curl -X POST http://localhost:5093/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"masterPassword":"correct-password"}'

# 期望响应：
# {
#   "data": null,
#   "error": {
#     "code": "TOO_MANY_ATTEMPTS",
#     "message": "登录失败次数过多,请1分钟后再试",
#     "retryAfter": 60
#   }
# }
```

### 测试场景4：使用Token访问受保护API

```bash
# 1. 登录获取Token
TOKEN=$(curl -s -X POST http://localhost:5093/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"masterPassword":"your-password"}' \
  | jq -r '.data.token')

# 2. 使用Token访问API
curl http://localhost:5093/api/websites \
  -H "Authorization: Bearer $TOKEN"

# 期望：返回网站列表

# 3. 不带Token访问（应该失败）
curl http://localhost:5093/api/websites

# 期望：返回401 Unauthorized
```

## Common Issues

### Issue 1: Token验证失败

**症状**: 前端获得Token后,所有API请求仍返回401

**可能原因**:
1. JWT密钥配置错误（后端配置与生成Token时使用的密钥不一致）
2. Token过期时间配置错误
3. Issuer/Audience验证失败

**解决方案**:
```bash
# 1. 检查appsettings.json的JwtSettings
# 2. 确认Program.cs中JWT配置与JwtService一致
# 3. 使用jwt.io网站解码Token,检查Claims
```

### Issue 2: CORS错误

**症状**: 前端登录请求被浏览器阻止,控制台显示CORS错误

**解决方案**:
```csharp
// Program.cs确保CORS配置正确
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:5173") // 前端开发服务器地址
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 确保中间件顺序正确
app.UseRouting();
app.UseCors(); // 必须在UseRouting之后,UseEndpoints之前
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

### Issue 3: 登录后刷新页面,又跳转到登录页

**可能原因**: Token未正确存储在localStorage

**解决方案**:
```typescript
// 确认登录成功后保存Token
const { token } = await authService.login(password)
localStorage.setItem('authToken', token) // 确保这行执行

// 确认应用启动时检查Token
useEffect(() => {
  const token = authService.getToken()
  if (token) {
    setIsAuthenticated(true)
  }
}, [])
```

### Issue 4: 登录失败限制不生效

**可能原因**: IMemoryCache未正确注册或清理策略有问题

**解决方案**:
```csharp
// Program.cs确保注册MemoryCache
builder.Services.AddMemoryCache();

// 检查缓存键是否正确
var cacheKey = $"login_failures:{ipAddress}";
```

## Security Checklist

在部署到生产环境前,确保:

- [ ] JWT SecretKey至少256位,使用强随机生成
- [ ] SecretKey存储在环境变量中,不提交到版本控制
- [ ] 生产环境启用HTTPS（`app.UseHttpsRedirection()`）
- [ ] Token有效期不超过24小时
- [ ] 登录失败限制已启用
- [ ] 所有敏感API端点都添加了`[Authorize]`特性
- [ ] API Key认证（/api/external/*）不受影响
- [ ] CORS配置仅允许可信来源
- [ ] 生产数据库定期清理旧的LoginAttempts记录

## Performance Tips

### 后端优化

1. **使用IMemoryCache缓存登录失败记录**
   - 避免每次登录都查询数据库
   - 设置合理的缓存过期时间（15分钟）

2. **优化数据库查询**
   - LoginAttempts表添加索引（IPAddress, AttemptTime）
   - 定期清理超过30天的旧记录

3. **JWT验证开销**
   - JWT验证非常快（< 1ms）,无需额外缓存
   - 避免在Token Payload中包含大量数据

### 前端优化

1. **避免重复验证Token**
   - 使用React Context缓存认证状态
   - 仅在应用启动时检查一次localStorage

2. **延迟Token刷新**
   - 当前设计：24小时过期,不需要刷新
   - 如果未来实现刷新机制,在Token快过期时（如23小时）才刷新

## Next Steps

完成本feature后,建议:

1. **监控和告警**: 实施登录失败监控,异常时发送告警
2. **审计日志**: 可视化登录历史,分析登录模式
3. **多因素认证**: 考虑添加TOTP或WebAuthn支持
4. **会话管理**: 允许用户查看和撤销其他设备的登录

## Reference

- [JWT官方文档](https://jwt.io/introduction)
- [ASP.NET Core JWT认证](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt)
- [React Router认证示例](https://reactrouter.com/en/main/start/examples#auth)
- [OWASP认证最佳实践](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
