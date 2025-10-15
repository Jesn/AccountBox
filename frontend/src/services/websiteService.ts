import apiClient from './apiClient'
import type { ApiResponse, PagedResult } from '../types/common'

export interface WebsiteResponse {
  id: number
  domain: string
  displayName?: string
  tags?: string
  createdAt: string
  updatedAt: string
  activeAccountCount: number
  deletedAccountCount: number
}

export interface CreateWebsiteRequest {
  domain: string
  displayName?: string
  tags?: string
}

export interface UpdateWebsiteRequest {
  domain: string
  displayName?: string
  tags?: string
}

export interface AccountCountResponse {
  activeCount: number
  deletedCount: number
  totalCount: number
}

class WebsiteService {
  private readonly baseUrl = '/api/websites'

  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<WebsiteResponse>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<WebsiteResponse>>>(
      `${this.baseUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    )
    return response.data!
  }

  async getById(id: number): Promise<ApiResponse<WebsiteResponse>> {
    const response = await apiClient.get<ApiResponse<WebsiteResponse>>(`${this.baseUrl}/${id}`)
    return response.data!
  }

  async create(request: CreateWebsiteRequest): Promise<ApiResponse<WebsiteResponse>> {
    const response = await apiClient.post<ApiResponse<WebsiteResponse>>(this.baseUrl, request)
    return response.data!
  }

  async update(id: number, request: UpdateWebsiteRequest): Promise<ApiResponse<WebsiteResponse>> {
    const response = await apiClient.put<ApiResponse<WebsiteResponse>>(`${this.baseUrl}/${id}`, request)
    return response.data!
  }

  async delete(id: number): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.delete<ApiResponse<{ message: string }>>(`${this.baseUrl}/${id}`)
    return response.data!
  }

  async getAccountCount(id: number): Promise<ApiResponse<AccountCountResponse>> {
    const response = await apiClient.get<ApiResponse<AccountCountResponse>>(`${this.baseUrl}/${id}/accounts/count`)
    return response.data!
  }
}

export const websiteService = new WebsiteService()
