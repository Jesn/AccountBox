# Tasks: API密钥管理与外部API服务

**Input**: Design documents from `/specs/006-api-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api-specification.yaml

**Tests**: 测试任务在此功能中未被请求，因此不包含在任务列表中。

**Organization**: 任务按用户故事组织，以实现独立实施和测试。

## Format: `[ID] [P?] [Story] Description`
- **[P]**: 可并行运行（不同文件，无依赖关系）
- **[Story]**: 此任务属于哪个用户故事（如 US1、US2、US3）
- 任务描述包含精确的文件路径

## Path Conventions
- **Web app**: `backend/src/`, `frontend/src/`
- 后端：`backend/src/AccountBox.Core/`, `backend/src/AccountBox.Api/`
- 前端：`frontend/src/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 项目初始化和基础依赖配置

- [X] T001 安装 BCrypt.Net-Next 包到 `backend/src/AccountBox.Api/` 项目（用于API密钥哈希）
- [X] T002 [P] 在前端安装必要的依赖（如果需要新的shadcn/ui组件）

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 核心数据模型和基础设施，所有用户故事依赖于此阶段

**⚠️ CRITICAL**: 在此阶段完成之前，不能开始任何用户故事的工作

- [X] T003 [P] 创建 `AccountStatus` 枚举在 `backend/src/AccountBox.Core/Enums/AccountStatus.cs`
- [X] T004 [P] 创建 `ApiKeyScopeType` 枚举在 `backend/src/AccountBox.Core/Enums/ApiKeyScopeType.cs`
- [X] T005 扩展 `Account` 实体，添加 `Status` 和 `ExtendedData` 字段在 `backend/src/AccountBox.Core/Models/Account.cs`
- [X] T006 [P] 创建 `ApiKey` 实体在 `backend/src/AccountBox.Core/Models/ApiKey.cs`
- [X] T007 [P] 创建 `ApiKeyWebsiteScope` 实体在 `backend/src/AccountBox.Core/Models/ApiKeyWebsiteScope.cs`
- [X] T008 更新 `ApplicationDbContext`，添加 `DbSet<ApiKey>` 和 `DbSet<ApiKeyWebsiteScope>`，配置实体关系在 `backend/src/AccountBox.Data/ApplicationDbContext.cs`
- [X] T009 创建 EF Core 迁移，添加 `ApiKeys` 和 `ApiKeyWebsiteScopes` 表
- [X] T010 创建 EF Core 迁移，为 `Accounts` 表添加 `Status` 和 `ExtendedData` 列
- [X] T011 应用数据库迁移到开发数据库

**Checkpoint**: 数据模型基础就绪 - 用户故事实现现在可以并行开始

---

## Phase 3: User Story 1 - API密钥管理 (Priority: P1) 🎯 MVP

**Goal**: 用户可以在Web UI中创建、查看、删除API密钥，支持作用域控制（所有网站 vs 指定网站）

**Independent Test**: 用户可以独立测试API密钥的创建、查看明文、复制、删除功能。验证作用域选择（全部网站/指定网站）工作正常。

### Backend Implementation for User Story 1

- [X] T012 [P] [US1] 创建 `IApiKeyService` 接口在 `backend/src/AccountBox.Core/Services/IApiKeyService.cs`（定义密钥生成、验证、作用域检查方法）
- [X] T013 [P] [US1] 创建 `ApiKeyService` 实现在 `backend/src/AccountBox.Api/Services/ApiKeyService.cs`（实现密钥生成、BCrypt哈希、作用域验证）
- [X] T014 [US1] 在 `Program.cs` 中注册 `IApiKeyService` 为 scoped 服务
- [X] T015 [P] [US1] 创建 `ApiKeyDto` 在 `backend/src/AccountBox.Api/DTOs/ApiKeyDto.cs`
- [X] T016 [P] [US1] 创建 `CreateApiKeyRequest` DTO 在 `backend/src/AccountBox.Api/DTOs/CreateApiKeyRequest.cs`
- [X] T017 [US1] 创建 `ApiKeysController` 在 `backend/src/AccountBox.Api/Controllers/ApiKeysController.cs`，实现以下端点：
  - `GET /api/api-keys` - 获取密钥列表
  - `POST /api/api-keys` - 创建密钥
  - `DELETE /api/api-keys/{id}` - 删除密钥
- [X] T018 [US1] 为 `ApiKeysController` 添加输入验证和错误处理

### Frontend Implementation for User Story 1

- [X] T019 [P] [US1] 创建 `ApiKey` 类型定义在 `frontend/src/types/ApiKey.ts`
- [X] T020 [P] [US1] 创建 `apiKeyService.ts` 在 `frontend/src/services/apiKeyService.ts`（封装API调用）
- [X] T021 [P] [US1] 创建 `CreateApiKeyDialog` 组件在 `frontend/src/components/api-keys/CreateApiKeyDialog.tsx`（包含名称输入、作用域选择、网站多选）
- [X] T022 [P] [US1] 创建 `ApiKeyList` 组件在 `frontend/src/components/api-keys/ApiKeyList.tsx`（显示密钥列表，支持显示/隐藏明文、复制按钮）
- [X] T023 [P] [US1] 创建 `DeleteApiKeyDialog` 组件在 `frontend/src/components/api-keys/DeleteApiKeyDialog.tsx`
- [X] T024 [US1] 创建 `ApiKeysPage` 在 `frontend/src/pages/ApiKeysPage.tsx`，整合上述组件
- [ ] T025 [US1] 在 `App.tsx` 中添加 `/api-keys` 路由，指向 `ApiKeysPage`

**Checkpoint**: 用户故事1应该完全功能化并可独立测试。用户可以创建、查看、复制、删除API密钥。

---

## Phase 4: User Story 2 - 账号启用/禁用功能 (Priority: P2)

**Goal**: 用户可以在账号列表中启用或禁用账号，禁用账号仍然显示但有视觉标识

**Independent Test**: 在账号列表页面，用户可以独立测试账号的禁用和启用操作。验证：(1) 禁用后账号状态变为"已禁用"，在列表中显示但有视觉区分；(2) 已禁用账号仍可查看、编辑；(3) 启用后账号恢复活跃状态；(4) 禁用的账号也可以被删除。

### Backend Implementation for User Story 2

- [ ] T026 [US2] 扩展 `AccountService`（或创建新的服务方法），添加 `EnableAccountAsync` 和 `DisableAccountAsync` 方法在 `backend/src/AccountBox.Api/Services/AccountService.cs`
- [ ] T027 [US2] 在 `AccountsController` 中添加启用/禁用端点：
  - `PUT /api/accounts/{id}/enable` - 启用账号
  - `PUT /api/accounts/{id}/disable` - 禁用账号
- [ ] T028 [US2] 更新 `GET /api/websites/{id}/accounts` 端点，确保返回所有账号（活跃+禁用），包含 `status` 字段

### Frontend Implementation for User Story 2

- [ ] T029 [US2] 更新 `Account` 类型定义，添加 `status` 字段在 `frontend/src/types/Account.ts`
- [ ] T030 [P] [US2] 创建 `AccountStatusBadge` 组件在 `frontend/src/components/accounts/AccountStatusBadge.tsx`（显示状态标识，如"活跃"/"已禁用"）
- [ ] T031 [US2] 更新 `accountService.ts`，添加 `enableAccount` 和 `disableAccount` 方法在 `frontend/src/services/accountService.ts`
- [ ] T032 [US2] 更新账号列表组件（如 `AccountListPage` 或相关组件），集成 `AccountStatusBadge`，添加启用/禁用按钮
- [ ] T033 [US2] 为已禁用账号添加视觉样式（如灰色背景、禁用图标），在 `frontend/src/index.css` 或组件样式中
- [ ] T034 [US2] 更新网站详情页面，分别统计并显示活跃账号数量和禁用账号数量

**Checkpoint**: 用户故事1和用户故事2应该都可以独立工作。用户可以禁用/启用账号，并在UI中看到清晰的状态标识。

---

## Phase 5: User Story 3 - 账号扩展字段管理 (Priority: P3)

**Goal**: 用户可以为账号添加、编辑、删除自定义扩展字段（JSON键值对）

**Independent Test**: 在账号编辑页面，用户可以独立测试扩展字段的添加、编辑、删除。验证：(1) 可以添加新的键值对；(2) 可以编辑已有的键值对；(3) 可以删除键值对；(4) 扩展字段以JSON格式存储。

### Backend Implementation for User Story 3

- [ ] T035 [US3] 创建自定义JSON验证属性 `JsonValidationAttribute` 在 `backend/src/AccountBox.Api/Validation/JsonValidationAttribute.cs`（验证JSON格式和10KB大小限制）
- [ ] T036 [US3] 更新 `AccountDto` 和相关DTO，包含 `extendedData` 字段（类型为 `Dictionary<string, object>` 或 `JsonDocument`）
- [ ] T037 [US3] 确保 `AccountsController` 的创建和更新端点支持读写 `extendedData` 字段
- [ ] T038 [US3] 添加扩展字段大小验证（≤10KB）在服务层或控制器层

### Frontend Implementation for User Story 3

- [ ] T039 [US3] 更新 `Account` 类型定义，添加 `extendedData` 字段在 `frontend/src/types/Account.ts`
- [ ] T040 [P] [US3] 创建 `ExtendedFieldsEditor` 组件在 `frontend/src/components/accounts/ExtendedFieldsEditor.tsx`（键值对编辑器，支持添加、编辑、删除键值对）
- [ ] T041 [US3] 集成 `ExtendedFieldsEditor` 到账号创建对话框 `CreateAccountDialog.tsx`
- [ ] T042 [US3] 集成 `ExtendedFieldsEditor` 到账号编辑对话框 `EditAccountDialog.tsx`
- [ ] T043 [US3] 添加前端验证，限制扩展字段总大小≤10KB，提示用户超出限制

**Checkpoint**: 所有三个用户故事（US1、US2、US3）应该都可以独立工作。用户可以管理API密钥、启用/禁用账号、添加扩展字段。

---

## Phase 6: User Story 4 - 外部API：账号CRUD操作 (Priority: P4)

**Goal**: 外部系统可以通过RESTful API进行账号的创建、启用/禁用、删除、查询操作（需API密钥认证）

**Independent Test**: 使用curl或Postman，携带有效的API密钥调用各个API接口。验证：(1) 创建账号成功并返回账号详情；(2) 启用/禁用账号成功；(3) 删除账号成功并移入回收站；(4) 获取账号列表成功并正确过滤已禁用账号。

### Backend Implementation for User Story 4

- [ ] T044 [P] [US4] 创建 `ApiKeyAuthMiddleware` 在 `backend/src/AccountBox.Api/Middleware/ApiKeyAuthMiddleware.cs`（从Header提取密钥、验证有效性、存入HttpContext）
- [ ] T045 [US4] 在 `Program.cs` 中注册 `ApiKeyAuthMiddleware`，仅应用于 `/api/external/*` 路径
- [ ] T046 [P] [US4] 创建 `CreateAccountRequest` DTO（外部API版本），包含扩展字段在 `backend/src/AccountBox.Api/DTOs/External/CreateAccountRequest.cs`
- [ ] T047 [P] [US4] 创建 `UpdateAccountStatusRequest` DTO 在 `backend/src/AccountBox.Api/DTOs/External/UpdateAccountStatusRequest.cs`
- [ ] T048 [US4] 创建 `ExternalApiController` 在 `backend/src/AccountBox.Api/Controllers/ExternalApiController.cs`，实现以下端点：
  - `POST /api/external/accounts` - 创建账号
  - `DELETE /api/external/accounts/{id}` - 删除账号（移入回收站）
  - `PUT /api/external/accounts/{id}/status` - 启用/禁用账号
  - `GET /api/external/websites/{websiteId}/accounts` - 获取账号列表（支持status参数过滤）
- [ ] T049 [US4] 在 `ExternalApiController` 中添加作用域验证逻辑（检查API密钥是否有权访问目标网站）
- [ ] T050 [US4] 为外部API端点添加错误处理（401、403、404、400等标准HTTP状态码）
- [ ] T051 [US4] 添加密码非空验证，但不进行密码强度检查（符合FR-025）

**Checkpoint**: 外部系统可以使用API密钥进行账号的CRUD操作。验证作用域控制工作正常（403错误）。

---

## Phase 7: User Story 5 - 外部API：随机获取启用账号 (Priority: P5)

**Goal**: 外部系统可以通过API接口随机获取某个网站下的一个启用状态的账号

**Independent Test**: 使用curl或Postman，携带有效的API密钥多次调用随机获取接口。验证：(1) 每次调用返回不同的账号（在有多个账号时）；(2) 仅返回启用状态的账号；(3) 当无启用账号时返回404错误。

### Backend Implementation for User Story 5

- [ ] T052 [P] [US5] 创建 `RandomAccountService` 在 `backend/src/AccountBox.Api/Services/RandomAccountService.cs`（实现随机选择逻辑，使用 `EF.Functions.Random()`）
- [ ] T053 [US5] 在 `Program.cs` 中注册 `RandomAccountService` 为 scoped 服务
- [ ] T054 [US5] 在 `ExternalApiController` 中添加端点：`GET /api/external/websites/{websiteId}/accounts/random` - 随机获取启用账号
- [ ] T055 [US5] 添加错误处理：当网站无启用账号时返回404，提示"该网站没有可用的启用账号"
- [ ] T056 [US5] 添加作用域验证（检查API密钥是否有权访问该网站）

**Checkpoint**: 所有五个用户故事应该都可以独立工作。外部系统可以随机获取启用账号，适用于爬虫轮询等场景。

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 跨多个用户故事的改进和完善

- [ ] T057 [P] 更新 `ExceptionMiddleware`，确保所有API错误返回统一的 `ApiResponse<T>` 格式
- [ ] T058 [P] 添加API调用日志记录（记录API密钥ID、操作类型、目标资源、时间戳）在 `ApiKeyAuthMiddleware` 中
- [ ] T059 [P] 更新API密钥的 `LastUsedAt` 时间戳，在每次API调用时更新
- [ ] T060 [P] 代码格式化和清理（使用 dotnet format 和 Prettier）
- [ ] T061 [P] 运行 `quickstart.md` 中的所有示例，验证功能完整性
- [ ] T062 添加必要的shadcn/ui组件（如Badge、Slider等），如果之前未安装
- [ ] T063 更新 CLAUDE.md，添加本功能的技术栈和命令信息

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖 - 可以立即开始
- **Foundational (Phase 2)**: 依赖于Setup完成 - 阻塞所有用户故事
- **User Stories (Phase 3-7)**: 所有用户故事依赖于Foundational阶段完成
  - 用户故事之间可以并行进行（如果有足够人力）
  - 或按优先级顺序进行（P1 → P2 → P3 → P4 → P5）
- **Polish (Phase 8)**: 依赖于所有期望的用户故事完成

### User Story Dependencies

- **User Story 1 (P1)**: Foundational阶段后可以开始 - 无其他故事依赖
- **User Story 2 (P2)**: Foundational阶段后可以开始 - 独立于US1
- **User Story 3 (P3)**: Foundational阶段后可以开始 - 独立于US1和US2
- **User Story 4 (P4)**: Foundational阶段后可以开始 - 依赖于US1（API密钥）和US3（扩展字段），但可以并行开发然后集成
- **User Story 5 (P5)**: Foundational阶段后可以开始 - 依赖于US1（API密钥）和US2（状态字段），但可以并行开发然后集成

### Within Each User Story

- Backend优先于Frontend（确保API端点可用）
- 同一故事内标记[P]的任务可以并行
- 不同文件的任务可以并行，同一文件的任务必须顺序执行
- 每个故事完成后验证独立功能

### Parallel Opportunities

- Phase 1中所有标记[P]的任务可以并行
- Phase 2中所有标记[P]的任务可以并行（在阶段内）
- Foundational阶段完成后，所有用户故事可以并行开始（如果团队容量允许）
- 每个用户故事内标记[P]的任务可以并行
- 不同用户故事可以由不同团队成员并行工作

---

## Parallel Example: User Story 1 Backend

```bash
# 同时启动User Story 1的多个后端任务：
Task: "创建 IApiKeyService 接口"
Task: "创建 ApiKeyService 实现"
Task: "创建 ApiKeyDto"
Task: "创建 CreateApiKeyRequest DTO"

# 完成后顺序执行：
Task: "在 Program.cs 中注册 IApiKeyService"
Task: "创建 ApiKeysController"
Task: "添加输入验证和错误处理"
```

## Parallel Example: User Story 1 Frontend

```bash
# 同时启动User Story 1的多个前端任务：
Task: "创建 ApiKey 类型定义"
Task: "创建 apiKeyService.ts"
Task: "创建 CreateApiKeyDialog 组件"
Task: "创建 ApiKeyList 组件"
Task: "创建 DeleteApiKeyDialog 组件"

# 完成后顺序执行：
Task: "创建 ApiKeysPage"
Task: "在 App.tsx 中添加路由"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational（关键 - 阻塞所有故事）
3. 完成 Phase 3: User Story 1
4. **停止并验证**: 独立测试User Story 1
5. 准备好后部署/演示

### Incremental Delivery

1. 完成 Setup + Foundational → 基础就绪
2. 添加 User Story 1 → 独立测试 → 部署/演示（MVP！）
3. 添加 User Story 2 → 独立测试 → 部署/演示
4. 添加 User Story 3 → 独立测试 → 部署/演示
5. 添加 User Story 4 → 独立测试 → 部署/演示
6. 添加 User Story 5 → 独立测试 → 部署/演示
7. 每个故事添加价值而不破坏之前的故事

### Parallel Team Strategy

多个开发者场景：

1. 团队一起完成 Setup + Foundational
2. Foundational完成后：
   - 开发者 A: User Story 1（API密钥管理）
   - 开发者 B: User Story 2（账号状态管理）
   - 开发者 C: User Story 3（扩展字段）
   - 开发者 D: User Story 4（外部API CRUD）- 依赖A和C
   - 开发者 E: User Story 5（随机获取）- 依赖A和B
3. 故事独立完成并集成

---

## Notes

- [P] 任务 = 不同文件，无依赖关系
- [Story] 标签将任务映射到特定用户故事，便于追踪
- 每个用户故事应该可以独立完成和测试
- 每个任务或逻辑组完成后提交
- 在任何检查点停止以独立验证故事
- 避免：模糊任务、同文件冲突、破坏独立性的跨故事依赖

---

## Task Summary

**Total Tasks**: 63
- Phase 1 (Setup): 2 tasks
- Phase 2 (Foundational): 9 tasks
- Phase 3 (User Story 1): 14 tasks (Backend: 7, Frontend: 7)
- Phase 4 (User Story 2): 9 tasks (Backend: 3, Frontend: 6)
- Phase 5 (User Story 3): 9 tasks (Backend: 4, Frontend: 5)
- Phase 6 (User Story 4): 8 tasks (Backend: 8)
- Phase 7 (User Story 5): 5 tasks (Backend: 5)
- Phase 8 (Polish): 7 tasks

**Parallel Opportunities**: 25+ tasks marked [P] can run in parallel within their phases

**MVP Scope**: User Story 1 (API密钥管理) - 14 tasks after foundational setup

**Estimated Timeline**:
- MVP (US1): ~2-3 days
- Full Feature (US1-US5): ~7-10 days
- With parallel team: ~4-5 days
