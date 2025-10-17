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
  disabledAccountCount: number
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
}

export const websiteService = new WebsiteService()
