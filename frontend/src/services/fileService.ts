import type { ApiResponse } from '@/types/common';
import { api } from './api';

export interface StoredFileResult {
  id: string;
  relativePath: string;
  url: string;
  originalFileName: string;
  storedFileName: string;
  contentType: string;
  sizeBytes: number;
  sha256: string;
}

export const fileService = {
  /**
   * Upload ảnh công khai
   */
  async uploadPublicImage(
    file: File,
    category: 'properties' | 'projects' | 'avatars' = 'properties',
    entityId?: string,
    onProgress?: (progressEvent: any) => void
  ): Promise<ApiResponse<StoredFileResult>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('category', category);
    if (entityId) {
      formData.append('entityId', entityId);
    }

    const { data } = await api.post<ApiResponse<StoredFileResult>>('/files/public-images', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: onProgress
    });
    return data;
  },

  /**
   * Upload tài liệu riêng tư
   */
  async uploadPrivateDocument(
    file: File,
    category: 'contracts' | 'documents' = 'documents',
    entityId?: string,
    onProgress?: (progressEvent: any) => void
  ): Promise<ApiResponse<StoredFileResult>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('category', category);
    if (entityId) {
      formData.append('entityId', entityId);
    }

    const { data } = await api.post<ApiResponse<StoredFileResult>>('/files/private-documents', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: onProgress
    });
    return data;
  },

  /**
   * Lấy metadata file
   */
  async getFileMetadata(id: string): Promise<ApiResponse<StoredFileResult>> {
    const { data } = await api.get<ApiResponse<StoredFileResult>>(`/files/${id}`);
    return data;
  },

  /**
   * Download file riêng tư
   */
  async downloadFile(id: string, fileName: string): Promise<void> {
    const response = await api.get(`/files/${id}/download`, {
      responseType: 'blob'
    });
    
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  /**
   * Xóa file (soft delete)
   */
  async deleteFile(id: string): Promise<void> {
    await api.delete(`/files/${id}`);
  }
};
