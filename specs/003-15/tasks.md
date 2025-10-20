---
description: "表格视图增强 - 可执行任务列表"
---

# Tasks: 表格视图增强

**Input**: Design documents from `/specs/003-15/`
**Prerequisites**: plan.md ✅, spec.md ✅, quickstart.md ✅

**Tests**: 本功能采用手动验收测试，无自动化测试任务

**Organization**: 任务按用户故事组织，每个故事独立可测试

## Format: `[ID] [P?] [Story] Description`
- **[P]**: 可并行运行（不同文件，无依赖）
- **[Story]**: 任务所属用户故事（US1, US2, US3, US4）
- 包含准确的文件路径

## Path Conventions
- **Web app**: `frontend/src/` - 本功能仅涉及前端
- 所有路径相对于仓库根目录

---

## Phase 1: Setup（环境准备）

**Purpose**: 项目环境验证和必需依赖确认

### Tasks

- [ ] **T001** [Setup] 验证开发环境就绪
  - 验证 Node.js >= 20.19.0
  - 验证 pnpm 已安装
  - 验证前端开发服务器可启动：`cd frontend && pnpm dev`
  - 验证后端 API 可访问：http://localhost:5093
  - 参考：quickstart.md "前提条件" 章节

- [ ] **T002** [Setup] 准备测试数据
  - 确保数据库至少有 25 个网站数据（用于测试分页和搜索）
  - 为至少一个网站添加 25 个账号（用于测试账号列表分页）
  - 包含不同长度的显示名称、域名、备注（用于测试截断功能）
  - 参考：quickstart.md "测试数据准备" 章节

**Checkpoint**: 环境就绪，测试数据充足

---

## Phase 2: Foundational（基础前提）

**Purpose**: 无需基础任务 - 复用现有基础设施

**⚠️ 说明**: 本功能纯 UI 优化，以下组件已存在且无需修改：
- ✅ Table 组件（002 功能已添加）
- ✅ Input 组件（shadcn/ui 已有）
- ✅ Pagination 组件
- ✅ WebsiteResponse/AccountResponse 接口
- ✅ websiteService/accountService API

**Checkpoint**: 基础设施已就绪（无需额外工作）

---

## Phase 3: User Story 1 - 网站列表紧凑显示和分页优化 (Priority: P1) 🎯 MVP

**Goal**: 减小表格行高和内边距，将每页显示数量从 10 条增加到 15 条，提升单屏信息密度

**Independent Test**: 访问网站管理页面（需有 25+ 网站），验证：(1) 表格行间距和内边距明显减小；(2) 第一页显示 15 个网站；(3) 分页器显示"第1页，共2页"；(4) 鼠标悬停行有明显高亮

### Implementation for User Story 1

- [ ] **T003** [US1] 为 WebsiteList 表格添加紧凑模式样式
  - 文件：`frontend/src/components/websites/WebsiteList.tsx`
  - 为 Table 组件添加 `className="text-sm"`（减小字体大小）
  - 为所有 TableHead 添加 `h-10` 类名（减小表头高度，从默认 h-12）
  - 为所有 TableCell 添加 `py-2 px-3` 类名（减小单元格内边距，从默认 p-4）
  - 保持现有的响应式类名和功能不变
  - 参考：quickstart.md "Step 1.1" 的详细代码示例
  - 提交信息：`feat[ui]: 为网站列表表格添加紧凑模式样式

- 添加 text-sm、h-10、py-2 px-3 类名
- 提升单屏显示网站数量至少 30%
- 保持响应式布局和可读性

完成 T003 - User Story 1 紧凑显示`

- [ ] **T004** [US1] 调整 WebsitesPage 分页大小为 15
  - 文件：`frontend/src/pages/WebsitesPage.tsx`
  - 找到 `const pageSize = 10`（大约第 31 行）
  - 修改为 `const pageSize = 15`
  - 无需其他修改（分页逻辑自动适配）
  - 参考：quickstart.md "Step 1.2"
  - 提交信息：`feat[ui]: 调整网站列表分页大小为 15 条

- 从每页 10 条改为 15 条
- 减少翻页次数 30%
- 提升浏览效率

完成 T004 - User Story 1 分页优化`

### Manual Testing for User Story 1

- [ ] **T005** [US1] 手动验收：紧凑显示和分页功能
  - 访问 http://localhost:5173/websites（需有 25+ 网站数据）
  - ✅ 验证表格行高明显减小，信息更紧凑
  - ✅ 验证第一页显示 15 个网站
  - ✅ 验证分页器显示"第1页，共2页"（25 个网站）
  - ✅ 验证点击"下一页"切换到第2页，显示剩余 10 个网站
  - ✅ 验证点击"上一页"返回第1页，快速加载
  - ✅ 验证鼠标悬停某一行，该行有明显的背景色高亮
  - ✅ 验证所有列的信息清晰可读，操作按钮不拥挤
  - 参考：quickstart.md "验收清单" - User Story 1 部分

**Checkpoint**: User Story 1 完成 - 紧凑显示和分页优化正常工作，独立可测试

---

## Phase 4: User Story 2 - 网站列表快速搜索 (Priority: P2)

**Goal**: 添加搜索框，支持按域名或显示名称实时过滤网站列表，大幅提升查找效率

**Independent Test**: 在网站列表页面顶部输入搜索框中输入"github"，验证：(1) 表格立即过滤出域名或显示名称包含"github"的网站；(2) 分页器根据搜索结果重新计算；(3) 清空搜索框后，表格恢复显示所有网站

### Implementation for User Story 2

- [ ] **T006** [US2] 在 WebsitesPage 添加搜索状态和前端过滤逻辑
  - 文件：`frontend/src/pages/WebsitesPage.tsx`
  - 导入 useMemo：修改第1行 `import { useState, useEffect, useCallback, useMemo } from 'react'`
  - 添加搜索状态：`const [searchQuery, setSearchQuery] = useState('')`（在其他 useState 后）
  - 添加前端过滤逻辑（使用 useMemo）：
    ```typescript
    const filteredWebsites = useMemo(() => {
      if (!searchQuery.trim()) return websites
      const query = searchQuery.toLowerCase()
      return websites.filter(
        (website) =>
          website.displayName?.toLowerCase().includes(query) ||
          website.domain.toLowerCase().includes(query)
      )
    }, [websites, searchQuery])
    ```
  - 修改 WebsiteList 组件调用，使用 `filteredWebsites` 替代 `websites`
  - 修改分页器显示条件，使用 `filteredWebsites.length > 0` 替代 `websites.length > 0`
  - 参考：quickstart.md "Step 2.1 - 添加前端过滤逻辑"
  - 提交信息：`feat[ui]: 实现网站列表前端搜索过滤逻辑

- 添加搜索状态 searchQuery
- 使用 useMemo 实现实时过滤
- 支持域名和显示名称模糊搜索，不区分大小写

完成 T006 - User Story 2 过滤逻辑`

- [ ] **T007** [US2] 在 WebsitesPage 添加搜索框 UI
  - 文件：`frontend/src/pages/WebsitesPage.tsx`（继续修改）
  - 导入 Input 和图标：`import { Input } from '@/components/ui/input'` 和 `import { Search, X } from 'lucide-react'`
  - 在页面标题和按钮组之间添加搜索框 UI（大约第 125 行）：
    ```typescript
    <div className="relative">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
      <Input
        type="text"
        placeholder="搜索网站（域名或显示名称）"
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        className="pl-10 pr-10"
      />
      {searchQuery && (
        <button
          onClick={() => setSearchQuery('')}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
        >
          <X className="h-4 w-4" />
        </button>
      )}
    </div>
    ```
  - 参考：quickstart.md "Step 2.1 - 添加搜索框 UI"
  - 提交信息：`feat[ui]: 添加网站列表搜索框 UI

- 添加 Input 组件作为搜索框
- 集成 Search 和 X 图标
- 支持点击 X 图标清空搜索

完成 T007 - User Story 2 搜索框 UI`

- [ ] **T008** [US2] 添加搜索无结果的友好提示
  - 文件：`frontend/src/pages/WebsitesPage.tsx`（继续修改）
  - 在 `<WebsiteList />` 之前添加搜索无结果提示：
    ```typescript
    {!isLoading && searchQuery && filteredWebsites.length === 0 && websites.length > 0 && (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600 mb-4">
            未找到匹配 "{searchQuery}" 的网站
          </p>
          <Button variant="outline" onClick={() => setSearchQuery('')}>
            清空搜索
          </Button>
        </CardContent>
      </Card>
    )}
    ```
  - 将 WebsiteList 和 Pagination 包裹在条件渲染中：
    ```typescript
    {(!searchQuery || filteredWebsites.length > 0) && (
      <>
        <WebsiteList ... />
        {!isLoading && filteredWebsites.length > 0 && (
          <Pagination ... />
        )}
      </>
    )}
    ```
  - 参考：quickstart.md "Step 2.2"
  - 提交信息：`feat[ui]: 添加网站列表搜索无结果提示

- 搜索无匹配时显示友好提示
- 提供清空搜索按钮
- 保持搜索结果分页正确性

完成 T008 - User Story 2 空状态提示`

### Manual Testing for User Story 2

- [ ] **T009** [US2] 手动验收：搜索功能
  - 访问 http://localhost:5173/websites（需有 50+ 网站，包含"google"等关键词）
  - ✅ 在搜索框输入"google"，验证立即过滤出包含"google"的网站
  - ✅ 验证搜索不区分大小写（输入"git"能匹配"GitHub"）
  - ✅ 验证搜索部分匹配（输入"exam"能匹配"example.com"）
  - ✅ 验证分页器根据搜索结果重新计算（如5个结果显示"第1页，共1页"）
  - ✅ 输入不存在的关键词"xyz"，验证显示"未找到匹配的网站"提示
  - ✅ 点击搜索框的 X 图标，验证搜索框清空，表格恢复显示所有网站
  - ✅ 点击"清空搜索"按钮，验证同样效果
  - 参考：quickstart.md "验收清单" - User Story 2 部分

**Checkpoint**: User Story 2 完成 - 搜索功能正常工作，可独立测试

---

## Phase 5: User Story 3 - 账号列表表格化显示 (Priority: P3)

**Goal**: 将账号列表从卡片布局改为表格布局，并添加分页功能，提升账号管理效率

**Independent Test**: 为某个网站添加 25+ 个账号后，访问该网站的账号列表页面，验证：(1) 账号以表格形式展示，包含 7 列；(2) 第一页显示 15 个账号；(3) 点击密码列的"眼睛"图标可切换显示/隐藏密码；(4) 点击"复制"图标可复制密码到剪贴板；(5) 点击"编辑"或"删除"按钮打开相应对话框

### Implementation for User Story 3

- [ ] **T010** [US3] 重构 AccountList 组件为表格布局
  - 文件：`frontend/src/components/accounts/AccountList.tsx`
  - 导入 Table 组件（保留 Card 用于空状态）：
    ```typescript
    import {
      Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
    } from '@/components/ui/table'
    import { Card, CardContent } from '@/components/ui/card'
    ```
  - 完全重写列表部分（第 53 行开始），从卡片布局改为表格布局
  - 实现 7 列表格：用户名、密码、标签、备注、创建时间、更新时间、操作
  - 保持密码显示/隐藏逻辑（useState + togglePasswordVisibility）
  - 保持复制功能（navigator.clipboard.writeText）
  - 为备注列添加截断：`className="hidden lg:table-cell max-w-xs truncate" title={account.notes || ''}`
  - 为时间列添加格式化：`new Date(account.createdAt).toLocaleString('zh-CN')`
  - 响应式设计：
    - 标签列 `hidden md:table-cell`
    - 备注列 `hidden lg:table-cell`
    - 时间列 `hidden xl:table-cell`
  - 参考：quickstart.md "Step 3.1" 的完整代码示例
  - 提交信息：`feat[ui]: 重构账号列表为表格布局

- 从卡片布局改为表格布局（7 列）
- 保持密码显示/隐藏和复制功能
- 添加响应式设计，小屏幕隐藏次要列
- 备注列截断显示，悬停显示完整内容

完成 T010 - User Story 3 表格布局`

- [ ] **T011** [US3] 在 AccountsPage 添加分页功能
  - 文件：`frontend/src/pages/AccountsPage.tsx`
  - 导入 Pagination 组件：`import Pagination from '@/components/common/Pagination'`
  - 添加分页状态（在 useState 区域）：
    ```typescript
    const [currentPage, setCurrentPage] = useState(1)
    const [totalPages, setTotalPages] = useState(1)
    const pageSize = 15
    ```
  - 修改 `loadAccounts` 函数支持分页（第 36 行开始）：
    ```typescript
    const response = await accountService.getByWebsite(
      Number(websiteId),
      currentPage,
      pageSize
    )
    if (response.success && response.data) {
      setAccounts(response.data.items as AccountResponse[])
      setTotalPages(response.data.totalPages)
    }
    ```
  - 修改 useEffect 依赖数组，添加 `currentPage`：
    ```typescript
    useEffect(() => {
      loadAccounts()
      loadWebsite()
    }, [websiteId, currentPage])
    ```
  - 在 AccountList 组件调用之后添加 Pagination 组件：
    ```typescript
    {!isLoading && accounts.length > 0 && (
      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={setCurrentPage}
      />
    )}
    ```
  - 参考：quickstart.md "Step 3.2"
  - 提交信息：`feat[ui]: 为账号列表添加分页功能

- 添加分页状态（currentPage, totalPages, pageSize=15）
- 修改 loadAccounts 支持分页参数
- 集成 Pagination 组件
- useEffect 依赖 currentPage 自动刷新

完成 T011 - User Story 3 分页功能`

### Manual Testing for User Story 3

- [ ] **T012** [US3] 手动验收：表格展示和分页功能
  - 为某个网站添加 25+ 个账号
  - 访问该网站的账号列表页面（从网站管理页点击"查看账号"）
  - ✅ 验证账号以表格形式展示，包含 7 列
  - ✅ 验证第一页显示 15 个账号
  - ✅ 验证密码列默认为"••••••••"（隐藏状态）
  - ✅ 点击某账号的"眼睛"图标，验证密码切换为明文显示，图标从 Eye 变为 EyeOff
  - ✅ 再次点击"眼睛"图标，验证密码恢复隐藏
  - ✅ 点击"复制"图标，验证密码被复制到剪贴板（可粘贴验证）
  - ✅ 验证分页器显示"第1页，共2页"（25 个账号）
  - ✅ 点击"下一页"，验证切换到第2页，显示剩余 10 个账号
  - ✅ 验证长备注（超过 100 字）截断显示，鼠标悬停显示完整内容
  - ✅ 访问无账号的网站，验证显示空状态卡片"暂无账号"
  - 参考：quickstart.md "验收清单" - User Story 3 部分

**Checkpoint**: User Story 3 完成 - 账号列表表格化和分页功能正常工作，可独立测试

---

## Phase 6: User Story 4 - 账号列表紧凑显示优化 (Priority: P4)

**Goal**: 为账号列表表格添加紧凑模式样式，与网站列表保持一致的紧凑风格

**Independent Test**: 对比优化前后的账号列表表格，验证：(1) 表格行高减小，单屏可显示更多账号；(2) 列间距和内边距适当减小但不影响可读性；(3) 操作按钮仍然易于点击，无误操作风险

### Implementation for User Story 4

- [ ] **T013** [US4] 为 AccountList 表格添加紧凑模式样式
  - 文件：`frontend/src/components/accounts/AccountList.tsx`（在 T010 基础上继续修改）
  - 为 Table 组件添加 `className="text-sm"`（减小字体大小）
  - 为所有 TableHead 添加 `h-10` 类名（减小表头高度）
  - 为所有 TableCell 添加 `py-2 px-3` 类名（减小单元格内边距）
  - 确保按钮保持最小尺寸 `size="sm"`（已有，无需修改）
  - 确保备注列截断样式正确：`max-w-xs truncate` + `title` 属性
  - 参考：quickstart.md "Step 4.1" 的详细代码示例
  - 提交信息：`feat[ui]: 为账号列表表格添加紧凑模式样式

- 添加 text-sm、h-10、py-2 px-3 类名
- 提升单屏显示账号数量至少 30%
- 保持良好的可读性和可操作性

完成 T013 - User Story 4 紧凑显示`

### Manual Testing for User Story 4

- [ ] **T014** [US4] 手动验收：紧凑模式可读性和可操作性
  - 访问账号列表页面（需有 25+ 账号）
  - ✅ 验证表格行高明显减小，单屏显示更多账号
  - ✅ 验证信息仍然清晰可读，无重叠或拥挤
  - ✅ 验证鼠标悬停某一行，该行有明显的背景色高亮
  - ✅ 验证操作按钮仍然易于点击，无误触其他行的按钮
  - ✅ 验证"眼睛"和"复制"图标易于点击，响应准确
  - ✅ 在 1080p 屏幕下，对比优化前后，单屏可见账号数量增加至少 30%
  - 参考：quickstart.md "验收清单" - User Story 4 部分

**Checkpoint**: User Story 4 完成 - 账号列表紧凑显示优化正常工作，可独立测试

---

## Phase 7: Polish & Cross-Cutting Concerns（完善与横切关注点）

**Purpose**: 跨用户故事的改进和最终验收

### Tasks

- [ ] **T015** [P] [Polish] 运行 Linter 和修复警告
  - 运行命令：`cd frontend && pnpm lint`
  - 修复所有 ESLint 错误和警告
  - 特别检查：
    - React Hooks exhaustive-deps 警告
    - 未使用的 import
    - TypeScript strict mode 错误
  - 参考：quickstart.md "代码审查 - 运行 Linter"

- [ ] **T016** [P] [Polish] 运行代码格式化
  - 运行命令：`cd frontend && pnpm format`
  - 验证所有修改的文件已格式化
  - 检查 git diff，确保格式化改动符合预期

- [ ] **T017** [Polish] 运行完整验收清单
  - 按照 quickstart.md "完整验收清单" 逐项验证：
    - ✅ User Story 1 的 4 个验收场景
    - ✅ User Story 2 的 5 个验收场景
    - ✅ User Story 3 的 6 个验收场景
    - ✅ User Story 4 的 3 个验收场景
    - ✅ 边界情况测试（9 个场景）
  - 记录任何问题并修复
  - 参考：quickstart.md "完整验收清单"

- [ ] **T018** [P] [Polish] 性能验证
  - 使用 Chrome DevTools Performance 面板
  - 测试场景 1：搜索性能
    - 添加 100+ 个网站
    - 在搜索框输入关键词，录制性能
    - ✅ 验证过滤响应时间 < 200ms
  - 测试场景 2：表格渲染性能
    - 刷新页面，录制性能
    - ✅ 验证页面渲染时间 < 2 秒
  - 测试场景 3：交互响应性
    - 点击密码显示/隐藏、复制、分页按钮
    - ✅ 验证所有交互响应 < 100ms
  - 参考：quickstart.md "性能验证"

- [ ] **T019** [Polish] 最终代码审查和提交
  - 检查代码质量：
    - ✅ 无 console.log 或调试代码
    - ✅ 无未使用的 import
    - ✅ Props 接口正确定义
    - ✅ 遵循 React Hooks 最佳实践
    - ✅ 遵循 Tailwind CSS 类名约定
  - 提交信息：`chore[ui]: 代码审查和性能验证 - 表格视图增强

- 通过所有 Linter 检查
- 运行代码格式化
- 完成完整验收清单（27 个场景）
- 通过性能验证（搜索、渲染、交互）
- 清理调试代码

完成 T019 - 最终审查`

- [ ] **T020** [Polish] 更新文档（可选）
  - 如果有特殊使用模式或最佳实践，更新 `CLAUDE.md`
  - 示例内容：
    - "搜索功能使用 useMemo 优化性能，避免不必要的过滤计算"
    - "紧凑模式使用 Tailwind 类名 text-sm, h-10, py-2 px-3"
  - 如无特殊内容，跳过此任务
  - 提交信息：`docs: 更新 CLAUDE.md 记录表格增强最佳实践`

**Checkpoint**: 所有用户故事完成，功能完整，代码质量达标

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 无依赖 - 立即开始
  - T001: 环境验证
  - T002: 测试数据准备

- **Foundational (Phase 2)**: 依赖 Setup 完成 - **无任务（复用现有基础设施）**

- **User Stories (Phase 3-6)**: 依赖 Setup 完成后可立即开始
  - **User Story 1 (Phase 3)**: T003 → T004 → T005（手动测试）
  - **User Story 2 (Phase 4)**: 依赖 Phase 3 完成 → T006 → T007 → T008 → T009（手动测试）
  - **User Story 3 (Phase 5)**: 独立于 Phase 4，可并行 → T010 → T011 → T012（手动测试）
  - **User Story 4 (Phase 6)**: 依赖 T010 完成 → T013 → T014（手动测试）

- **Polish (Phase 7)**: 依赖所有用户故事完成
  - T015, T016, T018 可并行
  - T017, T019, T020 顺序执行

### User Story Dependencies

- **User Story 1 (P1) - 网站列表紧凑显示和分页优化**:
  - 可在 Setup 完成后立即开始
  - 无其他依赖
  - **内部顺序**: T003 → T004 → T005（测试）

- **User Story 2 (P2) - 网站列表快速搜索**:
  - 依赖 US1 完成（在紧凑模式基础上添加搜索）
  - **内部顺序**: T006 → T007 → T008 → T009（测试）

- **User Story 3 (P3) - 账号列表表格化显示**:
  - 独立于 US1 和 US2（不同页面和组件）
  - 可与 US2 并行开发
  - **内部顺序**: T010 → T011 → T012（测试）

- **User Story 4 (P4) - 账号列表紧凑显示优化**:
  - 依赖 US3 的 T010（表格布局必须存在）
  - 在 US3 完成 T010 后可开始
  - **内部顺序**: T013 → T014（测试）

### Within Each User Story

- **User Story 1 内部顺序**:
  - T003（紧凑样式）→ T004（分页大小）→ T005（手动测试）

- **User Story 2 内部顺序**:
  - T006（过滤逻辑）→ T007（搜索框 UI）→ T008（空状态提示）→ T009（手动测试）

- **User Story 3 内部顺序**:
  - T010（表格布局）→ T011（分页功能）→ T012（手动测试）

- **User Story 4 内部顺序**:
  - T013（紧凑样式）→ T014（手动测试）

### Parallel Opportunities

- **Setup 阶段**:
  - T001 和 T002 可并行（不同关注点）

- **User Stories**:
  - **US2 和 US3 可并行开发**（不同文件，无依赖）
  - US2: 修改 WebsitesPage.tsx
  - US3: 修改 AccountList.tsx 和 AccountsPage.tsx

- **Polish 阶段**:
  - T015（Linter）、T016（格式化）、T018（性能验证）可并行
  - T017, T019, T020 顺序执行

- **跨团队并行（如有多人）**:
  - 团队 A：US1 → US2
  - 团队 B：US3 → US4
  - 最后合并：Polish 阶段

---

## Parallel Example: User Stories 2 & 3

```bash
# US2 和 US3 可以并行开发（不同文件，无冲突）：

# 团队成员 A 或 并行任务 1：
Task: "实现网站列表搜索功能（T006-T009）"
Files: frontend/src/pages/WebsitesPage.tsx

# 团队成员 B 或 并行任务 2（同时进行）：
Task: "实现账号列表表格化（T010-T012）"
Files: frontend/src/components/accounts/AccountList.tsx, frontend/src/pages/AccountsPage.tsx

# 完成后合并，然后执行 US4 和 Polish
```

---

## Implementation Strategy

### MVP First (User Story 1 Only) ⭐ 推荐

1. ✅ Complete Phase 1: Setup（T001-T002）
2. ⏭️ Skip Phase 2: Foundational（无需任务）
3. ✅ Complete Phase 3: User Story 1（T003-T005）
4. **STOP and VALIDATE**: 验证紧凑显示和分页功能
   - 单屏显示 15 条网站
   - 信息密度提升 30%
   - 分页功能正常
5. ✅ Deploy/Demo MVP（紧凑显示和分页优化已可用）

### Incremental Delivery（增量交付）⭐ 最适合本功能

1. ✅ Setup → User Story 1 → **MVP 可用** 🎉
2. ✅ Add User Story 2 → 搜索功能完善 → Deploy/Demo
3. ✅ Add User Story 3 → 账号列表优化 → Deploy/Demo
4. ✅ Add User Story 4 → 账号列表紧凑显示 → Deploy/Demo
5. ✅ Polish → 最终完善 → Deploy/Demo

每个故事添加价值，不破坏之前的功能。

### Parallel Strategy（多人开发）

适用于有 2-3 人团队的场景：

1. ✅ Phase 1: Setup（T001-T002）- 一人完成
2. ✅ Phase 3: User Story 1（T003-T005）- 一人完成
3. 🔀 **并行阶段**:
   - **成员 A**: Phase 4: User Story 2（T006-T009）
   - **成员 B**: Phase 5: User Story 3（T010-T012）
4. ✅ Phase 6: User Story 4（T013-T014）- 依赖 T010，一人完成
5. 🔀 **并行阶段**:
   - **成员 A**: T015（Linter）
   - **成员 B**: T016（格式化）
   - **成员 C**: T018（性能验证）
6. ✅ Phase 7: Polish 收尾（T017, T019, T020）- 一人完成

**预计工作量**: 单人 4-5 小时，双人 3-4 小时（包括测试验收）

### Sequential Strategy（单人开发）⭐ 最常见

按优先级顺序执行：

1. ✅ Phase 1: Setup（T001-T002）
2. ✅ Phase 3: User Story 1（T003-T005）- 网站列表紧凑显示
3. ✅ Phase 4: User Story 2（T006-T009）- 网站列表搜索
4. ✅ Phase 5: User Story 3（T010-T012）- 账号列表表格化
5. ✅ Phase 6: User Story 4（T013-T014）- 账号列表紧凑显示
6. ✅ Phase 7: Polish（T015-T020）- 最终完善

**预计工作量**: 4-5 小时（包括测试验收）

---

## Notes

- **[P]** 标记：不同文件，无依赖，可并行
- **[Story]** 标记：任务所属用户故事，便于追溯
- **每个用户故事独立可测试**：US1 完成后即可独立验证和使用
- **无自动化测试**：本功能采用手动验收测试，参考 quickstart.md
- **提交策略**：每完成一个任务立即提交，提交信息使用中文，遵循 Conventional Commits 格式
- **验收标准**：所有验收场景必须通过，参考 quickstart.md 完整验收清单
- **性能目标**：
  - 搜索过滤响应 < 200ms（500 个网站以内）
  - 表格交互响应 < 100ms
  - 页面渲染 < 2 秒
  - 单屏显示数量提升 30%-50%
- **停止点**：每个 Checkpoint 都是验证故事独立性的停止点
- **避免**：
  - 模糊的任务描述
  - 同一文件冲突（本功能主要修改 4 个文件）
  - 跨故事依赖破坏独立性

---

## Summary

**Total Tasks**: 20 个任务
- Phase 1 Setup: 2 个任务
- Phase 2 Foundational: 0 个任务（无需）
- Phase 3 User Story 1: 3 个任务（2 实现 + 1 手动测试）
- Phase 4 User Story 2: 4 个任务（3 实现 + 1 手动测试）
- Phase 5 User Story 3: 3 个任务（2 实现 + 1 手动测试）
- Phase 6 User Story 4: 2 个任务（1 实现 + 1 手动测试）
- Phase 7 Polish: 6 个任务

**Parallel Opportunities**:
- Setup: T001 和 T002 可并行
- User Stories: US2（T006-T009）和 US3（T010-T012）可并行
- Polish: T015, T016, T018 可并行

**Independent Test Standards**:
- US1: 25 个网站，验证紧凑显示和分页（每页 15 条）
- US2: 搜索"google"，验证立即过滤和分页重新计算
- US3: 25 个账号，验证表格展示、密码切换、复制、分页
- US4: 验证紧凑模式单屏显示增加 30%

**Suggested MVP Scope**:
- User Story 1（网站列表紧凑显示和分页优化）= MVP ✅
- User Story 2（网站列表快速搜索）= 增强
- User Story 3（账号列表表格化显示）= 完善
- User Story 4（账号列表紧凑显示优化）= 锦上添花

**Estimated Time**: 4-5 小时（单人，包括测试验收）
