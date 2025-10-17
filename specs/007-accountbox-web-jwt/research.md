# Research: JWT身份认证系统

**Feature**: 007-accountbox-web-jwt
**Date**: 2025-10-17
**Researcher**: SpecKit Agent

## Research Tasks

### 1. JWT最佳实践（ASP.NET Core 9.0）

**Decision**: 使用`Microsoft.AspNetCore.Authentication.JwtBearer`包实现JWT认证

**Rationale**:
- Microsoft官方推荐的JWT认证中间件
- 与ASP.NET Core深度集成,支持依赖注入和配置系统
- 支持对称加密（HMAC）和非对称加密（RSA）
- 内置Token验证、过期检查、签名验证功能
- 性能优化,支持高并发场景

**Considered Alternatives**:
- **手动实现JWT解析**：灵活性高,但需要自己处理Token验证、过期检查、签名验证等逻辑,容易出现安全漏洞
- **第三方JWT库（如IdentityServer）**：功能过于复杂,不适合单用户场景
- **选择原因**：Microsoft官方库提供了安全性和易用性的最佳平衡

**Implementation Details**:
- 使用HMAC-SHA256对称加密算法（HS256）
- JWT密钥长度至少256位（32字节）
- Token Payload包含：`sub`（主体）、`iat`（签发时间）、`exp`（过期时间）、`jti`（唯一标识）
- 配置ValidateIssuer、ValidateAudience、ValidateLifetime、ValidateIssuerSigningKey

### 2. 与VaultManager集成策略

**Decision**: 复用`IVaultManager.Unlock`方法验证主密码

**Rationale**:
- VaultManager已经实现了完整的主密码验证逻辑（Argon2id + AES-GCM）
- 避免重复实现密码验证逻辑
- 保持密码验证的一致性（前端登录和Vault解锁使用相同的密码）
- 不需要在数据库中额外存储用户凭证

**Considered Alternatives**:
- **单独的用户表和密码哈希**：增加复杂度,与现有Vault系统脱节
- **JWT直接存储主密码哈希**：安全风险高,Token泄露会暴露密码信息
- **选择原因**：复用现有逻辑最简洁且安全

**Implementation Details**:
- AuthController调用IVaultManager.Unlock验证主密码
- 验证成功后生成JWT Token,不存储任何密码信息
- Vault的加密密钥在登录后保持在内存中（由VaultManager管理）
- Token不包含密码或Vault密钥信息,仅包含用户标识

### 3. 登录失败限制策略

**Decision**: 使用内存缓存（IMemoryCache）+ 数据库记录实现登录失败限制

**Rationale**:
- **内存缓存**：快速检查登录失败次数,避免频繁数据库查询
- **数据库记录**：持久化失败记录,应用重启后仍然生效
- **双层设计**：平衡性能和持久性
- 失败记录包含IP地址、时间戳、失败原因

**Considered Alternatives**:
- **仅使用内存缓存**：应用重启后失败计数清零,可能被利用
- **仅使用数据库**：每次登录都查询数据库,性能开销大
- **Redis缓存**：过度设计,单用户系统不需要分布式缓存
- **选择原因**：双层设计在性能和安全性之间取得平衡

**Implementation Details**:
- 登录失败时,同时更新MemoryCache和数据库
- MemoryCache存储：Key=IP地址, Value=失败次数和最后失败时间
- 数据库实体：LoginAttempt（Id、IPAddress、AttemptTime、IsSuccessful、FailureReason）
- 冷却期1分钟（60秒）,5次失败后触发
- 冷却期内所有登录请求直接拒绝,返回429状态码

### 4. React路由保护最佳实践

**Decision**: 使用React Context + ProtectedRoute组件实现路由保护

**Rationale**:
- **React Context**：全局认证状态管理,避免prop drilling
- **ProtectedRoute组件**：封装路由守卫逻辑,可复用
- **react-router-dom v7**：使用最新的loader和action API
- **localStorage持久化**：Token存储在localStorage,页面刷新后仍然有效

**Considered Alternatives**:
- **Redux/Zustand状态管理**：过度设计,认证状态足够简单
- **sessionStorage**：关闭浏览器后Token清除,不符合"记住我"需求
- **Cookie存储**：需要处理CSRF,且不支持跨域（如果前后端分离部署）
- **选择原因**：Context API足够简单且原生支持

**Implementation Details**:
- AuthContext提供：`isAuthenticated`、`token`、`login()`、`logout()`
- ProtectedRoute检查`isAuthenticated`,未登录跳转到`/login`
- 记录原始URL,登录成功后跳转回去
- Token在localStorage中的键名：`authToken`
- Axios拦截器自动附加Token到所有请求

### 5. JWT Token刷新策略（Out of Scope）

**Decision**: 本次不实现Token刷新机制

**Rationale**:
- 24小时有效期对个人使用场景足够长
- 避免引入Refresh Token的复杂性（存储、轮换、撤销等）
- 简化前端逻辑,减少安全攻击面
- Token过期后重新登录,用户体验可接受（个人使用场景下24小时内很少重新登录）

**Future Consideration**:
- 如果用户反馈24小时过期太频繁,可以考虑延长有效期或实现Refresh Token
- Refresh Token需要额外的数据库表、撤销机制、Token轮换策略等

### 6. HTTPS和安全传输

**Decision**: 开发环境允许HTTP,生产环境强制HTTPS

**Rationale**:
- JWT Token通过Authorization header传输,HTTP下可能被中间人攻击
- 开发环境（localhost）使用HTTP简化配置
- 生产环境必须配置HTTPS证书（Let's Encrypt或自签名）
- ASP.NET Core可以强制HTTPS重定向

**Implementation Details**:
- Program.cs添加`app.UseHttpsRedirection()`（仅生产环境）
- 前端axios配置不验证SSL证书（仅开发环境）
- 生产环境检查：如果未使用HTTPS,拒绝启动或显示警告

### 7. JWT Token结构设计

**Decision**: 使用最小化Claims集合

**Claims**:
```json
{
  "sub": "vault-user",           // 固定值,单用户系统
  "jti": "uuid",                 // Token唯一标识
  "iat": 1234567890,             // 签发时间（Unix timestamp）
  "exp": 1234654290,             // 过期时间（iat + 24h）
  "iss": "AccountBox",           // 签发者
  "aud": "AccountBox-Web"        // 受众
}
```

**Rationale**:
- 最小化Token大小,减少网络传输开销
- 单用户系统不需要复杂的权限或角色信息
- `jti`用于追踪和潜在的Token撤销（未来功能）
- `iss`和`aud`用于验证Token来源

**Not Included**:
- 用户名、邮箱等敏感信息（单用户系统不需要）
- 权限、角色（所有登录用户拥有完全访问权限）
- 刷新Token相关字段

### 8. 错误处理和用户体验

**Decision**: 区分不同的认证错误类型,提供明确的提示

**Error Types**:
1. **密码错误**：返回401 + "密码错误,请重试"
2. **Token过期**：返回401 + "会话已过期,请重新登录"
3. **Token无效**：返回401 + "认证无效,请重新登录"
4. **登录冷却期**：返回429 + "登录失败次数过多,请1分钟后再试"
5. **网络错误**：返回500 + "网络连接失败,请检查网络"

**Implementation Details**:
- 后端返回标准化的错误响应：`{ "error": { "code": "PASSWORD_INCORRECT", "message": "密码错误" } }`
- 前端Axios拦截器解析错误代码,显示友好提示
- 使用Toast组件显示错误消息（shadcn/ui的Sonner）

## Research Summary

所有技术决策已明确,无需进一步澄清。核心架构：
- 后端：ASP.NET Core JWT Bearer认证 + IVaultManager密码验证
- 前端：React Context + ProtectedRoute + localStorage
- 安全：登录失败限制 + HTTPS（生产）+ 最小化Claims

**Status**: ✅ Ready for Phase 1 Design
