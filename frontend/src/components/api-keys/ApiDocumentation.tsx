import { useState } from 'react'
import { Copy, Check } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion'
import { copyToClipboard } from '@/lib/clipboard'

interface ApiEndpoint {
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

const API_ENDPOINTS: ApiEndpoint[] = [
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
    description: '为指定网站创建新账号。密码不能为空,但不进行强度验证。',
    curlExample: `curl -X POST 'http://localhost:5093/api/external/accounts' \\
  -H 'Content-Type: application/json' \\
  -H 'X-API-Key: YOUR_API_KEY' \\
  -d '{
    "websiteId": 1,
    "username": "user@example.com",
    "password": "MyPassword123",
    "tags": "测试账号",
    "notes": "通过API创建的测试账号"
  }'`,
    requestBody: `{
  "websiteId": 1,
  "username": "user@example.com",
  "password": "MyPassword123",
  "tags": "测试账号",
  "notes": "通过API创建的测试账号"
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
      '获取指定网站的账号列表。可选参数 status=Active 或 status=Disabled 进行过滤。',
    curlExample: `curl -X GET 'http://localhost:5093/api/external/websites/1/accounts?status=Active' \\
  -H 'X-API-Key: YOUR_API_KEY'`,
    successResponse: `{
  "success": true,
  "data": {
    "websiteId": 1,
    "totalCount": 5,
    "accounts": [
      {
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

/**
 * API 使用文档组件
 * 展示外部 API 端点的 curl 使用示例
 */
export function ApiDocumentation() {
  const [copiedId, setCopiedId] = useState<string | null>(null)

  const handleCopy = async (text: string, id: string) => {
    const success = await copyToClipboard(text)
    if (success) {
      setCopiedId(id)
      setTimeout(() => setCopiedId(null), 2000)
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-2xl font-bold mb-2">API 使用文档</h2>
        <p className="text-muted-foreground">
          以下是外部 API 端点的 curl 使用示例。请将示例中的{' '}
          <code className="bg-muted px-1 py-0.5 rounded">YOUR_API_KEY</code>{' '}
          替换为你的实际 API 密钥。
        </p>
      </div>

      <Accordion type="single" collapsible className="w-full">
        {API_ENDPOINTS.map((endpoint) => (
          <AccordionItem key={endpoint.id} value={endpoint.id}>
            <AccordionTrigger>
              <div className="flex items-center gap-3 text-left">
                <span
                  className={`px-2 py-1 text-xs font-semibold rounded ${
                    endpoint.method === 'GET'
                      ? 'bg-blue-100 text-blue-700'
                      : endpoint.method === 'POST'
                        ? 'bg-green-100 text-green-700'
                        : endpoint.method === 'PUT'
                          ? 'bg-yellow-100 text-yellow-700'
                          : 'bg-red-100 text-red-700'
                  }`}
                >
                  {endpoint.method}
                </span>
                <div>
                  <div className="font-semibold">{endpoint.title}</div>
                  <div className="text-sm text-muted-foreground font-mono">
                    {endpoint.path}
                  </div>
                </div>
              </div>
            </AccordionTrigger>
            <AccordionContent>
              <div className="space-y-4 pt-2">
                {/* 描述 */}
                <p className="text-sm text-muted-foreground">
                  {endpoint.description}
                </p>

                {/* curl 示例 */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium">curl 示例</span>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        handleCopy(
                          endpoint.curlExample,
                          `curl-${endpoint.id}`
                        )
                      }
                    >
                      {copiedId === `curl-${endpoint.id}` ? (
                        <>
                          <Check className="h-4 w-4 mr-1" />
                          已复制
                        </>
                      ) : (
                        <>
                          <Copy className="h-4 w-4 mr-1" />
                          复制
                        </>
                      )}
                    </Button>
                  </div>
                  <pre className="bg-muted p-3 rounded-md overflow-x-auto text-sm">
                    <code>{endpoint.curlExample}</code>
                  </pre>
                </div>

                {/* 请求体 */}
                {endpoint.requestBody && (
                  <div>
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-sm font-medium">请求体</span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          handleCopy(
                            endpoint.requestBody!,
                            `request-${endpoint.id}`
                          )
                        }
                      >
                        {copiedId === `request-${endpoint.id}` ? (
                          <>
                            <Check className="h-4 w-4 mr-1" />
                            已复制
                          </>
                        ) : (
                          <>
                            <Copy className="h-4 w-4 mr-1" />
                            复制
                          </>
                        )}
                      </Button>
                    </div>
                    <pre className="bg-muted p-3 rounded-md overflow-x-auto text-sm">
                      <code>{endpoint.requestBody}</code>
                    </pre>
                  </div>
                )}

                {/* 成功响应 */}
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium">成功响应</span>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() =>
                        handleCopy(
                          endpoint.successResponse,
                          `success-${endpoint.id}`
                        )
                      }
                    >
                      {copiedId === `success-${endpoint.id}` ? (
                        <>
                          <Check className="h-4 w-4 mr-1" />
                          已复制
                        </>
                      ) : (
                        <>
                          <Copy className="h-4 w-4 mr-1" />
                          复制
                        </>
                      )}
                    </Button>
                  </div>
                  <pre className="bg-muted p-3 rounded-md overflow-x-auto text-sm">
                    <code>{endpoint.successResponse}</code>
                  </pre>
                </div>

                {/* 错误响应 */}
                {endpoint.errorResponse && (
                  <div>
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-sm font-medium">错误响应示例</span>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() =>
                          handleCopy(
                            endpoint.errorResponse!,
                            `error-${endpoint.id}`
                          )
                        }
                      >
                        {copiedId === `error-${endpoint.id}` ? (
                          <>
                            <Check className="h-4 w-4 mr-1" />
                            已复制
                          </>
                        ) : (
                          <>
                            <Copy className="h-4 w-4 mr-1" />
                            复制
                          </>
                        )}
                      </Button>
                    </div>
                    <pre className="bg-muted p-3 rounded-md overflow-x-auto text-sm">
                      <code>{endpoint.errorResponse}</code>
                    </pre>
                  </div>
                )}
              </div>
            </AccordionContent>
          </AccordionItem>
        ))}
      </Accordion>

      {/* 通用说明 */}
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <h3 className="font-semibold text-yellow-900 mb-2">重要提示</h3>
        <ul className="text-sm text-yellow-800 space-y-1 list-disc list-inside">
          <li>所有 API 请求都需要在请求头中包含 X-API-Key</li>
          <li>API 密钥的作用域控制访问范围(所有网站 或 指定网站)</li>
          <li>密码以明文形式返回,请确保在安全环境下使用</li>
          <li>建议仅在 localhost 或 VPN 环境下访问 API</li>
          <li>删除 API 密钥后,使用该密钥的请求将返回 401 错误</li>
          <li>
            随机获取账号接口采用 24
            小时缓存机制：相同密钥在同一网站获取的账号在 24
            小时内保持不变，避免频繁切换账号
          </li>
        </ul>
      </div>
    </div>
  )
}
