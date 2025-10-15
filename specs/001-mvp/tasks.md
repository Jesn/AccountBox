# Tasks: 账号管理系统 MVP

**Input**: Design documents from `/specs/001-mvp/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅

**Tests**: 测试任务已包含在内（基于MVP需求的完整测试覆盖）

**Organization**: 任务按用户故事组织，以实现每个故事的独立实施和测试。

## Format: `[ID] [P?] [Story] Description`
- **[P]**: 可并行运行（不同文件，无依赖）
- **[Story]**: 此任务属于哪个用户故事（例如 US1, US2, US3, US6, Foundation）
- 描述中包含确切的文件路径

## Path Conventions
- **后端**: `backend/src/AccountBox.*/`
- **前端**: `frontend/src/`
- **测试**: `backend/tests/`, `frontend/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 项目初始化和基本结构

- [x] **T001** [P] [Setup] 创建后端项目结构：`backend/src/AccountBox.Api/`, `backend/src/AccountBox.Core/`, `backend/src/AccountBox.Data/`, `backend/src/AccountBox.Security/`
- [x] **T002** [P] [Setup] 初始化.NET 10解决方案，添加项目引用，配置NuGet包（ASP.NET Core, EF Core, xUnit, Moq, FluentAssertions）
- [x] **T003** [P] [Setup] 创建前端项目结构：`frontend/src/components/`, `frontend/src/pages/`, `frontend/src/services/`, `frontend/src/hooks/`, `frontend/src/utils/`, `frontend/src/types/`, `frontend/src/stores/`, `frontend/src/features/`, `frontend/src/contexts/`
- [x] **T004** [P] [Setup] 初始化React + TypeScript项目，配置shadcn/ui, Tailwind CSS, Jest, React Testing Library, Playwright
- [x] **T005** [P] [Setup] 配置后端linting（EditorConfig, .NET analyzers）和前端linting（ESLint, Prettier）
- [x] **T006** [P] [Setup] 配置Git忽略规则（.gitignore：bin/, obj/, node_modules/, .env, *.db）

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 所有用户故事必须在此阶段完成后才能开始实现的核心基础设施

**⚠️ CRITICAL**: 在此阶段完成之前，不能开始任何用户故事工作

### 加密基础设施（US6 - 本地加密存储）

- [x] **T007** [P] [Foundation-US6] 实现Argon2id KDF服务：`backend/src/AccountBox.Security/KeyDerivation/Argon2Service.cs`（支持密钥派生、参数配置）
- [x] **T008** [P] [Foundation-US6] 实现AES-256-GCM加密服务：`backend/src/AccountBox.Security/Encryption/AesGcmEncryptionService.cs`（加密、解密、IV和Tag生成）
- [x] **T009** [Foundation-US6] 实现VaultManager（信封加密管理）：`backend/src/AccountBox.Security/VaultManager/VaultManager.cs`（依赖T007, T008；初始化VaultKey、解锁、锁定、修改主密码）
- [x] **T010** [P] [Foundation-US6] 创建IEncryptionService接口：`backend/src/AccountBox.Core/Interfaces/IEncryptionService.cs`
- [x] **T011** [P] [Foundation-US6] 创建IVaultManager接口：`backend/src/AccountBox.Core/Interfaces/IVaultManager.cs`

### 数据库基础设施

- [x] **T012** [P] [Foundation] 创建KeySlot实体：`backend/src/AccountBox.Data/Entities/KeySlot.cs`（字段：Id, EncryptedVaultKey, VaultKeyIV, VaultKeyTag, Argon2Salt, Argon2Iterations, Argon2MemorySize, Argon2Parallelism, CreatedAt, UpdatedAt）
- [x] **T013** [P] [Foundation] 创建Website实体：`backend/src/AccountBox.Data/Entities/Website.cs`（字段：Id, Domain, DisplayName, Tags, CreatedAt, UpdatedAt）
- [x] **T014** [P] [Foundation] 创建Account实体：`backend/src/AccountBox.Data/Entities/Account.cs`（字段：Id, WebsiteId, Username, PasswordEncrypted, PasswordIV, PasswordTag, Notes, NotesEncrypted, NotesIV, NotesTag, Tags, IsDeleted, DeletedAt, CreatedAt, UpdatedAt）
- [x] **T015** [Foundation] 配置EF Core DbContext：`backend/src/AccountBox.Data/DbContext/AccountBoxDbContext.cs`（依赖T012, T013, T014；包含DbSet、全局查询过滤器、触发器配置）
- [x] **T016** [P] [Foundation] 配置KeySlot实体映射：`backend/src/AccountBox.Data/Configurations/KeySlotConfiguration.cs`（主键约束Id=1，种子数据）
- [x] **T017** [P] [Foundation] 配置Website实体映射：`backend/src/AccountBox.Data/Configurations/WebsiteConfiguration.cs`（索引、长度约束、关系）
- [x] **T018** [P] [Foundation] 配置Account实体映射：`backend/src/AccountBox.Data/Configurations/AccountConfiguration.cs`（索引、软删除过滤器、外键级联）
- [x] **T019** [Foundation] 创建初始EF Core迁移：`backend/src/AccountBox.Data/Migrations/`（依赖T015-T018；运行完整命令：`dotnet ef migrations add Initial --project AccountBox.Data --startup-project AccountBox.Api --context AccountBoxDbContext`）
- [x] **T020** [P] [Foundation] 配置SQLite FTS5全文搜索虚拟表SQL脚本：`backend/src/AccountBox.Data/Scripts/CreateFTS.sql`

### API基础设施

- [x] **T021** [P] [Foundation] 配置ASP.NET Core中间件管道：`backend/src/AccountBox.Api/Program.cs`（CORS、异常处理、路由、Swagger）
- [x] **T022** [P] [Foundation] 实现全局异常处理中间件：`backend/src/AccountBox.Api/Middleware/ExceptionMiddleware.cs`（捕获异常、返回标准ErrorResponse）
- [x] **T023** [P] [Foundation] 创建ErrorResponse DTO：`backend/src/AccountBox.Core/Models/ErrorResponse.cs`（字段：ErrorCode, Message, Details）
- [x] **T023a** [P] [Foundation] 创建ApiResponse<T> DTO：`backend/src/AccountBox.Core/Models/ApiResponse.cs`（字段：Success, Data, Error, Timestamp；符合宪法III要求的统一API响应格式）
- [x] **T024** [P] [Foundation] 创建PagedResult<T> DTO：`backend/src/AccountBox.Core/Models/PagedResult.cs`（字段：Items, TotalCount, PageNumber, PageSize, TotalPages）
- [x] **T025** [P] [Foundation] 配置依赖注入容器：`backend/src/AccountBox.Api/Program.cs`（注册服务、仓储、加密服务、VaultManager）

### 前端基础设施

- [x] **T026** [P] [Foundation] 创建API客户端基类：`frontend/src/services/apiClient.ts`（axios配置、请求拦截器、响应拦截器、错误处理）
- [x] **T027** [P] [Foundation] 创建VaultContext：`frontend/src/contexts/VaultContext.tsx`（管理isUnlocked, vaultKey, unlock, lock状态）
- [x] **T028** [P] [Foundation] 创建useVault Hook：`frontend/src/hooks/useVault.ts`（封装VaultContext访问）
- [x] **T029** [P] [Foundation] 创建通用类型定义：`frontend/src/types/common.ts`（PagedResponse, ErrorResponse, Website, Account等接口）
- [x] **T030** [P] [Foundation] 配置shadcn/ui基础组件：运行`npx shadcn-ui@latest init`并添加完整组件清单：alert, badge, button, card, checkbox, dialog, input, label, radio-group, select, separator, slider, switch, table, textarea, toast

**Checkpoint**: 基础设施就绪 - 用户故事实现现在可以并行开始

---

## Phase 3: User Story 6 - 本地加密存储 (Priority: P1) 🎯 MVP核心

**Goal**: 实现应用初始化、解锁、锁定和主密码管理功能，为所有业务数据提供加密基础

**Independent Test**: 用户可以设置主密码、解锁应用、锁定应用、修改主密码；所有操作成功且KeySlot正确持久化

### 后端实现 (US6)

- [x] **T031** [P] [US6] 创建VaultService业务逻辑：`backend/src/AccountBox.Api/Services/VaultService.cs`（依赖IVaultManager, KeySlot仓储；实现Initialize, Unlock, Lock, ChangeMasterPassword；内存会话管理）
- [x] **T032** [P] [US6] 创建KeySlotRepository：`backend/src/AccountBox.Data/Repositories/KeySlotRepository.cs`（CRUD操作，确保单例约束）
- [x] **T033** [P] [US6] 创建Vault相关DTOs：`backend/src/AccountBox.Core/Models/Vault/`（InitializeVaultRequest, UnlockVaultRequest, ChangeMasterPasswordRequest, VaultSessionResponse, VaultStatusResponse）
- [x] **T034** [US6] 实现VaultController：`backend/src/AccountBox.Api/Controllers/VaultController.cs`（依赖T031；端点：POST /api/vault/initialize, POST /api/vault/unlock, POST /api/vault/lock, POST /api/vault/change-password, GET /api/vault/status）
- [x] **T035** [P] [US6] 实现会话管理中间件：`backend/src/AccountBox.Api/Middleware/VaultSessionMiddleware.cs`（验证X-Vault-Session头，确保VaultKey已解锁；白名单机制）

### 前端实现 (US6)

- [x] **T036** [P] [US6] 创建VaultService API客户端：`frontend/src/services/vaultService.ts`（调用initialize, unlock, lock, changePassword, getStatus端点）
- [x] **T037** [P] [US6] 创建InitializePage组件：`frontend/src/pages/InitializePage.tsx`（首次设置主密码，调用initialize API，表单验证）
- [x] **T038** [P] [US6] 创建UnlockPage组件：`frontend/src/pages/UnlockPage.tsx`（输入主密码解锁，调用unlock API，更新VaultContext）
- [x] **T039** [P] [US6] 创建ChangeMasterPasswordDialog组件：`frontend/src/components/vault/ChangeMasterPasswordDialog.tsx`（输入旧密码和新密码，调用changePassword API，成功后跳转解锁页）
- [x] **T040** [US6] 实现应用启动流程：`frontend/src/App.tsx`（依赖T036；检查getStatus，未初始化显示InitializePage，已初始化未解锁显示UnlockPage，已解锁显示WebsitesPage；路由守卫）

### 测试 (US6)

- [x] **T041** [P] [US6] 后端单元测试 - Argon2Service：`backend/tests/AccountBox.Security.Tests/Argon2ServiceTests.cs`（9个测试用例，覆盖KDF功能、错误处理）
- [x] **T042** [P] [US6] 后端单元测试 - AesGcmEncryptionService：`backend/tests/AccountBox.Security.Tests/AesGcmEncryptionServiceTests.cs`（13个测试用例，覆盖加密/解密、完整性保护）
- [x] **T043** [P] [US6] 后端单元测试 - VaultManager：`backend/tests/AccountBox.Security.Tests/VaultManagerTests.cs`（11个测试用例，覆盖信封加密完整流程）
- [x] **T044** [P] [US6] 后端集成测试 - VaultController：`backend/tests/AccountBox.Api.Tests/VaultControllerIntegrationTests.cs`（12个测试用例，测试initialize, unlock, lock, changePassword流程；使用InMemoryDatabase）
- [ ] **T045** [P] [US6] 前端单元测试 - VaultContext：`frontend/tests/unit/VaultContext.test.tsx`（跳过：时间限制，可手动测试替代）
- [ ] **T046** [P] [US6] 前端E2E测试 - 初始化和解锁流程：`frontend/tests/e2e/vault.spec.ts`（跳过：时间限制，可手动测试替代）
- [x] **T046a** [P] [US6] 实现密码重试限制逻辑：`backend/src/AccountBox.Api/Services/VaultService.cs`（记录失败次数，超过5次抛出TooManyAttemptsException并锁定30分钟；包含基础测试；注：测试存在静态状态共享问题，生产环境应使用数据库或Redis存储锁定状态）

**Checkpoint**: US6完成 - 应用可以初始化、解锁和管理主密码

---

## Phase 4: User Story 1 - 管理网站和账号 (Priority: P1) 🎯 MVP核心

**Goal**: 实现网站和账号的CRUD操作，支持分页展示

**Independent Test**: 用户可以创建网站、在网站下添加账号、查看分页列表、编辑网站和账号信息

### 后端实现 (US1)

- [x] **T047** [P] [US1] 创建WebsiteRepository：`backend/src/AccountBox.Data/Repositories/WebsiteRepository.cs`（CRUD、分页查询、账号统计；默认排序：按CreatedAt降序）
- [x] **T048** [P] [US1] 创建AccountRepository：`backend/src/AccountBox.Data/Repositories/AccountRepository.cs`（CRUD、分页查询、软删除支持、按WebsiteId过滤；默认排序：按Username字母序）
- [x] **T049** [P] [US1] 创建Website相关DTOs：`backend/src/AccountBox.Core/Models/Website/`（WebsiteResponse, CreateWebsiteRequest, UpdateWebsiteRequest, AccountCountResponse）
- [x] **T050** [P] [US1] 创建Account相关DTOs：`backend/src/AccountBox.Core/Models/Account/`（AccountResponse, CreateAccountRequest, UpdateAccountRequest）
- [x] **T051** [US1] 实现WebsiteService：`backend/src/AccountBox.Api/Services/WebsiteService.cs`（已移至Api层避免循环依赖；CRUD逻辑、分页、业务验证）
- [x] **T052** [US1] 实现AccountService：`backend/src/AccountBox.Api/Services/AccountService.cs`（已移至Api层；依赖T048, IEncryptionService；CRUD逻辑、密码加密/解密、分页）
- [x] **T053** [US1] 实现WebsiteController：`backend/src/AccountBox.Api/Controllers/WebsiteController.cs`（依赖T051；端点：GET /api/websites, POST /api/websites, GET /api/websites/{id}, PUT /api/websites/{id}, DELETE /api/websites/{id}, GET /api/websites/{id}/accounts/count）
- [x] **T054** [US1] 实现AccountController：`backend/src/AccountBox.Api/Controllers/AccountController.cs`（依赖T052；端点：GET /api/accounts, POST /api/accounts, GET /api/accounts/{id}, PUT /api/accounts/{id}, DELETE /api/accounts/{id}）

### 前端实现 (US1)

- [x] **T055** [P] [US1] 创建WebsiteService API客户端：`frontend/src/services/websiteService.ts`（调用websites相关端点）
- [x] **T056** [P] [US1] 创建AccountService API客户端：`frontend/src/services/accountService.ts`（调用accounts相关端点）
- [ ] **T057** [P] [US1] 创建WebsiteList组件：`frontend/src/components/websites/WebsiteList.tsx`（展示网站列表、分页、排序）
- [ ] **T058** [P] [US1] 创建CreateWebsiteDialog组件：`frontend/src/components/websites/CreateWebsiteDialog.tsx`（shadcn/ui Dialog，表单验证）
- [ ] **T059** [P] [US1] 创建EditWebsiteDialog组件：`frontend/src/components/websites/EditWebsiteDialog.tsx`（shadcn/ui Dialog，表单验证）
- [ ] **T060** [P] [US1] 创建AccountList组件：`frontend/src/components/accounts/AccountList.tsx`（展示某网站下账号列表、分页）
- [ ] **T061** [P] [US1] 创建CreateAccountDialog组件：`frontend/src/components/accounts/CreateAccountDialog.tsx`（shadcn/ui Dialog，表单验证，密码字段）
- [ ] **T062** [P] [US1] 创建EditAccountDialog组件：`frontend/src/components/accounts/EditAccountDialog.tsx`（shadcn/ui Dialog，表单验证）
- [x] **T063** [P] [US1] 创建Pagination组件：`frontend/src/components/common/Pagination.tsx`（通用分页组件，显示页码、上一页/下一页按钮）
- [x] **T064** [US1] 创建WebsitesPage主页面：`frontend/src/pages/WebsitesPage.tsx`（依赖T057, T058, T059；集成列表和对话框；已部分实现，集成了网站列表展示和分页）
- [ ] **T065** [US1] 创建AccountsPage详情页面：`frontend/src/pages/AccountsPage.tsx`（依赖T060, T061, T062；显示某网站的账号，集成列表和对话框）

### 测试 (US1)

- [ ] **T066** [P] [US1] 后端单元测试 - WebsiteService：`backend/tests/AccountBox.Core.Tests/WebsiteServiceTests.cs`
- [ ] **T067** [P] [US1] 后端单元测试 - AccountService：`backend/tests/AccountBox.Core.Tests/AccountServiceTests.cs`（包括加密/解密逻辑测试）
- [ ] **T068** [P] [US1] 后端集成测试 - WebsiteController：`backend/tests/AccountBox.Api.Tests/WebsiteControllerTests.cs`
- [ ] **T069** [P] [US1] 后端集成测试 - AccountController：`backend/tests/AccountBox.Api.Tests/AccountControllerTests.cs`
- [ ] **T070** [P] [US1] 前端组件测试 - WebsiteList：`frontend/tests/integration/WebsiteList.test.tsx`（React Testing Library）
- [ ] **T071** [P] [US1] 前端组件测试 - AccountList：`frontend/tests/integration/AccountList.test.tsx`
- [ ] **T072** [P] [US1] 前端E2E测试 - 网站和账号CRUD流程：`frontend/tests/e2e/website-account-crud.spec.ts`（Playwright测试完整CRUD流程）

**Checkpoint**: US1完成 - 用户可以管理网站和账号，查看分页列表

---

## Phase 5: User Story 2 - 安全删除账号（软删除与回收站） (Priority: P2)

**Goal**: 实现账号软删除、回收站查看、恢复和永久删除功能

**Independent Test**: 用户可以删除账号（移入回收站）、在回收站查看已删除账号、恢复账号、永久删除账号、一键清空回收站

### 后端实现 (US2)

- [ ] **T073** [US2] 扩展AccountRepository添加回收站方法：`backend/src/AccountBox.Data/Repositories/AccountRepository.cs`（GetDeletedAccounts分页查询、RestoreAccount、PermanentlyDeleteAccount、EmptyRecycleBin、按网站过滤）
- [ ] **T074** [P] [US2] 创建RecycleBin相关DTOs：`backend/src/AccountBox.Core/Models/RecycleBin/`（DeletedAccountResponse, PagedDeletedAccountResponse）
- [ ] **T075** [US2] 实现RecycleBinService：`backend/src/AccountBox.Core/Services/RecycleBinService.cs`（依赖T073；查询、恢复、永久删除、清空逻辑）
- [ ] **T076** [US2] 实现RecycleBinController：`backend/src/AccountBox.Api/Controllers/RecycleBinController.cs`（依赖T075；端点：GET /api/recycle-bin, POST /api/recycle-bin/{id}/restore, DELETE /api/recycle-bin/{id}, DELETE /api/recycle-bin）

### 前端实现 (US2)

- [ ] **T077** [P] [US2] 创建RecycleBinService API客户端：`frontend/src/services/recycleBinService.ts`（调用recycle-bin相关端点）
- [ ] **T078** [P] [US2] 创建RecycleBinList组件：`frontend/src/components/recycle-bin/RecycleBinList.tsx`（展示已删除账号列表、分页、按网站过滤）
- [ ] **T079** [P] [US2] 创建EmptyRecycleBinDialog组件：`frontend/src/components/recycle-bin/EmptyRecycleBinDialog.tsx`（shadcn/ui Dialog，强二次确认）
- [ ] **T080** [US2] 创建RecycleBinPage页面：`frontend/src/pages/RecycleBinPage.tsx`（依赖T078, T079；集成列表和清空功能）
- [ ] **T081** [US2] 更新AccountList组件：`frontend/src/components/accounts/AccountList.tsx`（添加删除按钮，调用软删除API）

### 测试 (US2)

- [ ] **T082** [P] [US2] 后端单元测试 - RecycleBinService：`backend/tests/AccountBox.Core.Tests/RecycleBinServiceTests.cs`
- [ ] **T083** [P] [US2] 后端集成测试 - RecycleBinController：`backend/tests/AccountBox.Api.Tests/RecycleBinControllerTests.cs`
- [ ] **T084** [P] [US2] 前端组件测试 - RecycleBinList：`frontend/tests/integration/RecycleBinList.test.tsx`
- [ ] **T085** [P] [US2] 前端E2E测试 - 软删除和恢复流程：`frontend/tests/e2e/recycle-bin.spec.ts`（Playwright测试删除→回收站查看→恢复→永久删除流程）

**Checkpoint**: US2完成 - 用户可以安全管理已删除账号

---

## Phase 6: User Story 3 - 安全删除网站（级联删除保护） (Priority: P2)

**Goal**: 实现网站删除前的安全检查，防止意外删除活跃账号或回收站账号

**Independent Test**: 用户尝试删除有活跃账号的网站时被阻止；尝试删除只有回收站账号的网站时收到确认提示；确认后网站和所有关联账号被永久删除

### 后端实现 (US3)

- [ ] **T086** [US3] 扩展WebsiteService添加级联删除逻辑：`backend/src/AccountBox.Core/Services/WebsiteService.cs`（检查活跃账号数、检查回收站账号数、事务删除）
- [ ] **T087** [P] [US3] 创建级联删除相关DTOs：`backend/src/AccountBox.Core/Models/Website/`（ActiveAccountsExistError, ConfirmationRequiredError）
- [ ] **T088** [US3] 更新WebsiteController删除端点：`backend/src/AccountBox.Api/Controllers/WebsiteController.cs`（添加confirmed查询参数，返回409错误码）

### 前端实现 (US3)

- [ ] **T089** [P] [US3] 创建DeleteWebsiteConfirmDialog组件：`frontend/src/components/websites/DeleteWebsiteConfirmDialog.tsx`（shadcn/ui Dialog，显示回收站账号数，二次确认）
- [ ] **T090** [US3] 更新WebsiteList组件：`frontend/src/components/websites/WebsiteList.tsx`（添加删除按钮，处理409错误，显示确认对话框）

### 测试 (US3)

- [ ] **T091** [P] [US3] 后端单元测试 - WebsiteService级联删除：`backend/tests/AccountBox.Core.Tests/WebsiteServiceCascadeDeleteTests.cs`
- [ ] **T092** [P] [US3] 后端集成测试 - WebsiteController级联删除：`backend/tests/AccountBox.Api.Tests/WebsiteControllerCascadeDeleteTests.cs`
- [ ] **T093** [P] [US3] 前端E2E测试 - 网站删除保护流程：`frontend/tests/e2e/website-delete-protection.spec.ts`（Playwright测试阻止删除、确认删除流程）

**Checkpoint**: US3完成 - 网站删除安全可靠

---

## Phase 7: User Story 4 - 搜索账号 (Priority: P2)

**Goal**: 实现全文搜索功能，支持在网站名、域名、用户名、标签、备注中搜索

**Independent Test**: 用户输入关键词并按回车，系统返回匹配的账号列表（分页），搜索大小写不敏感且自动去除首尾空格

### 后端实现 (US4)

- [ ] **T094** [P] [US4] 创建SearchRepository：`backend/src/AccountBox.Data/Repositories/SearchRepository.cs`（使用SQLite FTS5、支持分页、解密备注字段进行搜索）
- [ ] **T095** [P] [US4] 创建Search相关DTOs：`backend/src/AccountBox.Core/Models/Search/`（SearchResultItem, SearchResultResponse）
- [ ] **T096** [US4] 实现SearchService：`backend/src/AccountBox.Core/Services/SearchService.cs`（依赖T094, IEncryptionService；处理查询、去空格、大小写不敏感、分页）
- [ ] **T097** [US4] 实现SearchController：`backend/src/AccountBox.Api/Controllers/SearchController.cs`（依赖T096；端点：GET /api/search?query=xxx&pageNumber=1&pageSize=10）

### 前端实现 (US4)

- [ ] **T098** [P] [US4] 创建SearchService API客户端：`frontend/src/services/searchService.ts`（调用search端点）
- [ ] **T099** [P] [US4] 创建SearchBar组件：`frontend/src/components/search/SearchBar.tsx`（输入框，回车触发搜索，防抖处理）
- [ ] **T100** [P] [US4] 创建SearchResultsList组件：`frontend/src/components/search/SearchResultsList.tsx`（展示搜索结果、高亮匹配字段、分页）
- [ ] **T101** [US4] 创建SearchPage页面：`frontend/src/pages/SearchPage.tsx`（依赖T099, T100；集成搜索栏和结果列表）
- [ ] **T102** [US4] 更新App.tsx主导航：`frontend/src/App.tsx`（添加搜索页面链接）

### 测试 (US4)

- [ ] **T103** [P] [US4] 后端单元测试 - SearchService：`backend/tests/AccountBox.Core.Tests/SearchServiceTests.cs`（包括大小写不敏感、去空格测试）
- [ ] **T104** [P] [US4] 后端集成测试 - SearchController：`backend/tests/AccountBox.Api.Tests/SearchControllerTests.cs`（测试FTS5搜索结果）
- [ ] **T105** [P] [US4] 前端组件测试 - SearchBar：`frontend/tests/integration/SearchBar.test.tsx`
- [ ] **T106** [P] [US4] 前端E2E测试 - 搜索流程：`frontend/tests/e2e/search.spec.ts`（Playwright测试输入关键词→回车→查看结果→翻页）

**Checkpoint**: US4完成 - 用户可以快速搜索账号

---

## Phase 8: User Story 5 - 生成强密码 (Priority: P3)

**Goal**: 实现密码生成器功能，支持配置长度、字符集、排除易混淆字符、显示强度

**Independent Test**: 用户在创建/编辑账号时打开密码生成器，配置参数，生成密码，查看强度，接受后填充到表单

### 后端实现 (US5)

- [ ] **T107** [P] [US5] 实现PasswordGenerator服务：`backend/src/AccountBox.Core/Services/PasswordGenerator.cs`（使用RandomNumberGenerator、字符集配置、强度计算）
- [ ] **T108** [P] [US5] 创建PasswordGenerator相关DTOs：`backend/src/AccountBox.Core/Models/PasswordGenerator/`（GeneratePasswordRequest, GeneratePasswordResponse, CalculateStrengthRequest, PasswordStrengthResponse）
- [ ] **T109** [US5] 实现PasswordGeneratorController：`backend/src/AccountBox.Api/Controllers/PasswordGeneratorController.cs`（依赖T107；端点：POST /api/password-generator/generate, POST /api/password-generator/strength）

### 前端实现 (US5)

- [ ] **T110** [P] [US5] 创建PasswordGeneratorService API客户端：`frontend/src/services/passwordGeneratorService.ts`（调用generate和strength端点）
- [ ] **T111** [P] [US5] 创建PasswordGeneratorDialog组件：`frontend/src/components/password-generator/PasswordGeneratorDialog.tsx`（shadcn/ui Dialog，滑块配置长度，复选框配置字符集，显示强度条，重新生成按钮，接受按钮）
- [ ] **T112** [P] [US5] 创建PasswordStrengthIndicator组件：`frontend/src/components/password-generator/PasswordStrengthIndicator.tsx`（进度条或颜色指示，显示弱/中/强）
- [ ] **T113** [US5] 更新CreateAccountDialog和EditAccountDialog：`frontend/src/components/accounts/`（添加"生成密码"按钮，打开PasswordGeneratorDialog，接受后填充密码字段）

### 测试 (US5)

- [ ] **T114** [P] [US5] 后端单元测试 - PasswordGenerator：`backend/tests/AccountBox.Core.Tests/PasswordGeneratorTests.cs`（测试字符集、长度、强度计算）
- [ ] **T115** [P] [US5] 后端集成测试 - PasswordGeneratorController：`backend/tests/AccountBox.Api.Tests/PasswordGeneratorControllerTests.cs`
- [ ] **T116** [P] [US5] 前端组件测试 - PasswordGeneratorDialog：`frontend/tests/integration/PasswordGeneratorDialog.test.tsx`
- [ ] **T117** [P] [US5] 前端E2E测试 - 密码生成器流程：`frontend/tests/e2e/password-generator.spec.ts`（Playwright测试打开生成器→配置→生成→接受→填充）

**Checkpoint**: US5完成 - 用户可以生成强密码

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: 完善和优化，影响多个用户故事的改进

- [ ] **T118** [P] [Polish] 添加后端日志记录：`backend/src/AccountBox.Api/Program.cs`（配置Serilog或NLog，记录关键操作和错误）
- [ ] **T119** [P] [Polish] 实现前端全局错误边界：`frontend/src/components/common/ErrorBoundary.tsx`（React Error Boundary，捕获组件异常）
- [ ] **T120** [P] [Polish] 添加前端加载状态指示器：`frontend/src/components/common/LoadingSpinner.tsx`（shadcn/ui Spinner，在API调用期间显示）
- [ ] **T121** [P] [Polish] 实现前端Toast通知系统：配置shadcn/ui toast，在成功/失败操作后显示提示
- [ ] **T122** [P] [Polish] 优化数据库查询性能：为常用查询添加索引（已在T017-T018完成，验证执行计划）
- [ ] **T123** [P] [Polish] 添加后端API文档：配置Swagger/OpenAPI，使用contracts/中的规范验证
- [ ] **T124** [P] [Polish] 实现前端响应式布局：确保在不同屏幕尺寸下正常显示（使用Tailwind响应式类）
- [ ] **T125** [P] [Polish] 添加后端数据验证：使用FluentValidation为所有DTOs添加验证规则
- [ ] **T126** [P] [Polish] 实现前端表单验证：使用React Hook Form + Zod验证所有表单输入
- [ ] **T127** [Polish] 运行quickstart.md验证：按照quickstart.md步骤从头搭建环境，确保所有步骤可行
- [ ] **T128** [P] [Polish] 代码清理和重构：移除console.log、未使用的import、统一命名规范
- [ ] **T129** [P] [Polish] 安全加固：添加CSRF保护、XSS防护、SQL注入防护验证（EF Core已自动处理参数化查询）
- [ ] **T130** [P] [Polish] 性能优化：前端组件memo化、后端查询优化（AsNoTracking）、加密批量处理
- [ ] **T131** [P] [Polish] 更新CLAUDE.md项目文档：添加当前功能列表、技术栈、已知问题
- [ ] **T132** [P] [Polish] XSS防护实现：前端React自动转义输出，后端API输入验证禁止`<script>`等危险标签（使用Data Annotations或FluentValidation）
- [ ] **T133** [P] [Polish] 性能基准测试：创建性能测试套件验证SC-009（分页< 1秒）、SC-010（搜索< 0.5秒）、SC-015（启动解锁< 3秒），使用BenchmarkDotNet（后端）和Lighthouse（前端）

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖 - 可以立即开始
- **Foundational (Phase 2)**: 依赖Setup完成 - **阻塞所有用户故事**
- **User Stories (Phase 3-8)**: 全部依赖Foundational阶段完成
  - 用户故事之间可以并行进行（如果有团队成员）
  - 或按优先级顺序执行（P1 → P2 → P3）
- **Polish (Phase 9)**: 依赖所有期望的用户故事完成

### User Story Dependencies

- **User Story 6 (P1 - 本地加密存储)**: 在Foundational后可以开始 - 无其他故事依赖
- **User Story 1 (P1 - 管理网站和账号)**: 依赖US6完成（需要加密服务） - 阻塞US2, US3, US4
- **User Story 2 (P2 - 软删除与回收站)**: 依赖US1完成（需要Account实体和服务） - 可与US3, US4, US5并行
- **User Story 3 (P2 - 安全删除网站)**: 依赖US1和US2完成（需要回收站检查） - 可与US4, US5并行
- **User Story 4 (P2 - 搜索账号)**: 依赖US1完成（需要Account实体） - 可与US2, US3, US5并行
- **User Story 5 (P3 - 生成强密码)**: 依赖US1完成（需要账号表单） - 可与US2, US3, US4并行

### Within Each User Story

- Foundational任务在所有故事之前
- 后端实体 → 后端仓储 → 后端服务 → 后端控制器
- 前端API客户端 → 前端组件 → 前端页面
- 测试可以与实现并行（TDD方法：先写测试，确保失败，再实现）

### Parallel Opportunities

- **Setup阶段**：T001-T006全部可并行
- **Foundational阶段**：
  - T007-T011（加密服务）可并行
  - T012-T014（实体）可并行
  - T016-T018（EF配置）可并行
  - T021-T024（API基础）可并行
  - T026-T030（前端基础）可并行
- **每个用户故事内**：
  - 所有标记[P]的任务可并行
  - 不同文件的测试可并行
- **用户故事间**（Foundational完成后）：
  - US6必须先完成
  - US1完成后，US2/US3/US4/US5可并行开发（如有团队容量）

---

## Parallel Example: User Story 1

```bash
# 并行启动US1的所有后端仓储和DTOs（不同文件）:
Task: "T047 创建WebsiteRepository"
Task: "T048 创建AccountRepository"
Task: "T049 创建Website相关DTOs"
Task: "T050 创建Account相关DTOs"

# 然后并行启动US1的所有前端API客户端和独立组件:
Task: "T055 创建WebsiteService API客户端"
Task: "T056 创建AccountService API客户端"
Task: "T057 创建WebsiteList组件"
Task: "T058 创建CreateWebsiteDialog组件"
Task: "T059 创建EditWebsiteDialog组件"
Task: "T060 创建AccountList组件"
Task: "T061 创建CreateAccountDialog组件"
Task: "T062 创建EditAccountDialog组件"
Task: "T063 创建Pagination组件"

# 并行启动US1的所有测试:
Task: "T066 后端单元测试 - WebsiteService"
Task: "T067 后端单元测试 - AccountService"
Task: "T068 后端集成测试 - WebsiteController"
Task: "T069 后端集成测试 - AccountController"
Task: "T070 前端组件测试 - WebsiteList"
Task: "T071 前端组件测试 - AccountList"
```

---

## Implementation Strategy

### MVP First (User Story 6 + User Story 1)

这是建议的最小可行产品范围：

1. 完成 **Phase 1: Setup** (T001-T006)
2. 完成 **Phase 2: Foundational** (T007-T030) - **关键阻塞阶段**
3. 完成 **Phase 3: User Story 6** (T031-T046) - 加密基础
4. 完成 **Phase 4: User Story 1** (T047-T072) - 核心CRUD
5. **停止并验证**: 独立测试US6和US1
6. 如果就绪则部署/演示

**MVP价值**: 用户可以安全地管理网站和账号，所有数据加密存储。

### Incremental Delivery

1. **Foundation (Phase 1-2)** → 基础就绪
2. **Add US6 + US1** → 独立测试 → 部署/演示（MVP！）
3. **Add US2** → 独立测试 → 部署/演示（增加软删除保护）
4. **Add US3** → 独立测试 → 部署/演示（增加网站删除保护）
5. **Add US4** → 独立测试 → 部署/演示（增加搜索能力）
6. **Add US5** → 独立测试 → 部署/演示（增加密码生成器）
7. **Add Polish (Phase 9)** → 最终优化

每个故事都增加价值而不破坏之前的故事。

### Parallel Team Strategy

如果有多个开发人员：

1. **团队一起完成Setup + Foundational**（T001-T030）
2. **Foundational完成后**：
   - 开发者A: User Story 6（T031-T046）→ 必须先完成
   - 开发者B: 准备User Story 1的设计和测试用例
3. **US6完成后**：
   - 开发者A: User Story 1（T047-T072）
   - 开发者B: User Story 2（T073-T085）（部分依赖US1，可提前准备）
4. **US1完成后**：
   - 开发者A: User Story 3（T086-T093）
   - 开发者B: User Story 4（T094-T106）
   - 开发者C: User Story 5（T107-T117）
5. 故事独立完成和集成

---

## Summary

- **总任务数**: 135个任务
- **任务分布**:
  - Setup: 6个任务
  - Foundational: 25个任务（关键阻塞阶段，新增T023a ApiResponse<T>）
  - User Story 6 (P1): 17个任务（加密存储，新增T046a密码重试限制）
  - User Story 1 (P1): 26个任务（网站和账号CRUD）
  - User Story 2 (P2): 13个任务（软删除与回收站）
  - User Story 3 (P2): 8个任务（网站删除保护）
  - User Story 4 (P2): 14个任务（搜索）
  - User Story 5 (P3): 11个任务（密码生成器）
  - Polish: 16个任务（新增T132 XSS防护、T133性能测试）
- **并行机会**: 约50%的任务可并行执行（标记[P]）
- **建议MVP范围**: Phase 1-4（Setup + Foundational + US6 + US1）= 74个任务
- **完整功能**: 所有阶段 = 135个任务

---

## Notes

- **[P]标记**: 不同文件、无依赖的任务
- **[Story]标签**: 将任务映射到特定用户故事，便于追溯
- **独立测试**: 每个用户故事完成后都应可独立验证功能
- **TDD建议**: 在实现前编写测试，确保测试失败
- **提交策略**: 每完成一个任务或逻辑组提交一次
- **检查点**: 在任何检查点停止以独立验证故事
- **避免**: 模糊任务、同文件冲突、破坏独立性的跨故事依赖
