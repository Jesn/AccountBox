import { test, expect, Page } from '@playwright/test'

/**
 * 网站删除保护 E2E 测试
 * 测试完整的用户交互流程：阻止删除、确认删除
 */

const BASE_URL = process.env.BASE_URL || 'http://localhost:5173'
const MASTER_PASSWORD = 'TestPassword123!'

// 辅助函数：初始化和解锁 vault
async function initializeAndUnlockVault(page: Page) {
  await page.goto(`${BASE_URL}/`)

  // 如果需要初始化
  const initializeButton = page.getByRole('button', { name: /设置主密码|initialize/i })
  if (await initializeButton.isVisible().catch(() => false)) {
    await page.getByLabel(/主密码|master password/i).fill(MASTER_PASSWORD)
    await page.getByLabel(/确认密码|confirm password/i).fill(MASTER_PASSWORD)
    await initializeButton.click()
  }

  // 解锁 vault
  const unlockButton = page.getByRole('button', { name: /解锁|unlock/i })
  if (await unlockButton.isVisible().catch(() => false)) {
    await page.getByLabel(/主密码|master password/i).fill(MASTER_PASSWORD)
    await unlockButton.click()
  }

  // 等待进入网站管理页面
  await expect(page).toHaveURL(/\/websites/)
  await expect(page.getByRole('heading', { name: /网站管理/i })).toBeVisible()
}

// 辅助函数：创建网站
async function createWebsite(page: Page, domain: string, displayName: string) {
  await page.getByRole('button', { name: /添加网站/i }).click()
  await page.getByLabel(/域名|domain/i).fill(domain)
  await page.getByLabel(/显示名|display name/i).fill(displayName)
  await page.getByRole('button', { name: /确认|create|保存/i }).click()

  // 等待对话框关闭
  await expect(page.getByRole('dialog')).not.toBeVisible()

  // 等待新网站出现在列表中
  await expect(page.getByText(displayName)).toBeVisible()
}

// 辅助函数：创建账号
async function createAccount(page: Page, websiteName: string, username: string, password: string) {
  // 点击"查看账号"按钮
  const websiteRow = page.locator('tr', { hasText: websiteName })
  await websiteRow.getByRole('button', { name: /查看账号/i }).click()

  // 等待跳转到账号页面
  await expect(page).toHaveURL(/\/websites\/\d+\/accounts/)

  // 添加账号
  await page.getByRole('button', { name: /添加账号/i }).click()
  await page.getByLabel(/用户名|username/i).fill(username)
  await page.getByLabel(/密码|password/i).fill(password)
  await page.getByRole('button', { name: /确认|create|保存/i }).click()

  // 等待对话框关闭
  await expect(page.getByRole('dialog')).not.toBeVisible()

  // 等待新账号出现
  await expect(page.getByText(username)).toBeVisible()

  // 返回网站列表
  await page.goto(`${BASE_URL}/websites`)
}

// 辅助函数：软删除账号
async function softDeleteAccount(page: Page, websiteName: string, username: string) {
  // 进入账号页面
  const websiteRow = page.locator('tr', { hasText: websiteName })
  await websiteRow.getByRole('button', { name: /查看账号/i }).click()

  await expect(page).toHaveURL(/\/websites\/\d+\/accounts/)

  // 删除账号
  const accountRow = page.locator('tr', { hasText: username })
  await accountRow.getByRole('button', { name: /删除/i }).click()

  // 确认删除
  await page.getByRole('dialog').getByRole('button', { name: /确认删除|confirm/i }).click()

  // 等待对话框关闭
  await expect(page.getByRole('dialog')).not.toBeVisible()

  // 返回网站列表
  await page.goto(`${BASE_URL}/websites`)
}

test.describe('网站删除保护流程', () => {
  test.beforeEach(async ({ page }) => {
    await initializeAndUnlockVault(page)
  })

  test('应该阻止删除有活跃账号的网站', async ({ page }) => {
    // Arrange: 创建网站和活跃账号
    const websiteName = 'Test Site with Active Account'
    const domain = 'test-active.com'

    await createWebsite(page, domain, websiteName)
    await createAccount(page, websiteName, 'activeuser', 'password123')

    // Act: 尝试删除网站
    const websiteRow = page.locator('tr', { hasText: websiteName })
    await websiteRow.getByRole('button', { name: /删除/i }).click()

    // Assert: 应该显示错误提示
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/无法删除网站/i)).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/活跃账号/i)).toBeVisible()

    // 删除按钮应该不可用或显示错误
    const confirmButton = page.getByRole('dialog').getByRole('button', { name: /确认删除|confirm/i })
    await expect(confirmButton).toBeDisabled().catch(() => {
      // 如果不是禁用的，至少应该显示错误信息
      return expect(page.getByRole('dialog').getByText(/请先删除/i)).toBeVisible()
    })

    // 关闭对话框
    await page.getByRole('dialog').getByRole('button', { name: /取消|cancel/i }).click()

    // 验证网站仍然存在
    await expect(page.getByText(websiteName)).toBeVisible()
  })

  test('应该要求确认删除只有回收站账号的网站', async ({ page }) => {
    // Arrange: 创建网站、创建账号、软删除账号
    const websiteName = 'Test Site with Deleted Account'
    const domain = 'test-deleted.com'

    await createWebsite(page, domain, websiteName)
    await createAccount(page, websiteName, 'deleteduser', 'password123')
    await softDeleteAccount(page, websiteName, 'deleteduser')

    // Act: 第一次尝试删除网站
    let websiteRow = page.locator('tr', { hasText: websiteName })
    await websiteRow.getByRole('button', { name: /删除/i }).click()

    // Assert: 应该显示确认提示
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/回收站/i)).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/永久删除/i)).toBeVisible()

    // 确认按钮应该显示为"确认永久删除"
    const confirmButton = page.getByRole('dialog').getByRole('button', { name: /确认永久删除/i })
    await expect(confirmButton).toBeVisible()
    await expect(confirmButton).toBeEnabled()

    // Act: 确认删除
    await confirmButton.click()

    // Assert: 对话框应该关闭，网站应该被删除
    await expect(page.getByRole('dialog')).not.toBeVisible()
    await expect(page.getByText(websiteName)).not.toBeVisible()
  })

  test('应该允许直接删除没有账号的网站', async ({ page }) => {
    // Arrange: 创建网站（不创建账号）
    const websiteName = 'Empty Test Site'
    const domain = 'test-empty.com'

    await createWebsite(page, domain, websiteName)

    // Act: 删除网站
    const websiteRow = page.locator('tr', { hasText: websiteName })
    await websiteRow.getByRole('button', { name: /删除/i }).click()

    // Assert: 应该显示标准删除确认对话框（不是错误或警告）
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/确认删除/i)).toBeVisible()

    // 不应该显示活跃账号或回收站的警告
    await expect(page.getByRole('dialog').getByText(/活跃账号/i)).not.toBeVisible()
    await expect(page.getByRole('dialog').getByText(/回收站/i)).not.toBeVisible()

    // 确认删除
    const confirmButton = page.getByRole('dialog').getByRole('button', { name: /确认删除|confirm/i })
    await expect(confirmButton).toBeEnabled()
    await confirmButton.click()

    // Assert: 对话框应该关闭，网站应该被删除
    await expect(page.getByRole('dialog')).not.toBeVisible()
    await expect(page.getByText(websiteName)).not.toBeVisible()
  })

  test('应该在删除前显示正确的账号统计信息', async ({ page }) => {
    // Arrange: 创建网站和多个账号（部分在回收站）
    const websiteName = 'Site with Mixed Accounts'
    const domain = 'test-mixed.com'

    await createWebsite(page, domain, websiteName)
    await createAccount(page, websiteName, 'activeuser1', 'password123')
    await createAccount(page, websiteName, 'activeuser2', 'password123')
    await createAccount(page, websiteName, 'deleteduser1', 'password123')
    await softDeleteAccount(page, websiteName, 'deleteduser1')

    // Act: 尝试删除网站
    const websiteRow = page.locator('tr', { hasText: websiteName })
    await websiteRow.getByRole('button', { name: /删除/i }).click()

    // Assert: 应该显示账号统计信息
    await expect(page.getByRole('dialog')).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/活跃账号.*2/i)).toBeVisible()
    await expect(page.getByRole('dialog').getByText(/回收站账号.*1/i)).toBeVisible()

    // 应该阻止删除（因为有活跃账号）
    await expect(page.getByRole('dialog').getByText(/无法删除/i)).toBeVisible()

    // 关闭对话框
    await page.getByRole('dialog').getByRole('button', { name: /取消|cancel/i }).click()
  })

  test('应该支持取消删除操作', async ({ page }) => {
    // Arrange: 创建网站（不创建账号）
    const websiteName = 'Site to Cancel Delete'
    const domain = 'test-cancel.com'

    await createWebsite(page, domain, websiteName)

    // Act: 打开删除对话框
    const websiteRow = page.locator('tr', { hasText: websiteName })
    await websiteRow.getByRole('button', { name: /删除/i }).click()

    await expect(page.getByRole('dialog')).toBeVisible()

    // 取消删除
    await page.getByRole('dialog').getByRole('button', { name: /取消|cancel/i }).click()

    // Assert: 对话框应该关闭，网站应该仍然存在
    await expect(page.getByRole('dialog')).not.toBeVisible()
    await expect(page.getByText(websiteName)).toBeVisible()
  })
})
