import apiClient from './apiClient'
import type { ApiResponse, PagedResult } from '../types/common'

export interface AccountResponse {
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
  isDeleted: boolean
  deletedAt?: string
}

export interface CreateAccountRequest {
  websiteId: number
  username: string
  password: string
  notes?: string
  tags?: string
}

export interface UpdateAccountRequest {
  username: string
  password: string
  notes?: string
  tags?: string
}

class AccountService {
  private readonly baseUrl = '/api/accounts'

  async getAll(
    pageNumber: number = 1,
    pageSize: number = 10,
    websiteId?: number
  ): Promise<ApiResponse<PagedResult<AccountResponse>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    })

    if (websiteId) {
      params.append('websiteId', websiteId.toString())
    }

    return await apiClient.get<PagedResult<AccountResponse>>(
      `${this.baseUrl}?${params.toString()}`
    )
  }

  async getById(id: number): Promise<ApiResponse<AccountResponse>> {
    return await apiClient.get<AccountResponse>(`${this.baseUrl}/${id}`)
  }

  async create(
    request: CreateAccountRequest
  ): Promise<ApiResponse<AccountResponse>> {
    return await apiClient.post<AccountResponse>(this.baseUrl, request)
  }

  async update(
    id: number,
    request: UpdateAccountRequest
  ): Promise<ApiResponse<AccountResponse>> {
    return await apiClient.put<AccountResponse>(
      `${this.baseUrl}/${id}`,
      request
    )
  }

  async delete(id: number): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.delete<{ message: string }>(`${this.baseUrl}/${id}`)
  }

  async enable(id: number): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.put<{ message: string }>(`${this.baseUrl}/${id}/enable`)
  }

  async disable(id: number): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.put<{ message: string }>(`${this.baseUrl}/${id}/disable`)
  }
}

export const accountService = new AccountService()
