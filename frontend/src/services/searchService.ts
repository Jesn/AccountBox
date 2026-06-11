import apiClient from './apiClient'
import { API_ENDPOINTS } from '@/lib/constants'
import type { ApiResponse, PagedResult, SearchResultItem } from '@/types'

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
