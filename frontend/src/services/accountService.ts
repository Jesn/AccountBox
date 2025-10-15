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

    const response = await apiClient.get<ApiResponse<PagedResult<AccountResponse>>>(
      `${this.baseUrl}?${params.toString()}`
    )
    return response.data!
  }

  async getById(id: number): Promise<ApiResponse<AccountResponse>> {
    const response = await apiClient.get<ApiResponse<AccountResponse>>(`${this.baseUrl}/${id}`)
    return response.data!
  }

  async create(request: CreateAccountRequest): Promise<ApiResponse<AccountResponse>> {
    const response = await apiClient.post<ApiResponse<AccountResponse>>(this.baseUrl, request)
    return response.data!
  }

  async update(id: number, request: UpdateAccountRequest): Promise<ApiResponse<AccountResponse>> {
    const response = await apiClient.put<ApiResponse<AccountResponse>>(`${this.baseUrl}/${id}`, request)
    return response.data!
  }

  async delete(id: number): Promise<ApiResponse<{ message: string }>> {
    const response = await apiClient.delete<ApiResponse<{ message: string }>>(`${this.baseUrl}/${id}`)
    return response.data!
  }
}

export const accountService = new AccountService()
