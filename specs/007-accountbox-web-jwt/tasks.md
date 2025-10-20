# Tasks: Web前端JWT身份认证系统

**Input**: Design documents from `/specs/007-accountbox-web-jwt/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Tests are OPTIONAL - this feature does NOT explicitly request comprehensive test coverage, but basic tests will be included for critical security components.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- **Web app**: `backend/src/`, `frontend/src/`
- Paths follow the structure defined in plan.md

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and dependency installation

- [ ] T001 [P] 添加后端JWT NuGet包：在`backend/src/AccountBox.Api`运行`dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`和`dotnet add package System.IdentityModel.Tokens.Jwt`
- [ ] T002 [P] 配置JWT设置：在`backend/src/AccountBox.Api/appsettings.json`添加JwtSettings和LoginThrottle配置节（使用openssl生成256位SecretKey）
- [ ] T003 [P] 添加前端依赖验证：检查`frontend/package.json`确认react-router-dom 7.9.4已安装（已存在，无需额外操作）

**Checkpoint**: 依赖包和基础配置已就绪

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Foundation

- [ ] T004 创建LoginAttempt实体：在`backend/src/AccountBox.Data/Entities/LoginAttempt.cs`创建实体类（Id, IPAddress, AttemptTime, IsSuccessful, FailureReason, UserAgent）
- [ ] T005 添加LoginAttempts DbSet：在`backend/src/AccountBox.Data/DbContext/AccountBoxDbContext.cs`添加`public DbSet<LoginAttempt> LoginAttempts { get; set; }`
- [ ] T006 创建数据库迁移：在`backend/src/AccountBox.Api`运行`dotnet ef migrations add AddLoginAttempts`
- [ ] T007 应用数据库迁移：在`backend/src/AccountBox.Api`运行`dotnet ef database update`

### Backend Models & Services

- [ ] T008 [P] 创建JwtSettings模型：在`backend/src/AccountBox.Core/Models/Auth/JwtSettings.cs`创建配置模型（SecretKey, Issuer, Audience, ExpirationHours等）
- [ ] T009 [P] 创建LoginRequest DTO：在`backend/src/AccountBox.Core/Models/Auth/LoginRequest.cs`创建DTO（MasterPassword字段，Data Annotations验证）
- [ ] T010 [P] 创建LoginResponse DTO：在`backend/src/AccountBox.Core/Models/Auth/LoginResponse.cs`创建DTO（Token, ExpiresAt字段）
- [ ] T011 创建IJwtService接口：在`backend/src/AccountBox.Core/Interfaces/IJwtService.cs`定义接口（GenerateToken, ValidateToken, GetClaimsFromToken方法）
- [ ] T012 实现JwtService：在`backend/src/AccountBox.Api/Services/JwtService.cs`实现JWT Token生成和验证逻辑（使用JwtSecurityTokenHandler，HS256签名，包含sub/jti/iat/exp/iss/aud claims）

### Backend Authentication Middleware

- [ ] T013 配置JWT认证中间件：在`backend/src/AccountBox.Api/Program.cs`添加`AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer()`配置，设置TokenValidationParameters
- [ ] T014 注册JwtService：在`backend/src/AccountBox.Api/Program.cs`添加`builder.Services.AddScoped<IJwtService, JwtService>()`和MemoryCache注册
- [ ] T015 启用认证和授权中间件：在`backend/src/AccountBox.Api/Program.cs`添加`app.UseAuthentication()`和`app.UseAuthorization()`（注意顺序：在UseRouting之后，MapControllers之前）

### Frontend Foundation

- [ ] T016 [P] 创建authService：在`frontend/src/services/authService.ts`创建认证服务（login, logout, getToken, isAuthenticated方法，使用localStorage存储token）
- [ ] T017 配置Axios拦截器：修改`frontend/src/lib/axios.ts`添加请求拦截器（自动附加Authorization header）和响应拦截器（401自动跳转登录页）

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - 用户首次访问需要登录 (Priority: P1) 🎯 MVP

**Goal**: 实现核心登录功能，用户输入主密码后可以访问所有应用页面

**Independent Test**: 访问http://localhost:5173，未登录时跳转到/login，输入正确主密码后进入主界面，刷新页面仍保持登录状态

### Backend Implementation for US1

- [ ] T018 [US1] 创建AuthController：在`backend/src/AccountBox.Api/Controllers/AuthController.cs`创建控制器，添加`[ApiController]`和`[Route("api/auth")]`特性
- [ ] T019 [US1] 实现登录端点：在AuthController中实现`POST /api/auth/login`端点（接受LoginRequest，调用IVaultManager.Unlock验证密码，成功后生成JWT Token，返回ApiResponse<LoginResponse>）
- [ ] T020 [US1] 添加登录失败记录：在登录端点中添加LoginAttempt记录逻辑（成功和失败都记录，包含IP地址、时间戳、UserAgent）
- [ ] T021 [US1] 实现基本错误处理：在登录端点中捕获CryptographicException（密码错误）返回401，其他异常返回500

### Frontend Implementation for US1

- [ ] T022 [P] [US1] 创建LoginPage组件：在`frontend/src/pages/LoginPage.tsx`创建登录页面（使用shadcn/ui Card, Input, Button, Label组件）
- [ ] T023 [P] [US1] 实现登录表单逻辑：在LoginPage中添加密码输入、错误提示、加载状态、Enter键提交、调用authService.login
- [ ] T024 [P] [US1] 创建ProtectedRoute组件：在`frontend/src/components/auth/ProtectedRoute.tsx`创建路由守卫（检查isAuthenticated，未登录跳转/login）
- [ ] T025 [US1] 更新App路由：修改`frontend/src/App.tsx`添加登录路由和保护路由（/login公开，其他路由用ProtectedRoute包裹）
- [ ] T026 [US1] 添加应用启动认证检查：在`frontend/src/main.tsx`或App.tsx添加初始化逻辑（检查localStorage中的token，设置初始认证状态）

### Testing for US1 (Basic Security Tests Only)

- [ ] T027 [P] [US1] 测试密码验证：使用curl测试正确密码和错误密码的响应（200 vs 401）
- [ ] T028 [P] [US1] 测试Token生成：使用jwt.io验证生成的Token包含正确的claims（sub, jti, iat, exp, iss, aud）

**Checkpoint**: At this point, User Story 1 should be fully functional - users can log in and access protected routes

---

## Phase 4: User Story 2 - 持久化登录状态 (Priority: P2)

**Goal**: 用户登录后关闭浏览器，24小时内重新打开仍保持登录状态

**Independent Test**: 登录后关闭浏览器，5分钟后重新打开http://localhost:5173，无需重新登录。24小时后访问应跳转登录页

### Implementation for US2

- [ ] T029 [US2] 验证localStorage持久化：确认authService.login正确保存token到localStorage（键名：authToken）
- [ ] T030 [US2] 验证Token过期检查：前端添加Token过期时间检查逻辑（解码JWT获取exp，与当前时间对比）
- [ ] T031 [US2] 测试跨标签页同步：在多个标签页打开应用，验证登录状态在所有标签页共享

**Checkpoint**: Persistent login across browser sessions works correctly

---

## Phase 5: User Story 3 - 主动登出功能 (Priority: P2)

**Goal**: 用户可以主动点击登出按钮，清除认证信息并跳转登录页

**Independent Test**: 登录后点击登出按钮，验证跳转到登录页，token被清除，无法访问受保护页面

### Backend Implementation for US3

- [ ] T032 [US3] 实现登出端点：在`backend/src/AccountBox.Api/Controllers/AuthController.cs`添加`POST /api/auth/logout`端点（添加[Authorize]特性，返回成功消息）

### Frontend Implementation for US3

- [ ] T033 [P] [US3] 添加登出按钮：在主布局导航栏（或用户菜单）添加登出按钮（调用authService.logout）
- [ ] T034 [P] [US3] 实现登出逻辑：确认authService.logout清除localStorage并跳转/login
- [ ] T035 [US3] 测试多标签页登出：在一个标签页登出，验证其他标签页的受保护页面也无法访问（可选：添加storage事件监听实现跨标签页同步登出）

**Checkpoint**: Logout functionality works correctly, including multi-tab scenarios

---

## Phase 6: User Story 4 - Token自动刷新和错误处理 (Priority: P3)

**Goal**: Token过期或无效时自动检测并引导用户重新登录，提供友好的错误提示

**Independent Test**: 手动删除或修改localStorage中的token，访问任何页面应自动跳转登录并显示友好提示

### Backend Implementation for US4

- [ ] T036 [US4] 优化错误响应格式：在AuthController中返回标准化错误代码（PASSWORD_INCORRECT, TOKEN_EXPIRED, TOKEN_INVALID, INTERNAL_ERROR）
- [ ] T037 [US4] 添加Token验证错误处理：在JWT中间件配置中添加OnChallenge事件处理，返回明确的错误消息

### Frontend Implementation for US4

- [ ] T038 [US4] 实现错误消息映射：在Axios响应拦截器中解析错误代码，显示对应的友好提示（使用shadcn/ui Sonner Toast）
- [ ] T039 [US4] 添加网络错误区分：区分401认证错误和500/网络错误，显示不同的提示消息
- [ ] T040 [US4] 测试Token过期场景：使用修改后的token测试自动跳转和错误提示

**Checkpoint**: Error handling provides clear user feedback for all authentication failure scenarios

---

## Phase 7: Login Failure Protection (Security Feature)

**Purpose**: Prevent brute force attacks with login throttling

- [ ] T041 创建LoginThrottleMiddleware：在`backend/src/AccountBox.Api/Middleware/LoginThrottleMiddleware.cs`创建中间件（使用IMemoryCache检查失败次数，超过5次返回429）
- [ ] T042 注册LoginThrottleMiddleware：在`backend/src/AccountBox.Api/Program.cs`添加`app.UseMiddleware<LoginThrottleMiddleware>()`（仅应用于/api/auth/login路径）
- [ ] T043 实现冷却期逻辑：在中间件中检查最后失败时间，60秒内拒绝登录请求
- [ ] T044 同步更新数据库和缓存：登录失败时同时更新MemoryCache和LoginAttempts表
- [ ] T045 测试登录限制：连续5次输入错误密码，验证第6次返回429并显示"请1分钟后再试"

**Checkpoint**: Login throttling prevents brute force attacks

---

## Phase 8: Protect Existing API Endpoints

**Purpose**: Apply JWT authentication to all internal API endpoints

- [ ] T046 添加全局认证策略：在`backend/src/AccountBox.Api/Program.cs`配置全局授权策略（所有Controller默认需要[Authorize]，除了/api/auth/*和/api/external/*）
- [ ] T047 [P] 或者，添加[Authorize]特性：如果不使用全局策略，在所有现有Controller上添加[Authorize]特性（WebsiteController, AccountController, ApiKeyController等）
- [ ] T048 验证API Key路径不受影响：测试/api/external/*端点仍然使用API Key认证，不受JWT影响
- [ ] T049 测试受保护端点：尝试不带token访问/api/websites，验证返回401

**Checkpoint**: All internal APIs are protected by JWT authentication

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements and validation

- [ ] T050 [P] 添加HTTPS重定向（生产环境）：在`backend/src/AccountBox.Api/Program.cs`添加`app.UseHttpsRedirection()`（仅在非开发环境）
- [ ] T051 [P] 添加登录页样式优化：优化LoginPage的UI/UX（居中布局、响应式设计、密码可见性切换）
- [ ] T052 [P] 添加加载动画：在登录过程中显示Spinner组件
- [ ] T053 清理旧的LoginAttempts记录：添加后台任务或手动脚本，删除30天前的登录记录
- [ ] T054 验证quickstart.md中的所有测试场景：按照quickstart.md中的Manual Testing章节逐一验证
- [ ] T055 代码格式化：运行`dotnet format`（后端）和`pnpm prettier --write "src/**/*.{ts,tsx}"`（前端）
- [ ] T056 最终安全检查：核对quickstart.md中的Security Checklist，确保所有项都已完成

**Checkpoint**: Feature is production-ready

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (P1) - MVP核心功能，必须先完成
  - US2 (P2) - 依赖US1的登录功能，但可以在US1完成后立即开始
  - US3 (P2) - 依赖US1的登录功能，可以与US2并行
  - US4 (P3) - 依赖US1的错误处理框架，应在US1-3完成后实施
- **Login Throttling (Phase 7)**: 可以在US1完成后任何时间添加
- **Protect Endpoints (Phase 8)**: 依赖US1的JWT认证基础设施
- **Polish (Phase 9)**: Depends on all previous phases

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories - **MVP核心**
- **User Story 2 (P2)**: Can start after US1 complete - Tests persistent login built in US1
- **User Story 3 (P2)**: Can start after US1 complete - Adds logout to existing login system - **Can run in parallel with US2**
- **User Story 4 (P3)**: Can start after US1 complete - Enhances error handling from US1

### Within Each User Story

- Backend models/services before controllers
- Frontend services before components
- Core functionality before edge cases
- Basic tests before advanced scenarios

### Parallel Opportunities

#### Phase 1 (Setup)
```bash
# All setup tasks can run in parallel:
T001: 添加JWT NuGet包
T002: 配置JWT设置
T003: 验证前端依赖
```

#### Phase 2 (Foundation) - Backend Models
```bash
# Models can be created in parallel:
T008: JwtSettings模型
T009: LoginRequest DTO
T010: LoginResponse DTO
```

#### Phase 3 (US1) - Frontend Components
```bash
# Frontend components can be created in parallel:
T022: LoginPage组件
T023: 登录表单逻辑
T024: ProtectedRoute组件
```

#### Phase 5 (US3) - Logout Feature
```bash
# Frontend logout UI can be developed in parallel with backend:
T032: 后端登出端点（backend）
T033: 添加登出按钮（frontend）
T034: 实现登出逻辑（frontend）
```

---

## Parallel Example: User Story 1 (MVP)

```bash
# After Foundational phase completes, launch US1 backend and frontend in parallel:

# Backend Team:
Task T018: 创建AuthController
Task T019: 实现登录端点
Task T020: 添加登录失败记录
Task T021: 实现错误处理

# Frontend Team (in parallel):
Task T022: 创建LoginPage组件
Task T023: 实现登录表单逻辑
Task T024: 创建ProtectedRoute组件

# Then integrate:
Task T025: 更新App路由
Task T026: 添加应用启动认证检查

# Finally test together:
Task T027: 测试密码验证
Task T028: 测试Token生成
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 2: Foundational (T004-T017) - **CRITICAL blocking phase**
3. Complete Phase 3: User Story 1 (T018-T028)
4. **STOP and VALIDATE**:
   - 测试登录流程（正确密码、错误密码）
   - 测试路由保护（未登录跳转、已登录访问）
   - 测试Token验证（使用jwt.io解码）
5. **MVP Ready** - 可以demo基本的登录和认证功能

### Incremental Delivery

1. **Foundation** (Phase 1-2): 完成后，JWT基础设施就绪
2. **MVP** (Phase 3): 完成后，基本登录功能可用 → **第一次Demo**
3. **Enhanced UX** (Phase 4-5): 添加持久化登录和登出 → **第二次Demo**
4. **Error Handling** (Phase 6): 优化错误提示 → **第三次Demo**
5. **Security** (Phase 7-8): 添加登录限制和API保护 → **生产就绪**
6. **Polish** (Phase 9): 最终优化 → **正式发布**

### Parallel Team Strategy

如果有2个开发者：

1. **Together**: Phase 1-2 (Setup + Foundation)
2. **Split after Foundation completes**:
   - **Developer A**: Backend (T018-T021, T032, T036-T037, T041-T049)
   - **Developer B**: Frontend (T022-T026, T033-T035, T038-T040, T051-T052)
3. **Integrate and Test**: T027-T028, T031, T054
4. **Polish together**: T050, T053, T055-T056

---

## Notes

- **[P] tasks** = different files, no dependencies, can run in parallel
- **[Story] label** maps task to specific user story for traceability
- **MVP = User Story 1 only** - 实现核心登录功能即可demo
- **Tests are minimal** - 仅包含关键的安全验证测试，未包含完整的单元测试套件（规格说明未明确要求）
- **Commit after each task** - 遵循项目章程，每完成一个任务立即commit（使用中文commit消息）
- **IVaultManager密码验证** - 复用现有逻辑，无需重复实现
- **API Key系统不受影响** - /api/external/*继续使用现有的ApiKeyAuthMiddleware
- **HTTPS仅生产环境** - 开发环境使用HTTP简化测试
- **停止点** - 每个Phase完成后都可以停下来验证功能

---

## Security Checklist (Before Production)

在部署到生产环境前，确保完成以下检查项：

- [ ] JWT SecretKey至少256位，使用强随机生成（openssl rand -base64 32）
- [ ] SecretKey存储在环境变量中，不提交到git
- [ ] 生产环境启用HTTPS重定向（T050）
- [ ] Token有效期设置为24小时
- [ ] 登录失败限制已启用（T041-T045）
- [ ] 所有内部API端点添加了[Authorize]或全局策略（T046-T049）
- [ ] API Key认证（/api/external/*）不受JWT影响（T048）
- [ ] CORS配置仅允许可信来源
- [ ] 所有错误响应不泄露敏感信息
- [ ] LoginAttempts表有定期清理策略（T053）
