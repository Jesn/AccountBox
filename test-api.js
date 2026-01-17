const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: true });

  const page = await browser.newPage();

  console.log('=== 开始测试 API ===');

  try {
    // 访问页面
    await page.goto('http://localhost:5173');
    console.log('✓ 页面加载成功');

    // 检查页面是否有内容
    const content = await page.content();
    console.log('页面内容:', content.substring(0, 200));

    // 测试登录
    const response = await page.evaluate(async () => {
      // 尝试获取登录按钮
      const loginButton = await page.$('[type="submit"], [name="password"]');
      if (loginButton) {
        await page.fill('[type="password"], 'admin123');
        await loginButton.click();
        console.log('✓ 已点击登录按钮');

        // 等待响应
        await page.waitForTimeout(5000);

        // 检查是否登录成功
        const bodyText = await page.body();
        console.log('页面 body:', bodyText.substring(0, 500));

        if (bodyText.includes('登录成功') || bodyText.includes('admin123')) {
          console.log('✓ 登录可能成功');

          // 获取 token
          const token = await page.evaluate(() => {
            // 尝试从 localStorage 或 network 获取 token
            return localStorage.getItem('token');
          });

          if (token) {
            console.log('✓ 找到 token:', token.substring(0, 50));

            // 测试 API keys
            const testResponse = await fetch('http://localhost:5093/api/apikeys', {
              headers: {
                'Authorization': `Bearer ${token}`,
                'Accept': 'application/json'
              }
            });

            const status = testResponse.status;
            const responseText = await testResponse.text();
            console.log(`API keys 状态码: ${status}`);
            console.log(`响应内容: ${responseText.substring(0, 200)}`);
          } else {
            console.log('✗ 未找到 token');
          }
        });

      } else {
        console.log('✗ 未找到登录按钮');
      }

    } catch (error) {
      console.error('测试失败:', error.message);
    } finally {
    await browser.close();
  }

  console.log('=== 测试完成 ===');
})();
