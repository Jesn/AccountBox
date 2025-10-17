# Feature Specification: Web前端JWT身份认证系统

**Feature Branch**: `007-accountbox-web-jwt`
**Created**: 2025-10-17
**Status**: Draft
**Input**: User description: "为AccountBox Web前端实现JWT Token身份认证系统"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - 用户首次访问需要登录 (Priority: P1)

当用户访问AccountBox Web应用时,如果未登录,系统会引导用户到登录页面,用户输入主密码后可以访问所有功能。

**Why this priority**: 这是认证系统的核心功能,没有这个功能,应用无法保护任何数据。这是最小可行产品（MVP）的基础。

**Independent Test**: 可以通过访问应用首页、输入正确的主密码、验证能否进入主界面来独立测试。

**Acceptance Scenarios**:

1. **Given** 用户未登录,**When** 访问任何应用页面（如 /websites）,**Then** 自动跳转到登录页面
2. **Given** 用户在登录页面,**When** 输入正确的主密码并点击登录,**Then** 获得访问令牌并跳转到原始请求的页面
3. **Given** 用户已登录,**When** 访问任何应用页面,**Then** 直接显示页面内容,不需要再次登录
4. **Given** 用户在登录页面,**When** 输入错误的主密码,**Then** 显示错误提示"密码错误",且不允许登录

---

### User Story 2 - 持久化登录状态 (Priority: P2)

用户登录后,即使关闭浏览器重新打开,在Token有效期（24小时）内仍然保持登录状态,无需重复输入密码。

**Why this priority**: 提升用户体验,避免频繁登录。这是在P1核心功能之上的改进,但不是MVP必需的。

**Independent Test**: 登录后关闭浏览器,24小时内重新打开应用,验证是否仍然保持登录状态。

**Acceptance Scenarios**:

1. **Given** 用户已登录,**When** 关闭浏览器并在5分钟后重新打开,**Then** 仍然保持登录状态,无需再次输入密码
2. **Given** 用户已登录,**When** 距离登录时间超过24小时后访问应用,**Then** Token过期,自动跳转到登录页面
3. **Given** 用户已登录,**When** 在不同的浏览器标签页访问应用,**Then** 所有标签页共享同一个登录状态

---

### User Story 3 - 主动登出功能 (Priority: P2)

用户可以主动点击"登出"按钮退出登录,清除本地存储的认证信息,保护账户安全。

**Why this priority**: 安全功能,允许用户在共享设备上主动退出。虽然重要,但不如P1的登录功能关键。

**Independent Test**: 登录后点击登出按钮,验证是否清除Token并跳转到登录页面。

**Acceptance Scenarios**:

1. **Given** 用户已登录,**When** 点击页面上的"登出"按钮,**Then** 清除本地Token并跳转到登录页面
2. **Given** 用户已登出,**When** 尝试访问任何受保护页面,**Then** 被重定向到登录页面
3. **Given** 用户在多个标签页已登录,**When** 在一个标签页点击登出,**Then** 所有标签页的登录状态都被清除

---

### User Story 4 - Token自动刷新和错误处理 (Priority: P3)

当用户的Token过期或无效时,系统自动检测并引导用户重新登录,不会出现奇怪的错误或卡住的情况。

**Why this priority**: 这是优化用户体验的功能,确保错误情况下的优雅降级。属于边缘情况处理。

**Independent Test**: 手动删除localStorage中的Token或修改为无效值,访问页面验证是否自动跳转登录。

**Acceptance Scenarios**:

1. **Given** 用户的Token已过期,**When** 调用任何API接口,**Then** 后端返回401错误,前端自动清除Token并跳转登录页
2. **Given** 用户的Token被篡改,**When** 调用任何API接口,**Then** 后端验证失败返回401,前端自动跳转登录页
3. **Given** 用户正在操作过程中Token过期,**When** 提交表单或请求数据,**Then** 显示友好提示"会话已过期,请重新登录"

---

### Edge Cases

- **多次登录失败**: 用户连续输入错误密码5次后,系统应该显示警告并可能实施短暂的登录冷却期（如1分钟）以防止暴力破解
- **并发登录**: 如果用户在多个设备或浏览器同时登录,是否允许？（假设：允许多设备登录,每个设备有独立的Token）
- **Token在请求中间过期**: 如果用户正在填写长表单时Token过期,提交时应该提示用户重新登录,而不是丢失表单数据
- **网络错误 vs 认证错误**: 系统需要区分网络连接问题（500错误）和认证问题（401错误）,给出不同的提示
- **本地存储被清除**: 如果用户手动清除浏览器数据或localStorage被清空,下次访问应该正常跳转登录页

## Requirements *(mandatory)*

### Functional Requirements

#### 后端认证系统

- **FR-001**: 系统必须提供 `/api/auth/login` 端点,接受主密码并返回JWT Token
- **FR-002**: 系统必须验证提交的主密码是否与Vault主密码一致（复用现有的主密码验证逻辑）
- **FR-003**: 系统必须生成标准的JWT Token,包含用户标识、签发时间（iat）、过期时间（exp,24小时后）
- **FR-004**: 系统必须在所有 `/api/*` 端点（除了 `/api/auth/*` 和 `/api/external/*`）上启用JWT认证中间件
- **FR-005**: 系统必须在收到未携带Token或Token无效的请求时,返回HTTP 401 Unauthorized状态码
- **FR-006**: 系统必须保持 `/api/external/*` 端点使用现有的API Key认证机制,不受JWT认证影响
- **FR-007**: 系统必须在JWT配置中使用安全的密钥（至少256位）,存储在配置文件中
- **FR-008**: 系统必须记录登录失败尝试,并在连续5次失败后实施1分钟的冷却期
- **FR-009**: 系统必须在Token验证失败时返回明确的错误信息（如"Token已过期"、"Token无效"）

#### 前端登录系统

- **FR-010**: 系统必须提供登录页面,包含密码输入框和登录按钮
- **FR-011**: 系统必须在登录成功后将JWT Token存储在localStorage中（键名：`authToken`）
- **FR-012**: 系统必须配置Axios请求拦截器,自动在所有请求头中添加 `Authorization: Bearer {token}`
- **FR-013**: 系统必须配置Axios响应拦截器,捕获401错误并自动清除Token、跳转到登录页
- **FR-014**: 系统必须在应用启动时检查localStorage中是否有有效Token,决定显示登录页还是主界面
- **FR-015**: 系统必须提供登出功能,清除localStorage中的Token并跳转到登录页
- **FR-016**: 系统必须在登录页面显示密码输入错误时的友好提示信息
- **FR-017**: 系统必须在密码输入框支持"Enter"键提交登录
- **FR-018**: 系统必须在登录过程中显示加载状态,防止重复提交

#### 路由保护

- **FR-019**: 系统必须实现路由守卫,在访问受保护路由前检查Token是否存在
- **FR-020**: 系统必须在未登录时访问受保护路由,自动跳转到登录页并记录原始URL,登录成功后返回原始URL
- **FR-021**: 系统必须将登录页面（`/login`）设置为公开路由,无需Token即可访问

### Key Entities

- **JWT Token**: 包含以下声明（claims）：
  - `sub`: 主体标识（可以是固定值,如"vault-user",因为是单用户系统）
  - `iat`: 签发时间戳
  - `exp`: 过期时间戳（签发时间 + 24小时）
  - `jti`: Token唯一标识（可选,用于追踪）

- **认证状态**: 前端维护的认证状态,包含：
  - `isAuthenticated`: 布尔值,表示当前是否已认证
  - `token`: 当前的JWT Token字符串
  - `expiresAt`: Token过期时间（从Token解码或从响应获取）

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 用户可以在10秒内完成登录流程（从打开登录页到进入主界面）
- **SC-002**: 所有内部API端点（除了auth和external）都需要有效Token才能访问,未携带Token的请求100%被拒绝
- **SC-003**: 用户在24小时内无需重复登录,关闭浏览器后重新打开仍保持登录状态
- **SC-004**: Token过期或无效时,用户会在2秒内看到明确的提示并被引导到登录页,不会出现"卡住"或无响应的情况
- **SC-005**: 外部API调用（使用API Key）不受JWT认证系统影响,功能正常运行
- **SC-006**: 连续5次登录失败后,系统实施1分钟冷却期,防止暴力破解
- **SC-007**: 登录错误时,用户在1秒内看到明确的错误提示（如"密码错误"）

## Assumptions *(optional)*

- **单用户系统**: AccountBox是为个人使用设计的,因此JWT Token不需要包含复杂的用户标识或权限信息
- **本地部署**: 假设应用主要在本地或可信网络环境中运行,生产环境需要配置HTTPS以保护Token传输
- **主密码已设置**: 假设用户已经在初始化Vault时设置了主密码,认证系统可以直接复用这个密码
- **无Token刷新机制**: 24小时有效期对于个人使用场景足够长,不需要实现Token刷新（refresh token）机制
- **浏览器支持**: 假设用户使用现代浏览器,支持localStorage和ES6+特性
- **无多用户管理**: 系统不需要区分不同用户或角色,所有登录用户拥有完全访问权限
- **密钥管理**: JWT签名密钥存储在后端配置文件（appsettings.json）,在生产环境应该使用环境变量或密钥管理服务

## Out of Scope *(optional)*

以下功能明确不在本次实现范围内：

- **Token刷新机制**: 不实现refresh token,Token过期后必须重新登录
- **记住我复选框**: 默认使用localStorage持久化,不提供"记住我"选项（始终记住）
- **多因素认证（MFA）**: 不实现两步验证或其他多因素认证
- **密码重置功能**: 不提供"忘记密码"或密码重置功能（这是单用户系统,用户需要自行记住主密码）
- **会话管理界面**: 不提供查看或管理活动会话（多设备登录）的界面
- **登录历史记录**: 不记录或显示用户的登录历史和IP地址
- **OAuth/SSO集成**: 不支持第三方登录或单点登录
- **用户注册功能**: 不提供用户注册流程（主密码在Vault初始化时设置）

## Dependencies *(optional)*

### 现有功能依赖

- **Vault主密码验证**: 依赖现有的主密码验证逻辑（来自004-mvp-vault功能）
- **现有API端点**: 需要在现有的所有Controller上添加 `[Authorize]` 特性或全局认证策略

### 技术依赖

- **后端NuGet包**:
  - `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT认证中间件
  - `System.IdentityModel.Tokens.Jwt` - JWT生成和验证

- **前端npm包**:
  - `axios` - HTTP客户端（已存在）
  - `react-router-dom` - 路由管理（需确认版本）
  - `jwt-decode` - 客户端JWT解码（可选,用于提取Token信息）

### 配置依赖

- **appsettings.json**: 需要添加JWT配置节（密钥、签发者、受众、过期时间）
- **环境变量**: 生产环境需要通过环境变量配置JWT密钥

## Notes *(optional)*

### 安全考虑

1. **密钥强度**: JWT签名密钥必须至少256位,使用强随机生成器生成
2. **HTTPS要求**: 生产环境必须使用HTTPS,否则Token可能在传输过程中被窃取
3. **XSS防护**: Token存储在localStorage中,需要确保前端代码防止XSS攻击（使用React的内置XSS防护）
4. **CSRF防护**: JWT Token不存储在Cookie中,天然防止CSRF攻击
5. **暴力破解防护**: 实施登录失败冷却期机制

### 实现提示

1. **后端优先**: 建议先实现后端JWT认证系统,然后再开发前端登录界面
2. **测试Token**: 可以使用 https://jwt.io 网站解码和验证生成的Token
3. **错误处理**: 确保401错误和其他错误（如网络错误）有不同的处理逻辑
4. **开发环境**: 开发时可以暂时允许HTTP,但要确保生产环境强制HTTPS

### 后续改进（未来可能的功能）

- 实现Token刷新机制,延长会话有效期而不需要重新登录
- 添加"记住我"选项,让用户选择是否持久化登录状态
- 实现多设备会话管理,允许用户查看和撤销其他设备的登录
- 添加登录历史和审计日志
- 支持生物识别认证（如指纹、Face ID）作为快速登录方式
