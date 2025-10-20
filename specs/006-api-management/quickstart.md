# Quick Start: API密钥管理与外部API服务

**Feature**: 006-api-management | **Date**: 2025-10-16
**Purpose**: 开发者快速上手指南

## 概述

本功能为AccountBox添加完整的外部API服务能力。开发者可以：
1. 在Web UI中创建和管理API密钥
2. 使用API密钥调用外部API进行账号管理
3. 为账号添加自定义扩展字段
4. 管理账号的启用/禁用状态

**5分钟快速体验**：创建密钥 → 调用API创建账号 → 随机获取账号

---

## 前置条件

- AccountBox已启动（前端：http://localhost:5173，后端：http://localhost:5093）
- 已创建Vault并解锁
- 至少有一个网站记录

---

## 步骤1：创建API密钥（Web UI）

### 1.1 进入API密钥管理页面

访问 http://localhost:5173/api-keys

### 1.2 创建新密钥

点击"创建API密钥"按钮，填写：
- **名称**: 测试密钥
- **作用域**: 选择"所有网站"或"指定网站"

提交后，系统生成密钥（格式：`sk_xxx...`）。

### 1.3 复制密钥

在密钥列表中，点击"复制"按钮复制密钥明文。

**示例密钥**:
```
sk_abcdefghijklmnopqrstuvwxyz123456
```

---

## 步骤2：使用API密钥调用外部API

### 2.1 创建账号

```bash
curl -X POST 'http://localhost:5093/api/external/accounts' \
  -H 'Content-Type: application/json' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456' \
  -d '{
    "websiteId": 1,
    "username": "testuser@example.com",
    "password": "MyPassword123",
    "extendedData": {
      "email": "testuser@example.com",
      "registrationDate": "2025-10-16"
    },
    "tags": ["API创建", "测试"],
    "notes": "通过API创建的测试账号"
  }'
```

**成功响应** (201):
```json
{
  "data": {
    "id": 42,
    "websiteId": 1,
    "username": "testuser@example.com",
    "password": "MyPassword123",
    "status": "Active",
    "extendedData": {
      "email": "testuser@example.com",
      "registrationDate": "2025-10-16"
    },
    "tags": ["API创建", "测试"],
    "notes": "通过API创建的测试账号",
    "createdAt": "2025-10-16T10:30:00Z",
    "updatedAt": "2025-10-16T10:30:00Z"
  }
}
```

### 2.2 禁用账号

```bash
curl -X PUT 'http://localhost:5093/api/external/accounts/42/status' \
  -H 'Content-Type: application/json' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456' \
  -d '{
    "status": "Disabled"
  }'
```

**成功响应** (200):
```json
{
  "data": {
    "id": 42,
    "status": "Disabled",
    ...
  }
}
```

### 2.3 启用账号

```bash
curl -X PUT 'http://localhost:5093/api/external/accounts/42/status' \
  -H 'Content-Type: application/json' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456' \
  -d '{
    "status": "Active"
  }'
```

### 2.4 获取账号列表

```bash
# 获取所有账号（活跃+禁用）
curl -X GET 'http://localhost:5093/api/external/websites/1/accounts' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456'

# 仅获取活跃账号
curl -X GET 'http://localhost:5093/api/external/websites/1/accounts?status=Active' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456'

# 仅获取禁用账号
curl -X GET 'http://localhost:5093/api/external/websites/1/accounts?status=Disabled' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456'
```

### 2.5 随机获取启用账号

```bash
curl -X GET 'http://localhost:5093/api/external/websites/1/accounts/random' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456'
```

**成功响应** (200):
```json
{
  "data": {
    "id": 15,
    "username": "randomuser@example.com",
    "password": "SomePassword",
    "status": "Active",
    ...
  }
}
```

**无启用账号时** (404):
```json
{
  "error": {
    "message": "该网站没有可用的启用账号",
    "code": "NOT_FOUND"
  }
}
```

### 2.6 删除账号

```bash
curl -X DELETE 'http://localhost:5093/api/external/accounts/42' \
  -H 'X-API-Key: sk_abcdefghijklmnopqrstuvwxyz123456'
```

**成功响应** (204): 无响应体

**说明**: 账号被移入回收站，保留原有状态（Active/Disabled）

---

## 步骤3：Web UI中管理账号扩展字段

### 3.1 查看账号详情

在账号列表中点击账号，查看详情对话框。

### 3.2 编辑扩展字段

在"扩展字段"部分，点击"添加字段"按钮：
- **键**: email
- **值**: user@example.com

点击"保存"。

### 3.3 验证扩展字段

通过API再次获取该账号，验证`extendedData`包含新添加的字段。

---

## 常见场景示例

### 场景1：爬虫脚本轮询账号

```python
import requests
import random
import time

API_KEY = "sk_abcdefghijklmnopqrstuvwxyz123456"
BASE_URL = "http://localhost:5093/api/external"
WEBSITE_ID = 1

def get_random_account():
    response = requests.get(
        f"{BASE_URL}/websites/{WEBSITE_ID}/accounts/random",
        headers={"X-API-Key": API_KEY}
    )
    if response.status_code == 200:
        account = response.json()["data"]
        return account["username"], account["password"]
    elif response.status_code == 404:
        print("没有可用账号")
        return None, None
    else:
        print(f"错误: {response.status_code}")
        return None, None

# 每10秒随机获取一个账号
while True:
    username, password = get_random_account()
    if username:
        print(f"使用账号: {username}")
        # 执行爬虫逻辑...
    time.sleep(10)
```

### 场景2：批量创建测试账号

```bash
#!/bin/bash
API_KEY="sk_abcdefghijklmnopqrstuvwxyz123456"
WEBSITE_ID=1

for i in {1..10}; do
  curl -X POST 'http://localhost:5093/api/external/accounts' \
    -H 'Content-Type: application/json' \
    -H "X-API-Key: $API_KEY" \
    -d "{
      \"websiteId\": $WEBSITE_ID,
      \"username\": \"testuser$i@example.com\",
      \"password\": \"TestPass$i\",
      \"tags\": [\"批量创建\", \"测试$i\"],
      \"notes\": \"批量创建的测试账号 #$i\"
    }"
  echo "创建账号 #$i"
done
```

### 场景3：账号状态管理

```javascript
const axios = require('axios');

const API_KEY = 'sk_abcdefghijklmnopqrstuvwxyz123456';
const BASE_URL = 'http://localhost:5093/api/external';

// 禁用所有包含"过期"标签的账号
async function disableExpiredAccounts(websiteId) {
  const { data } = await axios.get(
    `${BASE_URL}/websites/${websiteId}/accounts`,
    { headers: { 'X-API-Key': API_KEY } }
  );

  const expiredAccounts = data.data.filter(
    account => account.tags.includes('过期')
  );

  for (const account of expiredAccounts) {
    await axios.put(
      `${BASE_URL}/accounts/${account.id}/status`,
      { status: 'Disabled' },
      { headers: { 'X-API-Key': API_KEY } }
    );
    console.log(`已禁用账号: ${account.username}`);
  }
}

disableExpiredAccounts(1);
```

---

## 错误处理

### 401 Unauthorized - API密钥无效

```json
{
  "error": {
    "message": "Invalid API key",
    "code": "UNAUTHORIZED"
  }
}
```

**原因**:
- API密钥格式错误
- 密钥已被删除
- 请求头缺少`X-API-Key`

**解决**: 检查密钥是否正确，或在Web UI中重新创建

### 403 Forbidden - 无权访问

```json
{
  "error": {
    "message": "API密钥无权访问该网站",
    "code": "FORBIDDEN"
  }
}
```

**原因**: API密钥的作用域为"指定网站"，但请求的网站不在允许列表中

**解决**: 修改API密钥作用域，或使用有权限的密钥

### 400 Bad Request - 参数错误

```json
{
  "error": {
    "message": "密码不能为空",
    "code": "VALIDATION_ERROR"
  }
}
```

**原因**: 请求参数不符合验证规则

**解决**: 检查请求体是否完整且符合要求

---

## 安全注意事项

1. **保护API密钥**: 不要将密钥提交到代码仓库，使用环境变量存储
2. **作用域最小化**: 创建密钥时只授权必要的网站
3. **定期轮换**: 定期删除旧密钥并创建新密钥
4. **HTTPS**: 生产环境必须使用HTTPS防止密钥被窃听
5. **监控使用**: 定期检查密钥的`lastUsedAt`，发现异常及时删除

---

## 下一步

- 查看 [data-model.md](./data-model.md) 了解数据结构
- 查看 [contracts/api-specification.yaml](./contracts/api-specification.yaml) 获取完整API文档
- 运行 `/speckit.tasks` 生成实施任务清单

---

## 故障排查

### 问题：无法创建API密钥

**检查**:
1. 是否已登录且Vault已解锁？
2. 浏览器控制台是否有错误？
3. 后端API是否正常运行？

### 问题：API调用返回401

**检查**:
1. 请求头是否包含`X-API-Key`？
2. 密钥格式是否正确（"sk_"前缀 + 32字符）？
3. 密钥是否在Web UI中可见（未被删除）？

### 问题：随机获取账号总是返回404

**检查**:
1. 该网站是否有账号？
2. 账号是否都处于"Disabled"状态？
3. API密钥是否有权访问该网站？

---

## 开发者资源

- **OpenAPI文档**: `contracts/api-specification.yaml`
- **Postman集合**: 导入OpenAPI文档生成
- **示例代码**: 参见"常见场景示例"部分
- **数据模型**: `data-model.md`
