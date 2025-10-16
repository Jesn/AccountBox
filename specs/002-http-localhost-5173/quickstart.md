# Quickstart Guide: 网站列表表格视图

**Feature**: 002-http-localhost-5173 | **Date**: 2025-10-16 | **Plan**: [plan.md](./plan.md)

## Overview

本快速启动指南帮助开发者快速实施网站列表表格视图功能。这是一个纯前端 UI 重构任务，将网站管理页面从卡片布局改为表格布局，无需后端修改。

---

## Prerequisites

### 环境要求

- **Node.js**: 版本 20.19.0 或更高 (已在 frontend/package.json 中指定)
- **pnpm**: 已安装 (项目使用 pnpm 作为包管理器)
- **shadcn CLI**: 用于添加 UI 组件

确认环境：
```bash
node --version  # 应显示 >= 20.19.0
pnpm --version  # 应显示已安装
```

### 项目启动

确保前端和后端服务正常运行：

```bash
# 方式 1: 使用项目提供的启动脚本 (推荐)
./start.sh

# 方式 2: 手动启动
# 终端 1 - 启动后端
cd backend/src/AccountBox.Api
dotnet run

# 终端 2 - 启动前端
cd frontend
pnpm dev
```

验证服务：
- 后端 API: http://localhost:5093
- 前端应用: http://localhost:5173
- Swagger 文档: http://localhost:5093/swagger

---

## Implementation Steps

### Step 1: 添加 shadcn/ui Table 组件

shadcn/ui 的 Table 组件是本功能的核心 UI 元素。

```bash
cd frontend
npx shadcn@latest add table
```

**预期结果**:
- 自动创建 `src/components/ui/table.tsx`
- 包含 Table, TableHeader, TableBody, TableRow, TableHead, TableCell 等组件
- 自动安装必要的依赖 (如 @radix-ui 相关包)

**验证**:
```bash
ls src/components/ui/table.tsx
# 应显示文件存在
```

### Step 2: 修改 WebsiteList 组件

打开 `frontend/src/components/websites/WebsiteList.tsx`，重构为表格布局。

**当前结构** (卡片布局):
```tsx
// 当前使用 Card 组件展示每个网站
<div className="grid gap-4">
  {websites.map((website) => (
    <Card key={website.id}>
      {/* 网站信息和操作按钮 */}
    </Card>
  ))}
</div>
```

**目标结构** (表格布局):
```tsx
// 使用 Table 组件展示所有网站
<Table>
  <TableHeader>
    <TableRow>
      <TableHead>显示名</TableHead>
      <TableHead>域名</TableHead>
      {/* 其他列标题 */}
    </TableRow>
  </TableHeader>
  <TableBody>
    {websites.map((website) => (
      <TableRow key={website.id}>
        <TableCell>{website.displayName}</TableCell>
        {/* 其他单元格 */}
      </TableRow>
    ))}
  </TableBody>
</Table>
```

**修改要点**:
1. 导入 Table 相关组件：
   ```tsx
   import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
   ```

2. 移除 Card 相关导入 (不再需要)

3. 保持 Props 接口 `WebsiteListProps` 不变

4. 保持空状态和加载状态的处理逻辑

5. 表格列定义：
   - 显示名 (displayName 或 domain 作为备用)
   - 域名 (domain)
   - 标签 (tags)
   - 活跃账号数 (activeAccountCount)
   - 回收站账号数 (deletedAccountCount)
   - 操作 (三个按钮：查看账号、编辑、删除)

6. 响应式布局：
   - 使用 Tailwind CSS 的 `overflow-x-auto` 支持横向滚动
   - 在小屏幕上可以隐藏次要列 (如标签列) 使用 `hidden md:table-cell`

**示例代码片段**:
```tsx
export function WebsiteList({ websites, isLoading, onViewAccounts, onEdit, onDelete, onCreateNew }: WebsiteListProps) {
  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-gray-600">加载中...</p>
        </CardContent>
      </Card>
    )
  }

  if (websites.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>开始使用 AccountBox</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-gray-600 mb-4">还没有添加任何网站</p>
          <Button onClick={onCreateNew}>
            <Plus className="mr-2 h-4 w-4" />
            添加第一个网站
          </Button>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>显示名</TableHead>
            <TableHead>域名</TableHead>
            <TableHead className="hidden md:table-cell">标签</TableHead>
            <TableHead>活跃账号</TableHead>
            <TableHead>回收站</TableHead>
            <TableHead className="text-right">操作</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {websites.map((website) => (
            <TableRow key={website.id}>
              <TableCell className="font-medium">
                {website.displayName || website.domain}
              </TableCell>
              <TableCell>{website.domain}</TableCell>
              <TableCell className="hidden md:table-cell">
                {website.tags || '-'}
              </TableCell>
              <TableCell>{website.activeAccountCount}</TableCell>
              <TableCell>{website.deletedAccountCount}</TableCell>
              <TableCell className="text-right">
                <div className="flex gap-2 justify-end">
                  <Button variant="outline" size="sm" onClick={() => onViewAccounts(website.id)}>
                    查看账号
                  </Button>
                  <Button variant="outline" size="sm" onClick={() => onEdit(website)}>
                    编辑
                  </Button>
                  <Button variant="destructive" size="sm" onClick={() => onDelete(website)}>
                    删除
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
```

### Step 3: 调整 WebsitesPage 样式 (可选)

打开 `frontend/src/pages/WebsitesPage.tsx`，检查容器样式是否需要调整。

**可能的调整**:
- 如果表格过宽，可以增加最大宽度：
  ```tsx
  <div className="mx-auto max-w-7xl">  // 从 max-w-6xl 改为 max-w-7xl
  ```

- 如果需要更紧凑的布局，可以减少内边距：
  ```tsx
  <div className="min-h-screen bg-gray-50 p-4">  // 从 p-8 改为 p-4
  ```

**注意**: WebsitesPage 的逻辑无需改动，所有 Props 传递和事件处理保持不变。

### Step 4: 视觉验证

完成修改后，在浏览器中验证功能：

1. **访问网站管理页面**: http://localhost:5173/websites

2. **检查表格显示**:
   - ✅ 列标题清晰可见：显示名、域名、标签、活跃账号、回收站、操作
   - ✅ 数据行对齐整齐，信息易读
   - ✅ 表格边框和间距合理

3. **测试操作按钮**:
   - ✅ 点击"查看账号"按钮，导航到 `/websites/{id}/accounts`
   - ✅ 点击"编辑"按钮，打开编辑对话框并预填充数据
   - ✅ 点击"删除"按钮，打开删除确认对话框

4. **测试空状态**:
   - 删除所有网站 (或清空数据库)
   - ✅ 显示"开始使用 AccountBox"卡片
   - ✅ 显示"添加第一个网站"按钮

5. **测试加载状态**:
   - 刷新页面 (F5)
   - ✅ 短暂显示"加载中..."提示

6. **测试响应式布局**:
   - 缩小浏览器窗口到平板尺寸 (768px 左右)
   - ✅ 标签列隐藏 (如使用 `hidden md:table-cell`)
   - ✅ 表格支持横向滚动 (如使用 `overflow-x-auto`)
   - ✅ 操作按钮仍然可点击

7. **测试分页功能**:
   - 添加 10+ 个网站 (超过每页显示数量)
   - ✅ 表格底部显示分页器
   - ✅ 点击下一页，加载新数据

### Step 5: 验收场景清单

根据功能规格说明 (spec.md) 的验收场景，逐项验证：

#### User Story 1: 查看大量网站的表格视图

- [ ] **AS1.1**: 已登录且有 10 个以上的网站，访问网站管理页面，看到表格形式展示的网站列表，包含列标题
- [ ] **AS1.2**: 查看网站表格，每行显示一个网站的完整信息，信息对齐且易于阅读
- [ ] **AS1.3**: 网站表格中有多条记录，向下滚动，表头保持固定在顶部 (可选实现)

#### User Story 2: 在表格中执行网站操作

- [ ] **AS2.1**: 查看网站表格，点击某行的"查看账号"按钮，导航到该网站的账号列表页面
- [ ] **AS2.2**: 查看网站表格，点击某行的"编辑"按钮，打开编辑对话框并预填充该网站信息
- [ ] **AS2.3**: 查看网站表格，点击某行的"删除"按钮，打开删除确认对话框
- [ ] **AS2.4**: 表格中的操作按钮，鼠标悬停在按钮上，按钮有明显的视觉反馈

#### User Story 3: 空状态和加载状态的友好提示

- [ ] **AS3.1**: 没有任何网站，访问网站管理页面，显示空状态提示和"添加第一个网站"的按钮
- [ ] **AS3.2**: 访问网站管理页面，数据正在加载，显示加载中的状态指示器
- [ ] **AS3.3**: 显示空状态提示，点击"添加第一个网站"按钮，打开创建网站对话框

#### 边界情况测试

- [ ] **EC1**: 网站的显示名或域名非常长 (超过 50 个字符)，表格单元格截断并显示省略号
- [ ] **EC2**: 网站有 10 个以上的标签，标签列适当显示或折叠
- [ ] **EC3**: 用户的屏幕宽度较小 (如平板或小笔记本)，表格响应式调整 (横向滚动或隐藏次要列)
- [ ] **EC4**: 回收站中的账号数量为 0，回收站列显示为 0

---

## Troubleshooting

### 问题 1: shadcn add table 失败

**错误信息**: `Command not found: shadcn` 或 `Failed to add component`

**解决方案**:
```bash
# 确保在 frontend 目录下
cd frontend

# 如果 npx 不工作，尝试全局安装
npm install -g shadcn-ui

# 或者使用项目中的 components.json 手动配置
# shadcn/ui 依赖 components.json 文件
cat components.json  # 确认配置存在
```

### 问题 2: Table 组件样式不显示

**错误信息**: 表格无边框、无间距、样式混乱

**解决方案**:
1. 确认 Tailwind CSS 配置正确：
   ```bash
   cat tailwind.config.ts  # 确认 content 路径包含 ./src/**/*.{ts,tsx}
   ```

2. 确认 `src/index.css` 中引入了 Tailwind：
   ```css
   @tailwind base;
   @tailwind components;
   @tailwind utilities;
   ```

3. 重启开发服务器：
   ```bash
   pnpm dev
   ```

### 问题 3: 表格操作按钮点击无响应

**错误信息**: 控制台显示 `TypeError: Cannot read property 'id' of undefined`

**解决方案**:
1. 检查 `WebsiteListProps` 的回调函数签名：
   ```tsx
   onViewAccounts: (websiteId: number) => void
   onEdit: (website: WebsiteResponse) => void
   onDelete: (website: WebsiteResponse) => void
   ```

2. 确认在 TableCell 中正确传递参数：
   ```tsx
   onClick={() => onViewAccounts(website.id)}  // 传递 ID
   onClick={() => onEdit(website)}              // 传递整个对象
   onClick={() => onDelete(website)}            // 传递整个对象
   ```

### 问题 4: 响应式布局不工作

**错误信息**: 在小屏幕上标签列仍然显示，或表格溢出

**解决方案**:
1. 确认使用了 Tailwind 的响应式 class：
   ```tsx
   <TableHead className="hidden md:table-cell">标签</TableHead>
   <TableCell className="hidden md:table-cell">{website.tags}</TableCell>
   ```

2. 为表格容器添加横向滚动：
   ```tsx
   <div className="rounded-md border overflow-x-auto">
     <Table>
       {/* ... */}
     </Table>
   </div>
   ```

---

## Performance Validation

### 成功标准验证

根据 spec.md 中定义的成功标准，验证性能：

1. **SC-001**: 显示数量提升
   - **测试方法**: 在相同屏幕尺寸 (如 1920x1080) 下，对比卡片视图和表格视图可见的网站数量
   - **目标**: 表格视图至少是卡片视图的 2 倍
   - **示例**: 卡片视图显示 3 个网站，表格视图应显示至少 6 个网站

2. **SC-002**: 快速定位
   - **测试方法**: 添加 20 个网站，计时从页面加载到定位特定网站的时间
   - **目标**: 3 秒内完成视觉扫描

3. **SC-003**: 响应时间保持
   - **测试方法**: 使用浏览器开发者工具 (F12 → Performance) 记录按钮点击响应时间
   - **目标**: 操作按钮响应时间 < 100ms

4. **SC-004**: 页面渲染时间
   - **测试方法**: 在 Network 面板中模拟 100 个网站的响应 (分页后每页 10 个)，记录渲染时间
   - **目标**: < 2 秒

5. **SC-005**: 易用性
   - **测试方法**: 邀请团队成员或用户首次使用表格视图，观察是否能成功完成操作
   - **目标**: 95% 的用户成功完成查看账号、编辑或删除操作

---

## Next Steps

完成快速启动后，您可以：

1. **运行任务列表生成命令**:
   ```bash
   /speckit.tasks  # 生成详细的实施任务列表
   ```

2. **查看计划文档**:
   - [plan.md](./plan.md) - 实施计划详解
   - [spec.md](./spec.md) - 功能规格说明

3. **提交更改**:
   ```bash
   git add .
   git commit -m "feat[ui]: 实现网站列表表格视图

   - 添加 shadcn/ui Table 组件
   - 重构 WebsiteList 组件为表格布局
   - 保持现有分页和对话框功能
   - 支持响应式布局

   完成 002-http-localhost-5173"
   ```

4. **更新 CLAUDE.md** (可选):
   如果发现表格组件有值得记录的使用模式或最佳实践，可以添加到项目的 CLAUDE.md 文件中。

---

## Useful Commands

### 前端开发

```bash
# 启动开发服务器
cd frontend
pnpm dev

# 构建生产版本
pnpm build

# 运行 linter
pnpm lint

# 格式化代码
pnpm format

# 添加 shadcn/ui 组件
npx shadcn@latest add [component-name]
```

### Git 操作

```bash
# 查看当前分支
git branch

# 查看修改
git status
git diff

# 提交更改
git add .
git commit -m "feat[ui]: 描述"

# 查看提交历史
git log --oneline
```

---

## Resources

- **shadcn/ui Table 文档**: https://ui.shadcn.com/docs/components/table
- **Tailwind CSS 响应式设计**: https://tailwindcss.com/docs/responsive-design
- **React Hooks 文档**: https://react.dev/reference/react
- **TypeScript 手册**: https://www.typescriptlang.org/docs/

---

## Support

遇到问题？

1. 查看 [Troubleshooting](#troubleshooting) 章节
2. 检查浏览器控制台 (F12) 的错误信息
3. 查看项目的 Issues 列表

Happy Coding! 🚀
