import { test, expect, Page } from '@playwright/test'

const BASE_URL = process.env.BASE_URL || 'http://localhost:5173'
const MASTER_PASSWORD = 'TestPassword123!'

/**
 * 密码生成器 E2E 测试
 * 测试密码生成功能的完整用户流程
 */

// Helper: 初始化 vault 并解锁
async function initializeAndUnlock(page: Page) {
  await page.goto(BASE_URL)

  // Check if already unlocked (回收站 button visible)
  const recycleBinButton = page.getByRole('button', { name: /回收站/ })
  if (await recycleBinButton.isVisible()) {
    return // Already unlocked
  }

  // Check if needs initialization
  const initButton = page.getByRole('button', { name: /初始化 Vault/ })
  if (await initButton.isVisible()) {
    // Initialize vault
    await page.getByPlaceholder(/设置主密码/).fill(MASTER_PASSWORD)
    await page.getByPlaceholder(/确认主密码/).fill(MASTER_PASSWORD)
    await initButton.click()
    await expect(recycleBinButton).toBeVisible({ timeout: 10000 })
  } else {
    // Unlock vault
    const unlockButton = page.getByRole('button', { name: /解锁/ })
    if (await unlockButton.isVisible()) {
      await page.getByPlaceholder(/输入主密码/).fill(MASTER_PASSWORD)
      await unlockButton.click()
      await expect(recycleBinButton).toBeVisible({ timeout: 10000 })
    }
  }
}

// Helper: 创建测试网站
async function createTestWebsite(page: Page, domain: string) {
  await page.getByRole('button', { name: /添加网站/ }).click()
  await page.getByLabel(/域名/).fill(domain)
  await page.getByLabel(/显示名称/).fill(`测试网站 - ${domain}`)
  await page.getByRole('button', { name: /^创建$/ }).click()
  await expect(page.getByText(domain)).toBeVisible({ timeout: 5000 })
}

test.describe('密码生成器 E2E 测试', () => {
  test.beforeEach(async ({ page }) => {
    await initializeAndUnlock(page)
  })

  test('应该能在创建账号时打开密码生成器', async ({ page }) => {
    // 确保有网站
    await createTestWebsite(page, 'password-test1.com')

    // 点击查看账号
    await page.getByRole('button', { name: /查看账号/ }).first().click()

    // 等待页面加载
    await expect(page.getByRole('heading', { name: /账号列表/ })).toBeVisible()

    // 点击添加账号
    await page.getByRole('button', { name: /添加账号/ }).click()

    // 打开密码生成器
    await page.getByRole('button', { name: /生成密码/ }).click()

    // 验证密码生成器对话框打开
    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()
  })

  test('应该能生成密码并显示强度信息', async ({ page }) => {
    await createTestWebsite(page, 'password-test2.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByRole('button', { name: /生成密码/ }).click()

    // 等待密码生成（对话框打开时自动生成）
    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 验证密码已生成（不是空状态）
    const passwordDisplay = page.locator('.font-mono').first()
    await expect(passwordDisplay).not.toHaveText('...')

    // 验证强度信息显示
    await expect(page.getByText(/分数/)).toBeVisible()
    await expect(page.locator('.rounded-full.bg-primary')).toBeVisible() // 强度进度条
  })

  test('应该能配置密码长度并重新生成', async ({ page }) => {
    await createTestWebsite(page, 'password-test3.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByRole('button', { name: /生成密码/ }).click()

    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 验证默认长度为 16
    await expect(page.getByText('16').nth(1)).toBeVisible() // nth(1) 因为第一个可能是其他16

    // 点击重新生成按钮
    const generateButton = page.getByRole('button', { name: /重新生成/ })
    await generateButton.click()

    // 等待生成完成
    await page.waitForTimeout(500)

    // 验证密码仍然存在
    const passwordDisplay = page.locator('.font-mono').first()
    await expect(passwordDisplay).not.toHaveText('...')
  })

  test('应该能切换字符类型选项', async ({ page }) => {
    await createTestWebsite(page, 'password-test4.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByRole('button', { name: /生成密码/ }).click()

    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 获取初始密码
    const passwordDisplay = page.locator('.font-mono').first()
    const initialPassword = await passwordDisplay.textContent()

    // 取消勾选符号
    const symbolsCheckbox = page.getByLabel(/符号/)
    await symbolsCheckbox.click()

    // 等待重新生成
    await page.waitForTimeout(500)

    // 验证密码已更改
    const newPassword = await passwordDisplay.textContent()
    expect(newPassword).not.toBe(initialPassword)
    expect(newPassword).not.toBe('...')
  })

  test('应该能复制生成的密码', async ({ page }) => {
    await createTestWebsite(page, 'password-test5.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByRole('button', { name: /生成密码/ }).click()

    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 等待密码生成
    await page.waitForTimeout(1000)

    // 点击复制按钮
    const copyButton = page.locator('button[type="button"]').filter({
      has: page.locator('svg.lucide-copy, svg.lucide-check'),
    }).first()

    await copyButton.click()

    // 验证复制成功（按钮变成勾选图标）
    await expect(page.locator('svg.lucide-check')).toBeVisible({ timeout: 2000 })
  })

  test('应该能使用生成的密码创建账号', async ({ page }) => {
    await createTestWebsite(page, 'password-test6.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()

    // 填写用户名
    await page.getByLabel(/用户名/).fill('testuser')

    // 打开密码生成器
    await page.getByRole('button', { name: /生成密码/ }).click()
    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 等待密码生成
    await page.waitForTimeout(1000)

    // 获取生成的密码
    const passwordDisplay = page.locator('.font-mono').first()
    const generatedPassword = await passwordDisplay.textContent()

    // 接受密码
    await page.getByRole('button', { name: /使用此密码/ }).click()

    // 验证对话框关闭
    await expect(page.getByRole('heading', { name: '密码生成器' })).not.toBeVisible()

    // 验证密码已填入
    // 注意：密码字段可能是隐藏的，需要先显示
    const showPasswordButton = page.getByRole('button', { name: /显示密码|隐藏密码/ })
    if (await showPasswordButton.isVisible()) {
      await showPasswordButton.click()
    }

    // 创建账号
    await page.getByRole('button', { name: /^创建$/ }).click()

    // 验证账号创建成功
    await expect(page.getByText('testuser')).toBeVisible({ timeout: 5000 })
  })

  test('应该能排除易混淆字符', async ({ page }) => {
    await createTestWebsite(page, 'password-test7.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByRole('button', { name: /生成密码/ }).click()

    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 勾选排除易混淆字符
    const excludeAmbiguousCheckbox = page.getByLabel(/排除易混淆字符/)
    await excludeAmbiguousCheckbox.click()

    // 等待重新生成
    await page.waitForTimeout(1000)

    // 获取生成的密码
    const passwordDisplay = page.locator('.font-mono').first()
    const password = await passwordDisplay.textContent()

    // 验证密码不包含易混淆字符 (0, O, 1, l, I)
    expect(password).not.toContain('0')
    expect(password).not.toContain('O')
    expect(password).not.toContain('1')
    expect(password).not.toContain('l')
    expect(password).not.toContain('I')
  })

  test('应该能在编辑账号时使用密码生成器', async ({ page }) => {
    // 先创建一个账号
    await createTestWebsite(page, 'password-test8.com')
    await page.getByRole('button', { name: /查看账号/ }).first().click()
    await page.getByRole('button', { name: /添加账号/ }).click()
    await page.getByLabel(/用户名/).fill('edituser')
    await page.getByLabel(/密码/).fill('oldpassword')
    await page.getByRole('button', { name: /^创建$/ }).click()
    await expect(page.getByText('edituser')).toBeVisible()

    // 编辑账号
    await page.getByRole('button', { name: /编辑/ }).first().click()

    // 打开密码生成器
    await page.getByRole('button', { name: /生成密码/ }).click()
    await expect(page.getByRole('heading', { name: '密码生成器' })).toBeVisible()

    // 等待密码生成
    await page.waitForTimeout(1000)

    // 使用生成的密码
    await page.getByRole('button', { name: /使用此密码/ }).click()

    // 保存
    await page.getByRole('button', { name: /保存/ }).click()

    // 验证编辑成功
    await expect(page.getByText('edituser')).toBeVisible({ timeout: 5000 })
  })
})
