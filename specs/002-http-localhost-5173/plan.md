# Implementation Plan: 网站列表表格视图

**Branch**: `002-http-localhost-5173` | **Date**: 2025-10-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-http-localhost-5173/spec.md`

## Summary

将网站管理页面的显示方式从卡片布局改为表格布局，以支持大量网站的集中显示。核心需求是提升信息密度，在相同屏幕空间内显示更多网站记录。技术方案采用 shadcn/ui 的 Table 组件，保持现有的分页、对话框和数据服务不变，仅重构 WebsiteList 组件的展示层。

## Technical Context

**Language/Version**: TypeScript 5.9.3 (已确认自 frontend/package.json)
**Primary Dependencies**: React 19, Vite 7, shadcn/ui, Tailwind CSS 4, axios 1.12
**Storage**: N/A (前端展示层功能，数据来自现有 websiteService API)
**Testing**: N/A (本功能为 UI 重构，手动测试验收)
**Target Platform**: 现代浏览器 (Chrome, Firefox, Safari, Edge 最新版)，支持响应式布局
**Project Type**: Web 应用 (frontend + backend，本功能仅涉及 frontend)
**Performance Goals**:
- 页面渲染时间 < 2秒 (100个网站配合分页)
- 表格交互响应 < 100ms (按钮点击、悬停效果)
**Constraints**:
- 必须保持现有 API 接口不变
- 必须复用现有对话框组件 (CreateWebsiteDialog, EditWebsiteDialog, DeleteWebsiteDialog)
- 必须保持现有分页功能
**Scale/Scope**:
- 单个组件重构 (WebsiteList.tsx)
- 预期支持显示 100+ 网站记录
- 影响范围：1 个页面组件，1 个列表组件

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
- 使用 shadcn/ui 组件 (Table 组件) ✓
- 使用 Tailwind CSS 样式 ✓

✅ **目录结构**:
- 组件位于 `src/components/` (本功能修改 `src/components/websites/WebsiteList.tsx`) ✓

### Security & Quality Check

✅ **数据验证**: N/A (无新增数据输入，仅展示层重构)
✅ **异步操作**: 现有 API 调用已使用 async/await ✓

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
specs/002-http-localhost-5173/
├── spec.md              # 功能规格说明 (已完成)
├── plan.md              # 本文件 - 实施计划
├── research.md          # 阶段 0 - 技术研究 (本功能将跳过，无需研究)
├── data-model.md        # 阶段 1 - 数据模型 (本功能将跳过，无后端修改)
├── quickstart.md        # 阶段 1 - 快速启动指南
├── contracts/           # 阶段 1 - API 契约 (本功能将跳过，无 API 变更)
├── tasks.md             # 阶段 2 - 任务列表 (/speckit.tasks 命令生成)
└── checklists/
    └── requirements.md  # 规格说明质量检查清单 (已完成)
```

### Source Code (repository root)

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/                    # shadcn/ui 基础组件
│   │   │   ├── button.tsx         # 现有按钮组件
│   │   │   ├── card.tsx           # 现有卡片组件 (将不再使用)
│   │   │   └── table.tsx          # 需要添加的表格组件 (shadcn/ui)
│   │   ├── websites/              # 网站功能组件
│   │   │   ├── WebsiteList.tsx    # ⚠️ 主要修改文件：重构为表格视图
│   │   │   ├── CreateWebsiteDialog.tsx   # 复用，无需修改
│   │   │   ├── EditWebsiteDialog.tsx     # 复用，无需修改
│   │   │   └── DeleteWebsiteDialog.tsx   # 复用，无需修改
│   │   └── common/
│   │       └── Pagination.tsx     # 复用，无需修改
│   ├── pages/
│   │   └── WebsitesPage.tsx       # 可能需要微调样式，逻辑无需改动
│   └── services/
│       └── websiteService.ts      # 无需修改，API 保持不变
└── tests/                         # 无自动化测试 (手动验收)

backend/                           # 本功能完全不涉及后端修改
```

**Structure Decision**:
本功能为纯前端 UI 重构，仅修改 `frontend/src/components/websites/WebsiteList.tsx` 组件，将卡片布局替换为表格布局。需要通过 shadcn/ui CLI 添加 Table 组件到 `src/components/ui/`。其他现有组件 (对话框、分页器) 和服务层完全不变。

## Phase 0: Research (跳过)

**决策理由**: 本功能无需研究阶段，因为：
1. 使用现有技术栈 (React + TypeScript + shadcn/ui)，无新技术引入
2. shadcn/ui Table 组件有完善文档，无需额外调研
3. 数据结构和 API 接口完全不变，仅 UI 层重构
4. 响应式布局使用 Tailwind CSS 标准断点，为既定方案

**跳过文件**: `research.md` 不创建

## Phase 1: Design & Contracts

### 数据模型 (跳过)

**决策理由**: 本功能无后端修改，数据结构保持不变：
- 使用现有 `WebsiteResponse` 接口 (来自 websiteService.ts)
- 无新增实体，无数据库变更
- 表格显示的字段与现有卡片视图完全一致

**跳过文件**: `data-model.md` 不创建

### API 契约 (跳过)

**决策理由**: 本功能无 API 变更：
- 使用现有 `websiteService.getAll(pageNumber, pageSize)` 接口
- 响应格式 `PaginatedResponse<WebsiteResponse>` 保持不变
- 无新增端点，无现有端点修改

**跳过目录**: `contracts/` 不创建

### 快速启动指南

**内容大纲**:
1. **本地开发环境验证**
   - 确认前端开发服务器运行 (`cd frontend && pnpm dev`)
   - 确认后端 API 可访问 (http://localhost:5093)

2. **添加 shadcn/ui Table 组件**
   ```bash
   cd frontend
   npx shadcn@latest add table
   ```
   这将自动安装 Table, TableHeader, TableBody, TableRow, TableCell 等组件到 `src/components/ui/table.tsx`

3. **修改 WebsiteList 组件**
   - 打开 `frontend/src/components/websites/WebsiteList.tsx`
   - 替换 Card 布局为 Table 布局
   - 保持 Props 接口不变 (WebsiteListProps)

4. **视觉验证步骤**
   - 访问 http://localhost:5173/websites
   - 确认表格显示：列标题、数据行、操作按钮
   - 测试空状态、加载状态
   - 测试响应式布局 (缩小浏览器窗口)
   - 测试操作按钮 (查看账号、编辑、删除)

5. **验收场景清单**
   - [ ] 表格显示 10+ 网站记录，信息对齐清晰
   - [ ] 列标题：显示名、域名、标签、活跃账号、回收站、操作
   - [ ] 点击"查看账号"导航到账号列表页
   - [ ] 点击"编辑"打开编辑对话框
   - [ ] 点击"删除"打开删除确认对话框
   - [ ] 空状态显示"添加第一个网站"提示
   - [ ] 加载状态显示加载指示器
   - [ ] 分页器功能正常

**输出文件**: `quickstart.md` (将在 Phase 1 创建)

## Phase 2: Task Breakdown (由 /speckit.tasks 生成)

**任务预览** (最终由 `/speckit.tasks` 命令生成详细任务列表):
1. 添加 shadcn/ui Table 组件
2. 重构 WebsiteList.tsx 组件为表格布局
3. 调整 WebsitesPage.tsx 容器样式 (如需要)
4. 测试验收：功能、空状态、加载状态、响应式
5. 更新 CLAUDE.md (如需要记录表格组件使用模式)

**注意**: 本计划在 Phase 1 后结束，不生成 tasks.md。运行 `/speckit.tasks` 命令将基于本计划生成详细任务。

## Complexity Tracking

**不适用** - 无章程违规项，无需跟踪。

## Phase 1 Completion Summary

### 生成的设计产物

1. ✅ **quickstart.md** - 快速启动指南
   - 环境验证步骤
   - shadcn/ui Table 组件安装指南
   - WebsiteList 组件重构示例代码
   - 视觉验证清单
   - 性能验证方法
   - 故障排除指南

2. ⏭️ **research.md** - 跳过 (无需技术研究)

3. ⏭️ **data-model.md** - 跳过 (无数据模型变更)

4. ⏭️ **contracts/** - 跳过 (无 API 契约变更)

5. ✅ **CLAUDE.md 更新** - 代理上下文已更新
   - 添加 TypeScript 5.9.3 技术栈信息
   - 添加 React 19, Vite 7, shadcn/ui, Tailwind CSS 4 依赖

### 重新评估章程检查 (Phase 1 后)

✅ **设计后验证 - 所有检查项通过**:

1. **Frontend Standards** - 完全符合
   - 使用 shadcn/ui Table 组件 (符合 UI 框架要求)
   - 使用 Tailwind CSS 响应式类 (符合 CSS 框架要求)
   - TypeScript strict mode (已验证项目配置)
   - 组件使用箭头函数定义 (quickstart 示例代码已遵循)

2. **Code Quality** - 完全符合
   - Props 接口 `WebsiteListProps` 已定义
   - 组件目录结构正确 (`src/components/websites/`)
   - 无复杂逻辑，仅 UI 展示层重构

3. **Security & Testing** - N/A
   - 无数据输入验证需求 (仅展示)
   - 无异步数据处理新增 (复用现有 API)

4. **Git Commit Standards** - 已在 quickstart 中明确
   - 提供了中文提交信息示例
   - 遵循 Conventional Commits 格式 (`feat[ui]:`)

**最终门控状态**: ✅ 通过 - 无章程违规，可以进入 Phase 2

## Next Steps

1. ✅ **Phase 0 完成** - 跳过 (无需研究)
2. ✅ **Phase 1 完成** - 设计与契约已完成
   - quickstart.md 已生成
   - 代理上下文已更新
   - 章程检查重新验证通过
3. ⏸️ **Phase 2 待命** - 由 `/speckit.tasks` 命令触发

**运行以下命令继续**：
```bash
/speckit.tasks  # 生成详细任务列表并开始实施
```

**预期任务数量**: 4-5 个任务
- T1: 添加 shadcn/ui Table 组件
- T2: 重构 WebsiteList.tsx 组件
- T3: 调整 WebsitesPage.tsx 样式 (可选)
- T4: 手动验收测试
- T5: 提交代码并更新文档
