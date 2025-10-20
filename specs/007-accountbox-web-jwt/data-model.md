# Data Model: JWT身份认证系统

**Feature**: 007-accountbox-web-jwt
**Date**: 2025-10-17

## Overview

JWT认证系统的数据模型主要包含登录失败记录实体和JWT配置模型。由于是单用户系统,不需要单独的用户表,主密码验证复用现有的VaultManager逻辑。

## Entities

### 1. LoginAttempt（登录尝试记录）

**Purpose**: 记录所有登录尝试（成功和失败）,用于实施登录失败限制和审计。

**Fields**:
- `Id` (long): 主键,自增ID
- `IPAddress` (string, 45 chars): 客户端IP地址（支持IPv6）
- `AttemptTime` (DateTime): 登录尝试时间（UTC）
- `IsSuccessful` (bool): 是否成功
- `FailureReason` (string, nullable, 200 chars): 失败原因（如"密码错误"、"冷却期限制"）
- `UserAgent` (string, nullable, 500 chars): 浏览器User-Agent（用于分析）

**Relationships**:
- 无外键关联（单用户系统,无需关联到用户表）

**Validation Rules**:
- IPAddress必填,不能为空
- AttemptTime必填,默认为当前UTC时间
- FailureReason在IsSuccessful=false时应该有值,否则可以为空

**State Transitions**:
- 无状态转换（记录是不可变的,仅追加）

**Indexes**:
- `IX_LoginAttempts_IPAddress_AttemptTime`: 索引（IPAddress ASC, AttemptTime DESC）- 用于快速查询某IP最近的失败记录
- `IX_LoginAttempts_AttemptTime`: 索引（AttemptTime DESC）- 用于清理旧记录

### 2. JwtSettings（JWT配置模型）

**Purpose**: JWT认证配置,从appsettings.json加载。

**Fields**:
- `SecretKey` (string): JWT签名密钥（至少256位,base64编码）
- `Issuer` (string): Token签发者（如"AccountBox"）
- `Audience` (string): Token受众（如"AccountBox-Web"）
- `ExpirationHours` (int): Token有效期（小时数,默认24）
- `ValidateIssuer` (bool): 是否验证Issuer（默认true）
- `ValidateAudience` (bool): 是否验证Audience（默认true）
- `ValidateLifetime` (bool): 是否验证过期时间（默认true）
- `ValidateIssuerSigningKey` (bool): 是否验证签名密钥（默认true）

**Validation Rules**:
- SecretKey长度至少32字符（256位）
- ExpirationHours范围：1-168（1小时到7天）
- Issuer和Audience不能为空

**Not in Database**: 这是配置模型,存储在appsettings.json中,不需要数据库表。

## DTOs (Data Transfer Objects)

### 1. LoginRequest（登录请求）

**Purpose**: 客户端发送的登录请求。

**Fields**:
- `MasterPassword` (string, required): 主密码

**Validation**:
- MasterPassword: Required, MinLength(1), MaxLength(1000)

### 2. LoginResponse（登录响应）

**Purpose**: 服务器返回的登录成功响应。

**Fields**:
- `Token` (string): JWT Token字符串
- `ExpiresAt` (DateTime): Token过期时间（ISO 8601格式）

### 3. AuthError（认证错误）

**Purpose**: 标准化的认证错误响应。

**Fields**:
- `Code` (string): 错误代码（如"PASSWORD_INCORRECT", "TOKEN_EXPIRED"）
- `Message` (string): 用户友好的错误消息
- `RetryAfter` (int?, nullable): 冷却期剩余秒数（仅登录失败限制时）

## Frontend State Models

### 1. AuthState（认证状态）

**Purpose**: 前端React Context维护的全局认证状态。

**Fields**:
- `isAuthenticated` (boolean): 是否已认证
- `token` (string | null): JWT Token字符串
- `expiresAt` (Date | null): Token过期时间

**Methods**:
- `login(token: string, expiresAt: Date): void`: 登录成功,保存Token
- `logout(): void`: 登出,清除Token和状态
- `isTokenExpired(): boolean`: 检查Token是否过期

## Database Schema Changes

### New Table: `LoginAttempts`

```sql
CREATE TABLE LoginAttempts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    IPAddress TEXT NOT NULL,
    AttemptTime TEXT NOT NULL,  -- SQLite stores DateTime as TEXT (ISO 8601)
    IsSuccessful INTEGER NOT NULL,  -- SQLite boolean as INTEGER (0/1)
    FailureReason TEXT,
    UserAgent TEXT
);

CREATE INDEX IX_LoginAttempts_IPAddress_AttemptTime
    ON LoginAttempts(IPAddress, AttemptTime DESC);

CREATE INDEX IX_LoginAttempts_AttemptTime
    ON LoginAttempts(AttemptTime DESC);
```

### Existing Tables

无需修改现有表。Vault相关表（VaultKeys）不受影响。

## Configuration Schema

### appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "<base64-encoded-256-bit-key>",
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

## Data Flow

### 1. 登录流程

```
Client                  AuthController          VaultManager        Database
  |                           |                       |                 |
  |--POST /api/auth/login---->|                       |                 |
  |  {masterPassword}         |                       |                 |
  |                           |--Check Failed Count-->|                 |
  |                           |<--Count (from cache)--|                 |
  |                           |                       |                 |
  |                           |--Unlock(password)---->|                 |
  |                           |<--Success/Failure-----|                 |
  |                           |                       |                 |
  |                           |--Generate JWT Token-->|                 |
  |                           |                       |                 |
  |                           |--Record Attempt------>|                 |
  |                           |                       |--INSERT-------->|
  |<--{token, expiresAt}------|                       |                 |
```

### 2. 受保护API访问流程

```
Client                  Middleware              Controller
  |                           |                       |
  |--GET /api/websites------->|                       |
  |  Authorization: Bearer X  |                       |
  |                           |--Validate JWT-------->|
  |                           |<--Claims--------------|
  |                           |                       |
  |                           |--Authorize----------->|
  |                           |                       |--Process Request
  |<--Response----------------|<--Result--------------|
```

### 3. Token过期处理流程

```
Client                  Axios Interceptor       Server
  |                           |                       |
  |--API Request------------->|                       |
  |  (expired token)          |--Request------------->|
  |                           |<--401 Unauthorized----|
  |                           |                       |
  |<--Clear Token & Redirect--|                       |
  |  to /login                |                       |
```

## Memory Management

### IMemoryCache缓存

**Key**: `login_failures:{IPAddress}`
**Value**: `{ Count: int, LastFailureTime: DateTime }`
**Expiration**: 绝对过期时间15分钟（从最后一次失败算起）

**Purpose**: 快速检查登录失败次数,避免每次登录都查询数据库。

## Security Considerations

### 1. 敏感信息保护

- JWT Token中**不包含**主密码或Vault密钥
- Token仅包含最小化Claims（sub, jti, iat, exp, iss, aud）
- LoginAttempt表不记录密码明文或哈希

### 2. SQL注入防护

- 所有数据库操作使用EF Core LINQ,自动参数化查询
- 无原生SQL字符串拼接

### 3. 暴力破解防护

- 登录失败记录 + 内存缓存双层检查
- 5次失败后1分钟冷却期
- IP级别限制（如果在NAT后可能误伤,可以考虑未来改进）

## Future Enhancements

### 1. Token撤销机制

- 添加`RevokedTokens`表,记录被撤销的Token（jti）
- 在Token验证时检查是否在撤销列表中
- 支持用户主动撤销其他设备的登录

### 2. 会话管理

- 添加`Sessions`表,记录所有活动会话
- 包含设备信息、登录时间、最后活动时间
- 用户可以查看和管理所有活动会话

### 3. 审计日志

- 扩展LoginAttempt表,增加更多审计字段
- 记录登录来源（浏览器、IP、地理位置）
- 可视化登录历史

## Summary

- **1个新表**: LoginAttempts（登录尝试记录）
- **0个表修改**: 现有表不受影响
- **4个DTO**: LoginRequest, LoginResponse, AuthError, AuthState
- **2个配置模型**: JwtSettings, LoginThrottleSettings
- **简洁设计**: 单用户系统,无需复杂的用户-角色-权限模型
