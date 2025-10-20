# Research: API密钥管理与外部API服务

**Feature**: 006-api-management | **Date**: 2025-10-16
**Purpose**: 技术调研和决策记录

## 研究主题

基于技术上下文，本功能的关键技术决策如下：

### 1. API密钥生成与存储策略

**决策**: 使用"sk_"前缀 + 32位随机Base62字符串，同时存储明文和BCrypt哈希值

**理由**:
- **明文存储**: 个人密码管理工具场景下，用户随时查看密钥是合理需求，不同于企业SaaS的"仅显示一次"模式
- **双存储**: 明文用于UI显示和复制，哈希值用于API请求验证
- **"sk_"前缀**: 业界惯例（如OpenAI、Stripe），便于识别密钥类型
- **32位Base62**: 提供192位熵（log2(62^32)），远超128位安全标准
- **BCrypt哈希**: 工作因子12，防御暴力破解（即使数据库泄露）

**考虑的备选方案**:
- ~~仅存储哈希值~~: 无法满足"随时查看明文"需求
- ~~使用UUID~~: 熵足够但格式冗长（36字符），不符合API密钥习惯
- ~~Argon2id哈希~~: 过度设计，BCrypt对API密钥验证已足够（非密码派生场景）

**实现细节**:
```csharp
// 生成密钥
public string GenerateApiKey()
{
    const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    var random = RandomNumberGenerator.Create();
    var bytes = new byte[32];
    random.GetBytes(bytes);
    var key = "sk_" + new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    return key;
}

// 验证密钥
public bool VerifyApiKey(string providedKey, string storedHash)
{
    return BCrypt.Net.BCrypt.Verify(providedKey, storedHash);
}
```

---

### 2. API密钥作用域控制机制

**决策**: 使用ScopeType枚举（All/Specific）+ ApiKeyWebsiteScope多对多表

**理由**:
- **类型安全**: 枚举避免magic string
- **查询效率**: 作用域为"All"时无需JOIN ApiKeyWebsiteScopes表
- **扩展性**: 未来可添加"ReadOnly"等新作用域类型
- **中间件验证**: 在ApiKeyAuthMiddleware中提前验证权限，避免业务逻辑重复检查

**考虑的备选方案**:
- ~~JSON列存储网站ID列表~~: 查询性能差，无法利用索引
- ~~每网站一条密钥记录~~: 数据冗余，无法表达"所有网站"语义
- ~~RBAC角色系统~~: 过度设计，当前仅需作用域控制

**数据模型**:
```
ApiKey (1) ---> (N) ApiKeyWebsiteScope (N) <--- (1) Website
   |
   +- ScopeType: All | Specific
   +- If Specific: ApiKeyWebsiteScopes表记录允许的网站列表
   +- If All: ApiKeyWebsiteScopes为空，代码逻辑允许所有网站
```

---

### 3. 账号扩展字段设计

**决策**: 使用JSON列（SQLite: TEXT, PostgreSQL: JSONB）存储键值对，10KB大小限制

**理由**:
- **灵活性**: 避免为每个可能字段创建数据库列（如"安全问题"、"绑定手机"等）
- **类型兼容**: SQLite不支持原生JSON但可用TEXT存储，PostgreSQL用JSONB获得索引和查询优化
- **10KB限制**: 平衡灵活性和性能，足够存储~100个键值对
- **EF Core映射**: 使用`.HasColumnType("TEXT")`或`.HasColumnType("jsonb")`，配合`JsonDocument`或自定义转换器

**考虑的备选方案**:
- ~~EAV模式（Entity-Attribute-Value）~~: 查询复杂，性能差
- ~~NoSQL文档数据库~~: 架构不一致，增加运维复杂度
- ~~无限制大小~~: 可能被滥用，影响查询性能

**实现示例**:
```csharp
// Entity
public class Account
{
    public int Id { get; set; }
    // ... 其他字段
    public string ExtendedData { get; set; } = "{}"; // JSON字符串
}

// DbContext配置
modelBuilder.Entity<Account>()
    .Property(a => a.ExtendedData)
    .HasColumnType("TEXT") // SQLite
    .HasMaxLength(10240); // 10KB

// API验证
[MaxLength(10240, ErrorMessage = "扩展字段不能超过10KB")]
public string? ExtendedData { get; set; }
```

---

### 4. 账号状态管理（Active/Disabled）

**决策**: 新增AccountStatus枚举字段，默认Active，禁用账号仍在列表显示

**理由**:
- **独立维度**: 状态（Active/Disabled）与删除（IsDeleted）是正交关系
- **回收站兼容**: 已删除账号保留原状态，恢复时恢复原状态
- **UI显示**: 所有账号始终显示，用Badge或灰色背景区分禁用账号
- **API过滤**: 默认返回所有账号，支持`?status=active`或`?status=disabled`过滤

**考虑的备选方案**:
- ~~布尔字段IsDisabled~~: 语义不清，容易与IsDeleted混淆
- ~~字符串状态~~: 无类型安全，易拼写错误
- ~~隐藏禁用账号~~: 用户体验差，不符合需求（需在列表中看到但有标识）

**数据模型**:
```csharp
public enum AccountStatus
{
    Active = 0,
    Disabled = 1
}

public class Account
{
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public bool IsDeleted { get; set; } = false; // 回收站标记
}
```

**状态组合**:
| Status | IsDeleted | 含义 |
|--------|-----------|------|
| Active | false | 正常可用 |
| Disabled | false | 已禁用但未删除 |
| Active | true | 回收站中（删除前是活跃） |
| Disabled | true | 回收站中（删除前是禁用） |

---

### 5. 随机获取启用账号算法

**决策**: 使用数据库ORDER BY RANDOM() LIMIT 1（SQLite）或ORDER BY RANDOM() LIMIT 1（PostgreSQL）

**理由**:
- **均匀分布**: 数据库内置随机函数保证统计学均匀性
- **简单实现**: 单条SQL查询完成
- **性能可接受**: 小数据量（<1万账号/网站）下性能影响可忽略
- **无状态**: 不需要维护已使用账号列表

**考虑的备选方案**:
- ~~应用层随机（先查列表再C#随机选择）~~: 两次数据库往返，浪费带宽
- ~~加权随机（根据使用频率）~~: 过度设计，需求未提及
- ~~轮询算法~~: 不符合"随机"需求

**实现示例**:
```csharp
// 服务层
public async Task<Account?> GetRandomEnabledAccountAsync(int websiteId)
{
    return await _context.Accounts
        .Where(a => a.WebsiteId == websiteId
                 && a.Status == AccountStatus.Active
                 && !a.IsDeleted)
        .OrderBy(a => EF.Functions.Random())
        .FirstOrDefaultAsync();
}
```

**性能考虑**:
- 小数据集（<1000账号）：<10ms
- 中数据集（1000-10000账号）：10-50ms
- 若成为瓶颈：可缓存启用账号ID列表，应用层随机

---

### 6. API密钥验证中间件设计

**决策**: 自定义ASP.NET Core中间件，从Header提取密钥并验证作用域

**理由**:
- **关注点分离**: 验证逻辑与业务逻辑解耦
- **统一认证**: 所有`/api/external/*`端点自动应用
- **性能优化**: 密钥验证失败时立即返回401，避免进入Controller
- **可测试性**: 中间件可独立单元测试

**考虑的备选方案**:
- ~~Action Filter~~: 需在每个Controller/Action手动标注
- ~~JWT Bearer Token~~: 过度设计，API密钥已满足需求
- ~~Basic Auth~~: 不符合API密钥习惯

**实现示例**:
```csharp
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        if (!context.Request.Path.StartsWithSegments("/api/external"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var providedKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Missing API key" });
            return;
        }

        var apiKey = await apiKeyService.ValidateAndGetApiKeyAsync(providedKey);
        if (apiKey == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // 将API密钥信息存入HttpContext，供Controller使用
        context.Items["ApiKey"] = apiKey;
        await _next(context);
    }
}
```

---

### 7. 前端密钥显示与复制

**决策**: 使用shadcn/ui的Input组件 + "显示/隐藏"切换按钮 + Clipboard API

**理由**:
- **用户控制**: 默认显示密钥（已存储明文），用户可切换隐藏
- **一键复制**: 点击复制按钮调用`navigator.clipboard.writeText()`
- **无第三方库**: 现代浏览器原生支持Clipboard API
- **无障碍**: 提供视觉反馈（Toast通知"已复制"）

**考虑的备选方案**:
- ~~始终脱敏显示~~: 不符合需求（需随时查看）
- ~~点击显示对话框~~: 增加交互步骤，用户体验差
- ~~使用clipboard.js库~~: 现代浏览器已无需

**实现示例**:
```tsx
const [showKey, setShowKey] = useState(false);

const copyToClipboard = async (text: string) => {
  await navigator.clipboard.writeText(text);
  toast({ title: "已复制到剪贴板" });
};

return (
  <div className="flex items-center gap-2">
    <Input
      type={showKey ? "text" : "password"}
      value={apiKey.key}
      readOnly
    />
    <Button
      variant="ghost"
      onClick={() => setShowKey(!showKey)}
    >
      {showKey ? <EyeOff /> : <Eye />}
    </Button>
    <Button
      variant="outline"
      onClick={() => copyToClipboard(apiKey.key)}
    >
      <Copy /> 复制
    </Button>
  </div>
);
```

---

## 技术风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 明文存储密钥被数据库泄露 | 高 | BCrypt哈希提供第二道防线；环境变量加密数据库连接字符串；生产环境使用PostgreSQL加密 |
| 扩展字段被滥用（超10KB） | 中 | API层验证大小；数据库列约束；前端UI限制键值对数量 |
| 随机算法性能瓶颈 | 低 | 当前数据规模（<1万账号）可忽略；监控API响应时间；必要时切换到缓存+应用层随机 |
| 中间件误拦截内部API | 低 | 路径前缀检查（仅`/api/external/*`）；集成测试覆盖 |

---

## 依赖库版本确认

| 库 | 版本 | 用途 | 说明 |
|----|------|------|------|
| BCrypt.Net-Next | 4.0.3+ | API密钥哈希 | 稳定版本，支持.NET 8 |
| EF Core | 9.0 | 数据库操作 | 已在项目中使用 |
| System.Text.Json | 内置 | JSON序列化 | .NET 8自带 |
| shadcn/ui | 最新 | UI组件 | 已在项目中配置 |
| react-router-dom | 7.9.4 | 前端路由 | 已安装，用于添加`/api-keys`页面 |

---

## 下一步：设计产物

基于以上研究，阶段1将生成：
1. **data-model.md**: ApiKey、ApiKeyWebsiteScope、Account扩展字段的详细实体设计
2. **contracts/*.yaml**: OpenAPI规范描述API端点
3. **quickstart.md**: 开发者快速上手指南（创建密钥→调用API）
