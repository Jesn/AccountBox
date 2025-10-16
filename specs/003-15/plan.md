# Implementation Plan: 表格视图增强 - 紧凑显示、搜索与分页优化

**Branch**: `003-15` | **Date**: 2025-10-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-15/spec.md`

## Summary

本功能对网站列表和账号列表进行UI层面的增强优化，包括：(1) 网站列表紧凑显示和分页从10条增加到15条；(2) 网站列表添加实时搜索功能（域名/显示名称）；(3) 账号列表从卡片布局改为表格布局并支持分页；(4) 账号列表紧凑显示优化。技术方案为纯前端修改，使用现有 shadcn/ui Table 组件，前端实时过滤搜索，保持响应式设计。无需后端API修改或数据模型变更。

## Technical Context

**Language/Version**: TypeScript 5.9.3（已确认自 frontend/package.json）
**Primary Dependencies**: React 19, Vite 7, shadcn/ui, Tailwind CSS 4, axios 1.12（已存在）
**Storage**: N/A（前端展示层优化，数据来自现有 API）
**Testing**: N/A（本功能为 UI 优化，采用手动验收测试）
**Target Platform**: 现代浏览器（Chrome, Firefox, Safari, Edge 最新版），支持响应式布局
**Project Type**: Web 应用（frontend + backend，本功能仅涉及 frontend）
**Performance Goals**:
- 前端搜索过滤响应时间 < 200ms（500 个网站以内）
- 表格交互响应 < 100ms（按钮点击、密码显示切换）
- 页面渲染时间 < 2秒（100 个网站/账号配合分页）
**Constraints**:
- 必须保持现有 API 接口不变
- 必须复用现有对话框组件（CreateWebsiteDialog, EditWebsiteDialog 等）
- 必须保持现有分页组件 Pagination
- 必须保持响应式设计和无障碍性
**Scale/Scope**:
- 2 个组件修改（WebsiteList.tsx, AccountList.tsx）
- 2 个页面组件修改（WebsitesPage.tsx, AccountsPage.tsx）
- 预期支持显示 500+ 网站，100+ 账号（单个网站下）
- 影响范围：4 个文件修改，无新增组件

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Frontend Standards Check

✅ **框架与工具**:
- 使用 React 19 function components with Hooks ✓
- 使用 TypeScript strict mode ✓
- 使用 Vite 7 作为构建工具 ✓
- 使用 pnpm 作为包管理器 ✓

✅ **代码规范**:
- 组件定义：使用箭头函数语法 `const Component = () => {}` ✓
- Props：定义 TypeScript 接口 ✓
- 命名：PascalCase 组件名，camelCase 工具函数 ✓

✅ **UI 标准**:
- 使用 shadcn/ui 组件（Table 组件已存在于 002 功能）✓
- 使用 Tailwind CSS 样式 ✓

✅ **目录结构**:
- 组件位于 `src/components/` ✓
- 页面位于 `src/pages/` ✓

### Security & Quality Check

✅ **数据验证**: N/A（无新增数据输入，搜索为前端过滤）
✅ **异步操作**: 现有 API 调用已使用 async/await ✓
✅ **测试**: 采用手动验收测试（参考 quickstart.md）

### Git Standards Check

✅ **提交规范**:
- 必须遵循 Conventional Commits 格式 ✓
- 提交信息必须使用中文 ✓
- 每个任务完成后立即提交 ✓

### Result

**门控状态**: ✅ 通过

无需复杂度跟踪表 - 无章程违规项。

## Project Structure

### Documentation (this feature)

```
specs/003-15/
├── spec.md              # 功能规格说明（已完成）
├── plan.md              # 本文件 - 实施计划
├── research.md          # 阶段 0 - 技术研究（本功能将跳过，无需研究）
├── data-model.md        # 阶段 1 - 数据模型（本功能将跳过，无后端修改）
├── quickstart.md        # 阶段 1 - 快速启动指南
├── contracts/           # 阶段 1 - API 契约（本功能将跳过，无 API 变更）
├── tasks.md             # 阶段 2 - 任务列表（/speckit.tasks 命令生成）
└── checklists/
    └── requirements.md  # 规格说明质量检查清单（已完成）
```

### Source Code (repository root)

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/                    # shadcn/ui 基础组件
│   │   │   ├── button.tsx         # 现有按钮组件
│   │   │   ├── card.tsx           # 现有卡片组件
│   │   │   ├── input.tsx          # 现有输入框组件（搜索框使用）
│   │   │   └── table.tsx          # 已存在（002 功能添加）
│   │   ├── websites/              # 网站功能组件
│   │   │   ├── WebsiteList.tsx    # ⚠️ 修改 1：添加紧凑模式样式
│   │   │   ├── CreateWebsiteDialog.tsx   # 复用，无需修改
│   │   │   ├── EditWebsiteDialog.tsx     # 复用，无需修改
│   │   │   └── DeleteWebsiteDialog.tsx   # 复用，无需修改
│   │   ├── accounts/              # 账号功能组件
│   │   │   ├── AccountList.tsx    # ⚠️ 修改 2：从卡片布局改为表格布局
│   │   │   ├── CreateAccountDialog.tsx   # 复用，无需修改
│   │   │   ├── EditAccountDialog.tsx     # 复用，无需修改
│   │   │   └── DeleteAccountDialog.tsx   # 复用，无需修改
│   │   └── common/
│   │       └── Pagination.tsx     # 复用，无需修改
│   ├── pages/
│   │   ├── WebsitesPage.tsx       # ⚠️ 修改 3：添加搜索框、调整分页大小为15
│   │   └── AccountsPage.tsx       # ⚠️ 修改 4：添加分页功能、调整分页大小为15
│   └── services/
│       ├── websiteService.ts      # 无需修改，API 保持不变
│       └── accountService.ts      # 无需修改，API 保持不变
└── tests/                         # 无自动化测试（手动验收）

backend/                           # 本功能完全不涉及后端修改
```

**Structure Decision**:
本功能为纯前端 UI 优化，仅修改 4 个文件：
1. `frontend/src/components/websites/WebsiteList.tsx` - 添加紧凑模式 Tailwind 样式
2. `frontend/src/components/accounts/AccountList.tsx` - 从卡片布局完全重构为表格布局
3. `frontend/src/pages/WebsitesPage.tsx` - 添加搜索框状态、过滤逻辑、调整 pageSize 为 15
4. `frontend/src/pages/AccountsPage.tsx` - 添加分页功能、调整 pageSize 为 15

无需新增组件，无后端修改，无 API 变更。

## Phase 0: Research (跳过)

**决策理由**: 本功能无需研究阶段，因为：
1. 使用现有技术栈（React + TypeScript + shadcn/ui + Tailwind CSS），无新技术引入
2. Table 组件已在 002 功能中添加，使用方式已验证
3. 搜索功能采用前端实时过滤（JavaScript filter + toLowerCase），为标准实现
4. 紧凑模式使用 Tailwind CSS 缩小行高/内边距类名（如 `py-2` 替代 `py-4`），无需研究
5. 分页功能已存在，仅需调整 pageSize 参数

**跳过文件**: `research.md` 不创建

## Phase 1: Design & Contracts

### 数据模型（跳过）

**决策理由**: 本功能无后端修改，数据结构保持不变：
- 使用现有 `WebsiteResponse` 接口（来自 websiteService.ts）
- 使用现有 `AccountResponse` 接口（来自 accountService.ts）
- 无新增实体，无数据库变更

**跳过文件**: `data-model.md` 不创建

### API 契约（跳过）

**决策理由**: 本功能无 API 变更：
- 使用现有 `websiteService.getAll(pageNumber, pageSize)` 接口
- 使用现有 `accountService.getByWebsite(websiteId, pageNumber, pageSize)` 接口
- 响应格式 `PaginatedResponse<T>` 保持不变
- 搜索功能在前端实现，无需新增后端端点

**跳过目录**: `contracts/` 不创建

### 快速启动指南

**内容大纲**:

1. **本地开发环境验证**
   - 确认前端开发服务器运行（`cd frontend && pnpm dev`）
   - 确认后端 API 可访问（http://localhost:5093）
   - 确认至少有 20+ 网站数据用于测试

2. **User Story 1: 网站列表紧凑显示和分页优化**
   - 修改 `WebsiteList.tsx`：
     - 为 Table 组件添加紧凑模式类名：`className="text-sm"`
     - 为 TableHead 减小高度：`h-10`（替代默认 `h-12`）
     - 为 TableCell 减小内边距：`py-2 px-3`（替代默认 `p-4`）
   - 修改 `WebsitesPage.tsx`：
     - 调整 `pageSize` 从 `10` 改为 `15`
   - 验证：单屏显示 15 条网站，行高紧凑

3. **User Story 2: 网站列表快速搜索**
   - 修改 `WebsitesPage.tsx`：
     - 添加搜索状态：`const [searchQuery, setSearchQuery] = useState('')`
     - 添加搜索框 UI（使用 shadcn/ui Input 组件）
     - 添加前端过滤逻辑：
       ```typescript
       const filteredWebsites = websites.filter(w =>
         w.displayName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
         w.domain.toLowerCase().includes(searchQuery.toLowerCase())
       )
       ```
     - 根据过滤结果重新计算分页
   - 验证：输入"google"立即过滤结果，清空搜索框恢复所有网站

4. **User Story 3: 账号列表表格化显示**
   - 修改 `AccountList.tsx`：
     - 移除 Card 组件导入和使用
     - 导入 Table 组件
     - 实现表格结构（7 列：用户名、密码、标签、备注、创建时间、更新时间、操作）
     - 保持密码显示/隐藏逻辑（useState + 图标切换）
     - 保持复制功能（navigator.clipboard.writeText）
   - 修改 `AccountsPage.tsx`：
     - 添加分页状态：`const [currentPage, setCurrentPage] = useState(1)`
     - 添加 `totalPages` 状态
     - 调整 `pageSize` 为 `15`
     - 添加 Pagination 组件
   - 验证：账号以表格展示，密码可切换显示，分页正常

5. **User Story 4: 账号列表紧凑显示优化**
   - 修改 `AccountList.tsx`（在 User Story 3 基础上）：
     - 为 Table 组件添加紧凑模式类名：`text-sm`
     - 为 TableHead 减小高度：`h-10`
     - 为 TableCell 减小内边距：`py-2 px-3`
     - 备注列截断：`max-w-xs truncate` + `title` 属性
   - 验证：单屏显示更多账号，可读性良好

6. **验收场景清单**
   - [ ] **US1-1**: 25 个网站，第一页显示 15 个，行高紧凑
   - [ ] **US1-2**: 分页器显示"第1页，共2页"
   - [ ] **US1-3**: 鼠标悬停行有明显高亮
   - [ ] **US2-1**: 搜索"google"立即过滤出匹配网站
   - [ ] **US2-2**: 搜索不存在关键词显示空状态
   - [ ] **US2-3**: 清空搜索框恢复所有网站
   - [ ] **US2-4**: 搜索不区分大小写（"git"匹配"GitHub"）
   - [ ] **US2-5**: 搜索部分匹配（"exam"匹配"example.com"）
   - [ ] **US3-1**: 25 个账号，第一页显示 15 个表格行
   - [ ] **US3-2**: 点击"眼睛"图标切换密码显示/隐藏
   - [ ] **US3-3**: 点击"复制"图标复制密码到剪贴板
   - [ ] **US3-4**: 分页器切换到第二页显示剩余账号
   - [ ] **US3-5**: 长备注截断显示，悬停显示完整内容
   - [ ] **US3-6**: 无账号时显示空状态卡片
   - [ ] **US4-1**: 账号列表紧凑模式单屏显示增加 30%
   - [ ] **US4-2**: 紧凑模式行悬停有明显高亮
   - [ ] **US4-3**: 紧凑模式按钮易于点击，无误触

**输出文件**: `quickstart.md`（将在 Phase 1 创建）

### 代理上下文更新

**操作**: 运行 `.specify/scripts/bash/update-agent-context.sh claude`

**说明**: 本功能无新增技术依赖，仅使用现有技术栈，代理上下文无需更新。但按流程仍需运行脚本以验证一致性。

## Phase 2: Task Breakdown（由 /speckit.tasks 生成）

**任务预览**（最终由 `/speckit.tasks` 命令生成详细任务列表）:

**User Story 1 (P1) - 网站列表紧凑显示和分页优化**:
1. 修改 WebsiteList.tsx 添加紧凑模式 Tailwind 样式
2. 修改 WebsitesPage.tsx 调整 pageSize 为 15
3. 手动测试：验证紧凑显示和分页功能

**User Story 2 (P2) - 网站列表快速搜索**:
4. 在 WebsitesPage.tsx 添加搜索框 UI 和搜索状态
5. 实现前端过滤逻辑（域名/显示名称，不区分大小写）
6. 根据搜索结果重新计算分页
7. 添加搜索结果为空的空状态提示
8. 手动测试：验证搜索功能和边界情况

**User Story 3 (P3) - 账号列表表格化显示**:
9. 重构 AccountList.tsx 从卡片布局改为表格布局
10. 保持密码显示/隐藏和复制功能
11. 在 AccountsPage.tsx 添加分页功能
12. 调整 AccountsPage.tsx pageSize 为 15
13. 手动测试：验证表格展示和所有操作

**User Story 4 (P4) - 账号列表紧凑显示优化**:
14. 为 AccountList 表格添加紧凑模式样式
15. 实现备注列截断和悬停显示完整内容
16. 手动测试：验证紧凑模式可读性和可操作性

**Polish（完善与优化）**:
17. 代码审查和格式化
18. 更新文档（如需要）

**注意**: 本计划在 Phase 1 后结束，不生成 tasks.md。运行 `/speckit.tasks` 命令将基于本计划生成详细任务。

## Complexity Tracking

**不适用** - 无章程违规项，无需跟踪。

## Phase 1 Completion Summary

### 生成的设计产物

1. ✅ **quickstart.md** - 快速启动指南
   - 环境验证步骤
   - 4 个用户故事的实现指导
   - 视觉验证清单
   - 故障排除指南

2. ⏭️ **research.md** - 跳过（无需技术研究）

3. ⏭️ **data-model.md** - 跳过（无数据模型变更）

4. ⏭️ **contracts/** - 跳过（无 API 契约变更）

5. ✅ **CLAUDE.md 更新** - 代理上下文（无新增技术，验证一致性）

### 重新评估章程检查（Phase 1 后）

✅ **设计后验证 - 所有检查项通过**:

1. **Frontend Standards** - 完全符合
   - 使用 shadcn/ui Table 和 Input 组件（符合 UI 框架要求）
   - 使用 Tailwind CSS 紧凑模式类名（符合 CSS 框架要求）
   - TypeScript strict mode（已验证项目配置）
   - 组件使用箭头函数定义（quickstart 示例代码已遵循）

2. **Code Quality** - 完全符合
   - Props 接口已定义（WebsiteListProps, AccountListProps）
   - 组件目录结构正确（`src/components/`, `src/pages/`）
   - 无复杂逻辑，仅 UI 展示层优化和前端过滤

3. **Security & Testing** - N/A
   - 搜索功能无安全风险（前端过滤，无 SQL 注入等风险）
   - 无异步数据处理新增（复用现有 API）

4. **Git Commit Standards** - 已在 quickstart 中明确
   - 提供了中文提交信息示例
   - 遵循 Conventional Commits 格式（`feat[ui]:`）

**最终门控状态**: ✅ 通过 - 无章程违规，可以进入 Phase 2

## Next Steps

1. ✅ **Phase 0 完成** - 跳过（无需研究）
2. ✅ **Phase 1 完成** - 设计与契约已完成
   - quickstart.md 已生成
   - 代理上下文已验证
   - 章程检查重新验证通过
3. ⏸️ **Phase 2 待命** - 由 `/speckit.tasks` 命令触发

**运行以下命令继续**：
```bash
/speckit.tasks  # 生成详细任务列表并开始实施
```

**预期任务数量**: 15-18 个任务
- User Story 1: 3 个任务
- User Story 2: 5 个任务
- User Story 3: 5 个任务
- User Story 4: 3 个任务
- Polish: 2-3 个任务
