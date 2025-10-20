# Data Model: API密钥管理与外部API服务

**Feature**: 006-api-management | **Date**: 2025-10-16
**Source**: 从spec.md的Key Entities和FR需求提取

## 实体概览

本功能涉及以下实体变更：

| 实体 | 类型 | 说明 |
|------|------|------|
| ApiKey | 新增 | API密钥主实体 |
| ApiKeyWebsiteScope | 新增 | API密钥与网站的多对多关联（作用域） |
| Account | 扩展 | 添加Status和ExtendedData字段 |
| AccountStatus | 新增 | 账号状态枚举（Active/Disabled） |
| ApiKeyScopeType | 新增 | 作用域类型枚举（All/Specific） |

## 详细实体定义

### 1. ApiKey (新增)

**用途**: 存储用户创建的API密钥，用于外部系统调用API

**字段**:

| 字段名 | 类型 | 约束 | 说明 | 映射到FR |
|--------|------|------|------|----------|
| Id | int | PK, Auto-increment | 主键 | - |
| Name | string | NOT NULL, MaxLength(100) | 密钥名称（用户自定义） | FR-008 |
| KeyPlaintext | string | NOT NULL, MaxLength(50), Unique | 密钥明文（"sk_"前缀+32字符） | FR-001, FR-002 |
| KeyHash | string | NOT NULL, MaxLength(200) | 密钥BCrypt哈希值（用于验证） | FR-004, FR-006 |
| ScopeType | ApiKeyScopeType | NOT NULL, Default(All) | 作用域类型（All/Specific） | FR-005 |
| CreatedAt | DateTime | NOT NULL, Default(CURRENT_TIMESTAMP) | 创建时间 | FR-008 |
| LastUsedAt | DateTime? | NULL | 最后使用时间 | FR-008, FR-035 |
| VaultId | int | FK, NOT NULL | 所属保险库ID | - |

**索引**:
- Unique Index on `KeyPlaintext`（加速查询和去重）
- Index on `VaultId`（查询用户的所有密钥）

**关系**:
- `VaultId` → `Vaults.Id` (Many-to-One)
- `ApiKeyWebsiteScopes` (One-to-Many, 仅当ScopeType=Specific时有数据)

**验证规则** (Data Annotations):
```csharp
[Required(ErrorMessage = "密钥名称不能为空")]
[MaxLength(100, ErrorMessage = "密钥名称不能超过100字符")]
public string Name { get; set; }

[Required]
[MaxLength(50)]
[RegularExpression(@"^sk_[A-Za-z0-9]{32}$", ErrorMessage = "密钥格式无效")]
public string KeyPlaintext { get; set; }
```

**业务规则**:
- 创建时自动生成`KeyPlaintext`和`KeyHash`（FR-001）
- `LastUsedAt`在每次API调用时更新（FR-035）
- 删除密钥时级联删除`ApiKeyWebsiteScopes`记录（FR-007）

---

### 2. ApiKeyWebsiteScope (新增)

**用途**: 当ApiKey的ScopeType为Specific时，记录该密钥可访问的网站列表

**字段**:

| 字段名 | 类型 | 约束 | 说明 | 映射到FR |
|--------|------|------|------|----------|
| ApiKeyId | int | PK, FK | 关联的API密钥ID | FR-005 |
| WebsiteId | int | PK, FK | 允许访问的网站ID | FR-005 |

**组合主键**: (ApiKeyId, WebsiteId)

**索引**:
- Composite Index on `(ApiKeyId, WebsiteId)`（提高查询效率）
- Index on `WebsiteId`（反向查询：哪些密钥有权访问某网站）

**关系**:
- `ApiKeyId` → `ApiKeys.Id` (Many-to-One, 级联删除)
- `WebsiteId` → `Websites.Id` (Many-to-One, 限制删除)

**业务规则**:
- 仅当`ApiKey.ScopeType = Specific`时此表才有数据（FR-005）
- 当`ApiKey.ScopeType = All`时，此表为空，代码逻辑允许访问所有网站（FR-005）
- 网站被删除时，相关作用域记录需要被删除或标记为无效（待定）

---

### 3. Account (扩展现有实体)

**变更**: 添加Status和ExtendedData字段

**新增/修改字段**:

| 字段名 | 类型 | 约束 | 说明 | 映射到FR |
|--------|------|------|------|----------|
| Status | AccountStatus | NOT NULL, Default(Active) | 账号状态（Active/Disabled） | FR-009 |
| ExtendedData | string (JSON) | NOT NULL, Default("{}"), MaxLength(10240) | 扩展字段（JSON键值对，10KB限制） | FR-016, FR-019 |

**现有字段保持不变**:
- Id, Username, Password, WebsiteId, Tags, Notes, CreatedAt, UpdatedAt, IsDeleted, DeletedAt 等

**数据库列类型**:
- SQLite: `ExtendedData TEXT`
- PostgreSQL: `ExtendedData JSONB`

**索引**:
- Index on `Status`（按状态过滤账号）
- 可选：GIN Index on `ExtendedData`（PostgreSQL，支持JSON查询）

**验证规则**:
```csharp
[Required]
public AccountStatus Status { get; set; } = AccountStatus.Active;

[Required]
[MaxLength(10240, ErrorMessage = "扩展字段不能超过10KB")]
[JsonValidation] // 自定义验证器：确保是有效JSON
public string ExtendedData { get; set; } = "{}";
```

**业务规则**:
- 新账号默认`Status = Active`，`ExtendedData = "{}"`（FR-009, FR-016）
- 禁用操作仅改变`Status`字段，不影响其他字段（FR-010）
- 删除操作设置`IsDeleted = true`并保留`Status`（FR-014）
- 从回收站恢复时，`Status`恢复为删除前的值（FR-014）
- 扩展字段JSON格式验证在API层和数据库约束层都执行（FR-018）
- 扩展字段键值对数量无限制，但总大小≤10KB（FR-019）

**ExtendedData示例**:
```json
{
  "email": "user@example.com",
  "phone": "+86 138****1234",
  "securityQuestion": "母亲的姓名",
  "securityAnswer": "张三",
  "registrationDate": "2023-01-15",
  "customField1": "任意值"
}
```

---

### 4. AccountStatus (新增枚举)

**用途**: 定义账号的启用/禁用状态

**枚举值**:

| 名称 | 值 | 说明 | 映射到FR |
|------|---|------|----------|
| Active | 0 | 活跃状态（可正常使用） | FR-009 |
| Disabled | 1 | 已禁用状态（暂停使用，但未删除） | FR-009 |

**说明**:
- 与`IsDeleted`字段正交：账号可以是"Active + IsDeleted"（回收站中的活跃账号）或"Disabled + IsDeleted"（回收站中的禁用账号）
- 默认值为`Active`（FR-009）

---

### 5. ApiKeyScopeType (新增枚举)

**用途**: 定义API密钥的作用域类型

**枚举值**:

| 名称 | 值 | 说明 | 映射到FR |
|------|---|------|----------|
| All | 0 | 访问所有网站 | FR-005 |
| Specific | 1 | 仅访问指定网站（需在ApiKeyWebsiteScopes表中定义） | FR-005 |

---

## ER图（实体关系）

```
Vault (1) ───< (N) ApiKey
                     │
                     │ ScopeType = Specific
                     │
                     └──< (N) ApiKeyWebsiteScope (N) >─── Website (1)

Website (1) ───< (N) Account
                     │
                     ├─ Status: AccountStatus (Active/Disabled)
                     └─ ExtendedData: JSON (键值对)
```

**关键关系说明**:
1. 每个Vault可以有多个ApiKey
2. 每个ApiKey可以关联多个Website（通过ApiKeyWebsiteScope），但仅当ScopeType=Specific时
3. 每个Website有多个Account
4. Account的Status和IsDeleted是独立维度

---

## 数据迁移策略

### 迁移1: 添加ApiKey和ApiKeyWebsiteScope表

```csharp
migrationBuilder.CreateTable(
    name: "ApiKeys",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
        Name = table.Column<string>(maxLength: 100, nullable: false),
        KeyPlaintext = table.Column<string>(maxLength: 50, nullable: false),
        KeyHash = table.Column<string>(maxLength: 200, nullable: false),
        ScopeType = table.Column<int>(nullable: false, defaultValue: 0),
        CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
        LastUsedAt = table.Column<DateTime>(nullable: true),
        VaultId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_ApiKeys", x => x.Id);
        table.ForeignKey(
            name: "FK_ApiKeys_Vaults_VaultId",
            column: x => x.VaultId,
            principalTable: "Vaults",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.UniqueConstraint("UQ_ApiKeys_KeyPlaintext", x => x.KeyPlaintext);
    });

migrationBuilder.CreateTable(
    name: "ApiKeyWebsiteScopes",
    columns: table => new
    {
        ApiKeyId = table.Column<int>(nullable: false),
        WebsiteId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_ApiKeyWebsiteScopes", x => new { x.ApiKeyId, x.WebsiteId });
        table.ForeignKey(
            name: "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId",
            column: x => x.ApiKeyId,
            principalTable: "ApiKeys",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_ApiKeyWebsiteScopes_Websites_WebsiteId",
            column: x => x.WebsiteId,
            principalTable: "Websites",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    });

migrationBuilder.CreateIndex(
    name: "IX_ApiKeys_VaultId",
    table: "ApiKeys",
    column: "VaultId");

migrationBuilder.CreateIndex(
    name: "IX_ApiKeyWebsiteScopes_WebsiteId",
    table: "ApiKeyWebsiteScopes",
    column: "WebsiteId");
```

### 迁移2: 扩展Account表

```csharp
migrationBuilder.AddColumn<int>(
    name: "Status",
    table: "Accounts",
    nullable: false,
    defaultValue: 0); // AccountStatus.Active

migrationBuilder.AddColumn<string>(
    name: "ExtendedData",
    table: "Accounts",
    type: "TEXT", // SQLite用TEXT，PostgreSQL用jsonb
    maxLength: 10240,
    nullable: false,
    defaultValue: "{}");

migrationBuilder.CreateIndex(
    name: "IX_Accounts_Status",
    table: "Accounts",
    column: "Status");
```

**回滚策略**:
- 迁移1回滚：删除ApiKeys和ApiKeyWebsiteScopes表
- 迁移2回滚：删除Accounts表的Status和ExtendedData列
- **数据安全**: 现有Account数据不受影响，新列有默认值

---

## 查询模式

### 查询1: 验证API密钥并获取作用域

```csharp
// 输入：提供的API密钥明文
// 输出：ApiKey实体（包含作用域信息）

var apiKey = await _context.ApiKeys
    .Include(k => k.ApiKeyWebsiteScopes) // 预加载作用域
    .FirstOrDefaultAsync(k => k.KeyPlaintext == providedKey);

if (apiKey != null && BCrypt.Verify(providedKey, apiKey.KeyHash))
{
    // 更新最后使用时间
    apiKey.LastUsedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return apiKey;
}
return null;
```

### 查询2: 检查API密钥是否有权访问某网站

```csharp
public bool CanAccessWebsite(ApiKey apiKey, int websiteId)
{
    if (apiKey.ScopeType == ApiKeyScopeType.All)
        return true;

    return apiKey.ApiKeyWebsiteScopes.Any(s => s.WebsiteId == websiteId);
}
```

### 查询3: 获取网站的活跃账号列表（按状态过滤）

```csharp
// 输入：websiteId, status (可选)
// 输出：Account列表

var query = _context.Accounts
    .Where(a => a.WebsiteId == websiteId && !a.IsDeleted);

if (status.HasValue)
    query = query.Where(a => a.Status == status.Value);

var accounts = await query.OrderBy(a => a.Username).ToListAsync();
```

### 查询4: 随机获取启用账号

```csharp
var account = await _context.Accounts
    .Where(a => a.WebsiteId == websiteId
             && a.Status == AccountStatus.Active
             && !a.IsDeleted)
    .OrderBy(a => EF.Functions.Random())
    .FirstOrDefaultAsync();
```

---

## 性能考虑

| 场景 | 预估QPS | 优化策略 |
|------|---------|----------|
| API密钥验证 | 100+ | 索引KeyPlaintext；考虑Redis缓存密钥哈希 |
| 作用域检查 | 100+ | 预加载ApiKeyWebsiteScopes（Eager Loading） |
| 账号列表查询 | 50+ | 索引Status字段；分页查询 |
| 随机获取账号 | 10+ | 小数据集（<1万）直接ORDER BY RANDOM()；大数据集考虑缓存 |
| 扩展字段读写 | 50+ | PostgreSQL用JSONB+GIN索引；SQLite用TEXT |

---

## 安全考虑

1. **API密钥明文存储**:
   - 风险：数据库泄露时密钥明文暴露
   - 缓解：BCrypt哈希提供二次防护；生产环境数据库加密

2. **扩展字段注入**:
   - 风险：用户输入恶意JSON
   - 缓解：API层验证JSON格式；大小限制10KB；不解释执行JSON内容

3. **SQL注入**:
   - 风险：无（使用EF Core LINQ，参数化查询）

---

## 下一步：API契约

基于此数据模型，将生成OpenAPI规范定义以下端点：
- `POST /api/api-keys` - 创建API密钥
- `GET /api/api-keys` - 获取密钥列表
- `DELETE /api/api-keys/{id}` - 删除密钥
- `POST /api/external/accounts` - 创建账号（需API密钥）
- `PUT /api/external/accounts/{id}/status` - 启用/禁用账号
- `GET /api/external/websites/{id}/accounts/random` - 随机获取启用账号
