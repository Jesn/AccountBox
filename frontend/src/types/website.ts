/**
 * 网站相关类型定义
 */

/**
 * 网站实体（基础）
 */
export interface Website {
  id: number
  domain: string
  displayName?: string
  tags?: string
  createdAt: string
  updatedAt: string
}

/**
 * 网站响应（包含账号统计）
 */
export interface WebsiteResponse extends Website {
  activeAccountCount: number
  disabledAccountCount: number
  deletedAccountCount: number
}

/**
 * 创建网站请求
 */
export interface CreateWebsiteRequest {
  domain: string
  displayName?: string
  tags?: string
}

/**
 * 更新网站请求
 */
export interface UpdateWebsiteRequest {
  domain: string
  displayName?: string
  tags?: string
}

/**
 * 账号数量响应
 */
export interface AccountCountResponse {
  activeCount: number
  deletedCount: number
  totalCount: number
}
