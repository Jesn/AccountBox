import apiClient from './apiClient';
import type { ApiKey, CreateApiKeyRequest } from '../types/ApiKey';
import type { ApiResponse } from '../types/common';

/**
 * API密钥服务
 */
export const apiKeyService = {
  /**
   * 获取所有API密钥
   */
  async getAll(): Promise<ApiKey[]> {
    const response = await apiClient.get<ApiResponse<ApiKey[]>>('/api-keys');
    if (!response.data || !response.data.data) {
      throw new Error('Invalid response data');
    }
    return response.data.data;
  },

  /**
   * 创建新的API密钥
   */
  async create(request: CreateApiKeyRequest): Promise<ApiKey> {
    const response = await apiClient.post<ApiResponse<ApiKey>>('/api-keys', request);
    if (!response.data || !response.data.data) {
      throw new Error('Invalid response data');
    }
    return response.data.data;
  },

  /**
   * 删除API密钥
   */
  async delete(id: number): Promise<void> {
    await apiClient.delete(`/api-keys/${id}`);
  },
};
