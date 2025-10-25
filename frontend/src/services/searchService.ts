import apiClient from './apiClient'
import { API_ENDPOINTS } from '@/lib/constants'
import type { ApiResponse, PagedResult } from '../types/common'

export interface SearchResultItem {
  accountId: number
  websiteId: number
  websiteDomain: string
  websiteDisplayName?: string
  username: string
  password: string
  notes?: string
  tags?: string
  createdAt: string
  updatedAt: string
  matchedFields: string[]
}

class SearchService {
  private readonly baseUrl = API_ENDPOINTS.SEARCH

  async search(
    query: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<ApiResponse<PagedResult<SearchResultItem>>> {
    return await apiClient.get<PagedResult<SearchResultItem>>(
      `${this.baseUrl}?query=${encodeURIComponent(query)}&pageNumber=${pageNumber}&pageSize=${pageSize}`
    )
  }
}

export const searchService = new SearchService()
