import apiClient from './apiClient'
import type { ApiResponse, PagedResult } from '../types/common'

export interface DeletedAccountResponse {
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
  deletedAt?: string
}

class RecycleBinService {
  private readonly baseUrl = '/api/recycle-bin'

  async getDeletedAccounts(
    pageNumber: number = 1,
    pageSize: number = 10,
    websiteId?: number
  ): Promise<ApiResponse<PagedResult<DeletedAccountResponse>>> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
    })

    if (websiteId) {
      params.append('websiteId', websiteId.toString())
    }

    return await apiClient.get<PagedResult<DeletedAccountResponse>>(
      `${this.baseUrl}?${params.toString()}`
    )
  }

  async restoreAccount(id: number): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.post<{ message: string }>(
      `${this.baseUrl}/${id}/restore`
    )
  }

  async permanentlyDeleteAccount(
    id: number
  ): Promise<ApiResponse<{ message: string }>> {
    return await apiClient.delete<{ message: string }>(`${this.baseUrl}/${id}`)
  }

  async emptyRecycleBin(
    websiteId?: number
  ): Promise<ApiResponse<{ message: string }>> {
    const params = websiteId ? `?websiteId=${websiteId}` : ''
    return await apiClient.delete<{ message: string }>(
      `${this.baseUrl}${params}`
    )
  }
}

export const recycleBinService = new RecycleBinService()
