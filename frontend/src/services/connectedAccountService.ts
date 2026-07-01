import type { ApiResponse } from '@/types/common';
import type { ConnectedAccount, ConnectedAccountCreateRequest, ConnectedAccountReconnectRequest, ConnectedAccountAuditLog } from '@/types/connectedAccount';
import type { TikTokOAuthAuthorizeResponse, TikTokCreatorInfo, ChannelCapabilities } from '@/types/tiktok';
import { api } from './api';

export const connectedAccountService = {
  /** Lấy danh sách tài khoản liên kết */
  async getAccounts(): Promise<ApiResponse<ConnectedAccount[]>> {
    const { data } = await api.get('/connectedaccounts');
    return data;
  },

  /** Lấy chi tiết tài khoản liên kết */
  async getAccountById(id: string): Promise<ApiResponse<ConnectedAccount>> {
    const { data } = await api.get(`/connectedaccounts/${id}`);
    return data;
  },

  /** Tạo liên kết tài khoản mới */
  async createAccount(request: ConnectedAccountCreateRequest): Promise<ApiResponse<ConnectedAccount>> {
    const { data } = await api.post('/connectedaccounts', request);
    return data;
  },

  /** Kết nối lại tài khoản */
  async reconnectAccount(id: string, request: ConnectedAccountReconnectRequest): Promise<ApiResponse<ConnectedAccount>> {
    const { data } = await api.post(`/connectedaccounts/${id}/reconnect`, request);
    return data;
  },

  /** Kiểm tra trạng thái liên kết */
  async checkAccountHealth(id: string): Promise<ApiResponse<ConnectedAccount>> {
    const { data } = await api.post(`/connectedaccounts/${id}/check-health`);
    return data;
  },

  /** Xóa tài khoản liên kết */
  async deleteAccount(id: string): Promise<ApiResponse<object>> {
    const { data } = await api.delete(`/connectedaccounts/${id}`);
    return data;
  },

  /** Lấy lịch sử log hoạt động của tài khoản */
  async getAuditLogs(id: string): Promise<ApiResponse<ConnectedAccountAuditLog[]>> {
    const { data } = await api.get(`/connectedaccounts/${id}/audit-logs`);
    return data;
  },

  /** Lấy URL OAuth TikTok */
  async getTikTokAuthorizeUrl(): Promise<ApiResponse<TikTokOAuthAuthorizeResponse>> {
    const { data } = await api.get('/connectedaccounts/tiktok/authorize');
    return data;
  },

  /** Lấy capabilities của tài khoản */
  async getCapabilities(id: string): Promise<ApiResponse<ChannelCapabilities>> {
    const { data } = await api.get(`/connectedaccounts/${id}/capabilities`);
    return data;
  },

  /** Lấy creator info TikTok (privacy options, avatar, v.v.) */
  async getTikTokCreatorInfo(id: string): Promise<ApiResponse<TikTokCreatorInfo>> {
    const { data } = await api.get(`/connectedaccounts/${id}/tiktok/creator-info`);
    return data;
  }
};
