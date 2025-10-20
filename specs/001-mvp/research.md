# Technical Research: 账号管理系统 MVP

**Feature**: 001-mvp | **Date**: 2025-10-14 | **Plan**: [plan.md](./plan.md)

## Overview

本文档记录账号管理系统MVP实施过程中的关键技术决策、最佳实践研究以及备选方案评估。

---

## 1. 加密存储方案

### 决策：信封加密（Envelope Encryption）+ AES-256-GCM

**选择理由**：

1. **信封加密优势**：
   - 主密码更改时无需重新加密所有业务数据，仅需重新包裹VaultKey
   - VaultKey仅驻留内存，降低密钥泄露风险
   - 符合行业最佳实践（AWS KMS、Google Cloud KMS等均采用此模式）

2. **AES-256-GCM选择理由**：
   - 提供认证加密（Authenticated Encryption），同时保证机密性和完整性
   - GCM模式性能优于CBC+HMAC组合
   - .NET的`System.Security.Cryptography`原生支持，无需第三方库

3. **Argon2id KDF选择理由**：
   - 2015年密码散列竞赛获胜者，专为密码派生设计
   - 抗GPU/ASIC暴力破解（内存密集型）
   - 结合Argon2i（抗侧信道攻击）和Argon2d（抗GPU攻击）优势

**考虑的备选方案**：

| 备选方案 | 优势 | 劣势 | 未选择原因 |
|---------|------|------|-----------|
| 直接主密码派生数据加密密钥 | 实现简单 | 修改主密码需重新加密所有数据（性能差） | 不符合FR-041需求 |
| PBKDF2 + AES-256-CBC | 广泛使用，成熟 | PBKDF2抗GPU攻击能力弱于Argon2；CBC需额外HMAC | 安全性和性能不如Argon2id+GCM |
| ChaCha20-Poly1305 | 软件实现性能好 | .NET支持较新，兼容性稍差 | AES-NI硬件加速在x86平台上更普及 |

**实现要点**：

```
1. 首次初始化流程：
   用户输入主密码 → Argon2id派生KEK → 生成随机VaultKey(256-bit)
   → KEK加密VaultKey → 持久化KeySlot

2. 解锁流程：
   用户输入主密码 → Argon2id派生KEK → 解密KeySlot获取VaultKey
   → VaultKey加载到内存

3. 数据加密/解密：
   业务数据 ←→ AES-256-GCM(VaultKey) ←→ 加密数据存储

4. 修改主密码：
   用户输入新主密码 → Argon2id派生新KEK → 新KEK重新加密VaultKey
   → 更新KeySlot（业务数据无需变动）
```

**参数配置**：

- **Argon2id参数**（平衡安全性和性能）：
  - 内存：64 MB（适合桌面应用）
  - 迭代次数：3-4次
  - 并行度：4（利用多核CPU）
  - 目标：~500ms派生时间（不影响用户体验，抗暴力破解）

- **AES-256-GCM参数**：
  - 密钥长度：256 bits
  - IV长度：96 bits（每次加密随机生成）
  - Tag长度：128 bits（认证标签）

---

## 2. 软删除与级联删除机制

### 决策：逻辑删除标记 + 事务保护的级联删除

**选择理由**：

1. **软删除实现**：
   - 实体添加`IsDeleted`布尔字段和`DeletedAt`时间戳
   - EF Core全局查询过滤器自动排除已删除记录
   - 回收站视图查询`IsDeleted=true`的记录

2. **级联删除保护**：
   - 删除网站前检查活跃账号数（`WHERE IsDeleted=false`）
   - 使用数据库事务确保删除操作原子性
   - 软删除账号在回收站中保留网站引用（外键约束）

**考虑的备选方案**：

| 备选方案 | 优势 | 劣势 | 未选择原因 |
|---------|------|------|-----------|
| 物理删除+备份表 | 主表无冗余数据 | 恢复复杂，需从备份表复制 | 不符合用户"回收站"快速恢复的需求 |
| 版本化数据（时态表） | 完整历史记录 | 存储开销大，查询复杂 | MVP不需要完整历史，仅需恢复能力 |
| 独立RecycleBin表 | 主表清爽 | 需要序列化/反序列化，schema不一致 | 增加复杂度，不利于数据一致性 |

**实现要点**：

```csharp
// Entity基类
public abstract class SoftDeletableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// EF Core全局查询过滤器
modelBuilder.Entity<Account>()
    .HasQueryFilter(a => !a.IsDeleted);

// 删除网站事务流程
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // 1. 检查活跃账号
    var activeCount = await _context.Accounts
        .Where(a => a.WebsiteId == id && !a.IsDeleted)
        .CountAsync();
    if (activeCount > 0)
        throw new InvalidOperationException("网站下还有活跃账号");

    // 2. 检查回收站账号
    var deletedCount = await _context.Accounts
        .IgnoreQueryFilters()
        .Where(a => a.WebsiteId == id && a.IsDeleted)
        .CountAsync();
    if (deletedCount > 0 && !confirmed)
        return new ConfirmationRequiredResult(deletedCount);

    // 3. 物理删除所有账号（包括回收站）
    var allAccounts = await _context.Accounts
        .IgnoreQueryFilters()
        .Where(a => a.WebsiteId == id)
        .ToListAsync();
    _context.Accounts.RemoveRange(allAccounts);

    // 4. 删除网站
    _context.Websites.Remove(website);

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

## 3. 全文搜索实现

### 决策：SQLite FTS5（全文搜索扩展）

**选择理由**：

1. **SQLite FTS5优势**：
   - 内置扩展，无需外部依赖
   - 支持中文分词（通过tokenizer=unicode61）
   - 性能优秀（千级记录<100ms查询）
   - 符合"搜索响应时间<0.5秒"的性能目标

2. **大小写不敏感**：
   - SQLite的LIKE和FTS5默认大小写不敏感（对ASCII字符）
   - 中文查询天然无大小写问题

**考虑的备选方案**：

| 备选方案 | 优势 | 劣势 | 未选择原因 |
|---------|------|------|-----------|
| LIKE模糊查询 | 实现简单 | 性能差（全表扫描），无分词 | 不满足性能需求（1000条<0.5s） |
| Elasticsearch | 功能强大，分词优秀 | 部署复杂，资源消耗高 | MVP过度设计，违反"本地存储"约束 |
| Lucene.NET | 功能丰富 | 额外依赖，嵌入式部署复杂 | SQLite FTS5足够满足需求 |

**实现要点**：

```sql
-- 创建FTS5虚拟表
CREATE VIRTUAL TABLE accounts_fts USING fts5(
    username,
    notes,
    tags,
    website_name,
    website_domain,
    content=accounts,
    content_rowid=id,
    tokenize='unicode61'  -- 支持Unicode分词
);

-- 搜索查询
SELECT a.*
FROM accounts a
JOIN accounts_fts fts ON a.id = fts.rowid
WHERE accounts_fts MATCH ?
  AND a.IsDeleted = 0
ORDER BY rank
LIMIT 10 OFFSET ?;
```

**PostgreSQL/MySQL备选方案**：

- **PostgreSQL**：使用`tsvector`和`tsquery`（全文搜索），配置中文分词器（zhparser）
- **MySQL**：使用全文索引（FULLTEXT INDEX），配置ngram解析器（支持中文）

---

## 4. 分页策略

### 决策：后端基于Offset的分页

**选择理由**：

1. **Offset分页适用场景**：
   - 用户需要跳页浏览（符合需求：翻页查看）
   - 数据量中等（1000条），性能可接受
   - 实现简单，EF Core原生支持

2. **性能优化**：
   - 在常用查询字段（网站名、创建时间）上建立索引
   - 限制最大页数（如100页），避免深分页性能问题

**考虑的备选方案**：

| 备选方案 | 优势 | 劣势 | 未选择原因 |
|---------|------|------|-----------|
| Cursor-based分页 | 深分页性能好，数据一致性强 | 无法跳页，仅支持上一页/下一页 | 不符合用户需求（需跳页） |
| 客户端全量加载 | 无需分页请求 | 初始加载慢，内存消耗大 | 不满足"分页加载<1s"需求 |

**实现要点**：

```csharp
// 分页DTO
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// EF Core分页查询
var query = _context.Accounts
    .Where(a => !a.IsDeleted)
    .OrderByDescending(a => a.CreatedAt);

var totalCount = await query.CountAsync();
var items = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

return new PagedResult<Account>
{
    Items = items,
    TotalCount = totalCount,
    PageNumber = pageNumber,
    PageSize = pageSize
};
```

---

## 5. 密码生成器算法

### 决策：System.Security.Cryptography.RandomNumberGenerator + 字符集过滤

**选择理由**：

1. **加密级随机数生成器**：
   - `RandomNumberGenerator`提供密码学安全的随机数（CSPRNG）
   - 避免`System.Random`可预测性问题

2. **字符集过滤**：
   - 支持用户自定义字符集（大写/小写/数字/符号）
   - 可排除易混淆字符（0/O, 1/l/I, 2/Z等）

3. **强度计算**：
   - 熵值计算：`entropy = log2(charset_size^length)`
   - 分级：弱(<60 bits)、中(60-80 bits)、强(>80 bits)

**实现要点**：

```csharp
public class PasswordGenerator
{
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";  // 排除l
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";  // 排除I,O
    private const string Digits = "3456789";  // 排除0,1,2
    private const string Symbols = "!@#$%^&*-_=+";

    public string Generate(PasswordOptions options)
    {
        var charset = BuildCharset(options);
        var password = new char[options.Length];

        using var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[options.Length * 4];
        rng.GetBytes(randomBytes);

        for (int i = 0; i < options.Length; i++)
        {
            var randomValue = BitConverter.ToUInt32(randomBytes, i * 4);
            password[i] = charset[(int)(randomValue % charset.Length)];
        }

        return new string(password);
    }

    public PasswordStrength CalculateStrength(string password, int charsetSize)
    {
        var entropy = password.Length * Math.Log2(charsetSize);
        if (entropy < 60) return PasswordStrength.Weak;
        if (entropy < 80) return PasswordStrength.Medium;
        return PasswordStrength.Strong;
    }
}
```

---

## 6. 前端状态管理

### 决策：React Context + Hooks（轻量方案）

**选择理由**：

1. **MVP规模适中**：
   - 状态较简单（用户认证、VaultKey、当前页面数据）
   - 组件树深度有限（2-3层）
   - React Context足够满足需求

2. **避免过度工程化**：
   - Redux对MVP来说过于重量级
   - 减少学习曲线和依赖

**考虑的备选方案**：

| 备选方案 | 优势 | 劣势 | 未选择原因 |
|---------|------|------|-----------|
| Redux Toolkit | 强大的状态管理，DevTools | 配置复杂，代码量大 | MVP过度设计 |
| Zustand | 轻量，API简洁 | 额外依赖 | Context已满足需求 |
| Jotai/Recoil | 原子化状态 | 相对新，生态较小 | 学习成本，MVP不需要 |

**实现架构**：

```typescript
// VaultContext：管理加密密钥和认证状态
interface VaultContextType {
  isUnlocked: boolean;
  vaultKey: Uint8Array | null;
  unlock: (masterPassword: string) => Promise<void>;
  lock: () => void;
}

// DataContext：管理当前页面的业务数据
interface DataContextType {
  websites: Website[];
  accounts: Account[];
  refreshWebsites: () => Promise<void>;
  refreshAccounts: (websiteId: string) => Promise<void>;
}

// 使用自定义Hooks封装
const useVault = () => useContext(VaultContext);
const useData = () => useContext(DataContext);
```

---

## 7. 错误处理策略

### 决策：全局异常中间件 + 结构化错误响应

**选择理由**：

1. **后端统一处理**：
   - ASP.NET Core中间件捕获所有未处理异常
   - 转换为标准化JSON错误响应
   - 记录日志（排除敏感信息）

2. **前端错误边界**：
   - React Error Boundary捕获组件树异常
   - axios interceptor统一处理HTTP错误

**实现要点**：

```csharp
// 后端错误响应模型
public class ErrorResponse
{
    public string ErrorCode { get; set; }  // e.g., "ACTIVE_ACCOUNTS_EXIST"
    public string Message { get; set; }    // 用户友好消息
    public Dictionary<string, string> Details { get; set; }  // 额外信息
}

// 全局异常中间件
public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (UnauthorizedException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }
}
```

```typescript
// 前端axios interceptor
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    const errorResponse = error.response?.data;

    // 根据错误码显示友好提示
    if (errorResponse?.errorCode === 'ACTIVE_ACCOUNTS_EXIST') {
      toast.error('请先删除该网站下的所有账号');
    } else if (error.response?.status === 401) {
      // 跳转到解锁页面
      router.push('/unlock');
    } else {
      toast.error('操作失败，请稍后重试');
    }

    return Promise.reject(error);
  }
);
```

---

## 8. 测试策略

### 决策：分层测试金字塔

**选择理由**：

1. **后端测试**：
   - **单元测试（xUnit）**：覆盖业务逻辑（Service层）、加密模块
   - **集成测试**：使用In-Memory SQLite测试Repository和DbContext
   - **API测试**：使用WebApplicationFactory测试端到端API流程

2. **前端测试**：
   - **单元测试（Jest）**：工具函数、Hooks
   - **组件测试（React Testing Library）**：组件交互和渲染
   - **E2E测试（Playwright）**：关键用户流程（创建账号、搜索、回收站）

**测试覆盖率目标**：

- 关键业务逻辑：>90%
- 加密模块：100%
- API端点：>80%
- 前端组件：>70%

**实现要点**：

```csharp
// 后端集成测试示例
public class WebsiteRepositoryTests : IDisposable
{
    private readonly AccountBoxDbContext _context;

    public WebsiteRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AccountBoxDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _context = new AccountBoxDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task DeleteWebsite_WithActiveAccounts_ShouldThrow()
    {
        // Arrange
        var website = new Website { Id = 1, Name = "Test" };
        var account = new Account { Id = 1, WebsiteId = 1, IsDeleted = false };
        _context.Websites.Add(website);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var repository = new WebsiteRepository(_context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.DeleteAsync(1)
        );
    }
}
```

```typescript
// 前端组件测试示例
describe('WebsiteList', () => {
  it('should display websites in paginated format', async () => {
    const mockWebsites = createMockWebsites(25);
    mockApi.getWebsites.mockResolvedValue({
      items: mockWebsites.slice(0, 10),
      totalCount: 25,
      pageNumber: 1,
      pageSize: 10
    });

    render(<WebsiteList />);

    await waitFor(() => {
      expect(screen.getAllByRole('row')).toHaveLength(10);
    });

    expect(screen.getByText('第1页 / 共3页')).toBeInTheDocument();
  });
});
```

---

## 9. 性能优化策略

### 关键优化点

1. **数据库优化**：
   - 索引：`Websites(Name)`, `Accounts(WebsiteId, IsDeleted, CreatedAt)`
   - FTS5全文索引：加速搜索
   - 连接池配置：MaxPoolSize=100

2. **加密性能**：
   - VaultKey常驻内存，避免重复派生
   - 批量加密/解密使用并行处理（Parallel.ForEach）
   - Argon2id参数调优（内存64MB，迭代3次，~500ms）

3. **前端优化**：
   - 列表虚拟化（react-window）：大列表性能
   - 防抖搜索输入（debounce 300ms）
   - React.memo优化组件重渲染
   - 懒加载密码生成器组件

4. **API优化**：
   - 响应压缩（Gzip/Brotli）
   - EF Core查询优化（AsNoTracking、Select投影）
   - 分页查询避免加载关联实体

---

## 10. 部署与运行

### 决策：单机部署（本地应用）

**部署模式**：

1. **后端**：
   - 打包为独立可执行文件（self-contained deployment）
   - 内嵌Kestrel服务器，监听localhost:5000
   - SQLite数据库文件存储在用户目录（如`~/.accountbox/data.db`）

2. **前端**：
   - 构建为静态文件（npm run build）
   - 后端托管静态文件（wwwroot/）
   - 或使用Electron封装为桌面应用（未来扩展）

3. **首次运行流程**：
   ```
   用户启动应用 → 检测数据库文件 → 不存在则引导设置主密码
   → 初始化数据库schema → 生成VaultKey → 加密持久化 → 进入主界面
   ```

**配置管理**：

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source={UserProfile}/.accountbox/data.db"
  },
  "Security": {
    "Argon2": {
      "MemorySize": 65536,
      "Iterations": 3,
      "Parallelism": 4
    }
  },
  "Pagination": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100
  }
}
```

---

## 总结

本研究文档覆盖了账号管理系统MVP的10个关键技术领域：

1. ✅ **加密存储**：信封加密 + AES-256-GCM + Argon2id
2. ✅ **软删除机制**：逻辑删除 + 事务保护
3. ✅ **全文搜索**：SQLite FTS5
4. ✅ **分页策略**：后端Offset分页
5. ✅ **密码生成器**：CSPRNG + 自定义字符集
6. ✅ **状态管理**：React Context + Hooks
7. ✅ **错误处理**：全局中间件 + 结构化响应
8. ✅ **测试策略**：分层测试金字塔
9. ✅ **性能优化**：数据库索引 + 加密优化 + 前端优化
10. ✅ **部署运行**：单机部署 + 自包含可执行文件

所有技术选择均基于功能需求、性能目标和用户提供的技术栈，确保实施计划的可行性和高质量交付。

**下一步**：进入阶段1，生成数据模型（data-model.md）和API契约（contracts/）。
