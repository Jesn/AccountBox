---
description: "网站列表表格视图 - 可执行任务列表"
---

# Tasks: 网站列表表格视图

**Input**: Design documents from `/specs/002-http-localhost-5173/`
**Prerequisites**: plan.md ✅, spec.md ✅, quickstart.md ✅

**Tests**: 本功能采用手动验收测试，无自动化测试任务

**Organization**: 任务按用户故事组织，每个故事独立可测试

## Format: `[ID] [P?] [Story] Description`
- **[P]**: 可并行运行（不同文件，无依赖）
- **[Story]**: 任务所属用户故事（US1, US2, US3）
- 包含准确的文件路径

## Path Conventions
- **Web app**: `frontend/src/` - 本功能仅涉及前端
- 所有路径相对于仓库根目录

---

## Phase 1: Setup（环境准备）

**Purpose**: 项目环境验证和必需组件安装

### Tasks

- [ ] **T001** [Setup] 验证开发环境就绪
  - 验证 Node.js >= 20.19.0
  - 验证 pnpm 已安装
  - 验证前端开发服务器可启动：`cd frontend && pnpm dev`
  - 验证后端 API 可访问：http://localhost:5093
  - 参考：quickstart.md "Prerequisites" 章节

- [x] **T002** [Setup] 添加 shadcn/ui Table 组件
  - 运行命令：`cd frontend && npx shadcn@latest add table`
  - 验证组件文件已创建：`frontend/src/components/ui/table.tsx`
  - 验证导出包含：Table, TableHeader, TableBody, TableRow, TableHead, TableCell
  - 提交信息：`chore[ui]: 添加 shadcn/ui Table 组件`

**Checkpoint**: 环境就绪，Table 组件可用

---

## Phase 2: Foundational（基础前提）

**Purpose**: 无需基础任务 - 复用现有基础设施

**⚠️ 说明**: 本功能纯 UI 重构，以下组件已存在且无需修改：
- ✅ WebsiteResponse 接口（来自 websiteService.ts）
- ✅ websiteService.getAll() API
- ✅ Pagination 组件
- ✅ CreateWebsiteDialog, EditWebsiteDialog, DeleteWebsiteDialog

**Checkpoint**: 基础设施已就绪（无需额外工作）

---

## Phase 3: User Story 1 - 查看大量网站的表格视图 (Priority: P1) 🎯 MVP

**Goal**: 将网站列表从卡片布局改为表格布局，包含列标题和数据行，提升信息密度

**Independent Test**: 添加 20+ 个网站后访问 http://localhost:5173/websites，验证表格视图显示所有关键信息（显示名、域名、标签、账号数量），且信息对齐清晰

### Implementation for User Story 1

- [x] **T003** [US1] 重构 WebsiteList 组件为表格布局
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`
  - 导入 Table 组件：`import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'`
  - 移除 Card 组件导入和使用
  - 保持 Props 接口 `WebsiteListProps` 不变
  - 实现表格结构：
    - TableHeader 包含 6 列：显示名、域名、标签、活跃账号、回收站、操作
    - TableBody 使用 `websites.map()` 渲染数据行
    - 每行显示：website.displayName（或 domain 备用）、domain、tags、activeAccountCount、deletedAccountCount、操作按钮
  - 保持加载状态处理（显示 Card + "加载中..."）
  - 保持空状态处理（显示 Card + "添加第一个网站"按钮）
  - 参考：quickstart.md "Step 2" 的示例代码
  - 提交信息：`feat[ui]: 重构 WebsiteList 组件为表格布局

- 替换 Card 布局为 Table 布局
- 包含 6 列：显示名、域名、标签、活跃账号、回收站、操作
- 保持加载状态和空状态处理
- 保持 Props 接口不变

完成 T003 - User Story 1 核心实现`

- [x] **T004** [US1] 添加表格响应式布局支持
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`（继续修改）
  - 为表格容器添加：`<div className="rounded-md border overflow-x-auto">`
  - 为标签列添加响应式类：`className="hidden md:table-cell"`（TableHead 和 TableCell）
  - 确保小屏幕下（< 768px）标签列隐藏
  - 确保表格支持横向滚动
  - 参考：quickstart.md "Step 4 - 测试响应式布局"
  - 提交信息：`feat[ui]: 添加网站列表表格的响应式布局支持

- 标签列在小屏幕下隐藏（hidden md:table-cell）
- 表格容器支持横向滚动（overflow-x-auto）
- 确保信息在各尺寸屏幕下清晰可读

完成 T004 - User Story 1 响应式支持`

- [x] **T005** [US1] 处理表格内容边界情况
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`（继续修改）
  - 长文本截断：为显示名和域名列添加：`className="max-w-xs truncate"`
  - 标签显示：如 tags 为空显示 "-"，如多标签考虑显示前几个或折叠
  - 回收站账号数：始终显示数字（包括 0）
  - 鼠标悬停提示：为截断内容添加 `title` 属性显示完整文本
  - 参考：spec.md "Edge Cases" 章节
  - 提交信息：`feat[ui]: 处理网站列表表格的边界情况

- 长文本截断显示（max-w-xs truncate）
- 标签为空显示 "-"
- 回收站账号数始终显示
- 悬停显示完整内容（title 属性）

完成 T005 - User Story 1 边界情况处理`

### Manual Testing for User Story 1

- [ ] **T006** [US1] 手动验收：表格显示功能
  - 访问 http://localhost:5173/websites（需有 10+ 网站数据）
  - ✅ 验证表格列标题显示：显示名、域名、标签、活跃账号、回收站、操作
  - ✅ 验证数据行信息对齐且易读
  - ✅ 验证显示密度提升：同屏显示的网站数量至少是原卡片视图的 2 倍
  - ✅ 验证长文本截断（添加长显示名或长域名测试）
  - ✅ 验证标签为空时显示 "-"
  - ✅ 验证回收站为 0 时显示 0
  - 参考：quickstart.md "验收场景清单" - User Story 1 部分

- [ ] **T007** [US1] 手动验收：响应式布局
  - 在 Chrome DevTools 中切换设备视图测试
  - ✅ 桌面 (1920x1080)：所有列都显示
  - ✅ 平板 (768x1024)：标签列隐藏，其他列显示
  - ✅ 手机 (375x667)：支持横向滚动，表格不破坏布局
  - ✅ 悬停长文本时显示完整内容（title 提示）
  - 参考：quickstart.md "Step 4 - 测试响应式布局"

**Checkpoint**: User Story 1 完成 - 表格视图功能完整，响应式布局正常，边界情况处理妥当

---

## Phase 4: User Story 2 - 在表格中执行网站操作 (Priority: P2)

**Goal**: 在表格的每一行添加操作按钮（查看账号、编辑、删除），保持现有功能可用性

**Independent Test**: 在表格视图中点击任意网站行的"查看账号"、"编辑"、"删除"按钮，验证相应的对话框打开且功能正常

### Implementation for User Story 2

- [x] **T008** [US2] 在表格操作列添加三个操作按钮
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`（已在 T003 中实现）
  - 在最后一列（操作列）添加按钮容器：`<div className="flex gap-2 justify-end">`
  - 添加三个 Button 组件：
    1. "查看账号"：`onClick={() => onViewAccounts(website.id)}`，variant="outline"
    2. "编辑"：`onClick={() => onEdit(website)}`，variant="outline"
    3. "删除"：`onClick={() => onDelete(website)}`，variant="destructive"
  - 所有按钮设置 `size="sm"` 保持紧凑
  - 确保回调函数参数正确（ID vs 整个对象）
  - 参考：quickstart.md "Step 2" 示例代码 - 操作列部分
  - 提交信息：已在 T003 中实现

- 添加查看账号、编辑、删除三个按钮
- 正确传递回调参数（ID 或对象）
- 按钮样式紧凑（size="sm"）
- 按钮布局右对齐（justify-end）

完成 T008 - User Story 2 操作按钮`

- [x] **T009** [US2] 优化操作按钮的视觉反馈
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`（已在 T003 中实现）
  - 验证 Button 组件已有 hover 状态（shadcn/ui 默认提供）✓
  - 确保按钮间距合理（gap-2）✓
  - 测试按钮点击响应时间 < 100ms（需要手动测试）
  - 提交信息：shadcn/ui Button 组件已提供良好的视觉反馈

- 验证 hover 状态明显
- 确保按钮间距合理
- 测试点击响应性能

完成 T009 - User Story 2 视觉反馈`

### Manual Testing for User Story 2

- [ ] **T010** [US2] 手动验收：操作按钮功能
  - 访问 http://localhost:5173/websites（需有网站数据）
  - ✅ 点击"查看账号"按钮，验证导航到 `/websites/{id}/accounts`
  - ✅ 点击"编辑"按钮，验证打开 EditWebsiteDialog 并预填充数据
  - ✅ 点击"删除"按钮，验证打开 DeleteWebsiteDialog
  - ✅ 验证所有对话框功能正常（保存、取消、确认删除）
  - ✅ 验证操作后表格正确刷新（编辑后、删除后）
  - 参考：quickstart.md "验收场景清单" - User Story 2 部分

- [ ] **T011** [US2] 手动验收：操作按钮视觉反馈
  - 鼠标悬停在每个操作按钮上
  - ✅ 验证 hover 状态有明显视觉变化（颜色、阴影）
  - ✅ 验证点击响应时间 < 100ms（使用 Chrome DevTools Performance）
  - ✅ 验证按钮间距合理，不会误点
  - 参考：spec.md - User Story 2 验收场景 4

**Checkpoint**: User Story 2 完成 - 表格操作按钮功能完整，视觉反馈良好

---

## Phase 5: User Story 3 - 空状态和加载状态的友好提示 (Priority: P3)

**Goal**: 当网站列表为空或正在加载时，显示友好的提示信息

**Independent Test**: 清空所有网站后访问页面，验证显示"添加第一个网站"的空状态提示；刷新页面时验证显示加载中状态

### Implementation for User Story 3

**⚠️ 说明**: User Story 3 的功能在 T003 中已经实现（保持了现有的加载状态和空状态处理），本阶段仅需验证和微调。

- [x] **T012** [US3] 验证空状态和加载状态展示正确
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`（已在 T003 中实现）
  - 验证加载状态：`if (isLoading)` 返回 Card + "加载中..." 提示 ✓
  - 验证空状态：`if (websites.length === 0)` 返回 Card + "开始使用 AccountBox" + "添加第一个网站"按钮 ✓
  - 验证空状态按钮点击调用 `onCreateNew()` ✓
  - Card 组件保留用于空状态和加载状态 ✓
  - 提交信息：空状态和加载状态已在 T003 中保留

- 确认加载状态提示清晰
- 确认空状态提示友好且有明确行动号召
- 确认按钮回调正确

完成 T012 - User Story 3 状态验证`

### Manual Testing for User Story 3

- [ ] **T013** [US3] 手动验收：空状态和加载状态
  - **空状态测试**：
    - 删除所有网站（或使用空数据库）
    - 访问 http://localhost:5173/websites
    - ✅ 验证显示 Card 卡片
    - ✅ 验证显示标题"开始使用 AccountBox"
    - ✅ 验证显示提示文本"还没有添加任何网站"
    - ✅ 验证显示"添加第一个网站"按钮
    - ✅ 点击按钮，验证打开 CreateWebsiteDialog
  - **加载状态测试**：
    - 刷新页面（F5）或清除缓存后首次访问
    - ✅ 验证短暂显示"加载中..."提示
    - ✅ 验证加载完成后正确显示表格或空状态
  - 参考：quickstart.md "验收场景清单" - User Story 3 部分

**Checkpoint**: User Story 3 完成 - 空状态和加载状态友好且功能正常

---

## Phase 6: Polish & Cross-Cutting Concerns（完善与横切关注点）

**Purpose**: 跨用户故事的改进和最终验收

### Tasks

- [ ] **T014** [Polish] 调整 WebsitesPage 容器样式（可选）
  - 文件：`frontend/src/pages/WebsitesPage.tsx`
  - 检查表格在页面中的显示效果
  - 如表格过宽，考虑增加最大宽度：`max-w-7xl`（从 `max-w-6xl`）
  - 如需更紧凑布局，考虑减少内边距：`p-4`（从 `p-8`）
  - 验证页面标题、按钮组（修改主密码、锁定、添加网站）布局无问题
  - 验证分页器在表格底部显示正常
  - 参考：quickstart.md "Step 3"
  - 提交信息：`style[ui]: 调整网站管理页面容器样式以适配表格视图

- 优化表格在页面中的显示
- 确保分页器位置合理

完成 T014 - 页面样式调整`

- [ ] **T015** [P] [Polish] 性能验证
  - 使用 Chrome DevTools Performance 面板
  - 测试场景：加载包含 100 个网站的页面（配合分页每页 10 个）
  - ✅ 验证页面渲染时间 < 2 秒（SC-004）
  - ✅ 验证操作按钮点击响应 < 100ms
  - ✅ 验证表格滚动流畅（60fps）
  - 如性能不达标，考虑优化：
    - 减少不必要的 re-render
    - 使用 React.memo 包裹子组件
    - 优化 CSS 类名使用
  - 参考：quickstart.md "Performance Validation" 章节

- [ ] **T016** [P] [Polish] 运行 quickstart.md 完整验收清单
  - 按照 quickstart.md "Step 5: 验收场景清单" 逐项验证
  - 确保所有验收场景通过：
    - ✅ User Story 1 的 3 个验收场景
    - ✅ User Story 2 的 4 个验收场景
    - ✅ User Story 3 的 3 个验收场景
    - ✅ 4 个边界情况测试
  - 记录任何问题并修复
  - 参考：quickstart.md "验收场景清单"

- [ ] **T017** [Polish] 最终代码审查和提交
  - 检查代码质量：
    - ✅ 无 console.log 或调试代码
    - ✅ 无未使用的 import
    - ✅ 遵循 TypeScript strict mode
    - ✅ 遵循 React Hooks 最佳实践
    - ✅ 遵循 Tailwind CSS 类名约定
  - 运行 linter：`cd frontend && pnpm lint`
  - 运行格式化：`cd frontend && pnpm format`
  - 修复所有 linter 错误和警告
  - 提交信息：`chore[ui]: 代码审查和格式化 - 网站列表表格视图

- 修复所有 linter 警告
- 运行代码格式化
- 清理调试代码

完成 T017 - 最终审查`

- [ ] **T018** [Polish] 更新文档（如需要）
  - 如果表格组件有特殊使用模式或最佳实践，更新 `CLAUDE.md`
  - 示例内容：
    - "使用 shadcn/ui Table 组件时，注意为响应式布局添加 `hidden md:table-cell`"
    - "长文本截断使用 `max-w-xs truncate` + `title` 属性"
  - 如无特殊内容，跳过此任务
  - 提交信息：`docs: 更新 CLAUDE.md 记录表格组件使用模式`

**Checkpoint**: 所有用户故事完成，功能完整，代码质量达标

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖 - 立即开始
  - T001: 环境验证
  - T002: 添加 Table 组件

- **Foundational (Phase 2)**: 依赖 Setup 完成 - **无任务（复用现有基础设施）**

- **User Stories (Phase 3-5)**: 依赖 Setup 完成后可立即开始
  - **User Story 1 (Phase 3)**: T003 → T004 → T005 → T006 → T007（手动测试）
  - **User Story 2 (Phase 4)**: 依赖 T003 完成 → T008 → T009 → T010 → T011（手动测试）
  - **User Story 3 (Phase 5)**: 依赖 T003 完成 → T012 → T013（手动测试）

- **Polish (Phase 6)**: 依赖所有用户故事完成
  - T014, T015, T016, T017, T018 可部分并行

### User Story Dependencies

- **User Story 1 (P1) - 表格视图核心**:
  - 可在 Setup 完成后立即开始
  - 无其他依赖
  - **必须完成后才能进行 US2 和 US3**

- **User Story 2 (P2) - 操作按钮**:
  - 依赖 US1 的 T003（表格结构必须存在）
  - 在 US1 完成 T003 后可开始

- **User Story 3 (P3) - 状态提示**:
  - 依赖 US1 的 T003（状态处理逻辑在 T003 中已实现）
  - 在 US1 完成 T003 后可验证

### Within Each User Story

- **User Story 1 内部顺序**:
  - T003（表格结构）→ T004（响应式）→ T005（边界情况）→ T006, T007（手动测试并行）

- **User Story 2 内部顺序**:
  - T008（添加按钮）→ T009（视觉反馈）→ T010, T011（手动测试并行）

- **User Story 3 内部顺序**:
  - T012（验证状态）→ T013（手动测试）

### Parallel Opportunities

- **Setup 阶段**:
  - T001 和 T002 **必须顺序执行**（T002 依赖 T001 的环境验证）

- **User Story 完成后的手动测试**:
  - T006 和 T007 可并行（不同测试场景）
  - T010 和 T011 可并行（不同测试场景）

- **Polish 阶段**:
  - T015（性能验证）和 T016（验收清单）可并行
  - T014 可在 T015/T016 之前或并行

- **跨团队并行（如有多人）**:
  - Setup 完成后，无法真正并行，因为 US2 和 US3 都依赖 US1 的 T003
  - 建议单人顺序执行：US1 → US2 → US3

---

## Parallel Example: Phase 6 Polish Tasks

```bash
# 在 Polish 阶段，以下任务可以并行执行（不同关注点）：

Task: "性能验证（Chrome DevTools Performance 测试）"
Task: "运行 quickstart.md 完整验收清单（功能验证）"

# 完成后再执行：
Task: "最终代码审查和提交"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only) ⭐ 推荐

1. ✅ Complete Phase 1: Setup（T001, T002）
2. ⏭️ Skip Phase 2: Foundational（无需任务）
3. ✅ Complete Phase 3: User Story 1（T003-T007）
4. **STOP and VALIDATE**: 验证表格视图功能完整
   - 显示密度提升 2 倍
   - 响应式布局正常
   - 边界情况处理妥当
5. ✅ Deploy/Demo MVP（表格视图已可用）

### Incremental Delivery（增量交付）

1. ✅ Setup → User Story 1 → **MVP 可用** 🎉
2. ✅ Add User Story 2 → 操作按钮功能完善 → Deploy/Demo
3. ✅ Add User Story 3 → 状态提示优化 → Deploy/Demo
4. ✅ Polish → 最终完善 → Deploy/Demo

每个故事添加价值，不破坏之前的功能。

### Sequential Strategy（单人开发）⭐ 最适合本功能

由于 US2 和 US3 都依赖 US1 的核心任务 T003，建议单人顺序执行：

1. ✅ Phase 1: Setup（T001-T002）
2. ✅ Phase 3: User Story 1（T003-T007）- 核心表格实现
3. ✅ Phase 4: User Story 2（T008-T011）- 添加操作按钮
4. ✅ Phase 5: User Story 3（T012-T013）- 验证状态提示
5. ✅ Phase 6: Polish（T014-T018）- 最终完善

**预计工作量**: 2-3 小时（包括测试验收）

---

## Notes

- **[P]** 标记：不同文件，无依赖，可并行
- **[Story]** 标记：任务所属用户故事，便于追溯
- **每个用户故事独立可测试**：US1 完成后即可独立验证和使用
- **无自动化测试**：本功能采用手动验收测试，参考 quickstart.md
- **提交策略**：每完成一个任务立即提交，提交信息使用中文，遵循 Conventional Commits 格式
- **验收标准**：所有验收场景必须通过，参考 spec.md 和 quickstart.md
- **性能目标**：
  - 页面渲染 < 2 秒（100 个网站配合分页）
  - 操作响应 < 100ms
  - 显示密度提升 2 倍
- **停止点**：每个 Checkpoint 都是验证故事独立性的停止点
- **避免**：
  - 模糊的任务描述
  - 同一文件冲突（本功能主要修改单个文件 WebsiteList.tsx）
  - 跨故事依赖破坏独立性

---

## Summary

**Total Tasks**: 18 个任务
- Phase 1 Setup: 2 个任务
- Phase 2 Foundational: 0 个任务（无需）
- Phase 3 User Story 1: 5 个任务（3 实现 + 2 手动测试）
- Phase 4 User Story 2: 4 个任务（2 实现 + 2 手动测试）
- Phase 5 User Story 3: 2 个任务（1 实现 + 1 手动测试）
- Phase 6 Polish: 5 个任务

**Parallel Opportunities**:
- Setup: T001 → T002（顺序）
- User Stories: US2 和 US3 依赖 US1 的 T003，建议顺序执行
- Polish: T015 和 T016 可并行

**Independent Test Standards**:
- US1: 添加 20+ 网站，验证表格显示密度提升 2 倍
- US2: 点击所有操作按钮，验证对话框打开且功能正常
- US3: 清空网站，验证空状态提示；刷新页面，验证加载状态

**Suggested MVP Scope**:
- User Story 1（表格视图核心）= MVP ✅
- User Story 2（操作按钮）= 增强
- User Story 3（状态提示）= 完善

**Estimated Time**: 2-3 小时（单人，包括测试验收）
