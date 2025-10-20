# Data Model: 账号管理系统 MVP

**Feature**: 001-mvp | **Date**: 2025-10-14 | **Plan**: [plan.md](./plan.md)

## Overview

本文档定义账号管理系统的数据模型，包括实体、关系、字段约束和状态转换。所有敏感字段（如密码）在存储前通过AES-256-GCM加密。

---

## Entity Relationship Diagram

```
┌──────────────┐         ┌──────────────┐
│   Website    │1      ∞│   Account    │
│              ├─────────┤              │
│ Id           │         │ Id           │
│ Domain       │         │ WebsiteId FK │
│ DisplayName  │         │ Username     │
│ Tags         │         │ Password     │
│ CreatedAt    │         │ Notes        │
│ UpdatedAt    │         │ Tags         │
└──────────────┘         │ IsDeleted    │
                         │ DeletedAt    │
                         │ CreatedAt    │
                         │ UpdatedAt    │
                         └──────────────┘

┌──────────────┐
│   KeySlot    │
│              │
│ Id           │
│ EncryptedKey │  ← KEK加密的VaultKey
│ Salt         │  ← Argon2id盐值
│ Iterations   │
│ MemorySize   │
│ Parallelism  │
│ CreatedAt    │
│ UpdatedAt    │
└──────────────┘
```

**关系说明**：

- Website ↔ Account：一对多关系（一个网站可有多个账号）
- Account.WebsiteId：外键，级联删除（删除网站时物理删除所有关联账号）
- KeySlot：单例表（始终只有1条记录），存储加密的VaultKey

---

## Entity Definitions

### 1. Website（网站）

**用途**：代表一个在线服务或平台（如GitHub、Gmail）

| 字段名 | 类型 | 约束 | 说明 | 示例 |
|--------|------|------|------|------|
| Id | int | PK, Auto-increment | 主键 | 1 |
| Domain | string(255) | NOT NULL, Index | 网站域名 | github.com |
| DisplayName | string(100) | NOT NULL, Index | 显示名称 | GitHub |
| Tags | string(500) | NULL | 标签（逗号分隔） | 开发,代码托管 |
| CreatedAt | datetime | NOT NULL, Default=NOW() | 创建时间 | 2025-10-14 10:30:00 |
| UpdatedAt | datetime | NOT NULL, Default=NOW() | 更新时间 | 2025-10-14 15:45:00 |

**索引**：

- `IX_Website_DisplayName`：加速按名称搜索和排序
- `IX_Website_Domain`：加速按域名查找

**验证规则**：

- Domain：可选但建议填写，格式无严格限制（支持IP、localhost等）
- DisplayName：必填，1-100字符
- Tags：可选，多个标签用逗号分隔

**业务逻辑**：

- 允许创建多个相同Domain的网站（区分测试/生产环境）
- 删除前必须检查是否有活跃账号（`WHERE IsDeleted=false`）

---

### 2. Account（账号）

**用途**：存储用户在某个网站上的登录凭证

| 字段名 | 类型 | 约束 | 说明 | 示例 |
|--------|------|------|------|------|
| Id | int | PK, Auto-increment | 主键 | 1 |
| WebsiteId | int | FK, NOT NULL, Index | 所属网站ID | 1 |
| Username | string(255) | NOT NULL | 用户名（明文） | john.doe@example.com |
| PasswordEncrypted | byte[] | NOT NULL | 加密后的密码（AES-256-GCM） | [二进制数据] |
| PasswordIV | byte[] | NOT NULL | 密码加密IV（96 bits） | [随机生成] |
| PasswordTag | byte[] | NOT NULL | 密码认证标签（128 bits） | [自动生成] |
| Notes | string(1000) | NULL | 备注（加密存储） | 工作账号 |
| NotesEncrypted | byte[] | NULL | 加密后的备注 | [二进制数据] |
| NotesIV | byte[] | NULL | 备注加密IV | [随机生成] |
| NotesTag | byte[] | NULL | 备注认证标签 | [自动生成] |
| Tags | string(500) | NULL | 标签（明文，逗号分隔） | 主账号,重要 |
| IsDeleted | bool | NOT NULL, Default=false, Index | 软删除标记 | false |
| DeletedAt | datetime | NULL | 删除时间 | null |
| CreatedAt | datetime | NOT NULL, Default=NOW() | 创建时间 | 2025-10-14 11:00:00 |
| UpdatedAt | datetime | NOT NULL, Default=NOW() | 更新时间 | 2025-10-14 14:20:00 |

**索引**：

- `IX_Account_WebsiteId_IsDeleted`：加速按网站查询活跃账号
- `IX_Account_IsDeleted_CreatedAt`：加速回收站查询和排序
- `IX_Account_Username`：加速搜索

**验证规则**：

- Username：必填，1-255字符
- PasswordEncrypted：必填，存储前必须加密
- Notes：可选，最长1000字符
- Tags：可选，多个标签用逗号分隔

**加密说明**：

```
明文数据 → AES-256-GCM(VaultKey) → {EncryptedData, IV, Tag}
解密验证：Tag不匹配时抛出异常（数据被篡改）
```

**软删除逻辑**：

- 删除账号时：设置`IsDeleted=true`, `DeletedAt=NOW()`
- 恢复账号时：设置`IsDeleted=false`, `DeletedAt=null`
- 永久删除：物理删除记录（`DELETE FROM Accounts WHERE Id=?`）

**全文搜索映射**（FTS5虚拟表）：

```sql
CREATE VIRTUAL TABLE accounts_fts USING fts5(
    username,           -- Account.Username
    notes_decrypted,    -- 解密后的Notes（搜索时临时解密）
    tags,               -- Account.Tags
    website_name,       -- Website.DisplayName
    website_domain,     -- Website.Domain
    content='',         -- 不存储内容，仅索引
    content_rowid='id'
);
```

**注意**：全文搜索需要在查询时临时解密Notes字段，因此搜索性能会受加密解密影响（可接受，因为搜索频率较低且数据量中等）。

---

### 3. KeySlot（密钥槽）

**用途**：存储加密的VaultKey和Argon2id参数

| 字段名 | 类型 | 约束 | 说明 | 示例 |
|--------|------|------|------|------|
| Id | int | PK | 主键（始终为1） | 1 |
| EncryptedVaultKey | byte[] | NOT NULL | KEK加密的VaultKey（256 bits） | [二进制数据] |
| VaultKeyIV | byte[] | NOT NULL | VaultKey加密IV（96 bits） | [随机生成] |
| VaultKeyTag | byte[] | NOT NULL | VaultKey认证标签（128 bits） | [自动生成] |
| Argon2Salt | byte[] | NOT NULL | Argon2id盐值（128 bits） | [随机生成] |
| Argon2Iterations | int | NOT NULL | Argon2id迭代次数 | 3 |
| Argon2MemorySize | int | NOT NULL | Argon2id内存大小（KB） | 65536 (64MB) |
| Argon2Parallelism | int | NOT NULL | Argon2id并行度 | 4 |
| CreatedAt | datetime | NOT NULL | 初始化时间 | 2025-10-14 10:00:00 |
| UpdatedAt | datetime | NOT NULL | 最后更新时间 | 2025-10-15 09:30:00 |

**单例约束**：

- 表中始终只有1条记录（Id=1）
- 应用启动时检查：不存在则引导用户设置主密码
- 修改主密码时更新此记录（不创建新记录）

**加密流程**：

```
1. 首次初始化：
   用户主密码 → Argon2id(salt, iterations, memory, parallelism) → KEK
   生成随机VaultKey(256-bit) → AES-256-GCM(KEK) → EncryptedVaultKey

2. 解锁应用：
   用户主密码 → Argon2id(从KeySlot读取参数) → KEK
   KEK → AES-256-GCM.Decrypt(EncryptedVaultKey, IV, Tag) → VaultKey
   VaultKey加载到内存（应用运行期间常驻）

3. 修改主密码：
   新主密码 → Argon2id(新salt) → 新KEK
   新KEK → AES-256-GCM.Encrypt(VaultKey) → 更新EncryptedVaultKey
   （业务数据无需重新加密）
```

---

## State Transitions

### Account State Diagram

```
      ┌─────────┐
      │ [Active]│ ← 正常账号（IsDeleted=false）
      │         │
      └────┬────┘
           │ 软删除
           ↓
      ┌─────────┐
      │[Deleted]│ ← 回收站账号（IsDeleted=true）
      │         │
      └────┬────┘
           │ 恢复 ↓ 永久删除
           ↓      ↓
      ┌─────────┐  ┌─────────┐
      │ [Active]│  │ [Gone]  │ ← 物理删除（记录不存在）
      └─────────┘  └─────────┘
```

**状态转换规则**：

| 当前状态 | 操作 | 新状态 | 说明 |
|---------|------|--------|------|
| Active | 软删除 | Deleted | `IsDeleted=true, DeletedAt=NOW()` |
| Deleted | 恢复 | Active | `IsDeleted=false, DeletedAt=null` |
| Deleted | 永久删除 | Gone | `DELETE FROM Accounts WHERE Id=?` |
| Active | 永久删除 | ❌ 不允许 | 必须先软删除 |

---

## Data Validation

### Website Validation

```csharp
public class WebsiteValidator : AbstractValidator<Website>
{
    public WebsiteValidator()
    {
        RuleFor(w => w.DisplayName)
            .NotEmpty().WithMessage("显示名称不能为空")
            .MaximumLength(100).WithMessage("显示名称不能超过100字符");

        RuleFor(w => w.Domain)
            .MaximumLength(255).WithMessage("域名不能超过255字符");

        RuleFor(w => w.Tags)
            .MaximumLength(500).WithMessage("标签不能超过500字符");
    }
}
```

### Account Validation

```csharp
public class AccountValidator : AbstractValidator<Account>
{
    public AccountValidator()
    {
        RuleFor(a => a.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(255).WithMessage("用户名不能超过255字符");

        RuleFor(a => a.WebsiteId)
            .GreaterThan(0).WithMessage("必须关联到有效网站");

        RuleFor(a => a.Notes)
            .MaximumLength(1000).WithMessage("备注不能超过1000字符");

        RuleFor(a => a.Tags)
            .MaximumLength(500).WithMessage("标签不能超过500字符");
    }
}
```

---

## Database Schema (SQLite)

```sql
-- 网站表
CREATE TABLE Websites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Domain TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Tags TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IX_Website_DisplayName ON Websites(DisplayName);
CREATE INDEX IX_Website_Domain ON Websites(Domain);

-- 账号表
CREATE TABLE Accounts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    WebsiteId INTEGER NOT NULL,
    Username TEXT NOT NULL,
    PasswordEncrypted BLOB NOT NULL,
    PasswordIV BLOB NOT NULL,
    PasswordTag BLOB NOT NULL,
    Notes TEXT,
    NotesEncrypted BLOB,
    NotesIV BLOB,
    NotesTag BLOB,
    Tags TEXT,
    IsDeleted INTEGER NOT NULL DEFAULT 0,  -- SQLite使用INTEGER存储布尔值
    DeletedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (WebsiteId) REFERENCES Websites(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Account_WebsiteId_IsDeleted ON Accounts(WebsiteId, IsDeleted);
CREATE INDEX IX_Account_IsDeleted_CreatedAt ON Accounts(IsDeleted, CreatedAt DESC);
CREATE INDEX IX_Account_Username ON Accounts(Username);

-- 全文搜索虚拟表
CREATE VIRTUAL TABLE accounts_fts USING fts5(
    username,
    notes_decrypted,
    tags,
    website_name,
    website_domain,
    tokenize='unicode61'
);

-- 密钥槽表
CREATE TABLE KeySlots (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),  -- 强制单例
    EncryptedVaultKey BLOB NOT NULL,
    VaultKeyIV BLOB NOT NULL,
    VaultKeyTag BLOB NOT NULL,
    Argon2Salt BLOB NOT NULL,
    Argon2Iterations INTEGER NOT NULL,
    Argon2MemorySize INTEGER NOT NULL,
    Argon2Parallelism INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now'))
);

-- 触发器：自动更新UpdatedAt
CREATE TRIGGER update_website_timestamp
AFTER UPDATE ON Websites
BEGIN
    UPDATE Websites SET UpdatedAt = datetime('now') WHERE Id = NEW.Id;
END;

CREATE TRIGGER update_account_timestamp
AFTER UPDATE ON Accounts
BEGIN
    UPDATE Accounts SET UpdatedAt = datetime('now') WHERE Id = NEW.Id;
END;

CREATE TRIGGER update_keyslot_timestamp
AFTER UPDATE ON KeySlots
BEGIN
    UPDATE KeySlots SET UpdatedAt = datetime('now') WHERE Id = NEW.Id;
END;
```

---

## EF Core Entity Configuration

```csharp
// WebsiteEntityConfiguration.cs
public class WebsiteEntityConfiguration : IEntityTypeConfiguration<Website>
{
    public void Configure(EntityTypeBuilder<Website> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Domain).HasMaxLength(255);
        builder.Property(w => w.Tags).HasMaxLength(500);
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.HasIndex(w => w.DisplayName);
        builder.HasIndex(w => w.Domain);

        builder.HasMany(w => w.Accounts)
               .WithOne(a => a.Website)
               .HasForeignKey(a => a.WebsiteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

// AccountEntityConfiguration.cs
public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Username).IsRequired().HasMaxLength(255);
        builder.Property(a => a.PasswordEncrypted).IsRequired();
        builder.Property(a => a.PasswordIV).IsRequired();
        builder.Property(a => a.PasswordTag).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.Tags).HasMaxLength(500);
        builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => new { a.WebsiteId, a.IsDeleted });
        builder.HasIndex(a => new { a.IsDeleted, a.CreatedAt });
        builder.HasIndex(a => a.Username);

        // 全局查询过滤器：默认排除软删除记录
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}

// KeySlotEntityConfiguration.cs
public class KeySlotEntityConfiguration : IEntityTypeConfiguration<KeySlot>
{
    public void Configure(EntityTypeBuilder<KeySlot> builder)
    {
        builder.HasKey(k => k.Id);
        builder.Property(k => k.EncryptedVaultKey).IsRequired();
        builder.Property(k => k.VaultKeyIV).IsRequired();
        builder.Property(k => k.VaultKeyTag).IsRequired();
        builder.Property(k => k.Argon2Salt).IsRequired();
        builder.Property(k => k.Argon2Iterations).IsRequired();
        builder.Property(k => k.Argon2MemorySize).IsRequired();
        builder.Property(k => k.Argon2Parallelism).IsRequired();
        builder.Property(k => k.CreatedAt).IsRequired();
        builder.Property(k => k.UpdatedAt).IsRequired();

        // 单例约束：种子数据仅1条记录
        builder.HasData(new KeySlot
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
            // 其他字段在首次设置主密码时更新
        });
    }
}
```

---

## Migration Strategy

### 初始迁移（001_Initial）

```bash
dotnet ef migrations add Initial --project AccountBox.Data
dotnet ef database update --project AccountBox.Data
```

### 未来迁移示例

如需添加新字段（如Account.Email）：

```bash
dotnet ef migrations add AddEmailToAccount --project AccountBox.Data
dotnet ef database update --project AccountBox.Data
```

**迁移原则**：

- 禁止破坏性迁移（删除列、修改列类型）
- 新增可空字段或提供默认值
- 加密字段迁移需特殊处理（先解密、迁移、重新加密）

---

## Security Considerations

### 1. 敏感数据加密

**必须加密**：

- Account.Password（明文永不落盘）
- Account.Notes（可能包含敏感信息）

**可不加密**（性能考虑）：

- Account.Username（用于搜索，加密影响性能）
- Account.Tags（用于分类，非核心敏感信息）
- Website.DisplayName/Domain（公开信息）

### 2. 防注入

- 使用参数化查询（EF Core自动处理）
- 用户输入验证（FluentValidation）
- 避免动态SQL拼接

### 3. 防时序攻击

- 主密码验证使用恒定时间比较
- Tag验证使用AES-GCM内置的恒定时间比较

### 4. 数据完整性

- AES-GCM的Tag字段验证数据未被篡改
- 外键约束防止孤立记录
- 事务保证级联删除的原子性

---

## Performance Considerations

### 索引策略

- 经常查询的字段建立索引（DisplayName, Username, IsDeleted）
- 复合索引用于复杂查询（WebsiteId + IsDeleted）
- 避免过度索引（影响写入性能）

### 查询优化

```csharp
// ✅ 优化：使用AsNoTracking（只读查询）
var websites = await _context.Websites
    .AsNoTracking()
    .OrderBy(w => w.DisplayName)
    .ToListAsync();

// ✅ 优化：Select投影（减少数据传输）
var accountSummaries = await _context.Accounts
    .Where(a => !a.IsDeleted)
    .Select(a => new { a.Id, a.Username, a.CreatedAt })
    .ToListAsync();

// ❌ 避免：N+1查询
foreach (var website in websites)
{
    var accounts = await _context.Accounts
        .Where(a => a.WebsiteId == website.Id)
        .ToListAsync();  // 每次循环都查询数据库
}

// ✅ 优化：使用Include预加载
var websites = await _context.Websites
    .Include(w => w.Accounts.Where(a => !a.IsDeleted))
    .ToListAsync();
```

### 加密性能

- VaultKey常驻内存，避免重复派生（单次Argon2id ~500ms）
- 批量加密使用`Parallel.ForEach`（CPU密集型任务）
- FTS5全文搜索避免对每条记录实时解密（考虑异步更新索引）

---

## Summary

数据模型定义完成，包含3个核心实体：

1. **Website**：网站信息（明文存储）
2. **Account**：账号凭证（密码和备注加密存储，支持软删除）
3. **KeySlot**：加密密钥管理（单例，存储加密的VaultKey）

**下一步**：生成API契约（contracts/），定义各实体的RESTful端点。
