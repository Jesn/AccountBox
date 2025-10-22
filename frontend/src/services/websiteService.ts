import apiClient from './apiClient'
import type {
  ApiResponse,
  PagedResult,
  WebsiteResponse,
  CreateWebsiteRequest,
  UpdateWebsiteRequest,
  AccountCountResponse,
  WebsiteOptionResponse,
} from '@/types'

// 重新导出类型供外部使用
export type { WebsiteResponse, CreateWebsiteRequest, UpdateWebsiteRequest, AccountCountResponse, WebsiteOptionResponse }

class WebsiteService {
  private readonly baseUrl = '/api/websites'

  async getAll(
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<ApiResponse<PagedResult<WebsiteResponse>>> {
    return await apiClient.get<PagedResult<WebsiteResponse>>(
      `${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    )
  }

  async getById(id: number): Promise<ApiResponse<WebsiteResponse>> {
    return await apiClient.get<WebsiteResponse>(`${this.baseUrl}/${id}`)
  }

  async create(
    request: CreateWebsiteRequest
  ): Promise<ApiResponse<WebsiteResponse>> {
    return await apiClient.post<WebsiteResponse>(this.baseUrl, request)
  }

  async update(
    id: number,
    request: UpdateWebsiteRequest
  ): Promise<ApiResponse<WebsiteResponse>> {
    return await apiClient.put<WebsiteResponse>(
      `${this.baseUrl}/${id}`,
      request
    )
  }

  async delete(
    id: number,
    params?: string
  ): Promise<ApiResponse<{ message: string }>> {
    const url = params
      ? `${this.baseUrl}/${id}${params}`
      : `${this.baseUrl}/${id}`
    return await apiClient.delete<{ message: string }>(url)
  }

  async getAccountCount(
    id: number
  ): Promise<ApiResponse<AccountCountResponse>> {
    return await apiClient.get<AccountCountResponse>(
      `${this.baseUrl}/${id}/accounts/count`
    )
  }

  async getOptions(): Promise<ApiResponse<WebsiteOptionResponse[]>> {
    return await apiClient.get<WebsiteOptionResponse[]>(
      `${this.baseUrl}/options`
    )
  }
}

export const websiteService = new WebsiteService()
