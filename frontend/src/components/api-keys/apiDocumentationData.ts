export interface ApiEndpointDoc {
  id: string
  title: string
  method: string
  path: string
  description: string
  curlExample: string
  requestBody?: string
  successResponse: string
  errorResponse?: string
}

export const API_ENDPOINT_DOCS: ApiEndpointDoc[] = [
  {
    id: 'list-websites',
    title: '获取权限内的网站列表',
    method: 'GET',
    path: '/api/external/websites',
    description:
      '获取当前 API 密钥有权访问的网站列表。如果密钥作用域为"所有网站"，则返回所有网站；如果为"指定网站"，则只返回授权的网站。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/websites' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "totalCount": 3,
    "websites": [
      {
        "id": 1,
        "domain": "example.com",
        "displayName": "示例网站",
        "createdAt": "2025-10-01T10:00:00Z",
        "updatedAt": "2025-10-01T10:00:00Z"
      },
      {
        "id": 2,
        "domain": "test.com",
        "displayName": "测试网站",
        "createdAt": "2025-10-02T10:00:00Z",
        "updatedAt": "2025-10-02T10:00:00Z"
      }
    ]
  }
}`,
  },
  {
    id: 'create-account',
    title: '创建账号',
    method: 'POST',
    path: '/api/external/accounts',
    description:
      '为指定网站创建新账号。密码不能为空,但不进行强度验证。extend 为可选的扩展字段,需要是有效的 JSON 字符串。',
    curlExample: `curl -X POST 'http://localhost:5093/api/external/accounts' \\
  -H 'Content-Type: application/json' \\
  -H 'X-API-Key: YOUR_API_KEY' \\
  -d '{
    "websiteId": 1,
    "username": "user@example.com",
    "password": "MyPassword123",
    "tags": "测试账号",
    "notes": "通过API创建的测试账号",
    "extend": "{\\"phone\\": \\"13800138000\\"}"
  }'`,
    requestBody: `{
  "websiteId": 1,          // 必填: 网站ID
  "username": "user@example.com",  // 必填: 用户名
  "password": "MyPassword123",     // 必填: 密码
  "tags": "测试账号",       // 可选: 标签
  "notes": "备注信息",      // 可选: 备注
  "extend": "{\\"key\\": \\"value\\"}"  // 可选: 扩展字段(JSON字符串)
}`,
    successResponse: `{
  "success": true,
  "data": {
    "id": 123,
    "websiteId": 1,
    "username": "user@example.com",
    "status": "Active",
    "createdAt": "2025-10-17T10:30:00Z"
  }
}`,
    errorResponse: `{
  "success": false,
  "error": {
    "errorCode": "ACCESS_DENIED",
    "message": "API密钥无权访问该网站"
  }
}`,
  },
  {
    id: 'delete-account',
    title: '删除账号',
    method: 'DELETE',
    path: '/api/external/accounts/{id}',
    description: '删除指定账号(移入回收站)。需要API密钥有权访问该账号所属的网站。',
    curlExample: `curl -X DELETE 'http://localhost:5093/api/external/accounts/123' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "id": 123,
    "message": "账号已移入回收站"
  }
}`,
    errorResponse: `{
  "success": false,
  "error": {
    "errorCode": "ACCOUNT_NOT_FOUND",
    "message": "账号不存在"
  }
}`,
  },
  {
    id: 'update-status',
    title: '更新账号状态',
    method: 'PUT',
    path: '/api/external/accounts/{id}/status',
    description: '启用或禁用账号。状态值可以是 "Active"(启用) 或 "Disabled"(禁用)。',
    curlExample: `curl -X PUT 'http://localhost:5093/api/external/accounts/123/status' \\
  -H 'Content-Type: application/json' \\
  -H 'X-API-Key: YOUR_API_KEY' \\
  -d '{
    "status": "Active"
  }'`,
    requestBody: `{
  "status": "Active"  // 或 "Disabled"
}`,
    successResponse: `{
  "success": true,
  "data": {
    "id": 123,
    "status": "Active",
    "updatedAt": "2025-10-17T10:35:00Z"
  }
}`,
  },
  {
    id: 'list-accounts',
    title: '获取账号列表',
    method: 'GET',
    path: '/api/external/websites/{websiteId}/accounts',
    description:
      '获取指定网站的分页账号列表。可选参数 status=Active 或 status=Disabled 进行状态过滤；pageNumber 从 1 开始，pageSize 默认 10，最大 100。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/websites/1/accounts?status=Active&pageNumber=1&pageSize=10' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "websiteId": 1,
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false,
    "accounts": [
      {
        "id": 123,
        "websiteId": 1,
        "username": "user@example.com",
        "password": "MyPassword123",
        "tags": "测试账号",
        "notes": "备注信息",
        "status": "Active",
        "extendedData": {
          "phone": "13800138000"
        },
        "createdAt": "2025-10-17T10:30:00Z",
        "updatedAt": "2025-10-17T10:35:00Z"
      }
    ]
  }
}`,
  },
  {
    id: 'random-account',
    title: '随机获取启用账号',
    method: 'GET',
    path: '/api/external/websites/{websiteId}/accounts/random',
    description:
      '从指定网站的启用账号中随机返回一个。适用于自动化测试或轮询使用场景。注意：使用相同 API 密钥在 24 小时内对同一网站重复调用此接口，将返回相同的账号（缓存防重复机制）。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/websites/1/accounts/random' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "id": 123,
    "websiteId": 1,
    "username": "user@example.com",
    "password": "MyPassword123",
    "tags": "测试账号",
    "notes": "备注信息",
    "status": "Active",
    "createdAt": "2025-10-17T10:30:00Z",
    "updatedAt": "2025-10-17T10:35:00Z"
  }
}`,
    errorResponse: `{
  "success": false,
  "error": {
    "errorCode": "NO_ENABLED_ACCOUNTS",
    "message": "该网站没有可用的启用账号"
  }
}`,
  },
  {
    id: 'check-username',
    title: '检查用户名是否存在',
    method: 'GET',
    path: '/api/external/websites/{websiteId}/accounts/check-username',
    description:
      '检查指定网站下是否存在指定用户名的账号。返回布尔值，true 表示用户名已存在，false 表示用户名不存在。适用于创建账号前的用户名重复检查。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/websites/1/accounts/check-username?username=user@example.com' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "websiteId": 1,
    "username": "user@example.com",
    "exists": true
  },
  "error": null,
  "timestamp": "2025-10-18T09:15:19.147771Z"
}`,
    errorResponse: `{
  "success": false,
  "error": {
    "errorCode": "USERNAME_REQUIRED",
    "message": "用户名参数不能为空"
  }
}`,
  },
  {
    id: 'generate-password',
    title: '生成随机密码',
    method: 'GET',
    path: '/api/external/password/generate',
    description:
      '生成密码学安全的随机密码。默认生成8位密码，可通过 length 参数自定义长度（8-128位）。密码包含大写字母、小写字母、数字和符号，并排除易混淆字符（0O1lI等）。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/password/generate?length=16' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "password": "uYv9pbFdV69rTd4t",
    "length": 16,
    "strength": {
      "score": 81,
      "level": "VeryStrong",
      "entropy": 95.27
    }
  },
  "error": null,
  "timestamp": "2025-10-18T08:35:51.670975Z"
}`,
    errorResponse: `{
  "success": false,
  "error": {
    "errorCode": "INVALID_LENGTH",
    "message": "密码长度必须在8到128之间"
  }
}`,
  },
]