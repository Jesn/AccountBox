export function ApiNotice() {
  return (
    <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-3 sm:p-4">
      <h3 className="font-semibold text-yellow-900 mb-2 text-sm sm:text-base">
        重要提示
      </h3>
      <ul className="text-xs sm:text-sm text-yellow-800 space-y-1 list-disc list-inside">
        <li>所有 API 请求都需要在请求头中包含 X-API-Key</li>
        <li>API 密钥的作用域控制访问范围(所有网站 或 指定网站)</li>
        <li>密码以明文形式返回,请确保在安全环境下使用</li>
        <li className="hidden sm:list-item">建议仅在 localhost 或 VPN 环境下访问 API</li>
        <li>删除 API 密钥后,使用该密钥的请求将返回 401 错误</li>
        <li className="hidden sm:list-item">
          随机获取账号接口采用 24 小时缓存机制：相同密钥在同一网站获取的账号在 24
          小时内保持不变，避免频繁切换账号
        </li>
      </ul>
    </div>
  )
}