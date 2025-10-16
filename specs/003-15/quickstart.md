# 快速启动指南：表格视图增强

**功能**: 表格视图增强 - 紧凑显示、搜索与分页优化
**分支**: `003-15`
**相关文档**: [spec.md](./spec.md) | [plan.md](./plan.md)

## 前提条件

### 环境要求

- Node.js >= 20.19.0
- pnpm 包管理器
- 前端开发服务器可运行
- 后端 API 服务可访问（http://localhost:5093）

### 验证环境

```bash
# 检查 Node.js 版本
node --version  # 应该 >= v20.19.0

# 检查 pnpm
pnpm --version

# 启动前端开发服务器
cd frontend
pnpm dev  # 应该在 http://localhost:5173 启动

# 验证后端 API（另开一个终端）
curl http://localhost:5093/health  # 或访问任意端点验证服务运行
```

### 测试数据准备

为了充分测试本功能，建议准备以下测试数据：

- **网站数量**: 至少 25 个网站（用于测试紧凑显示、分页和搜索）
- **账号数量**: 为某个网站添加至少 25 个账号（用于测试账号列表表格化和分页）
- **变体数据**: 包含不同长度的显示名称、域名、备注，以测试截断和悬停功能

## 实施步骤

### User Story 1: 网站列表紧凑显示和分页优化 (P1)

**目标**: 减小表格行高和内边距，将每页显示数量从 10 条增加到 15 条。

#### Step 1.1: 修改 WebsiteList.tsx 添加紧凑模式样式

**文件**: `frontend/src/components/websites/WebsiteList.tsx`

**修改内容**:

1. 为 Table 组件添加紧凑文本大小：

```typescript
// 找到这一行（大约第 66 行）
<div className="rounded-md border overflow-x-auto">
  <Table>

// 修改为
<div className="rounded-md border overflow-x-auto">
  <Table className="text-sm">
```

2. 为 TableHead 减小高度：

```typescript
// 找到所有 TableHead 标签，添加 h-10 类名
<TableHead className="h-10">显示名</TableHead>
<TableHead className="h-10">域名</TableHead>
<TableHead className="hidden md:table-cell h-10">标签</TableHead>
<TableHead className="h-10">活跃账号</TableHead>
<TableHead className="h-10">回收站</TableHead>
<TableHead className="text-right h-10">操作</TableHead>
```

3. 为 TableCell 减小内边距：

```typescript
// 找到所有 TableCell 标签，将 p-4 改为 py-2 px-3
// 例如，找到这一行
<TableCell
  className="font-medium max-w-xs truncate"
  title={website.displayName || website.domain}
>

// 修改为
<TableCell
  className="font-medium max-w-xs truncate py-2 px-3"
  title={website.displayName || website.domain}
>

// 对所有其他 TableCell 做相同修改
<TableCell className="max-w-xs truncate py-2 px-3" title={website.domain}>
<TableCell className="hidden md:table-cell py-2 px-3">
<TableCell className="py-2 px-3">{website.activeAccountCount}</TableCell>
<TableCell className="py-2 px-3">{website.deletedAccountCount}</TableCell>
<TableCell className="text-right py-2 px-3">
```

#### Step 1.2: 修改 WebsitesPage.tsx 调整分页大小

**文件**: `frontend/src/pages/WebsitesPage.tsx`

**修改内容**:

```typescript
// 找到这一行（大约第 31 行）
const pageSize = 10

// 修改为
const pageSize = 15
```

#### Step 1.3: 验证紧凑显示和分页

1. 访问 http://localhost:5173/websites
2. ✅ 验证表格行高明显减小，信息更紧凑
3. ✅ 验证第一页显示 15 个网站（如总数 >=15）
4. ✅ 验证分页器显示正确的总页数（如 25 个网站应显示"第1页，共2页"）
5. ✅ 验证鼠标悬停行有明显的背景色高亮

**提交代码**:

```bash
git add frontend/src/components/websites/WebsiteList.tsx frontend/src/pages/WebsitesPage.tsx
git commit -m "feat[ui]: 实现网站列表紧凑显示和分页优化

- 为网站列表表格添加紧凑模式样式（text-sm, h-10, py-2 px-3）
- 调整分页大小从10条改为15条
- 提升单屏显示网站数量至少50%

完成 User Story 1"
```

---

### User Story 2: 网站列表快速搜索 (P2)

**目标**: 添加搜索框，支持按域名或显示名称实时过滤网站列表。

#### Step 2.1: 在 WebsitesPage.tsx 添加搜索状态和 UI

**文件**: `frontend/src/pages/WebsitesPage.tsx`

**修改内容**:

1. 导入 Input 组件：

```typescript
// 在文件顶部添加导入（大约第6行之后）
import { Input } from '@/components/ui/input'
import { Search, X } from 'lucide-react'
```

2. 添加搜索状态：

```typescript
// 在 useState 声明区域添加（大约第30行之后）
const [searchQuery, setSearchQuery] = useState('')
```

3. 添加前端过滤逻辑：

```typescript
// 在 loadWebsites 函数之后添加（大约第51行之后）
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

4. 添加搜索框 UI：

```typescript
// 在页面标题和按钮组之间添加（大约第125行，</div> 之后）
// 找到这个结构
        </div>
      </div>

      <WebsiteList

// 修改为
        </div>

        {/* 搜索框 */}
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
      </div>

      <WebsiteList
```

5. 修改 WebsiteList 组件使用过滤后的数据：

```typescript
// 找到 <WebsiteList 调用（大约第134行）
      <WebsiteList
        websites={websites}  // 修改这一行
        isLoading={isLoading}

// 修改为
      <WebsiteList
        websites={filteredWebsites}
        isLoading={isLoading}
```

6. 根据搜索结果调整显示逻辑：

```typescript
// 找到分页器显示条件（大约第136行）
      {!isLoading && websites.length > 0 && (

// 修改为
      {!isLoading && filteredWebsites.length > 0 && (
```

7. 添加 useMemo 导入：

```typescript
// 在文件顶部修改 React 导入（第1行）
import { useState, useEffect, useCallback } from 'react'

// 修改为
import { useState, useEffect, useCallback, useMemo } from 'react'
```

#### Step 2.2: 添加搜索结果为空的提示

**修改内容**:

在 `WebsiteList.tsx` 中修改空状态逻辑，支持搜索无结果：

```typescript
// 找到空状态部分（大约第 47 行）
  // 空状态
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
```

由于搜索是在父组件过滤，WebsiteList 收到的就是过滤后的空数组，因此可以复用现有空状态。但为了更友好，可以在 WebsitesPage.tsx 添加特殊处理：

```typescript
// 在 <WebsiteList /> 之前添加搜索无结果提示
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

      {(!searchQuery || filteredWebsites.length > 0) && (
        <>
          <WebsiteList
            websites={filteredWebsites}
            isLoading={isLoading}
            onViewAccounts={handleViewAccounts}
            onEdit={handleEditWebsite}
            onDelete={handleDeleteWebsite}
            onCreateNew={() => setShowCreateWebsiteDialog(true)}
          />

          {!isLoading && filteredWebsites.length > 0 && (
            <Pagination
              currentPage={currentPage}
              totalPages={totalPages}
              onPageChange={setCurrentPage}
            />
          )}
        </>
      )}
```

#### Step 2.3: 验证搜索功能

1. 访问 http://localhost:5173/websites
2. ✅ 在搜索框输入 "google"，验证立即过滤出包含 "google" 的网站
3. ✅ 搜索不区分大小写（输入 "git" 能匹配 "GitHub"）
4. ✅ 搜索部分匹配（输入 "exam" 能匹配 "example.com"）
5. ✅ 搜索无结果时显示"未找到匹配的网站"提示
6. ✅ 点击搜索框的 X 图标或"清空搜索"按钮，恢复显示所有网站
7. ✅ 搜索后分页器根据过滤结果重新计算

**提交代码**:

```bash
git add frontend/src/pages/WebsitesPage.tsx
git commit -m "feat[ui]: 实现网站列表实时搜索功能

- 添加搜索框组件（Input with Search and X icons）
- 实现前端实时过滤（域名和显示名称，不区分大小写）
- 添加搜索无结果的友好提示
- 搜索结果自动重新计算分页

完成 User Story 2"
```

---

### User Story 3: 账号列表表格化显示 (P3)

**目标**: 将账号列表从卡片布局改为表格布局，并添加分页功能。

#### Step 3.1: 重构 AccountList.tsx 为表格布局

**文件**: `frontend/src/components/accounts/AccountList.tsx`

**修改内容**:

1. 修改导入：

```typescript
// 移除 Card 导入（第1行）
import { Card, CardContent } from '@/components/ui/card'

// 改为导入 Table 组件
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Card, CardContent } from '@/components/ui/card'  // 保留用于空状态
```

2. 重写列表部分为表格：

```typescript
// 找到返回的主体部分（大约第 53 行开始）
  return (
    <div className="grid gap-4">
      {accounts.map((account) => (
        <Card key={account.id}>
          <CardContent className="p-4">
            // ... 整个 Card 内容
          </CardContent>
        </Card>
      ))}
    </div>
  )

// 完全替换为表格结构
  return (
    <div className="rounded-md border overflow-x-auto">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>用户名</TableHead>
            <TableHead>密码</TableHead>
            <TableHead className="hidden md:table-cell">标签</TableHead>
            <TableHead className="hidden lg:table-cell">备注</TableHead>
            <TableHead className="hidden xl:table-cell">创建时间</TableHead>
            <TableHead className="hidden xl:table-cell">更新时间</TableHead>
            <TableHead className="text-right">操作</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {accounts.map((account) => (
            <TableRow key={account.id}>
              {/* 用户名 */}
              <TableCell className="font-medium">
                {account.username}
              </TableCell>

              {/* 密码（可切换显示/隐藏）*/}
              <TableCell>
                <div className="flex items-center gap-2">
                  <code className="text-sm font-mono">
                    {visiblePasswords.has(account.id)
                      ? account.password
                      : '••••••••'}
                  </code>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => togglePasswordVisibility(account.id)}
                  >
                    {visiblePasswords.has(account.id) ? (
                      <EyeOff className="h-4 w-4" />
                    ) : (
                      <Eye className="h-4 w-4" />
                    )}
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => copyToClipboard(account.password)}
                  >
                    <Copy className="h-4 w-4" />
                  </Button>
                </div>
              </TableCell>

              {/* 标签 */}
              <TableCell className="hidden md:table-cell">
                {account.tags || '-'}
              </TableCell>

              {/* 备注（截断显示）*/}
              <TableCell className="hidden lg:table-cell max-w-xs truncate" title={account.notes || ''}>
                {account.notes || '-'}
              </TableCell>

              {/* 创建时间 */}
              <TableCell className="hidden xl:table-cell text-sm text-gray-500">
                {new Date(account.createdAt).toLocaleString('zh-CN')}
              </TableCell>

              {/* 更新时间 */}
              <TableCell className="hidden xl:table-cell text-sm text-gray-500">
                {new Date(account.updatedAt).toLocaleString('zh-CN')}
              </TableCell>

              {/* 操作 */}
              <TableCell className="text-right">
                <div className="flex gap-2 justify-end">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => onEdit(account)}
                  >
                    编辑
                  </Button>
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => onDelete(account)}
                  >
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
```

#### Step 3.2: 在 AccountsPage.tsx 添加分页功能

**文件**: `frontend/src/pages/AccountsPage.tsx`

**修改内容**:

1. 导入 Pagination 组件：

```typescript
// 在导入区域添加（大约第12行之后）
import Pagination from '@/components/common/Pagination'
```

2. 添加分页状态：

```typescript
// 在 useState 声明区域添加（大约第29行之后）
const [currentPage, setCurrentPage] = useState(1)
const [totalPages, setTotalPages] = useState(1)
const pageSize = 15
```

3. 修改 loadAccounts 函数支持分页：

```typescript
// 找到 loadAccounts 函数（大约第36行）
  const loadAccounts = async () => {
    if (!websiteId) return

    setIsLoading(true)
    try {
      const response = await accountService.getByWebsite(Number(websiteId))
      if (response.success && response.data) {
        setAccounts(response.data as AccountResponse[])
      }
    } catch (error) {
      console.error('加载账号列表失败:', error)
    } finally {
      setIsLoading(false)
    }
  }

// 修改为支持分页
  const loadAccounts = async () => {
    if (!websiteId) return

    setIsLoading(true)
    try {
      const response = await accountService.getByWebsite(
        Number(websiteId),
        currentPage,
        pageSize
      )
      if (response.success && response.data) {
        setAccounts(response.data.items as AccountResponse[])
        setTotalPages(response.data.totalPages)
      }
    } catch (error) {
      console.error('加载账号列表失败:', error)
    } finally {
      setIsLoading(false)
    }
  }
```

4. 添加 currentPage 到 useEffect 依赖：

```typescript
// 找到 useEffect（大约第35行）
  useEffect(() => {
    loadAccounts()
    loadWebsite()
  }, [websiteId])

// 修改为
  useEffect(() => {
    loadAccounts()
    loadWebsite()
  }, [websiteId, currentPage])
```

5. 添加 Pagination 组件到页面底部：

```typescript
// 找到 AccountList 组件调用之后（大约第150行）
      <AccountList
        accounts={accounts}
        onEdit={handleEditAccount}
        onDelete={handleDeleteAccount}
      />

// 添加分页器
      <AccountList
        accounts={accounts}
        onEdit={handleEditAccount}
        onDelete={handleDeleteAccount}
      />

      {!isLoading && accounts.length > 0 && (
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={setCurrentPage}
        />
      )}
```

#### Step 3.3: 验证账号列表表格化和分页

1. 为某个网站添加 25+ 个账号
2. 访问该网站的账号列表页面（点击网站管理页的"查看账号"）
3. ✅ 验证账号以表格形式展示，包含7列
4. ✅ 验证第一页显示 15 个账号
5. ✅ 验证密码列默认为 "••••••••"
6. ✅ 点击"眼睛"图标，验证密码切换为明文显示
7. ✅ 点击"复制"图标，验证密码被复制到剪贴板
8. ✅ 验证分页器显示正确（如 25 个账号应显示"第1页，共2页"）
9. ✅ 点击"下一页"，验证切换到第二页显示剩余账号
10. ✅ 验证长备注截断显示，鼠标悬停显示完整内容

**提交代码**:

```bash
git add frontend/src/components/accounts/AccountList.tsx frontend/src/pages/AccountsPage.tsx
git commit -m "feat[ui]: 实现账号列表表格化显示和分页功能

- 重构 AccountList 从卡片布局改为表格布局
- 包含7列：用户名、密码、标签、备注、创建时间、更新时间、操作
- 保持密码显示/隐藏切换和复制功能
- 添加分页支持（每页15条）
- 备注列截断显示，悬停显示完整内容
- 响应式设计：小屏幕隐藏次要列

完成 User Story 3"
```

---

### User Story 4: 账号列表紧凑显示优化 (P4)

**目标**: 为账号列表表格添加紧凑模式样式，与网站列表保持一致。

#### Step 4.1: 为 AccountList 表格添加紧凑模式样式

**文件**: `frontend/src/components/accounts/AccountList.tsx`

**修改内容**:

1. 为 Table 组件添加紧凑文本大小：

```typescript
// 找到 Table 标签（User Story 3 中添加的）
    <div className="rounded-md border overflow-x-auto">
      <Table>

// 修改为
    <div className="rounded-md border overflow-x-auto">
      <Table className="text-sm">
```

2. 为 TableHead 减小高度：

```typescript
// 找到所有 TableHead 标签，添加 h-10 类名
<TableHead className="h-10">用户名</TableHead>
<TableHead className="h-10">密码</TableHead>
<TableHead className="hidden md:table-cell h-10">标签</TableHead>
<TableHead className="hidden lg:table-cell h-10">备注</TableHead>
<TableHead className="hidden xl:table-cell h-10">创建时间</TableHead>
<TableHead className="hidden xl:table-cell h-10">更新时间</TableHead>
<TableHead className="text-right h-10">操作</TableHead>
```

3. 为 TableCell 减小内边距：

```typescript
// 找到所有 TableCell 标签，添加 py-2 px-3 类名
<TableCell className="font-medium py-2 px-3">
<TableCell className="py-2 px-3">
<TableCell className="hidden md:table-cell py-2 px-3">
<TableCell className="hidden lg:table-cell max-w-xs truncate py-2 px-3" title={account.notes || ''}>
<TableCell className="hidden xl:table-cell text-sm text-gray-500 py-2 px-3">
<TableCell className="text-right py-2 px-3">
```

#### Step 4.2: 验证紧凑模式

1. 访问账号列表页面
2. ✅ 验证表格行高明显减小，单屏显示更多账号
3. ✅ 验证信息仍然清晰可读，无重叠或拥挤
4. ✅ 验证鼠标悬停行有明显的背景色高亮
5. ✅ 验证操作按钮仍然易于点击，无误触其他行的按钮
6. ✅ 对比优化前后，单屏可见账号数量增加至少 30%

**提交代码**:

```bash
git add frontend/src/components/accounts/AccountList.tsx
git commit -m "feat[ui]: 为账号列表添加紧凑显示模式

- 添加紧凑模式样式（text-sm, h-10, py-2 px-3）
- 提升单屏显示账号数量至少30%
- 保持良好的可读性和可操作性

完成 User Story 4"
```

---

### Polish: 代码审查和最终验收

#### 代码审查

1. **运行 Linter**:

```bash
cd frontend
pnpm lint
```

修复所有错误和警告。

2. **运行格式化**:

```bash
pnpm format
```

3. **类型检查**:

```bash
pnpm build  # 验证 TypeScript 编译无错误
```

4. **检查代码质量**:
   - ✅ 无 console.log 或调试代码
   - ✅ 无未使用的 import
   - ✅ Props 接口正确定义
   - ✅ 遵循 React Hooks 最佳实践
   - ✅ 遵循 Tailwind CSS 类名约定

**提交代码审查修复**:

```bash
git add .
git commit -m "chore[ui]: 代码审查和格式化 - 表格视图增强

- 运行 ESLint 修复所有警告
- 运行 Prettier 格式化所有文件
- 确认 TypeScript strict mode 编译通过
- 清理调试代码

完成代码审查"
```

#### 完整验收清单

运行以下所有验收场景，确保全部通过：

**User Story 1: 网站列表紧凑显示和分页优化**

- [ ] **US1-1**: 添加25个网站，访问网站管理页面，第一页显示15个，表格行高紧凑，列间距合理
- [ ] **US1-2**: 分页器显示"第1页，共2页"，可点击跳转
- [ ] **US1-3**: 切换回第1页快速加载
- [ ] **US1-4**: 鼠标悬停某一行，该行有明显的背景色高亮

**User Story 2: 网站列表快速搜索**

- [ ] **US2-1**: 搜索"google"立即过滤出域名包含"google"的网站，分页器根据结果重新计算
- [ ] **US2-2**: 搜索不存在的关键词"xyz"，显示"未找到匹配的网站"提示
- [ ] **US2-3**: 点击搜索框的X图标或"清空搜索"按钮，搜索框清空，表格恢复显示所有网站
- [ ] **US2-4**: 搜索"git"（小写），"GitHub"（大写）仍然出现在结果中（不区分大小写）
- [ ] **US2-5**: 搜索"exam"，"example.com"出现在结果中（部分匹配）

**User Story 3: 账号列表表格化显示**

- [ ] **US3-1**: 某网站有25个账号，访问账号列表页面，第一页显示15个表格行，包含7列
- [ ] **US3-2**: 点击某账号的"眼睛"图标，密码从"••••••••"切换为明文，图标从Eye变为EyeOff
- [ ] **US3-3**: 点击"复制"图标，密码被复制到剪贴板（可粘贴验证）
- [ ] **US3-4**: 点击分页器的"下一页"，表格切换到第二页，显示剩余10个账号
- [ ] **US3-5**: 长备注（超过100字）截断显示前50字+"..."，鼠标悬停显示完整备注
- [ ] **US3-6**: 某网站暂无账号，访问账号列表页面，显示空状态卡片"暂无账号"

**User Story 4: 账号列表紧凑显示优化**

- [ ] **US4-1**: 1080p屏幕下，紧凑模式单屏可见账号数量比优化前增加至少30%
- [ ] **US4-2**: 鼠标悬停某一行，该行有明显的背景色高亮
- [ ] **US4-3**: 点击"眼睛"或"复制"图标，按钮响应准确，无误触其他行的按钮

**边界情况测试**

- [ ] 搜索结果刚好为15条时，分页器显示"第1页，共1页"
- [ ] 搜索结果刚好为30条时，分页器显示"第1页，共2页"
- [ ] 在第3页进行搜索，搜索结果只有1页，自动跳转到第1页
- [ ] 标签和备注为空的账号，表格单元格显示"-"
- [ ] 密码显示状态切换：切换到下一页后再返回，密码重置为隐藏状态

## 性能验证

### 搜索性能测试

1. 添加 100+ 个网站
2. 打开 Chrome DevTools → Performance 面板
3. 开始录制
4. 在搜索框输入关键词
5. 停止录制
6. ✅ 验证过滤响应时间 < 200ms

### 表格渲染性能测试

1. 添加 100+ 个网站或账号
2. 打开 Chrome DevTools → Performance 面板
3. 开始录制
4. 刷新页面
5. 停止录制
6. ✅ 验证页面渲染时间 < 2秒

### 交互响应性测试

1. 打开 Chrome DevTools → Performance 面板
2. 开始录制
3. 点击密码显示/隐藏图标、复制图标、分页按钮
4. 停止录制
5. ✅ 验证所有交互响应时间 < 100ms

## 故障排除

### 问题 1: 搜索框输入时表格闪烁

**原因**: 频繁的 re-render
**解决方案**: 确保使用了 `useMemo` 包装 `filteredWebsites` 计算

### 问题 2: 分页器显示不正确

**原因**: 搜索后未重新计算分页
**解决方案**: 确保分页器使用 `filteredWebsites.length` 而非 `websites.length`

### 问题 3: 账号列表空状态一直显示

**原因**: 分页参数传递错误
**解决方案**: 检查 `accountService.getByWebsite()` 调用时是否正确传递 `currentPage` 和 `pageSize`

### 问题 4: 紧凑模式下按钮难以点击

**原因**: 内边距过小
**解决方案**: 调整 `py-2 px-3` 为 `py-2.5 px-3.5`，或为按钮保持 `size="sm"` 的最小尺寸

### 问题 5: 长备注未截断

**原因**: `max-w-xs` 类名未生效
**解决方案**: 确保父容器没有 `overflow: hidden`，并检查 Tailwind CSS 配置

## 相关文档

- [功能规格说明](./spec.md) - 完整的用户故事和验收场景
- [实施计划](./plan.md) - 技术上下文和架构决策
- [shadcn/ui Table 文档](https://ui.shadcn.com/docs/components/table) - Table 组件使用指南
- [Tailwind CSS 响应式设计](https://tailwindcss.com/docs/responsive-design) - 响应式类名参考

## 下一步

完成所有验收场景后，运行：

```bash
/speckit.tasks  # 如果需要生成详细的任务列表
```

或直接进入下一个功能开发周期。
