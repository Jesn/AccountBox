/**
 * 搜索相关类型定义
 */

/**
 * 搜索结果项
 */
export interface SearchResultItem {
  id: number
  websiteId: number
  websiteDomain: string
  websiteDisplayName?: string
  username: string
  password: string
  notes?: string
  tags?: string
  status: 'Active' | 'Disabled'
  createdAt: string
  updatedAt: string
}
